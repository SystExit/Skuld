using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Skuld.Core.Commands.TypeReaders;
using Skuld.Commands;
using Skuld.Models;
using Skuld.Models.Database;
using Skuld.Utilities.Discord;
using Skuld.Utilities.Messaging;
using StatsdClient;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Skuld.Services
{
    public class MessageService
    {
        private DatabaseService database;
        public MessageServiceConfig config;
        public CommandService commandService;
        private DiscordShardedClient discord;
        private CustomCommandService customCommand;
        private ExperienceService experienceService;

        public MessageService(DiscordShardedClient client, DatabaseService db, MessageServiceConfig serviceConfig)
        {
            discord = client;
            database = db;
            config = serviceConfig;
            customCommand = new CustomCommandService(client, serviceConfig, db);
            experienceService = new ExperienceService(client, db);
        }

        public async Task ConfigureAsync(CommandServiceConfig config)
        {
            try
            {
                commandService = new CommandService(config);

                commandService.Log += async (arg) =>
                   await HostService.Logger.AddToLogsAsync(new Core.Models.LogMessage("CommandService - " + arg.Source, arg.Message, arg.Severity, arg.Exception));

                await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), HostService.Services);
                commandService.AddTypeReader<Uri>(new UriTypeReader());
            }
            catch (Exception ex)
            {
                await HostService.Logger.AddToLogsAsync(new Core.Models.LogMessage("CS-Config", ex.Message, LogSeverity.Critical, ex));
            }
        }

        public async Task OnMessageRecievedAsync(SocketMessage arg)
        {
            DogStatsd.Increment("messages.recieved");

#pragma warning disable CS4014
            Task.Run(() => experienceService.MessageReceivedAsync(arg));
            Task.Run(() => customCommand.MessageReceivedAsync(arg));
#pragma warning restore CS4014

            if (arg.Author.IsBot) { return; }

            var message = arg as SocketUserMessage;
            if (message is null) { return; }

            var gchan = message.Channel as ITextChannel;
            if (gchan is null) { return; }

            if (!MessageTools.IsEnabledChannel(await gchan.Guild.GetUserAsync(message.Author.Id), (ITextChannel)message.Channel)) { return; }

            try
            {
                SkuldGuild sguild = await MessageTools.GetGuildAsync(gchan.Guild, database).ConfigureAwait(false);

                SkuldUser usr = await MessageTools.GetUserAsync(message.Author, database).ConfigureAwait(false);

                var context = new SkuldCommandContext(discord, message, database, usr, sguild);

                if (await database.CheckConnectionAsync())
                {
                    if (usr.AvatarUrl != context.User.GetAvatarUrl())
                    {
                        await database.SingleQueryAsync(new MySql.Data.MySqlClient.MySqlCommand($"UPDATE `users` SET `AvatarUrl` = \"{context.User.GetAvatarUrl()}\" WHERE `UserID` = {context.User.Id};"));
                    }
                }
                if (usr != null && usr.Banned) return;

                if (sguild != null) { if (!MessageTools.HasPrefix(message, config, sguild.Prefix)) { return; } }
                else { if (!MessageTools.HasPrefix(message, config)) { return; } }

                var cmds = commandService.Search(context, MessageTools.GetCmdName(message, HostService.Configuration.Discord, sguild)).Commands;
                if (cmds == null || cmds.Count == 0) { return; }

                var cmd = cmds.FirstOrDefault().Command;
                if (sguild != null) { if (MessageTools.ModuleDisabled(sguild.GuildSettings.Modules, cmd)) { return; } }

                DogStatsd.Increment("commands.total.threads", 1, 1, new[] { $"module:{cmd.Module.Name.ToLowerInvariant()}", $"cmd:{cmd.Name.ToLowerInvariant()}" });

                await DispatchCommandAsync(context, cmd).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await HostService.Logger.AddToLogsAsync(new Core.Models.LogMessage("CmdDisp", ex.Message, LogSeverity.Error, ex));
            }
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
                    await MessageTools.SendChannelAsync(context.Channel, "You seem to be missing a parameter or 2, here's the help", cmdembed).ConfigureAwait(false);
                    displayerror = false;
                }

                if (result.Error != CommandError.UnknownCommand && !result.ErrorReason.Contains("Timeout") && displayerror)
                {
                    await HostService.Logger.AddToLogsAsync(new Core.Models.LogMessage("CmdHand", "Error with command, Error is: " + result, LogSeverity.Error));
                    await MessageTools.SendChannelAsync(context.Channel, "",
                        new EmbedBuilder
                        {
                            Author = new EmbedAuthorBuilder { Name = "Error with the command" },
                            Description = Convert.ToString(result.ErrorReason),
                            Color = new Color(255, 0, 0)
                        }.Build()).ConfigureAwait(false);
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