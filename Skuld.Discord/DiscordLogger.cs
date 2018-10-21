using DiscordNet = Discord;
using Discord.WebSocket;
using Skuld.APIS;
using Skuld.Core;
using Skuld.Core.Models;
using Skuld.Database;
using StatsdClient;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Skuld.Discord.Utilities;

namespace Skuld.Discord
{
    public static class DiscordLogger
    {
        private static Random random = new Random();
        private static BotListingClient botListing;
        private static readonly SkuldConfig Configuration = SkuldConfig.Load();

        public static void AddBotLister(BotListingClient botlist)
        {
            botListing = botlist;
        }

        //DiscordLoging
        public static void RegisterEvents()
        {
            /*All Events needed for running Skuld*/
            BotService.DiscordClient.ShardReady += Bot_ShardReady;
            BotService.DiscordClient.JoinedGuild += Bot_JoinedGuild;
            BotService.DiscordClient.LeftGuild += Bot_LeftGuild;
            BotService.DiscordClient.UserJoined += Bot_UserJoined;
            BotService.DiscordClient.UserLeft += Bot_UserLeft;
            BotService.DiscordClient.ReactionAdded += Bot_ReactionAdded;
            BotService.DiscordClient.ShardConnected += Bot_ShardConnected;
            BotService.DiscordClient.ShardDisconnected += Bot_ShardDisconnected;
            BotService.DiscordClient.Log += Bot_Log;
            BotService.DiscordClient.UserUpdated += Bot_UserUpdated;
        }

        public static void UnRegisterEvents()
        {
            BotService.DiscordClient.ShardReady -= Bot_ShardReady;
            //BotService.DiscordClient.MessageReceived -= botService.OnMessageRecievedAsync;
            BotService.DiscordClient.JoinedGuild -= Bot_JoinedGuild;
            BotService.DiscordClient.LeftGuild -= Bot_LeftGuild;
            BotService.DiscordClient.UserJoined -= Bot_UserJoined;
            BotService.DiscordClient.UserLeft -= Bot_UserLeft;
            BotService.DiscordClient.ReactionAdded -= Bot_ReactionAdded;
            BotService.DiscordClient.ShardConnected -= Bot_ShardConnected;
            BotService.DiscordClient.ShardDisconnected -= Bot_ShardDisconnected;
            BotService.DiscordClient.Log -= Bot_Log;
            BotService.DiscordClient.UserUpdated -= Bot_UserUpdated;
        }

        private static async Task Bot_Log(DiscordNet.LogMessage arg)
            => await GenericLogger.AddToLogsAsync(new LogMessage("Discord-Log: " + arg.Source, arg.Message, arg.Severity, arg.Exception));

        private static async Task Bot_ShardReady(DiscordSocketClient arg)
        {
            await BotService.DiscordClient.SetGameAsync($"{Configuration.Discord.Prefix}help | {random.Next(0, BotService.DiscordClient.Shards.Count) + 1}/{BotService.DiscordClient.Shards.Count}", type: DiscordNet.ActivityType.Listening);
            arg.MessageReceived += MessageHandler.HandleCommandAsync;
        }

        private static async Task Bot_UserUpdated(SocketUser arg1, SocketUser arg2)
        {
            if (arg1.IsBot || arg1.IsWebhook) return;
            if (arg1.GetAvatarUrl() != arg2.GetAvatarUrl())
            {
                var usrResp = await DatabaseClient.GetUserAsync(arg2.Id);

                if(usrResp.Successful)
                {
                    if(usrResp.Data is SkuldUser usr)
                    {
                        usr.AvatarUrl = arg2.GetAvatarUrl();

                        await DatabaseClient.UpdateUserAsync(usr);
                    }
                }
            }
        }

        private static async Task Bot_ReactionAdded(DiscordNet.Cacheable<DiscordNet.IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if(arg3.User.IsSpecified)
            {
                var usr = arg3.User.GetValueOrDefault(null);
                if (usr != null)
                {
                    if (usr.IsBot || usr.IsWebhook) return;
                }
            }
            if (await DatabaseClient.CheckConnectionAsync())
            {
                var gld = BotService.DiscordClient.Guilds.FirstOrDefault(x => x.TextChannels.FirstOrDefault(z => z.Id == arg2.Id) != null);
                var guildResp = await DatabaseClient.GetGuildAsync(gld.Id);
                if (guildResp.Successful && gld != null)
                {
                    if (guildResp.Data is SkuldGuild guild)
                    {
                        if (guild.GuildSettings.Features.Pinning)
                        {
                            var dldedmsg = await arg1.GetOrDownloadAsync();
                            int pinboardThreshold = Configuration.Preferences.PinboardThreshold;
                            int pinboardReactions = 0;
                            if (arg3.Emote.Name == "📌")
                            { pinboardReactions = dldedmsg.Reactions.FirstOrDefault(x => x.Key.Name == "📌").Value.ReactionCount; }
                            if (pinboardReactions >= pinboardThreshold)
                            {
                                var now = dldedmsg.CreatedAt;
                                var dt = DateTime.UtcNow.AddDays(-Configuration.Preferences.PinboardDateLimit);
                                if ((now - dt).TotalDays > 0)
                                {
                                    if (!dldedmsg.IsPinned)
                                    {
                                        await dldedmsg.PinAsync();
                                        await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("PinBrd", $"Reached or Over Threshold, pinned a message in: {dldedmsg.Channel.Name} from: {gld.Name}", DiscordNet.LogSeverity.Info));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static async Task Bot_ShardConnected(DiscordSocketClient arg)
        {
            await arg.SetGameAsync($"{Configuration.Discord.Prefix}help | {arg.ShardId+1}/{BotService.DiscordClient.Shards.Count}", type: DiscordNet.ActivityType.Listening);
            DogStatsd.Event("shards.connected", $"Shard {arg.ShardId} Connected", alertType: "info");
        }

        private static Task Bot_ShardDisconnected(Exception arg1, DiscordSocketClient arg2)
        {
            DogStatsd.Event($"Shard.disconnected", $"Shard {arg2.ShardId} Disconnected, error: {arg1}", alertType: "error");
            return Task.CompletedTask;
        }

        //Start Users
        private static async Task Bot_UserJoined(SocketGuildUser arg)
        {
            await GenericLogger.AddToLogsAsync(new LogMessage("UsrJoin", $"User {arg.Username} joined {arg.Guild.Name}", DiscordNet.LogSeverity.Info));
            if (await DatabaseClient.CheckConnectionAsync())
            {
                await DatabaseClient.InsertUserAsync(arg);

                var guildResp = await DatabaseClient.GetGuildAsync(arg.Guild.Id);

                if (guildResp.Successful)
                {
                    if (guildResp.Data is SkuldGuild guild)
                    {
                        if (guild.JoinRole != 0)
                        {
                            var joinrole = arg.Guild.GetRole(guild.JoinRole);
                            await arg.AddRoleAsync(joinrole);
                            await GenericLogger.AddToLogsAsync(new LogMessage("UsrJoin", $"Gave user {arg.Username}, the automatic role as per request of {arg.Guild.Name}.", DiscordNet.LogSeverity.Info));
                        }
                        if (guild.UserJoinChannel != 0 && !string.IsNullOrEmpty(guild.JoinMessage))
                        {
                            var channel = arg.Guild.GetTextChannel(guild.UserJoinChannel);
                            var welcomemessage = guild.JoinMessage;
                            welcomemessage = welcomemessage.Replace("-m", "**" + arg.Mention + "**");
                            welcomemessage = welcomemessage.Replace("-s", "**" + arg.Guild.Name + "**");
                            welcomemessage = welcomemessage.Replace("-uc", Convert.ToString(arg.Guild.MemberCount));
                            welcomemessage = welcomemessage.Replace("-u", "**" + arg.Username + "**");
                            await BotService.DiscordClient.SendChannelAsync(channel, welcomemessage);
                        }
                        var discord = BotService.DiscordClient.GetUser(arg.Id);
                        var db = await DatabaseClient.GetUserAsync(arg.Id);
                        if (discord == null && db != null)
                        {
                            await DatabaseClient.DropUserAsync(arg.Id);
                        }
                        else if (discord != null && db == null)
                        {
                            await DatabaseClient.InsertUserAsync(discord);
                        }
                    }
                }
            }
        }

        private static async Task Bot_UserLeft(SocketGuildUser arg)
        {
            await GenericLogger.AddToLogsAsync(new LogMessage("UsrLeft", $"User {arg.Username} just left {arg.Guild.Name}", DiscordNet.LogSeverity.Info));
            if (await DatabaseClient.CheckConnectionAsync())
            {
                var guildResp = await DatabaseClient.GetGuildAsync(arg.Guild.Id);
                if(guildResp.Successful)
                {
                    if(guildResp.Data is SkuldGuild guild)
                    {
                        string leavemessage = guild.LeaveMessage;
                        if (!string.IsNullOrEmpty(guild.LeaveMessage))
                        {
                            var channel = arg.Guild.GetTextChannel(guild.UserLeaveChannel);
                            leavemessage = leavemessage.Replace("-m", arg.Mention);
                            leavemessage = leavemessage.Replace("-s", "**" + arg.Guild.Name + "**");
                            leavemessage = leavemessage.Replace("-uc", Convert.ToString(arg.Guild.MemberCount));
                            leavemessage = leavemessage.Replace("-u", "**" + arg.Username + "**");
                            await BotService.DiscordClient.SendChannelAsync(channel, leavemessage);
                        }
                        var discord = BotService.DiscordClient.GetUser(arg.Id);
                        var db = await DatabaseClient.GetUserAsync(arg.Id);
                        if (discord == null && db != null)
                        {
                            await DatabaseClient.DropUserAsync(arg.Id);
                        }
                        else if (discord != null && db == null)
                        {
                            await DatabaseClient.InsertUserAsync(discord);
                        }
                    }
                }
            }
        }

        //End Users

        //Start Guilds
        private static async Task Bot_LeftGuild(SocketGuild arg)
        {
            DogStatsd.Increment("guilds.left");

            await DatabaseClient.DropGuildAsync(arg);

            Thread thd = new Thread(async () =>
            {
                await DatabaseClient.CheckGuildUsersAsync(BotService.DiscordClient, arg);
            })
            {
                IsBackground = true
            };
            thd.Start();
            await botListing.SendDataAsync(Configuration.BotListing.SysExToken, Configuration.BotListing.DiscordPWKey, Configuration.BotListing.DBotsOrgKey);
        }

        private static async Task Bot_JoinedGuild(SocketGuild arg)
        {
            DogStatsd.Increment("guilds.joined");
            Thread thd = new Thread(async () =>
            {
                await DatabaseClient.InsertGuildAsync(arg.Id, Configuration.Discord.Prefix);
            })
            {
                IsBackground = true
            };
            thd.Start();
            await botListing.SendDataAsync(Configuration.BotListing.SysExToken, Configuration.BotListing.DiscordPWKey, Configuration.BotListing.DBotsOrgKey);
        }
        //End Guilds
    }
}