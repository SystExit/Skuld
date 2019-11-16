using Discord;
using Discord.WebSocket;
using Skuld.APIS;
using Skuld.Core.Generic.Models;
using Skuld.Core.Utilities;
using Skuld.Discord.Handlers;
using Skuld.Core.Models;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DiscordNet = Discord;

namespace Skuld.Discord.Services
{
    public static class DiscordLogger
    {
        private static BotListingClient botListing;
        private static readonly SkuldConfig Configuration = SkuldConfig.Load();
        private static List<int> ShardsReady = new List<int>();

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
            BotService.DiscordClient.JoinedGuild -= Bot_JoinedGuild;
            BotService.DiscordClient.LeftGuild -= Bot_LeftGuild;
            BotService.DiscordClient.UserJoined -= Bot_UserJoined;
            BotService.DiscordClient.UserLeft -= Bot_UserLeft;
            BotService.DiscordClient.ReactionAdded -= Bot_ReactionAdded;
            BotService.DiscordClient.ShardConnected -= Bot_ShardConnected;
            BotService.DiscordClient.ShardDisconnected -= Bot_ShardDisconnected;
            BotService.DiscordClient.Log -= Bot_Log;
            BotService.DiscordClient.UserUpdated -= Bot_UserUpdated;
            foreach (var shard in BotService.DiscordClient.Shards)
            {
                shard.MessageReceived -= MessageHandler.HandleMessageAsync;
            }
        }

        private static Task Bot_Log(LogMessage arg)
        {
            string key = "Discord-Log: ";
            switch (arg.Severity)
            {
                case LogSeverity.Info:
                    Log.Info(key + arg.Source, arg.Message);
                    break;

                case LogSeverity.Critical:
                    Log.Critical(key + arg.Source, arg.Message, arg.Exception);
                    break;

                case LogSeverity.Warning:
                    Log.Warning(key + arg.Source, arg.Message, arg.Exception);
                    break;

                case LogSeverity.Verbose:
                    Log.Verbose(key + arg.Source, arg.Message, arg.Exception);
                    break;

                case LogSeverity.Error:
                    Log.Error(key + arg.Source, arg.Message, arg.Exception);
                    break;

                case LogSeverity.Debug:
                    Log.Debug(key + arg.Source, arg.Message, arg.Exception);
                    break;
            }
            return Task.CompletedTask;
        }

        private static async Task Bot_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (arg3.User.IsSpecified)
            {
                var usr = arg3.User.GetValueOrDefault(null);
                if (usr != null)
                {
                    if (usr.IsBot || usr.IsWebhook) return;
                }
            }
            /*if (await DatabaseClient.CheckConnectionAsync())
            {
                var gld = BotService.DiscordClient.Guilds.FirstOrDefault(x => x.TextChannels.FirstOrDefault(z => z.Id == arg2.Id) != null);
                var guildResp = await DatabaseClient.GetGuildAsync(gld.Id);
                if (guildResp.Successful && gld != null)
                {
                    if (guildResp.Data is Guild guild)
                    {
                        if (guild.Features.Pinning)
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
                                        await GenericLogger.AddToLogsAsync(new LogMessage("PinBrd", $"Reached or Over Threshold, pinned a message in: {dldedmsg.Channel.Name} from: {gld.Name}", DiscordNet.LogSeverity.Info));
                                    }
                                }
                            }
                        }
                    }
                }
            }*/
        }

        #region Shards
        private static async Task Bot_ShardReady(DiscordSocketClient arg)
        {
            await BotService.DiscordClient.SetGameAsync($"{Configuration.Discord.Prefix}help | {arg.ShardId + 1}/{BotService.DiscordClient.Shards.Count}", type: DiscordNet.ActivityType.Listening);
            if (!ShardsReady.Contains(arg.ShardId))
            {
                arg.MessageReceived += MessageHandler.HandleMessageAsync;
                ShardsReady.Add(arg.ShardId);
            }

            Log.Info($"Shard #{arg.ShardId}", "Shard Ready");
        }

        private static async Task Bot_ShardConnected(DiscordSocketClient arg)
        {
            await arg.SetGameAsync($"{Configuration.Discord.Prefix}help | {arg.ShardId + 1}/{BotService.DiscordClient.Shards.Count}", type: ActivityType.Listening).ConfigureAwait(false);
            DogStatsd.Event("shards.connected", $"Shard {arg.ShardId} Connected", alertType: "info");
        }

        private static Task Bot_ShardDisconnected(Exception arg1, DiscordSocketClient arg2)
        {
            DogStatsd.Event($"Shard.disconnected", $"Shard {arg2.ShardId} Disconnected, error: {arg1}", alertType: "error");
            return Task.CompletedTask;
        }
        #endregion Shards

        #region Users
        private static async Task Bot_UserJoined(SocketGuildUser arg)
        {
            Log.Info("UsrJoin", $"User {arg.Username} joined {arg.Guild.Name}");

            /*if (await DatabaseClient.CheckConnectionAsync())
            {
                await DatabaseClient.InsertUserAsync(arg);

                var guildResp = await DatabaseClient.GetGuildAsync(arg.Guild.Id);

                if (guildResp.Successful)
                {
                    if (guildResp.Data is Guild guild)
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
            }*/
        }

        private static async Task Bot_UserLeft(SocketGuildUser arg)
        {
            Log.Info("UsrLeft", $"User {arg.Username} just left {arg.Guild.Name}");

            /*if (await DatabaseClient.CheckConnectionAsync())
            {
                var guildResp = await DatabaseClient.GetGuildAsync(arg.Guild.Id);
                if(guildResp.Successful)
                {
                    if(guildResp.Data is Guild guild)
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
            }*/
        }

        private static async Task Bot_UserUpdated(SocketUser arg1, SocketUser arg2)
        {
            if (arg1.IsBot || arg1.IsWebhook) return;
            if (arg1.GetAvatarUrl() != arg2.GetAvatarUrl())
            {
                /*var usrResp = await DatabaseClient.GetUserAsync(arg2.Id);

                if (usrResp.Successful)
                {
                    if (usrResp.Data is User usr)
                    {
                        usr.AvatarUrl = arg2.GetAvatarUrl();

                        await DatabaseClient.UpdateUserAsync(usr);
                    }
                }*/
            }
        }
        #endregion Users

        #region Guilds
        private static async Task Bot_LeftGuild(SocketGuild arg)
        {
            using var database = new SkuldDbContextFactory().CreateDbContext();

            DogStatsd.Increment("guilds.left");

            await botListing.SendDataAsync(Configuration.BotListing.DiscordGGKey, Configuration.BotListing.DBotsOrgKey, Configuration.BotListing.B4DToken).ConfigureAwait(false);

            Log.Verbose("Bot - LGuild", $"Left guild {arg.Name}");

            await database.DropGuildAsync(arg.Id);
        }

        private static async Task Bot_JoinedGuild(SocketGuild arg)
        {
            using var database = new SkuldDbContextFactory().CreateDbContext();

            DogStatsd.Increment("guilds.joined");

            await botListing.SendDataAsync(Configuration.BotListing.DiscordGGKey, Configuration.BotListing.DBotsOrgKey, Configuration.BotListing.B4DToken).ConfigureAwait(false);

            Log.Verbose("Bot - JGuild", $"Joined guild {arg.Name}");

            await database.InsertGuildAsync(arg, Configuration.Discord.Prefix, MessageHandler.cmdConfig.MoneyName, MessageHandler.cmdConfig.MoneyIcon);
        }
        #endregion
    }
}