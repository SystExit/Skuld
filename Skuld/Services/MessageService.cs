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

        public async Task<bool> CheckEmbedPermission(IGuildChannel channel)
            => (await channel.Guild.GetCurrentUserAsync()).GetPermissions(channel).EmbedLinks;

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
                SkuldGuild sguild = null;

                if (database.CanConnect)
                {
                    var usr = await database.GetUserAsync(context.User.Id);
                    if (usr == null) { await database.InsertUserAsync(context.User).ConfigureAwait(false); usr = await database.GetUserAsync(context.User.Id); }
                    if (usr.Banned) return;

                    if (context.Guild != null)
                    {
                        sguild = await database.GetGuildAsync(context.Guild.Id).ConfigureAwait(false);
                        if (sguild == null)
                        {
                            await database.InsertGuildAsync(context.Guild).ContinueWith(async x =>
                            {
                                sguild = await database.GetGuildAsync(context.Guild.Id).ConfigureAwait(false);
                            });
                        }
                        else
                        {
                            if (sguild.GuildSettings.Features.Experience)
                            {
                                var luserexperience = await database.GetUserExperienceAsync(context.User);

                                if (luserexperience is UserExperience)
                                {
                                    var luxp = (UserExperience)luserexperience;

                                    var gld = luxp.GuildExperiences.First(x => x.GuildID == context.Guild.Id);
                                    if (gld != null)
                                    {
                                        if (gld.LastGranted < (60 - DateTime.UtcNow.ToEpoch()))
                                        {
                                            var amount = (ulong)HostService.Services.GetRequiredService<Random>().Next(0, 25);

                                            var xptonextlevel = GetXPRequirement(gld.Level + 1, 1.618); //get next level xp requirement based on phi

                                            if ((gld.XP + amount) >= xptonextlevel) //if over or equal to next level requirement, update accordingly
                                            {
                                                gld.XP = 0;
                                                gld.TotalXP += amount;
                                                gld.Level++;
                                                gld.LastGranted = DateTime.UtcNow.ToEpoch();
                                                await context.Channel.SendMessageAsync($"Congratulations {context.User.Mention}!! You're now level **{gld.Level}**");
                                                await logger.logger.AddToLogsAsync(new Core.Models.LogMessage("MessageService", "User levelled up", LogSeverity.Info));
                                                DogStatsd.Increment("user.levelup");
                                            }
                                            else //otherwise append current status
                                            {
                                                gld.XP += amount;
                                                gld.TotalXP += amount;
                                                gld.LastGranted = DateTime.UtcNow.ToEpoch();
                                            }
                                            await database.UpdateGuildExperienceAsync(context.User, gld, context.Guild);
                                        }
                                    }
                                    else
                                    {
                                        gld = new GuildExperience();
                                        gld.LastGranted = DateTime.UtcNow.ToEpoch();
                                        gld.XP = gld.TotalXP = (ulong)HostService.Services.GetRequiredService<Random>().Next(0, 25);
                                        await database.UpdateGuildExperienceAsync(context.User, gld, context.Guild);
                                    }
                                }
                                else
                                {
                                    var gld = new GuildExperience();
                                    gld.LastGranted = DateTime.UtcNow.ToEpoch();
                                    gld.XP = gld.TotalXP = (ulong)HostService.Services.GetRequiredService<Random>().Next(0, 25);
                                    await database.InsertGuildExperienceAsync(context.User, context.Guild, gld);
                                }
                            }
                        }
                    }
                }

                if (sguild != null) { if (!HasPrefix(message, sguild.Prefix)) { return; } }
                else { if (!HasPrefix(message)) { return; } }

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

                var cmds = commandService.Search(context, GetCmdName(message, sguild)).Commands;
                if (cmds == null || cmds.Count == 0) { return; }

                var cmd = cmds.FirstOrDefault().Command;
                if (database.CanConnect)
                {
                    var mods = sguild.GuildSettings.Modules;
                    if (ModuleDisabled(mods, cmd)) { return; }
                }

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

        private ulong GetXPRequirement(ulong level, double growthmod)
            => (ulong)((level * 50) * (level * growthmod));

        private string GetCmdName(IUserMessage arg, SkuldGuild sguild = null)
        {
            string content = "";
            var contentsplit = arg.Content.Split(' ')[0];
            if (config.AltPrefix != null)
            {
                if (database.CanConnect)
                {
                    if (contentsplit.StartsWith(sguild.Prefix))
                        content = contentsplit.Replace(sguild.Prefix, "");
                    if (contentsplit.StartsWith(config.Prefix))
                        content = contentsplit.Replace(config.Prefix, "");
                    if (contentsplit.StartsWith(config.AltPrefix))
                        content = contentsplit.Replace(config.AltPrefix, "");
                }
                else
                {
                    if (contentsplit.StartsWith(config.Prefix))
                        content = contentsplit.Replace(config.Prefix, "");
                    if (contentsplit.StartsWith(config.AltPrefix))
                        content = contentsplit.Replace(config.AltPrefix, "");
                }
            }
            else
            {
                if (database.CanConnect)
                {
                    if (contentsplit.StartsWith(sguild.Prefix))
                        content = contentsplit.Replace(sguild.Prefix, "");
                    if (contentsplit.StartsWith(config.Prefix))
                        content = contentsplit.Replace(config.Prefix, "");
                }
                else
                {
                    if (contentsplit.StartsWith(config.Prefix))
                        content = contentsplit.Replace(config.Prefix, "");
                }
            }
            return content;
        }

        private bool HasPrefix(IUserMessage message, string gprefix = null)
        {
            bool retn;
            if (gprefix != null)
            {
                retn = message.HasStringPrefix(gprefix, ref config.ArgPos) ||
                       message.HasStringPrefix(config.Prefix, ref config.ArgPos) ||
                       message.HasStringPrefix(config.AltPrefix, ref config.ArgPos);
            }
            else
            {
                retn = message.HasStringPrefix(config.Prefix, ref config.ArgPos) ||
                       message.HasStringPrefix(config.AltPrefix, ref config.ArgPos);
            }
            return retn;
        }

        private static bool ModuleDisabled(GuildCommandModules cmdmods, CommandInfo command)
        {
            if (!cmdmods.AccountsEnabled && command.Module.Name.ToLowerInvariant() == "accounts")
            { return true; }
            if (!cmdmods.ActionsEnabled && command.Module.Name.ToLowerInvariant() == "actions")
            { return true; }
            if (!cmdmods.AdminEnabled && command.Module.Name.ToLowerInvariant() == "admin")
            { return true; }
            if (!cmdmods.FunEnabled && command.Module.Name.ToLowerInvariant() == "fun")
            { return true; }
            if (!cmdmods.InformationEnabled && command.Module.Name.ToLowerInvariant() == "information")
            { return true; }
            if (!cmdmods.LewdEnabled && command.Module.Name.ToLowerInvariant() == "lewd")
            { return true; }
            if (!cmdmods.SearchEnabled && command.Module.Name.ToLowerInvariant() == "search")
            { return true; }
            if (!cmdmods.StatsEnabled && command.Module.Name.ToLowerInvariant() == "stats")
            { return true; }
            if (!cmdmods.WeebEnabled && command.Module.Name.ToLowerInvariant() == "weeb")
            { return true; }

            return false;
        }

        public async Task DispatchCommandAsync(ICommandContext context, CustomCommand command)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await SendChannelAsync(context.Channel, command.Content).ConfigureAwait(false);
            stopwatch.Stop();

            DogStatsd.Histogram("commands.latency", stopwatch.ElapsedMilliseconds(), 0.5, new[] { $"module:custom", $"cmd:{command.CommandName.ToLowerInvariant()}" });

            await InsertCommand(command, context.User).ConfigureAwait(false);

            DogStatsd.Increment("commands.processed", 1, 1, new[] { $"module:custom", $"cmd:{command.CommandName.ToLowerInvariant()}" });
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
                if (database.CanConnect)
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
                    await SendChannelAsync(context.Channel, "You seem to be missing a parameter or 2, here's the help", cmdembed).ConfigureAwait(false);
                    displayerror = false;
                }

                if (result.Error != CommandError.UnknownCommand && !result.ErrorReason.Contains("Timeout") && displayerror)
                {
                    await logger.logger.AddToLogsAsync(new Core.Models.LogMessage("CmdHand", "Error with command, Error is: " + result, LogSeverity.Error));
                    await SendChannelAsync(context.Channel, "", new EmbedBuilder { Author = new EmbedAuthorBuilder { Name = "Error with the command" }, Description = Convert.ToString(result.ErrorReason), Color = new Color(255, 0, 0) }.Build()).ConfigureAwait(false);
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

        //MessageSending
        public async Task<IUserMessage> SendChannelAsync(IChannel channel, string message)
        {
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                if (channel == null || textChan == null || mesgChan == null) { return null; }
                await mesgChan.TriggerTypingAsync();
                await logger.logger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                return await mesgChan.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                await logger.logger.AddToLogsAsync(new Core.Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }

        public async Task<IUserMessage> SendChannelAsync(IChannel channel, string message, Embed embed)
        {
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                if (channel == null || textChan == null || mesgChan == null) { return null; }
                await mesgChan.TriggerTypingAsync();
                IUserMessage msg;
                var perm = await CheckEmbedPermission(textChan);
                if (!perm)
                {
                    if (message != null)
                    {
                        msg = await mesgChan.SendMessageAsync(message + "\n" + embed.ToMessage());
                    }
                    else
                    {
                        msg = await mesgChan.SendMessageAsync(embed.ToMessage());
                    }
                }
                else
                {
                    msg = await mesgChan.SendMessageAsync(message, false, embed);
                }
                await logger.logger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                return msg;
            }
            catch (Exception ex)
            {
                await logger.logger.AddToLogsAsync(new Core.Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }

        public async Task<IUserMessage> SendChannelAsync(IChannel channel, string message, string filename)
        {
            var textChan = (ITextChannel)channel;
            var mesgChan = (IMessageChannel)channel;
            if (channel == null || textChan == null || mesgChan == null) { return null; }
            await mesgChan.TriggerTypingAsync();
            IUserMessage msg;
            try
            {
                msg = await mesgChan.SendFileAsync(filename, message);
                await logger.logger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                return msg;
            }
            catch (Exception ex)
            {
                await logger.logger.AddToLogsAsync(new Core.Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }

        public async Task SendChannelAsync(IChannel channel, string message, double timeout)
        {
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                if (channel == null || textChan == null || mesgChan == null) { return; }
                await mesgChan.TriggerTypingAsync();
                IUserMessage msg = await mesgChan.SendMessageAsync(message);
                await logger.logger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                await msg.DeleteAfterSecondsAsync((int)timeout);
            }
            catch (Exception ex)
            {
                await logger.logger.AddToLogsAsync(new Core.Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
            }
        }

        public async Task SendChannelAsync(IChannel channel, string message, double timeout, Embed embed)
        {
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                if (channel == null || textChan == null || mesgChan == null) { return; }
                await mesgChan.TriggerTypingAsync();
                var perm = await CheckEmbedPermission(textChan);
                IUserMessage msg = null;
                if (!perm)
                {
                    if (message != null)
                    {
                        msg = await mesgChan.SendMessageAsync(message + "\n" + embed.ToMessage());
                    }
                    else
                    {
                        msg = await mesgChan.SendMessageAsync(embed.ToMessage());
                    }
                }
                else
                {
                    msg = await mesgChan.SendMessageAsync(message, isTTS: false, embed: embed);
                }
                await logger.logger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                await msg.DeleteAfterSecondsAsync((int)timeout);
            }
            catch (Exception ex)
            {
                await logger.logger.AddToLogsAsync(new Core.Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
            }
        }

        public async Task SendChannelAsync(IChannel channel, string message, double timeout, string filename)
        {
            var textChan = (ITextChannel)channel;
            var mesgChan = (IMessageChannel)channel;
            if (channel == null || textChan == null || mesgChan == null) { return; }
            await mesgChan.TriggerTypingAsync();
            try
            {
                IUserMessage msg = await mesgChan.SendFileAsync(filename, message);
                await logger.logger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                await msg.DeleteAfterSecondsAsync((int)timeout);
            }
            catch (Exception ex)
            {
                await logger.logger.AddToLogsAsync(new Core.Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
            }
        }

        public async Task<IUserMessage> SendDMsAsync(IMessageChannel channel, IDMChannel user, string message)
        {
            try
            {
                IUserMessage msg = null;
                msg = await user.SendMessageAsync(message);
                await SendChannelAsync(channel, $":ok_hand: <@{user.Recipient.Id}> Check your DMs.");
                return msg;
            }
            catch (Exception ex)
            {
                await logger.logger.AddToLogsAsync(new Core.Models.LogMessage("MsgH-DM", "Error dispatching Direct Message to user, sending to channel instead. Printed exception to logs.", LogSeverity.Warning, ex));
                await SendChannelAsync(channel, "I couldn't send it to your DMs, so I sent it here instead... I hope you're not mad. <:blobcry:350681079415439361> " + message);
                return null;
            }
        }

        public async Task<IUserMessage> SendDMsAsync(IMessageChannel channel, IDMChannel user, string message, Embed embed)
        {
            try
            {
                IUserMessage msg = null;
                msg = await user.SendMessageAsync(message, isTTS: false, embed: embed);
                await SendChannelAsync(channel, $":ok_hand: <@{user.Recipient.Id}> Check your DMs.");
                return msg;
            }
            catch (Exception ex)
            {
                await logger.logger.AddToLogsAsync(new Core.Models.LogMessage("MsgH-DM", "Error dispatching Direct Message to user, sending to channel instead. Printed exception to logs.", LogSeverity.Warning, ex));
                await SendChannelAsync(channel, "I couldn't send it to your DMs, so I sent it here instead... I hope you're not mad. <:blobcry:350681079415439361> " + message, embed);
                return null;
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