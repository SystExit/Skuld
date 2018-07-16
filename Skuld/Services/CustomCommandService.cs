using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Skuld.Models.Database;
using Skuld.Utilities.Messaging;
using StatsdClient;
using System;
using System.Threading.Tasks;
using Skuld.Core.Services;

namespace Skuld.Services
{
    public class CustomCommandService
    {
        private readonly DiscordShardedClient client;
        private DatabaseService database;
        private GenericLogger logger;
        private MessageService messageService;

        public CustomCommandService(DiscordShardedClient cli, DatabaseService db, GenericLogger log, MessageService mess) //inherits from depinjection
        {
            client = cli;
            database = db;
            logger = log;
            messageService = mess;
            client.MessageReceived += Client_MessageReceived;
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg.Author.IsBot) { return; }

            var message = arg as SocketUserMessage;
            if (message is null) { return; }

            var context = new ShardedCommandContext(client, message);

            if (!MessageTools.IsEnabledChannel(context.Guild.GetUser(context.User.Id), (ITextChannel)context.Channel)) { return; }

            try
            {
                SkuldGuild sguild = await MessageTools.GetGuildAsync(context.Guild, database).ConfigureAwait(false);
                var usr = await MessageTools.GetUserAsync(context.User, database).ConfigureAwait(false);

                if (usr != null && usr.Banned) return;

                if (sguild != null) { if (!MessageTools.HasPrefix(message, messageService.config, sguild.Prefix)) { return; } }
                else { if (!MessageTools.HasPrefix(message, messageService.config)) { return; } }

                if (sguild != null && sguild.GuildSettings.Modules.CustomEnabled)
                {
                    var customcommand = await MessageTools.GetCustomCommandAsync(context.Guild,
                        MessageTools.GetCommandName(MessageTools.GetPrefixFromCommand(sguild, arg.Content, HostService.Configuration),
                        0, arg), database);

                    if (customcommand != null)
                    {
                        await DispatchCommandAsync(context, customcommand);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Core.Models.LogMessage("CmdDisp", ex.Message, LogSeverity.Error, ex));
            }
        }

        public async Task DispatchCommandAsync(ICommandContext context, CustomCommand command)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await MessageTools.SendChannelAsync(context.Channel, command.Content, logger).ConfigureAwait(false);
            stopwatch.Stop();

            DogStatsd.Histogram("commands.latency", stopwatch.ElapsedMilliseconds(), 0.5, new[] { $"module:custom", $"cmd:{command.CommandName.ToLowerInvariant()}" });

            await InsertCommand(command, context.User).ConfigureAwait(false);

            DogStatsd.Increment("commands.processed", 1, 1, new[] { $"module:custom", $"cmd:{command.CommandName.ToLowerInvariant()}" });
        }

        private async Task InsertCommand(CustomCommand command, IUser user)
        {
            var suser = await database.GetUserAsync(user.Id);
            if (suser != null) { await database.UpdateUserUsageAsync(suser, command.CommandName); }
            else
            {
                await database.InsertUserAsync(user).ConfigureAwait(false);
                await InsertCommand(command, user).ConfigureAwait(false);
            }
        }
    }
}
