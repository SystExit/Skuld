using Discord;
using Discord.Commands;
using Discord.WebSocket;
using StatsdClient;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Core.Models.Skuld;
using Skuld.Database;
using Skuld.Database.Extensions;
using Skuld.Discord.Commands;
using Skuld.Discord.Extensions;
using Skuld.Discord.Services;
using Skuld.Discord.TypeReaders;
using Skuld.Discord.Utilities;
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

        public static async Task<EventResult> ConfigureCommandServiceAsync(CommandServiceConfig Config, MessageServiceConfig cmdConf, SkuldConfig conf, Assembly ModuleAssembly, IServiceProvider ServiceProvider = null)
        {
            watch = new Stopwatch();
            SkuldConfig = conf;
            cmdConfig = cmdConf;
            try
            {
                CommandService = new CommandService(Config);

                CommandService.CommandExecuted += CommandService_CommandExecuted;

                CommandService.Log += async (arg) =>
                   await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("CommandService - " + arg.Source, arg.Message, arg.Severity, arg.Exception));

                CommandService.AddTypeReader<Uri>(new UriTypeReader());
                CommandService.AddTypeReader<GuildRoleConfig>(new RoleConfigTypeReader());
                CommandService.AddTypeReader<IPAddress>(new IPAddressTypeReader());
                await CommandService.AddModulesAsync(ModuleAssembly, ServiceProvider);

                return EventResult.FromSuccess();
            }
            catch (Exception ex)
            {
                return EventResult.FromFailureException(ex.Message, ex);
            }
        }
        private static async Task CommandService_CommandExecuted(Optional<CommandInfo> arg1, ICommandContext arg2, IResult arg3)
        {
            if(arg1.IsSpecified)
            {
                var cmd = arg1.Value;

                var cont = arg2 as SkuldCommandContext;

                DogStatsd.Increment("commands.total.threads", 1, 1, new[] { $"module:{cmd.Module.Name.ToLowerInvariant()}", $"cmd:{cmd.Name.ToLowerInvariant()}" });

                if (arg3.IsSuccess)
                {
                    DogStatsd.Histogram("commands.latency", watch.ElapsedMilliseconds(), 0.5, new[] { $"module:{cmd.Module.Name.ToLowerInvariant()}", $"cmd:{cmd.Name.ToLowerInvariant()}" });

                    await InsertCommandAsync(cmd, cont.DBUser).ConfigureAwait(false);

                    DogStatsd.Increment("commands.processed", 1, 1, new[] { $"module:{cmd.Module.Name.ToLowerInvariant()}", $"cmd:{cmd.Name.ToLowerInvariant()}" });
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
                        await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("CmdHand", "Error with command, Error is: " + arg3, LogSeverity.Error));
                        await new EmbedBuilder
                        {
                            Author = new EmbedAuthorBuilder { Name = "Error with the command" },
                            Description = Convert.ToString(arg3.ErrorReason),
                            Color = new Color(255, 0, 0)
                        }.Build().QueueMessage(Models.MessageType.Standard, arg2.User, arg2.Channel);
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
            }
            watch = new Stopwatch();
        }
        public static void HandleMessageAsync(SocketMessage arg)
        {
            var _ = HandleCommandAsync(arg).ConfigureAwait(false);
        }

        public static async Task<bool> CheckPermissionToSendMessageAsync(ITextChannel channel)
        {
            if(channel.Guild != null)
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

        public static async Task HandleCommandAsync(SocketMessage arg)
        {
            DogStatsd.Increment("messages.recieved");
            if (arg.Author.IsBot || arg.Author.IsWebhook || arg.Author.Discriminator.Equals("0000")) { return; }

            var message = arg as SocketUserMessage;
            if (message is null) { return; }

            if (message.Channel is ITextChannel)
            {
                if(!await CheckPermissionToSendMessageAsync(message.Channel as ITextChannel))
                {
                    return;
                }
            }

            var gldtemp = (message.Channel as ITextChannel).Guild;
            if(gldtemp != null)
            {
                var guser = await gldtemp.GetUserAsync(BotService.DiscordClient.CurrentUser.Id);

                if (!guser.GetPermissions(message.Channel as IGuildChannel).SendMessages) return;
            }

            if (!MessageTools.IsEnabledChannel(await (message.Channel as ITextChannel).Guild.GetUserAsync(message.Author.Id), (ITextChannel)message.Channel)) { return; }

            SkuldUser suser = null;
            SkuldGuild sguild = null;

            if (await DatabaseClient.CheckConnectionAsync())
            {
                suser = await MessageTools.GetUserOrInsertAsync(message.Author).ConfigureAwait(false);
                sguild = await MessageTools.GetGuildOrInsertAsync((message.Channel as ITextChannel).Guild).ConfigureAwait(false);

                if(!suser.IsUpToDate(message.Author))
                {
                    suser.FillDataFromDiscord(message.Author);
                    await suser.UpdateAsync();
                }

                if (suser != null && suser.Banned) return;
            }

            if (sguild != null)
            {
                if (sguild.Features.Experience)
                {
                    var _ = HandleExperienceAsync(message.Author, ((message.Channel as ITextChannel).Guild), sguild, message.Channel);
                }
            }

            if (!MessageTools.HasPrefix(message, BotService.DiscordClient, SkuldConfig, cmdConfig, sguild?.Prefix)) return;

            SkuldCommandContext context;

            if(suser != null && sguild != null)
            {
                context = new SkuldCommandContext(BotService.DiscordClient, message, suser, sguild);
            }
            else
            {
                context = new SkuldCommandContext(BotService.DiscordClient, message);
            }

            if (sguild != null)
            {
                if (sguild.Modules.CustomEnabled)
                {
                    var _ = HandleCustomCommandAsync(context);
                    return;
                }
            }

            await DispatchCommandAsync(context).ConfigureAwait(false);
        }
        public static async Task HandleExperienceAsync(IUser user, IGuild guild, SkuldGuild sguild, IMessageChannel backupChannel)
        {
            var luserexperienceResp = await DatabaseClient.GetUserExperienceAsync(user.Id);
            var skuldUser = await MessageTools.GetUserOrInsertAsync(user);

            ulong amount = (ulong)rnd.Next(1, 26);

            if (luserexperienceResp.Data is UserExperience)
            {
                var luxp = luserexperienceResp.Data as UserExperience;

                if (luxp.GuildExperiences.Count() != 0)
                {
                    var gld = luxp.GuildExperiences.FirstOrDefault(x => x.GuildID == guild.Id);
                    if (gld != null)
                    {
                        if (gld.LastGranted < (DateTime.UtcNow.ToEpoch() - 60))
                        {
                            var xptonextlevel = DiscordUtilities.GetXPLevelRequirement(gld.Level + 1, DiscordUtilities.PHI); //get next level xp requirement based on phi

                            if ((gld.XP + amount) >= xptonextlevel) //if over or equal to next level requirement, update accordingly
                            {
                                gld.XP = 0;
                                gld.TotalXP += amount;
                                gld.Level++;
                                gld.LastGranted = DateTime.UtcNow.ToEpoch();

                                if(sguild.LevelNotification == LevelNotification.Channel)
                                {
                                    ulong ChannelID = 0;
                                    if (sguild.LevelUpChannel != 0)
                                        ChannelID = sguild.LevelUpChannel;
                                    else
                                        ChannelID = backupChannel.Id;

                                    if (string.IsNullOrEmpty(sguild.LevelUpMessage))
                                        await $"Congratulations {user.Mention}!! You're now level **{gld.Level}**".QueueMessage(Models.MessageType.Standard, user, (IMessageChannel)await guild.GetChannelAsync(ChannelID));
                                    else
                                    {
                                        string msg = sguild.LevelUpMessage;
                                        msg = msg.Replace("-m", user.Mention).Replace("-u", user.Username).Replace("-l", $"{gld.Level}");

                                        await msg.QueueMessage(Models.MessageType.Standard, user, (IMessageChannel)await guild.GetChannelAsync(ChannelID));
                                    }
                                }
                                else if(sguild.LevelNotification == LevelNotification.DM)
                                {
                                    if (string.IsNullOrEmpty(sguild.LevelUpMessage))
                                        await $"Congratulations {user.Mention}!! You're now level **{gld.Level}**".QueueMessage(Models.MessageType.DMFail, user, backupChannel);
                                    else
                                    {
                                        string msg = sguild.LevelUpMessage;
                                        msg = msg.Replace("-m", user.Mention).Replace("-u", user.Username).Replace("-l", $"{gld.Level}");

                                        await msg.QueueMessage(Models.MessageType.DMFail, user, backupChannel);
                                    }
                                }

                                await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("MessageService", "User leveled up", LogSeverity.Info));
                                DogStatsd.Increment("user.levels.levelup");
                            }
                            else //otherwise append current status
                            {
                                gld.XP += amount;
                                gld.TotalXP += amount;
                                gld.LastGranted = DateTime.UtcNow.ToEpoch();
                            }
                            await skuldUser.UpdateGuildExperienceAsync(gld, guild.Id);
                            DogStatsd.Increment("user.levels.processed");
                        }
                    }
                    else
                    {
                        gld = new GuildExperience
                        {
                            LastGranted = DateTime.UtcNow.ToEpoch(),
                            XP = amount
                        };
                        gld.TotalXP = gld.XP;
                        await skuldUser.InsertGuildExperienceAsync(guild.Id, gld);
                        DogStatsd.Increment("user.levels.processed");
                    }
                }
                else
                {
                    var gld = new GuildExperience
                    {
                        LastGranted = DateTime.UtcNow.ToEpoch(),
                        XP = amount
                    };
                    gld.TotalXP = gld.XP;
                    await skuldUser.InsertGuildExperienceAsync(guild.Id, gld);
                    DogStatsd.Increment("user.levels.processed");
                }
            }
            else
            {
                var gld = new GuildExperience
                {
                    LastGranted = DateTime.UtcNow.ToEpoch(),
                    XP = amount
                };
                gld.TotalXP = gld.XP;
                await skuldUser.InsertGuildExperienceAsync(guild.Id, gld);
                DogStatsd.Increment("user.levels.processed");
            }

            return;
        }
        public static async Task HandleCustomCommandAsync(SkuldCommandContext context)
        {
            var prefix = MessageTools.GetPrefixFromCommand(context.DBGuild, context.Message.Content, SkuldConfig);
            var name = MessageTools.GetCommandName(prefix, context.Message);
            var customcommand = await MessageTools.GetCustomCommandAsync(context.Guild, name);

            if (customcommand != null)
            {
                await DispatchCommandAsync(context, customcommand);
                return;
            }
            return;
        }
        public static async Task DispatchCommandAsync(SkuldCommandContext context)
        {
            watch.Start();
            await CommandService.ExecuteAsync(context, cmdConfig.ArgPos, BotService.Services);
            watch.Stop();
        }
        public static async Task DispatchCommandAsync(SkuldCommandContext context, CustomCommand command)
        {
            watch.Start();
            await BotService.DiscordClient.SendChannelAsync(context.Channel, command.Content).ConfigureAwait(false);
            watch.Stop();

            DogStatsd.Histogram("commands.latency", watch.ElapsedMilliseconds(), 0.5, new[] { $"module:custom", $"cmd:{command.CommandName.ToLowerInvariant()}" });

            await InsertCommandAsync(command, context.DBUser).ConfigureAwait(false);

            DogStatsd.Increment("commands.processed", 1, 1, new[] { $"module:custom", $"cmd:{command.CommandName.ToLowerInvariant()}" });

            watch = new Stopwatch();
        }

        private static async Task InsertCommandAsync(CommandInfo command, SkuldUser user)
            => await user.UpdateUserCommandCountAsync(command.Name ?? command.Module.Name);

        private static async Task InsertCommandAsync(CustomCommand command, SkuldUser user)
            => await user.UpdateUserCommandCountAsync(command.CommandName);
    }
}
