using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Skuld.Core.Commands.TypeReaders;
using Skuld.Core.Extensions;
using Skuld.Extensions;
using Skuld.Models;
using Skuld.Models.Database;
using Skuld.Utilities.Discord;
using Skuld.Utilities.Messaging;
using StatsdClient;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Skuld.Services
{
    public class MessageService
    {
        private readonly DiscordShardedClient client;
        private DatabaseService database;
        public DiscordLogger logger;
        public MessageServiceConfig config;
        public CommandService commandService;

        public MessageService(DiscordShardedClient cli, DatabaseService db, DiscordLogger log, MessageServiceConfig serviceConfig) //inherits from depinjection
        {
            client = cli;
            database = db;
            config = serviceConfig;
            logger = log;
            logger.AddMessageService(this);
        }

        public async Task ConfigureAsync(CommandServiceConfig config, IServiceProvider services)
        {
            try
            {
                commandService = new CommandService(config);

                commandService.Log += (arg) =>
                   logger.logger.AddToLogsAsync(new Core.Models.LogMessage("CommandService - " + arg.Source, arg.Message, arg.Severity, arg.Exception));

                await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), services);
                commandService.AddTypeReader<Uri>(new UriTypeReader());
            }
            catch (Exception ex)
            {
                await logger.logger.AddToLogsAsync(new Core.Models.LogMessage("CS-Config", ex.Message, LogSeverity.Critical, ex));
            }
        }

        public async Task OnMessageRecievedAsync(SocketMessage arg)
        {
            DogStatsd.Increment("messages.recieved");

            if (arg.Author.IsBot) { return; }

            var message = arg as SocketUserMessage;
            if (message is null) { return; }

            var context = new ShardedCommandContext(client, message);

            if (!MessageTools.IsEnabledChannel(context.Guild.GetUser(context.User.Id), (ITextChannel)context.Channel)) { return; }

            try
            {
                SkuldGuild sguild = await MessageTools.GetGuildAsync(context.Guild, database).ConfigureAwait(false);
                await UserBannedAsync(context.User).ConfigureAwait(false);

                if (sguild != null) { if (!MessageTools.HasPrefix(message, config, sguild.Prefix)) { return; } }
                else { if (!MessageTools.HasPrefix(message, config)) { return; } }

                var cmds = commandService.Search(context, MessageTools.GetCmdName(message, HostService.Configuration.Discord, sguild)).Commands;
                if (cmds == null || cmds.Count == 0) { return; }

                var cmd = cmds.FirstOrDefault().Command;
                if (MessageTools.ModuleDisabled(sguild.GuildSettings.Modules, cmd)) { return; }

                Thread thd = new Thread(
                    async () =>
                    {
                        DogStatsd.Increment("commands.total.threads", 1, 1, new[] { $"module:{cmd.Module.Name.ToLowerInvariant()}", $"cmd:{cmd.Name.ToLowerInvariant()}" });
                        await DispatchCommandAsync(context, cmd).ConfigureAwait(false);
                    })
                {
                    IsBackground = true,
                    Name = $"CommandExecution - module:{cmd.Module.Name.ToLowerInvariant()}|cmd:{cmd.Name.ToLowerInvariant()}"
                };
                thd.Start();
            }
            catch (Exception ex)
            {
                await logger.logger.AddToLogsAsync(new Core.Models.LogMessage("CmdDisp", ex.Message, LogSeverity.Error, ex));
            }
        }

        public async Task<bool> UserBannedAsync(IUser user)
        {
            var usr = await MessageTools.GetUserAsync(user, database).ConfigureAwait(false);
            if (await database.CheckConnectionAsync())
            {
                if (usr.AvatarUrl != user.GetAvatarUrl())
                {
                    await database.SingleQueryAsync(new MySql.Data.MySqlClient.MySqlCommand($"UPDATE `users` SET `AvatarUrl` = \"{user.GetAvatarUrl()}\" WHERE `UserID` = {user.Id};"));
                }
            }
            if (usr != null && usr.Banned) return true;

            return false;
        }

        public async Task DispatchCommandAsync(ICommandContext context, CommandInfo command)
        {
            var watch = new Stopwatch();
            watch.Start();
            var result = await commandService.ExecuteAsync(context, config.ArgPos, HostService.Services);
            watch.Stop();

            if (result.IsSuccess)
            {
                DogStatsd.Histogram("commands.latency", watch.ElapsedMilliseconds(), 0.5, new[] { $"module:{command.Module.Name.ToLowerInvariant()}", $"cmd:{command.Name.ToLowerInvariant()}" });
                if (await database.CheckConnectionAsync())
                {
                    await InsertCommand(command, context.User).ConfigureAwait(false);
                }
                DogStatsd.Increment("commands.processed", 1, 1, new[] { $"module:{command.Module.Name.ToLowerInvariant()}", $"cmd:{command.Name.ToLowerInvariant()}" });
            }

            if (!result.IsSuccess)
            {
                bool displayerror = true;
                if (result.ErrorReason.Contains("few parameters"))
                {
                    var cmdembed = DiscordUtilities.GetCommandHelp(commandService, context, command.Name);
                    await MessageTools.SendChannelAsync(context.Channel, "You seem to be missing a parameter or 2, here's the help", cmdembed, logger.logger).ConfigureAwait(false);
                    displayerror = false;
                }

                if (result.Error != CommandError.UnknownCommand && !result.ErrorReason.Contains("Timeout") && displayerror)
                {
                    await logger.logger.AddToLogsAsync(new Core.Models.LogMessage("CmdHand", "Error with command, Error is: " + result, LogSeverity.Error));
                    await MessageTools.SendChannelAsync(context.Channel, "",
                        new EmbedBuilder
                        {
                            Author = new EmbedAuthorBuilder { Name = "Error with the command" },
                            Description = Convert.ToString(result.ErrorReason),
                            Color = new Color(255, 0, 0) }.Build(),
                        logger.logger).ConfigureAwait(false);
                }
                DogStatsd.Increment("commands.errors");

                switch (result.Error)
                {
                    case CommandError.UnmetPrecondition:
                        DogStatsd.Increment("commands.errors", 1, 1, new[] { "err:unm-precon" });
                        break;

                    case CommandError.Unsuccessful:
                        DogStatsd.Increment("commands.errors", 1, 1, new[] { "err:generic" });
                        break;

                    case CommandError.MultipleMatches:
                        DogStatsd.Increment("commands.errors", 1, 1, new[] { "err:multiple" });
                        break;

                    case CommandError.BadArgCount:
                        DogStatsd.Increment("commands.errors", 1, 1, new[] { "err:incorr-args" });
                        break;

                    case CommandError.ParseFailed:
                        DogStatsd.Increment("commands.errors", 1, 1, new[] { "err:parse-fail" });
                        break;

                    case CommandError.Exception:
                        DogStatsd.Increment("commands.errors", 1, 1, new[] { "err:exception" });
                        break;

                    case CommandError.UnknownCommand:
                        DogStatsd.Increment("commands.errors", 1, 1, new[] { "err:unk-cmd" });
                        break;
                }
            }
        }

        private async Task InsertCommand(CommandInfo command, IUser user)
        {
            var suser = await database.GetUserAsync(user.Id);
            if (suser != null) { await database.UpdateUserUsageAsync(suser, command.Name ?? command.Module.Name); }
            else
            {
                await database.InsertUserAsync(user).ConfigureAwait(false);
                await InsertCommand(command, user).ConfigureAwait(false);
            }
        }
    }
}