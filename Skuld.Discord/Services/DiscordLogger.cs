using Discord;
using Discord.WebSocket;
using Skuld.APIS;
using Skuld.Core.Generic.Models;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Discord.Extensions;
using Skuld.Discord.Handlers;
using Skuld.Discord.Utilities;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Discord.Services
{
    public static class DiscordLogger
    {
        public const string Key = "DiscordLog";
        private static SkuldConfig Configuration;
        private static readonly List<int> ShardsReady = new List<int>();

        public static void FeedConfiguration(SkuldConfig inConfig)
            => Configuration = inConfig;

        //DiscordLoging
        public static void RegisterEvents()
        {
            /*All Events needed for running Skuld*/
            BotService.DiscordClient.ShardReady += Bot_ShardReady;
            BotService.DiscordClient.JoinedGuild += Bot_JoinedGuild;
            BotService.DiscordClient.RoleDeleted += Bot_RoleDeleted;
            BotService.DiscordClient.LeftGuild += Bot_LeftGuild;
            BotService.DiscordClient.UserJoined += Bot_UserJoined;
            BotService.DiscordClient.UserLeft += Bot_UserLeft;
            BotService.DiscordClient.ReactionAdded += Bot_ReactionAdded;
            BotService.DiscordClient.ReactionRemoved += Bot_ReactionRemoved;
            BotService.DiscordClient.ReactionsCleared += Bot_ReactionsCleared;
            BotService.DiscordClient.ShardConnected += Bot_ShardConnected;
            BotService.DiscordClient.ShardDisconnected += Bot_ShardDisconnected;
            BotService.DiscordClient.Log += Bot_Log;
            BotService.DiscordClient.UserUpdated += Bot_UserUpdated;
        }

        public static void UnRegisterEvents()
        {
            BotService.DiscordClient.ShardReady -= Bot_ShardReady;
            BotService.DiscordClient.JoinedGuild -= Bot_JoinedGuild;
            BotService.DiscordClient.RoleDeleted -= Bot_RoleDeleted;
            BotService.DiscordClient.LeftGuild -= Bot_LeftGuild;
            BotService.DiscordClient.UserJoined -= Bot_UserJoined;
            BotService.DiscordClient.UserLeft -= Bot_UserLeft;
            BotService.DiscordClient.ReactionAdded -= Bot_ReactionAdded;
            BotService.DiscordClient.ReactionRemoved -= Bot_ReactionRemoved;
            BotService.DiscordClient.ReactionsCleared -= Bot_ReactionsCleared;
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
            var key = $"{Key} - {arg.Source}";
            switch (arg.Severity)
            {
                case LogSeverity.Info:
                    Log.Info(key, arg.Message);
                    break;

                case LogSeverity.Critical:
                    Log.Critical(key, arg.Message, arg.Exception);
                    break;

                case LogSeverity.Warning:
                    Log.Warning(key, arg.Message, arg.Exception);
                    break;

                case LogSeverity.Verbose:
                    Log.Verbose(key, arg.Message, arg.Exception);
                    break;

                case LogSeverity.Error:
                    Log.Error(key, arg.Message, arg.Exception);
                    break;

                case LogSeverity.Debug:
                    Log.Debug(key, arg.Message, arg.Exception);
                    break;

                default:
                    break;
            }
            return Task.CompletedTask;
        }

        #region Reactions

        private static async Task Bot_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            DogStatsd.Increment("messages.reactions.added");

            if (arg3.User.IsSpecified)
            {
                var usr = arg3.User.GetValueOrDefault(null);
                if (usr != null)
                {
                    if (usr.IsBot || usr.IsWebhook) return;
                }
            }

            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var guild = BotService.DiscordClient.Guilds.FirstOrDefault(x => x.TextChannels.FirstOrDefault(z => z.Id == arg2.Id) != null);
            var feats = Database.Features.FirstOrDefault(x => x.Id == guild.Id);

            if (feats.Pinning)
            {
                var pins = await arg2.GetPinnedMessagesAsync();

                if (pins.Count < 50)
                {
                    var dldedmsg = await arg1.GetOrDownloadAsync();

                    int pinboardThreshold = Configuration.PinboardThreshold;
                    int pinboardReactions = dldedmsg.Reactions.FirstOrDefault(x => x.Key.Name == "📌").Value.ReactionCount;

                    if (pinboardReactions >= pinboardThreshold)
                    {
                        var now = dldedmsg.CreatedAt;
                        var dt = DateTime.UtcNow.AddDays(-Configuration.PinboardDateLimit);
                        if ((now - dt).TotalDays > 0)
                        {
                            if (!dldedmsg.IsPinned)
                            {
                                await dldedmsg.PinAsync();

                                Log.Info("PinBoard", "Message reached threshold, pinned a message");
                            }
                        }
                    }

                    await dldedmsg.Channel.SendMessageAsync(
                        guild.Owner.Mention,
                        false,
                        new EmbedBuilder()
                            .AddAuthor(BotService.DiscordClient)
                            .WithDescription($"Can't pin a message in this channel, please clean out some pins.")
                            .WithColor(Color.Red)
                            .Build()
                    ).ConfigureAwait(false);
                }
            }
        }

        private static Task Bot_ReactionsCleared(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            DogStatsd.Increment("messages.reactions.cleared");
            return Task.CompletedTask;
        }

        private static Task Bot_ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            DogStatsd.Increment("messages.reactions.removed");
            return Task.CompletedTask;
        }

        #endregion Reactions

        #region Shards

        private static async Task Bot_ShardReady(DiscordSocketClient arg)
        {
            await BotService.DiscordClient.SetGameAsync($"{Configuration.Prefix}help | {arg.ShardId + 1}/{BotService.DiscordClient.Shards.Count}", type: ActivityType.Listening);
            if (!ShardsReady.Contains(arg.ShardId))
            {
                arg.MessageReceived += MessageHandler.HandleMessageAsync;
                ShardsReady.Add(arg.ShardId);
            }

            Log.Info($"Shard #{arg.ShardId}", "Shard Ready");
        }

        private static async Task Bot_ShardConnected(DiscordSocketClient arg)
        {
            await arg.SetGameAsync($"{Configuration.Prefix}help | {arg.ShardId + 1}/{BotService.DiscordClient.Shards.Count}", type: ActivityType.Listening).ConfigureAwait(false);
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
            DogStatsd.Increment("guild.users.joined");

            using (var ddb = new SkuldDbContextFactory().CreateDbContext())
            {
                await ddb.InsertUserAsync(arg as IUser).ConfigureAwait(false);
            }

            using var db = new SkuldDbContextFactory().CreateDbContext();

            var gld = await db.GetGuildAsync(arg.Guild).ConfigureAwait(false);

            if (gld != null)
            {
                if (gld.JoinRole != 0)
                {
                    var joinrole = arg.Guild.GetRole(gld.JoinRole);
                    await arg.AddRoleAsync(joinrole).ConfigureAwait(false);
                }

                if (gld.JoinChannel != 0 && !string.IsNullOrEmpty(gld.JoinMessage))
                {
                    var channel = arg.Guild.GetTextChannel(gld.JoinChannel);
                    var message = gld.JoinMessage.ReplaceGuildEventMessage(arg as IUser, arg.Guild as SocketGuild);
                    await BotService.DiscordClient.SendChannelAsync(channel, message);
                }
            }

            Log.Verbose(Key, $"{arg} joined {arg.Guild}");
        }

        private static async Task Bot_UserLeft(SocketGuildUser arg)
        {
            DogStatsd.Increment("guild.users.left");

            using var db = new SkuldDbContextFactory().CreateDbContext();

            var gld = await db.GetGuildAsync(arg.Guild).ConfigureAwait(false);

            if (gld != null)
            {
                if (gld.LeaveChannel != 0 && !string.IsNullOrEmpty(gld.LeaveMessage))
                {
                    var channel = arg.Guild.GetTextChannel(gld.JoinChannel);
                    var message = gld.LeaveMessage.ReplaceGuildEventMessage(arg as IUser, arg.Guild as SocketGuild);
                    await BotService.DiscordClient.SendChannelAsync(channel, message);
                }
            }

            Log.Verbose(Key, $"{arg} left {arg.Guild}");
        }

        private static async Task Bot_UserUpdated(SocketUser arg1, SocketUser arg2)
        {
            if (arg1.IsBot || arg1.IsWebhook) return;
            if (arg1.GetAvatarUrl() != arg2.GetAvatarUrl())
            {
                var db = new SkuldDbContextFactory().CreateDbContext();

                var user = await db.GetUserAsync(arg2).ConfigureAwait(false);

                user.AvatarUrl = new Uri(arg2.GetAvatarUrl() ?? arg2.GetDefaultAvatarUrl());

                await db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        #endregion Users

        #region Guilds
        private static async Task Bot_LeftGuild(SocketGuild arg)
        {
            using var database = new SkuldDbContextFactory().CreateDbContext();

            DogStatsd.Increment("guilds.left");

            await BotService.DiscordClient.SendDataAsync(Configuration.IsDevelopmentBuild, Configuration.DiscordGGKey, Configuration.DBotsOrgKey, Configuration.B4DToken).ConfigureAwait(false);

            MessageQueue.CheckForEmptyGuilds = true;

            Log.Verbose(Key, $"Just left {arg}");
        }

        private static async Task Bot_JoinedGuild(SocketGuild arg)
        {
            using var database = new SkuldDbContextFactory().CreateDbContext();

            DogStatsd.Increment("guilds.joined");

            await BotService.DiscordClient.SendDataAsync(Configuration.IsDevelopmentBuild, Configuration.DiscordGGKey, Configuration.DBotsOrgKey, Configuration.B4DToken).ConfigureAwait(false);

            await database.InsertGuildAsync(arg, Configuration.Prefix, MessageHandler.cmdConfig.MoneyName, MessageHandler.cmdConfig.MoneyIcon);

            MessageQueue.CheckForEmptyGuilds = true;
            Log.Verbose(Key, $"Just left {arg}");
        }

        private static async Task Bot_RoleDeleted(SocketRole arg)
        {
            DogStatsd.Increment("guilds.role.deleted");

            #region LevelRewards

            {
                using var database = new SkuldDbContextFactory().CreateDbContext();

                if (database.LevelRewards.Any(x => x.RoleId == arg.Id))
                {
                    database.LevelRewards.RemoveRange(database.LevelRewards.AsQueryable().Where(x => x.RoleId == arg.Id));

                    await database.SaveChangesAsync().ConfigureAwait(false);
                }
            }

            #endregion

            #region IAmRoles

            {
                using var database = new SkuldDbContextFactory().CreateDbContext();

                if (database.IAmRoles.Any(x => x.RoleId == arg.Id))
                {
                    database.IAmRoles.RemoveRange(database.IAmRoles.AsQueryable().Where(x => x.RoleId == arg.Id));

                    await database.SaveChangesAsync().ConfigureAwait(false);
                }
            }

            {
                using var database = new SkuldDbContextFactory().CreateDbContext();

                if (database.IAmRoles.Any(x => x.RequiredRoleId == arg.Id))
                {
                    foreach (var role in database.IAmRoles.AsQueryable().Where(x => x.RequiredRoleId == arg.Id))
                    {
                        role.RequiredRoleId = 0;
                    }

                    await database.SaveChangesAsync().ConfigureAwait(false);
                }
            }

            #endregion

            Log.Verbose(Key, $"{arg} deleted in {arg.Guild}");
        }
        #endregion
    }
}