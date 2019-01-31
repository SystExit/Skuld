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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Skuld.Core;
using Skuld.Discord.Commands;
using Skuld.Discord.Services;

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

                if (!arg3.IsSuccess)
                {
                    bool displayerror = true;
                    if (arg3.ErrorReason.Contains("few parameters"))
                    {
                        var cmdembed = DiscordUtilities.GetCommandHelp(CommandService, arg2, cmd.Name);
                        await BotService.DiscordClient.SendChannelAsync(arg2.Channel, "You seem to be missing a parameter or 2, here's the help", cmdembed).ConfigureAwait(false);
                        displayerror = false;
                    }

                    if (arg3.Error != CommandError.UnknownCommand && !arg3.ErrorReason.Contains("Timeout") && displayerror)
                    {
                        await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("CmdHand", "Error with command, Error is: " + arg3, LogSeverity.Error));
                        await BotService.DiscordClient.SendChannelAsync(arg2.Channel, "",
                            new EmbedBuilder
                            {
                                Author = new EmbedAuthorBuilder { Name = "Error with the command" },
                                Description = Convert.ToString(arg3.ErrorReason),
                                Color = new Color(255, 0, 0)
                            }.Build()).ConfigureAwait(false);
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
        public static async Task HandleCommandAsync(SocketMessage arg)
        {
            var cms = CommandService.Commands;
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

            if (!MessageTools.IsEnabledChannel(await (message.Channel as ITextChannel).Guild.GetUserAsync(message.Author.Id), (ITextChannel)message.Channel)) { return; }

            var context = new SkuldCommandContext(BotService.DiscordClient, message, suser, await MessageTools.GetGuildAsync((message.Channel as ITextChannel).Guild).ConfigureAwait(false));

            if (context.DBGuild != null) { if (!MessageTools.HasPrefix(message, BotService.DiscordClient, SkuldConfig, cmdConfig, context.DBGuild.Prefix)) { return; } }
            else { if (!MessageTools.HasPrefix(message, BotService.DiscordClient, SkuldConfig, cmdConfig)) { return; } }

            if(context.DBGuild != null)
            {
                if (context.DBGuild.Features.Experience)
                {
                    var _ = HandleExperienceAsync(context);
                }
                if (context.DBGuild.Modules.CustomEnabled)
                {
                    var _ = HandleCustomCommandAsync(context);
                }
            }

            await DispatchCommandAsync(context).ConfigureAwait(false);
        }
        public static async Task HandleExperienceAsync(SkuldCommandContext context)
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
            => await DatabaseClient.UpdateUserUsageAsync(user, command.Name ?? command.Module.Name);

        private static async Task InsertCommandAsync(CustomCommand command, SkuldUser user)
            => await DatabaseClient.UpdateUserUsageAsync(user, command.CommandName);
    }
}
