using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Skuld.Core.Extensions;
using Skuld.Core.Generic.Models;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Discord.Extensions;
using Skuld.Discord.Services;
using Skuld.Discord.TypeReaders;
using Skuld.Discord.Utilities;
using StatsdClient;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Skuld.Discord.Handlers
{
    public static class MessageHandler
    {
        private static SkuldConfig SkuldConfig;
        public static CommandService CommandService;
        public static MessageServiceConfig cmdConfig;
        private static readonly Random rnd = new Random();
        private static Stopwatch watch;

        public static async Task<EventResult> ConfigureCommandServiceAsync(CommandServiceConfig Config,
                                                                           MessageServiceConfig cmdConf,
                                                                           SkuldConfig conf,
                                                                           Assembly ModuleAssembly,
                                                                           IServiceProvider ServiceProvider = null)
        {
            watch = new Stopwatch();
            SkuldConfig = conf;
            cmdConfig = cmdConf;
            try
            {
                CommandService = new CommandService(Config);

                CommandService.CommandExecuted += CommandService_CommandExecuted;
                CommandService.Log += CommandService_Log;

                CommandService.AddTypeReader<Uri>(new UriTypeReader());
                CommandService.AddTypeReader<GuildRoleConfig>(new RoleConfigTypeReader());
                CommandService.AddTypeReader<IPAddress>(new IPAddressTypeReader());
                CommandService.AddTypeReader<Emoji>(new EmojiTypeReader());
                await CommandService.AddModulesAsync(ModuleAssembly, ServiceProvider).ConfigureAwait(false);

                return EventResult.FromSuccess();
            }
            catch (Exception ex)
            {
                return EventResult.FromFailureException(ex.Message, ex);
            }
        }

        #region CommandService Logs
        private static Task CommandService_Log(LogMessage arg)
        {
            var key = "CommandService - " + arg.Source;
            switch (arg.Severity)
            {
                case LogSeverity.Error:
                    Log.Error(key, arg.Message, arg.Exception);
                    break;

                case LogSeverity.Debug:
                    Log.Debug(key, arg.Message, arg.Exception);
                    break;

                case LogSeverity.Critical:
                    Log.Critical(key, arg.Message, arg.Exception);
                    break;

                case LogSeverity.Info:
                    Log.Info(key, arg.Message);
                    break;

                case LogSeverity.Verbose:
                    Log.Verbose(key, arg.Message, arg.Exception);
                    break;

                case LogSeverity.Warning:
                    Log.Warning(key, arg.Message, arg.Exception);
                    break;
            }
            return Task.CompletedTask;
        }

        private static async Task CommandService_CommandExecuted(Optional<CommandInfo> arg1, ICommandContext arg2, IResult arg3)
        {
            CommandInfo cmd = null;

            if (arg1.IsSpecified)
                cmd = arg1.Value;

            if (arg3.IsSuccess)
            {
                using var Database = new SkuldDbContextFactory().CreateDbContext();

                if (arg1.IsSpecified)
                {
                    var cont = arg2 as ShardedCommandContext;

                    DogStatsd.Increment("commands.total.threads", 1, 1, new[] { $"module:{cmd.Module.Name.ToLowerInvariant()}", $"cmd:{cmd.Name.ToLowerInvariant()}" });

                    DogStatsd.Histogram("commands.latency", watch.ElapsedMilliseconds(), 0.5, new[] { $"module:{cmd.Module.Name.ToLowerInvariant()}", $"cmd:{cmd.Name.ToLowerInvariant()}" });

                    await InsertCommandAsync(cmd, (await Database.GetUserAsync(cont.User))).ConfigureAwait(false);

                    DogStatsd.Increment("commands.processed", 1, 1, new[] { $"module:{cmd.Module.Name.ToLowerInvariant()}", $"cmd:{cmd.Name.ToLowerInvariant()}" });
                }
            }
            else
            {
                bool displayerror = true;
                if (arg3.ErrorReason.Contains("few parameters"))
                {
                    var cmdembed = DiscordUtilities.GetCommandHelp(CommandService, arg2, cmd.Name);
                    await BotService.DiscordClient.SendChannelAsync(arg2.Channel, "You seem to be missing a parameter or 2, here's the help", cmdembed).ConfigureAwait(false);
                    displayerror = false;
                }

                if (arg3.Error != CommandError.UnknownCommand && displayerror)
                {
                    Log.Error("CmdHand", "Error with command, Error is: " + arg3);
                    await new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder { Name = "Error with the command" },
                        Description = Convert.ToString(arg3.ErrorReason),
                        Color = new Color(255, 0, 0)
                    }.Build()
                    .QueueMessageAsync(arg2 as ShardedCommandContext)
                    .ConfigureAwait(false);
                }
                DogStatsd.Increment("commands.errors");

                switch (arg3.Error)
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
            watch = new Stopwatch();
        }
        #endregion

        #region HandleProcessing
        public static async Task HandleMessageAsync(SocketMessage arg)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            try
            {
                DogStatsd.Increment("messages.recieved");
                if (arg.Author.IsBot || arg.Author.IsWebhook || arg.Author.Discriminator.Equals("0000")) { return; }

                var message = arg as SocketUserMessage;
                if (message is null) { return; }

                if (message.Channel is ITextChannel)
                {
                    if (!await CheckPermissionToSendMessageAsync(message.Channel as ITextChannel))
                    {
                        return;
                    }
                }

                if (message.Channel is ITextChannel)
                {
                    var gldtemp = (message.Channel as ITextChannel).Guild;
                    if (gldtemp != null)
                    {
                        var guser = await gldtemp.GetUserAsync(BotService.DiscordClient.CurrentUser.Id);

                        if (!guser.GetPermissions(message.Channel as IGuildChannel).SendMessages) return;
                    }

                    if (!MessageTools.IsEnabledChannel(await (message.Channel as ITextChannel).Guild.GetUserAsync(message.Author.Id), (ITextChannel)message.Channel)) { return; }
                }

                User suser = null;
                Guild sguild = null;

                if (!Database.Users.Any(x => x.Id == arg.Author.Id))
                {
                    suser = await Database.InsertUserAsync(arg.Author);
                }
                else
                {
                    suser = await Database.GetUserAsync(arg.Author);
                    if (suser != null && suser.Banned) return;
                    if (!suser.IsUpToDate(message.Author))
                    {
                        suser.AvatarUrl = new Uri(message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl());
                        suser.Username = message.Author.Username;
                        await Database.SaveChangesAsync().ConfigureAwait(false);
                    }
                }

                if (message.Channel is ITextChannel)
                {
                    var gld = (message.Channel as ITextChannel).Guild;

                    sguild = await Database.GetGuildAsync(gld).ConfigureAwait(false);
                }

                if (sguild != null)
                {
                    var features = Database.Features.FirstOrDefault(x => x.Id == sguild.Id);
                    if (features.Experience)
                    {
                        var _ = HandleExperienceAsync(message, message.Author, ((message.Channel as ITextChannel).Guild), sguild, message.Channel);
                    }
                }

                if (!MessageTools.HasPrefix(message, BotService.DiscordClient, SkuldConfig, cmdConfig, sguild?.Prefix)) return;

                ShardedCommandContext context = new ShardedCommandContext(BotService.DiscordClient, message);

                if (sguild != null)
                {
                    var modules = Database.Modules.FirstOrDefault(x => x.Id == sguild.Id);
                    if (modules.Custom)
                    {
                        var _ = HandleCustomCommandAsync(context);
                    }
                }

                await DispatchCommandAsync(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Critical("CommandProcessor", ex.Message, ex);
            }
        }

        public static async Task HandleExperienceAsync(IUserMessage message, IUser user, IGuild guild, Guild sguild, IMessageChannel backupChannel)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var luxp = Database.UserXp.FirstOrDefault(x => x.UserId == user.Id && x.GuildId == guild.Id);

            var User = Database.GetUserAsync(user);

            ulong amount = (ulong)rnd.Next(1, 26);

            if (luxp != null)
            {
                if (luxp.LastGranted < (DateTime.UtcNow.ToEpoch() - 60))
                {
                    var xptonextlevel = DiscordTools.GetXPLevelRequirement(luxp.Level + 1, DiscordTools.PHI); //get next level xp requirement based on phi

                    if ((luxp.XP + amount) >= xptonextlevel) //if over or equal to next level requirement, update accordingly
                    {
                        luxp.XP = 0;
                        luxp.TotalXP += amount;
                        luxp.Level++;
                        luxp.LastGranted = DateTime.UtcNow.ToEpoch();

                        string msg = sguild.LevelUpMessage;

                        if(msg != null)
                            msg = msg
                                .Replace("-m", user.Mention)
                                .Replace("-u", user.Username)
                                .Replace("-l", luxp.Level.ToString("N0"));
                        else
                            msg = $"Congratulations {user.Mention}!! You're now level **{luxp.Level}**";

                        string appendix = null;

                        if ((sguild.LevelUpChannel != 0 && sguild.LevelUpChannel != backupChannel.Id) || sguild.LevelNotification == LevelNotification.DM)
                            appendix = $"\n\nMessage that caused your level up: {message.GetJumpUrl()}";

                        if (sguild.LevelNotification == LevelNotification.Channel)
                        {
                            ulong ChannelID = 0;
                            if (sguild.LevelUpChannel != 0)
                                ChannelID = sguild.LevelUpChannel;
                            else
                                ChannelID = backupChannel.Id;

                            if (string.IsNullOrEmpty(sguild.LevelUpMessage))
                            {
                                await (await guild.GetTextChannelAsync(ChannelID).ConfigureAwait(false))
                                    .SendMessageAsync($"{msg}{appendix}").ConfigureAwait(false);
                            }
                            else
                            {
                                await (await guild.GetTextChannelAsync(ChannelID).ConfigureAwait(false)).SendMessageAsync(msg).ConfigureAwait(false);
                            }
                        }
                        else if (sguild.LevelNotification == LevelNotification.DM)
                        {
                            if (string.IsNullOrEmpty(sguild.LevelUpMessage))
                                await user.SendMessageAsync($"{msg} in {guild.Name}{appendix}").ConfigureAwait(false);
                            else
                            {
                                await user.SendMessageAsync(msg).ConfigureAwait(false);
                            }
                        }

                        Log.Info("MessageService", "User leveled up");
                        DogStatsd.Increment("user.levels.levelup");
                    }
                    else //otherwise append current status
                    {
                        luxp.XP += amount;
                        luxp.TotalXP += amount;
                        luxp.LastGranted = DateTime.UtcNow.ToEpoch();
                    }

                    DogStatsd.Increment("user.levels.processed");
                }
            }
            else
            {
                Database.UserXp.Add(new UserExperience
                {
                    LastGranted = DateTime.UtcNow.ToEpoch(),
                    XP = amount,
                    UserId = user.Id,
                    GuildId = guild.Id,
                    TotalXP = amount
                });
                DogStatsd.Increment("user.levels.processed");
            }

            await Database.SaveChangesAsync().ConfigureAwait(false);

            return;
        }

        public static async Task HandleCustomCommandAsync(ShardedCommandContext context)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var prefix = MessageTools.GetPrefixFromCommand(await Database.GetGuildAsync(context.Guild), context.Message.Content, SkuldConfig);
            var name = MessageTools.GetCommandName(prefix, context.Message);
            var customcommand = Database.CustomCommands.FirstOrDefault(x => x.GuildId == context.Guild.Id && x.Name.ToLower() == name.ToLower());

            if (customcommand != null)
            {
                await DispatchCommandAsync(context, customcommand).ConfigureAwait(false);
                return;
            }
            return;
        }
        #endregion

        #region Dispatching
        public static async Task DispatchCommandAsync(ShardedCommandContext context)
        {
            watch.Start();
            await CommandService.ExecuteAsync(context, cmdConfig.ArgPos, BotService.Services).ConfigureAwait(false);
            watch.Stop();
        }

        public static async Task DispatchCommandAsync(ShardedCommandContext context, CustomCommand command)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            watch.Start();
            await BotService.DiscordClient.SendChannelAsync(context.Channel, command.Content).ConfigureAwait(false);
            watch.Stop();

            DogStatsd.Histogram("commands.latency", watch.ElapsedMilliseconds(), 0.5, new[] { $"module:custom", $"cmd:{command.Name.ToLowerInvariant()}" });

            await InsertCommandAsync(command, (await Database.GetUserAsync(context.User))).ConfigureAwait(false);

            DogStatsd.Increment("commands.processed", 1, 1, new[] { $"module:custom", $"cmd:{command.Name.ToLowerInvariant()}" });

            watch = new Stopwatch();
        }
        #endregion

        #region HandleInsertion
        private static async Task InsertCommandAsync(CommandInfo command, User user)
            => await InsertUserCommandUsageAsync(command.Name ?? command.Module.Name, user).ConfigureAwait(false);

        private static async Task InsertCommandAsync(CustomCommand command, User user)
            => await InsertUserCommandUsageAsync(command.Name, user).ConfigureAwait(false);

        private static async Task InsertUserCommandUsageAsync(string name, User user)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var experience = Database.UserCommandUsage.FirstOrDefault(x => x.UserId == user.Id && x.Command.ToLower() == name.ToLower());
            if (experience != null)
            {
                experience.Usage += 1;
            }
            else
            {
                Database.UserCommandUsage.Add(new UserCommandUsage
                {
                    Command = name,
                    UserId = user.Id,
                    Usage = 1
                });
            }

            await Database.SaveChangesAsync().ConfigureAwait(false);
        }
        #endregion

        public static async Task<bool> CheckPermissionToSendMessageAsync(ITextChannel channel)
        {
            if (channel.Guild != null)
            {
                var guild = channel.Guild;
                var currentuser = await channel.Guild.GetCurrentUserAsync();
                var chan = await channel.Guild.GetChannelAsync(channel.Id);
                var po = chan.GetPermissionOverwrite(currentuser);

                if (po.HasValue)
                {
                    if (po.Value.SendMessages != PermValue.Deny)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}