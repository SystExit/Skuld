using System.Threading.Tasks;
using Discord.WebSocket;
using Skuld.Tools;
using Discord;
using System.Linq;
using System;
using MySql.Data.MySqlClient;
using Skuld.Models;
using System.Collections.Generic;
using System.Threading;
using StatsdClient;

namespace Skuld.Events
{
    public class DiscordEvents : Bot
    {
        public static void RegisterEvents()
        {
            /*All Events needed for running Skuld*/
            bot.MessageReceived += CommandManager.Bot_MessageReceived;
            bot.JoinedGuild += Bot_JoinedGuild;
            bot.LeftGuild += Bot_LeftGuild;
            bot.UserJoined += Bot_UserJoined;
            bot.UserLeft += Bot_UserLeft;
            bot.ReactionAdded += Bot_ReactionAdded;
            bot.ShardConnected += Bot_ShardConnected;
            bot.ShardDisconnected += Bot_ShardDisconnected;
        }
        public static void UnRegisterEvents()
        {
            bot.MessageReceived -= CommandManager.Bot_MessageReceived;
            bot.JoinedGuild -= Bot_JoinedGuild;
            bot.LeftGuild -= Bot_LeftGuild;
            bot.UserJoined -= Bot_UserJoined;
            bot.UserLeft -= Bot_UserLeft;
            bot.ReactionAdded -= Bot_ReactionAdded;
            bot.ShardConnected -= Bot_ShardConnected;
            bot.ShardDisconnected -= Bot_ShardDisconnected;
        }

        static async Task Bot_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (!string.IsNullOrEmpty(Configuration.SqlDBHost))
            {
                var gld = bot.Guilds.FirstOrDefault(x => x.TextChannels.FirstOrDefault(z => z.Id == arg2.Id) != null);
                var guild = await Database.GetGuildAsync(gld.Id);
                if (guild != null && gld != null)
                {
                    if (guild.GuildSettings.Features.Pinning)
                    {
                        var dldedmsg = await arg1.GetOrDownloadAsync();
                        int pinboardThreshold = Configuration.PinboardThreshold;
                        int pinboardReactions = 0;
                        if (arg3.Emote.Name == "📌")
                        { pinboardReactions = dldedmsg.Reactions.FirstOrDefault(x => x.Key.Name == "📌").Value.ReactionCount; }
                        if (pinboardReactions >= pinboardThreshold)
                        {
                            var now = dldedmsg.CreatedAt;
                            var dt = DateTime.UtcNow.AddDays(-Configuration.PinboardDateLimit);
                            if ((now - dt).TotalDays > 0)
                            {
                                if (!dldedmsg.IsPinned)
                                {
                                    await dldedmsg.PinAsync();
                                    Logger.AddToLogs(new Models.LogMessage("PinBrd", $"Reached or Over Threshold, pinned a message in: {dldedmsg.Channel.Name} from: {gld.Name}", LogSeverity.Info));
                                }
                            }
                        }
                    }
                }
            }
        }
        
        static async Task Bot_ShardConnected(DiscordSocketClient arg)
        {            
            await arg.SetGameAsync($"{Configuration.Prefix}help | {random.Next(0, bot.Shards.Count) + 1}/{bot.Shards.Count}", type: ActivityType.Listening);
            DogStatsd.Event("shards.connected", $"Shard {arg.ShardId} Connected", alertType: "info");
        }
        static Task Bot_ShardDisconnected(Exception arg1, DiscordSocketClient arg2)
        {
            DogStatsd.Event($"Shard.disconnected", $"Shard {arg2.ShardId} Disconnected, error: {arg1}", alertType: "error");
            return Task.CompletedTask;
        }

        //Start Users
        static async Task Bot_UserJoined(SocketGuildUser arg)
        {
            DogStatsd.Increment("total.users");
            Logger.AddToLogs(new Models.LogMessage("UsrJoin", $"User {arg.Username} joined {arg.Guild.Name}", LogSeverity.Info));
            if (!string.IsNullOrEmpty(Configuration.SqlDBHost))
            {
                var command = new MySqlCommand("INSERT IGNORE INTO `accounts` (`ID`, `username`, `description`) VALUES (@userid , @username, \"I have no description\");");
                command.Parameters.AddWithValue("@username", arg.Username.Replace("\"","\\").Replace("\'","\\'"));
                command.Parameters.AddWithValue("@userid", arg.Id);
                await Database.NonQueryAsync(command);
                
                string autorole = Convert.ToString(await Database.GetSingleAsync(new MySqlCommand("SELECT `autojoinrole` FROM `guild` WHERE `id` = "+arg.Guild.Id)));

                if (!String.IsNullOrEmpty(autorole))
                {
                    var joinroleid = Convert.ToUInt64(autorole);
                    var joinrole = arg.Guild.GetRole(joinroleid);
                    await arg.AddRoleAsync(joinrole);
                    Logger.AddToLogs(new Models.LogMessage("UsrJoin", $"Gave user {arg.Username}, the automatic role as per request of {arg.Guild.Name}.", LogSeverity.Info));
                }

                string welcomemessage = await Database.GetSingleAsync(new MySqlCommand("SELECT `joinmessage` FROM `guild` WHERE `id` = " + arg.Guild.Id));
                if (!String.IsNullOrEmpty(welcomemessage))
                {
                    var channel = arg.Guild.GetTextChannel(Convert.ToUInt64(await Database.GetSingleAsync(new MySqlCommand("SELECT `userjoinchan` FROM `guild` WHERE `id` = " + arg.Guild.Id))));
                    welcomemessage = welcomemessage.Replace("-m", "**"+arg.Mention+"**");
                    welcomemessage = welcomemessage.Replace("-s", "**" + arg.Guild.Name + "**");
                    welcomemessage = welcomemessage.Replace("-uc", Convert.ToString(arg.Guild.MemberCount));
                    welcomemessage = welcomemessage.Replace("-u", "**" + arg.Username + "**");
                    await MessageHandler.SendChannelAsync(channel,welcomemessage);
                }
            }                
        }
        static async Task Bot_UserLeft(SocketGuildUser arg)
        {
            DogStatsd.Decrement("total.users");
            Logger.AddToLogs(new Models.LogMessage("UsrLeft", $"User {arg.Username} just left {arg.Guild.Name}", LogSeverity.Info));
            if (!string.IsNullOrEmpty(Configuration.SqlDBHost))
            {
                var guild = await Database.GetGuildAsync(arg.Guild.Id);
                string leavemessage = guild.LeaveMessage;
                if(!String.IsNullOrEmpty(guild.LeaveMessage))
                {
                    var channel = arg.Guild.GetTextChannel(guild.UserLeaveChannel);
                    leavemessage = leavemessage.Replace("-m", "**" + arg.Mention + "**");
                    leavemessage = leavemessage.Replace("-s", "**" + guild.Name + "**");
                    leavemessage = leavemessage.Replace("-uc", Convert.ToString(arg.Guild.MemberCount));
                    leavemessage = leavemessage.Replace("-u", "**" + arg.Username + "**");
                    await MessageHandler.SendChannelAsync(channel, leavemessage);
                }
                if(bot.GetUser(arg.Id)==null)
                {
                    await Database.DropUserAsync(arg);
                }
            }
        }
        //End Users

        //Start Guilds
        static async Task Bot_LeftGuild(SocketGuild arg)
        {
            DogStatsd.Increment("guilds.left");
            if (!string.IsNullOrEmpty(Configuration.SqlDBHost))
            {
                var command = new MySqlCommand("DELETE FROM guild WHERE id = @guildid");
                command.Parameters.AddWithValue("@guildid", arg.Id);
                await Database.NonQueryAsync(command);
            }
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                CheckGuildUsersAsync(arg).Wait();
            }).Start();
            Logger.AddToLogs(new Models.LogMessage("LeftGld", $"Left a guild", LogSeverity.Info));
        }
        static Task Bot_JoinedGuild(SocketGuild arg)
        {
            DogStatsd.Increment("guilds.joined");
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                PopulateSpecificGuild(arg).Wait();
            }).Start();
            return Task.CompletedTask;
        }
            
        //End Guilds
        
        public static async Task PopulateGuilds()
        {
            foreach (var guild in bot.Guilds)
            { await PopulateSpecificGuild(guild).ConfigureAwait(false); }
        }
        public static async Task PopulateSpecificGuild(SocketGuild guild)
        {
            if (await Database.GetGuildAsync(guild.Id) != null)
            {
                var gcmd = new MySqlCommand("INSERT IGNORE INTO `guild` (`ID`,`name`,`prefix`) VALUES ");
                gcmd.CommandText += $"( {guild.Id} , \"{guild.Name.Replace("\"", "\\").Replace("\'", "\\'")}\" ,\"{Bot.Configuration.Prefix}\" )";
                await Database.NonQueryAsync(gcmd);

                //Configures Modules
                gcmd = new MySqlCommand($"INSERT IGNORE INTO `guildcommandmodules` (`ID`,`Accounts`,`Actions`,`Admin`,`Fun`,`Help`,`Information`,`Search`,`Stats`) VALUES ( {guild.Id} , 1, 1, 1, 1, 1, 1, 1, 1 )");
                await Database.NonQueryAsync(gcmd);

                //Configures Settings
                gcmd = new MySqlCommand($"INSERT IGNORE INTO `guildfeaturemodules` (`ID`,`Experience`,`UserJoinLeave`) VALUES ( {guild.Id} , 0, 0)");
                await Database.NonQueryAsync(gcmd);

                Logger.AddToLogs(new Models.LogMessage("IsrtGld", $"Inserted {guild.Name}!", LogSeverity.Info));

                await PopulateEntireGuildUsers(guild).ConfigureAwait(false);
            }
        }
        public static async Task PopulateEntireGuildUsers(SocketGuild guild)
        {            
            await guild.DownloadUsersAsync();
            foreach (var user in guild.Users)
            {
                await Database.InsertUserAsync(user);
            }
        }
        public static async Task CheckGuildUsersAsync(SocketGuild guild)
        {
            foreach(var user in guild.Users)
            {
                if(bot.GetUser(user.Id)==null)
                {
                    await Database.DropUserAsync(user);
                }
            }
        }
    }
}
