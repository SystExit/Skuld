using Discord;
using Discord.WebSocket;
using Skuld.APIS;
using Skuld.Utilities.Messaging;
using StatsdClient;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Skuld.Services
{
    public class DiscordLogger
    {
        private DiscordShardedClient client;
        private BotService botService;
        private DatabaseService database;
        private Random random;
        private BotListingClient botListing;

        public DiscordLogger(DatabaseService db, DiscordShardedClient cli)
        {
            client = cli;
            database = db;
            random = new Random();
        }

        public void AddBotService(BotService srv)
        {
            botService = srv;
        }

        public void AddBotLister(BotListingClient botlist)
        {
            botListing = botlist;
        }

        //DiscordLoging
        public void RegisterEvents()
        {
            /*All Events needed for running Skuld*/
            client.ShardReady += Bot_ShardReady;
            client.JoinedGuild += Bot_JoinedGuild;
            client.LeftGuild += Bot_LeftGuild;
            client.UserJoined += Bot_UserJoined;
            client.UserLeft += Bot_UserLeft;
            client.ReactionAdded += Bot_ReactionAdded;
            client.ShardConnected += Bot_ShardConnected;
            client.ShardDisconnected += Bot_ShardDisconnected;
            client.Log += Bot_Log;
            client.UserUpdated += Bot_UserUpdated;
        }

        public void UnRegisterEvents()
        {
            client.ShardReady -= Bot_ShardReady;
            //client.MessageReceived -= botService.OnMessageRecievedAsync;
            client.JoinedGuild -= Bot_JoinedGuild;
            client.LeftGuild -= Bot_LeftGuild;
            client.UserJoined -= Bot_UserJoined;
            client.UserLeft -= Bot_UserLeft;
            client.ReactionAdded -= Bot_ReactionAdded;
            client.ShardConnected -= Bot_ShardConnected;
            client.ShardDisconnected -= Bot_ShardDisconnected;
            client.Log -= Bot_Log;
            client.UserUpdated -= Bot_UserUpdated;
        }

        private async Task Bot_Log(LogMessage arg)
            => await HostService.Logger.AddToLogsAsync(new Core.Models.LogMessage("Discord-Log: " + arg.Source, arg.Message, arg.Severity, arg.Exception));

        private async Task Bot_ShardReady(DiscordSocketClient arg)
        {
            await client.SetGameAsync($"{HostService.Configuration.Discord.Prefix}help | {random.Next(0, client.Shards.Count) + 1}/{client.Shards.Count}", type: ActivityType.Listening);
            arg.MessageReceived += botService.messageService.OnMessageRecievedAsync;
        }

        private async Task Bot_UserUpdated(SocketUser arg1, SocketUser arg2)
        {
            if (arg1.GetAvatarUrl() != arg2.GetAvatarUrl())
            {
                var usr = await database.GetUserAsync(arg2.Id);

                usr.AvatarUrl = arg2.GetAvatarUrl();

                await database.UpdateUserAsync(usr);
            }
        }

        private async Task Bot_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (await database.CheckConnectionAsync())
            {
                var gld = client.Guilds.FirstOrDefault(x => x.TextChannels.FirstOrDefault(z => z.Id == arg2.Id) != null);
                var guild = await database.GetGuildAsync(gld.Id);
                if (guild != null && gld != null)
                {
                    if (guild.GuildSettings.Features.Pinning)
                    {
                        var dldedmsg = await arg1.GetOrDownloadAsync();
                        int pinboardThreshold = HostService.Configuration.Preferences.PinboardThreshold;
                        int pinboardReactions = 0;
                        if (arg3.Emote.Name == "📌")
                        { pinboardReactions = dldedmsg.Reactions.FirstOrDefault(x => x.Key.Name == "📌").Value.ReactionCount; }
                        if (pinboardReactions >= pinboardThreshold)
                        {
                            var now = dldedmsg.CreatedAt;
                            var dt = DateTime.UtcNow.AddDays(-HostService.Configuration.Preferences.PinboardDateLimit);
                            if ((now - dt).TotalDays > 0)
                            {
                                if (!dldedmsg.IsPinned)
                                {
                                    await dldedmsg.PinAsync();
                                    await HostService.Logger.AddToLogsAsync(new Core.Models.LogMessage("PinBrd", $"Reached or Over Threshold, pinned a message in: {dldedmsg.Channel.Name} from: {gld.Name}", LogSeverity.Info));
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task Bot_ShardConnected(DiscordSocketClient arg)
        {
            await arg.SetGameAsync($"{HostService.Configuration.Discord.Prefix}help | {random.Next(0, client.Shards.Count) + 1}/{client.Shards.Count}", type: ActivityType.Listening);
            DogStatsd.Event("shards.connected", $"Shard {arg.ShardId} Connected", alertType: "info");
        }

        private Task Bot_ShardDisconnected(Exception arg1, DiscordSocketClient arg2)
        {
            DogStatsd.Event($"Shard.disconnected", $"Shard {arg2.ShardId} Disconnected, error: {arg1}", alertType: "error");
            return Task.CompletedTask;
        }

        //Start Users
        private async Task Bot_UserJoined(SocketGuildUser arg)
        {
            await HostService.Logger.AddToLogsAsync(new Core.Models.LogMessage("UsrJoin", $"User {arg.Username} joined {arg.Guild.Name}", LogSeverity.Info));
            if (await database.CheckConnectionAsync())
            {
                await database.InsertUserAsync(arg);

                var guild = await database.GetGuildAsync(arg.Guild.Id);

                if (guild != null && guild.JoinRole != 0)
                {
                    var joinrole = arg.Guild.GetRole(guild.JoinRole);
                    await arg.AddRoleAsync(joinrole);
                    await HostService.Logger.AddToLogsAsync(new Core.Models.LogMessage("UsrJoin", $"Gave user {arg.Username}, the automatic role as per request of {arg.Guild.Name}.", LogSeverity.Info));
                }
                if (guild != null && guild.UserJoinChannel != 0 && !String.IsNullOrEmpty(guild.JoinMessage))
                {
                    var channel = arg.Guild.GetTextChannel(guild.UserJoinChannel);
                    var welcomemessage = guild.JoinMessage;
                    welcomemessage = welcomemessage.Replace("-m", "**" + arg.Mention + "**");
                    welcomemessage = welcomemessage.Replace("-s", "**" + arg.Guild.Name + "**");
                    welcomemessage = welcomemessage.Replace("-uc", Convert.ToString(arg.Guild.MemberCount));
                    welcomemessage = welcomemessage.Replace("-u", "**" + arg.Username + "**");
                    await MessageTools.SendChannelAsync(channel, welcomemessage);
                }
                var discord = client.GetUser(arg.Id);
                var db = await database.GetUserAsync(arg.Id);
                if (discord == null && db != null)
                {
                    await database.DropUserAsync(arg.Id);
                }
                else if (discord != null && db == null)
                {
                    await database.InsertUserAsync(discord);
                }
            }
        }

        private async Task Bot_UserLeft(SocketGuildUser arg)
        {
            await HostService.Logger.AddToLogsAsync(new Core.Models.LogMessage("UsrLeft", $"User {arg.Username} just left {arg.Guild.Name}", LogSeverity.Info));
            if (await database.CheckConnectionAsync())
            {
                var guild = await database.GetGuildAsync(arg.Guild.Id);
                string leavemessage = guild.LeaveMessage;
                if (!String.IsNullOrEmpty(guild.LeaveMessage))
                {
                    var channel = arg.Guild.GetTextChannel(guild.UserLeaveChannel);
                    leavemessage = leavemessage.Replace("-m", "**" + arg.Mention + "**");
                    leavemessage = leavemessage.Replace("-s", "**" + arg.Guild.Name + "**");
                    leavemessage = leavemessage.Replace("-uc", Convert.ToString(arg.Guild.MemberCount));
                    leavemessage = leavemessage.Replace("-u", "**" + arg.Username + "**");
                    await MessageTools.SendChannelAsync(channel, leavemessage);
                }
                var discord = client.GetUser(arg.Id);
                var db = await database.GetUserAsync(arg.Id);
                if (discord == null && db != null)
                {
                    await database.DropUserAsync(arg.Id);
                }
                else if (discord != null && db == null)
                {
                    await database.InsertUserAsync(discord);
                }
            }
        }

        //End Users

        //Start Guilds
        private async Task Bot_LeftGuild(SocketGuild arg)
        {
            DogStatsd.Increment("guilds.left");

            await database.DropGuildAsync(arg);

            Thread thd = new Thread(async () =>
            {
                await database.CheckGuildUsersAsync(arg);
            })
            {
                IsBackground = true
            };
            thd.Start();
            await botListing.SendDataAsync(HostService.Configuration.BotListing.SysExToken, HostService.Configuration.BotListing.DiscordPWKey, HostService.Configuration.BotListing.DBotsOrgKey);
        }

        private async Task Bot_JoinedGuild(SocketGuild arg)
        {
            DogStatsd.Increment("guilds.joined");
            Thread thd = new Thread(async () =>
            {
                await database.InsertGuildAsync(arg);
            })
            {
                IsBackground = true
            };
            thd.Start();
            await botListing.SendDataAsync(HostService.Configuration.BotListing.SysExToken, HostService.Configuration.BotListing.DiscordPWKey, HostService.Configuration.BotListing.DBotsOrgKey);
        }

        //End Guilds
    }
}