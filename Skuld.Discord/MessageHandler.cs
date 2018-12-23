using Discord;
using Discord.Commands;
using Discord.WebSocket;
using StatsdClient;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Database;
using Skuld.Discord.TypeReaders;
using Skuld.Discord.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Skuld.Core;

namespace Skuld.Discord
{
    public static class MessageHandler
    {
        private static SkuldConfig SkuldConfig;
        public static CommandService CommandService;
        public static MessageServiceConfig cmdConfig;
        private static readonly Random rnd = new Random();

        public static async Task<EventResult> ConfigureCommandServiceAsync(CommandServiceConfig Config, MessageServiceConfig cmdConf, SkuldConfig conf, Assembly ModuleAssembly, IServiceProvider ServiceProvider = null)
        {
            SkuldConfig = conf;
            cmdConfig = cmdConf;
            try
            {
                CommandService = new CommandService(Config);

                CommandService.Log += async (arg) =>
                   await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("CommandService - " + arg.Source, arg.Message, arg.Severity, arg.Exception));

                CommandService.AddTypeReader<Uri>(new UriTypeReader());
                await CommandService.AddModulesAsync(ModuleAssembly, ServiceProvider);

                return EventResult.FromSuccess();
            }
            catch (Exception ex)
            {
                return EventResult.FromFailureException(ex.Message, ex);
            }
        }

        public static void HandleMessageAsync(SocketMessage arg)
        {
            var _ = HandleCommandAsync(arg).ConfigureAwait(false);
        }
        public static async Task HandleCommandAsync(SocketMessage arg)
        {
            DogStatsd.Increment("messages.recieved");
            if (arg.Author.IsBot || arg.Author.IsWebhook) { return; }

            var message = arg as SocketUserMessage;
            if (message is null) { return; }

            SkuldUser suser = await MessageTools.GetUserAsync(message.Author).ConfigureAwait(false);
            if (await DatabaseClient.CheckConnectionAsync())
            {
                if (suser.AvatarUrl != message.Author.GetAvatarUrl())
                {
                    await DatabaseClient.SingleQueryAsync(new MySql.Data.MySqlClient.MySqlCommand($"UPDATE `users` SET `AvatarUrl` = \"{message.Author.GetAvatarUrl()}\" WHERE `UserID` = {message.Author.Id};"));
                }
            }

            if (suser != null && suser.Banned) return;

            var gchan = message.Channel as ITextChannel;

            SkuldGuild sguild = await MessageTools.GetGuildAsync(gchan.Guild).ConfigureAwait(false);

            var context = new SkuldCommandContext(BotService.DiscordClient, message, suser, sguild);

            /*if (sguild.GuildSettings.Features.Experience)
            {
                var _ = HandleExperienceAsync(context);
            }
            if (sguild.GuildSettings.Modules.CustomEnabled)
            {
                var _ = HandleCustomCommandAsync(context);
            }*/

            if (!MessageTools.IsEnabledChannel(await gchan.Guild.GetUserAsync(message.Author.Id), (ITextChannel)message.Channel)) { return; }

            if (sguild != null) { if (!MessageTools.HasPrefix(message, BotService.DiscordClient, SkuldConfig, cmdConfig, sguild.Prefix)) { return; } }
            else { if (!MessageTools.HasPrefix(message, BotService.DiscordClient, SkuldConfig, cmdConfig)) { return; } }

            var cmds = CommandService.Search(context, MessageTools.GetCmdName(message, SkuldConfig.Discord, BotService.DiscordClient, sguild)).Commands;
            if (cmds == null || cmds.Count == 0) { return; }

            var cmd = cmds.FirstOrDefault().Command;
            /*if (sguild != null)
            {
                if (MessageTools.ModuleDisabled(sguild.GuildSettings.Modules, cmd))
                {
                    await context.Client.SendChannelAsync(context.Channel, $"The module: `{cmd.Module.Name}` is disabled, contact a server administrator to enable it");
                    return;
                }
            }*/

            DogStatsd.Increment("commands.total.threads", 1, 1, new[] { $"module:{cmd.Module.Name.ToLowerInvariant()}", $"cmd:{cmd.Name.ToLowerInvariant()}" });

            await DispatchCommandAsync(context, cmd).ConfigureAwait(false);
        }
        /*public static async Task HandleExperienceAsync(SkuldCommandContext context)
        {
            var luserexperienceResp = await DatabaseClient.GetUserExperienceAsync(context.User.Id);

            if (luserexperienceResp.Data is UserExperience)
            {
                var luxp = luserexperienceResp.Data as UserExperience;

                ulong amount = (ulong)rnd.Next(0, 25);

                if (luxp.GuildExperiences.Count() != 0)
                {
                    var gld = luxp.GuildExperiences.FirstOrDefault(x => x.GuildID == context.DBGuild.ID);
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
                                if(string.IsNullOrEmpty(context.DBGuild.LevelUpMessage))
                                    await context.Channel.SendMessageAsync($"Congratulations {context.User.Mention}!! You're now level **{gld.Level}**");
                                else
                                {
                                    string msg = context.DBGuild.LevelUpMessage;
                                    msg = msg.Replace("-m", context.User.Mention).Replace("-u", context.User.Username).Replace("-l", $"{gld.Level}");
                                    await context.Channel.SendMessageAsync(msg);
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
                            await DatabaseClient.UpdateGuildExperienceAsync(context.User.Id, gld, context.Guild.Id);
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
                        await DatabaseClient.InsertGuildExperienceAsync(context.User.Id, context.Guild.Id, gld);
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
                    await DatabaseClient.InsertGuildExperienceAsync(context.User.Id, context.Guild.Id, gld);
                    DogStatsd.Increment("user.levels.processed");
                }
            }
            else
            {
                var gld = new GuildExperience
                {
                    LastGranted = DateTime.UtcNow.ToEpoch(),
                    XP = (ulong)rnd.Next(0, 25)
                };
                gld.TotalXP = gld.XP;
                await DatabaseClient.InsertGuildExperienceAsync(context.User.Id, context.Guild.Id, gld);
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
        }*/

        public static async Task DispatchCommandAsync(SkuldCommandContext context, CommandInfo command)
        {
            var watch = new StatsdClient.Stopwatch();
            watch.Start();
            var result = await CommandService.ExecuteAsync(context, cmdConfig.ArgPos, BotService.Services);
            watch.Stop();

            if (result.IsSuccess)
            {
                DogStatsd.Histogram("commands.latency", watch.ElapsedMilliseconds(), 0.5, new[] { $"module:{command.Module.Name.ToLowerInvariant()}", $"cmd:{command.Name.ToLowerInvariant()}" });

                await InsertCommandAsync(command, context.DBUser).ConfigureAwait(false);

                DogStatsd.Increment("commands.processed", 1, 1, new[] { $"module:{command.Module.Name.ToLowerInvariant()}", $"cmd:{command.Name.ToLowerInvariant()}" });
            }

            if (!result.IsSuccess)
            {
                bool displayerror = true;
                if (result.ErrorReason.Contains("few parameters"))
                {
                    var cmdembed = DiscordUtilities.GetCommandHelp(CommandService, context, command.Name);
                    await BotService.DiscordClient.SendChannelAsync(context.Channel, "You seem to be missing a parameter or 2, here's the help", cmdembed).ConfigureAwait(false);
                    displayerror = false;
                }

                if (result.Error != CommandError.UnknownCommand && !result.ErrorReason.Contains("Timeout") && displayerror)
                {
                    await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("CmdHand", "Error with command, Error is: " + result, LogSeverity.Error));
                    await BotService.DiscordClient.SendChannelAsync(context.Channel, "",
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

        public static async Task DispatchCommandAsync(SkuldCommandContext context, CustomCommand command)
        {
            var stopwatch = new StatsdClient.Stopwatch();
            stopwatch.Start();
            await BotService.DiscordClient.SendChannelAsync(context.Channel, command.Content).ConfigureAwait(false);
            stopwatch.Stop();

            DogStatsd.Histogram("commands.latency", stopwatch.ElapsedMilliseconds(), 0.5, new[] { $"module:custom", $"cmd:{command.CommandName.ToLowerInvariant()}" });

            await InsertCommandAsync(command, context.DBUser).ConfigureAwait(false);

            DogStatsd.Increment("commands.processed", 1, 1, new[] { $"module:custom", $"cmd:{command.CommandName.ToLowerInvariant()}" });
        }

        private static async Task InsertCommandAsync(CommandInfo command, SkuldUser user)
            => await DatabaseClient.UpdateUserUsageAsync(user, command.Name ?? command.Module.Name);

        private static async Task InsertCommandAsync(CustomCommand command, SkuldUser user)
            => await DatabaseClient.UpdateUserUsageAsync(user, command.CommandName);
    }
}
