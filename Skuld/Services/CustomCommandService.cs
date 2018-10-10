using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Skuld.Models;
using Skuld.Models.Database;
using Skuld.Utilities.Messaging;
using StatsdClient;
using System;
using System.Threading.Tasks;

namespace Skuld.Services
{
    public class CustomCommandService
    {
        private DiscordShardedClient client;
        private MessageServiceConfig messageConfig;
        private DatabaseService databaseService;

        public CustomCommandService(DiscordShardedClient cli, MessageServiceConfig messconf, DatabaseService db)
        {
            client = cli;
            messageConfig = messconf;
            databaseService = db;
        }

        public async Task MessageReceivedAsync(SocketMessage arg)
        {
            if (arg.Author.IsBot) { return; }

            var message = arg as SocketUserMessage;
            if (message is null) { return; }

            var context = new ShardedCommandContext(client, message);

            if (!MessageTools.IsEnabledChannel(context.Guild.GetUser(context.User.Id), (ITextChannel)context.Channel)) { return; }

            try
            {
                SkuldGuild sguild = await MessageTools.GetGuildAsync(context.Guild, databaseService).ConfigureAwait(false);
                var usr = await MessageTools.GetUserAsync(context.User, databaseService).ConfigureAwait(false);

                if (usr != null && usr.Banned) return;

                if (sguild != null) { if (!MessageTools.HasPrefix(message, messageConfig, sguild.Prefix)) { return; } }
                else { if (!MessageTools.HasPrefix(message, messageConfig)) { return; } }

                if (sguild != null && sguild.GuildSettings.Modules.CustomEnabled)
                {
                    var customcommand = await MessageTools.GetCustomCommandAsync(context.Guild,
                        MessageTools.GetCommandName(MessageTools.GetPrefixFromCommand(sguild, arg.Content, HostService.Configuration),
                        0, arg), databaseService);

                    if (customcommand != null)
                    {
                        await DispatchCommandAsync(context, customcommand);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                await HostService.Logger.AddToLogsAsync(new Core.Models.LogMessage("CmdDisp", ex.Message, LogSeverity.Error, ex));
            }
        }

        public async Task DispatchCommandAsync(ICommandContext context, CustomCommand command)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await MessageTools.SendChannelAsync(context.Channel, command.Content).ConfigureAwait(false);
            stopwatch.Stop();

            DogStatsd.Histogram("commands.latency", stopwatch.ElapsedMilliseconds(), 0.5, new[] { $"module:custom", $"cmd:{command.CommandName.ToLowerInvariant()}" });

            await InsertCommand(command, context.User).ConfigureAwait(false);

            DogStatsd.Increment("commands.processed", 1, 1, new[] { $"module:custom", $"cmd:{command.CommandName.ToLowerInvariant()}" });
        }

        private async Task InsertCommand(CustomCommand command, IUser user)
        {
            var suser = await databaseService.GetUserAsync(user.Id);
            if (suser != null) { await databaseService.UpdateUserUsageAsync(suser, command.CommandName); }
            else
            {
                await databaseService.InsertUserAsync(user).ConfigureAwait(false);
                await InsertCommand(command, user).ConfigureAwait(false);
            }
        }
    }
}