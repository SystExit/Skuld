using System.Threading.Tasks;
using Discord.WebSocket;
using Skuld.Tools;
using Discord;
using System.Linq;
using System;
using MySql.Data.MySqlClient;
using System.Threading;
using Skuld.Models;
using System.Collections.Generic;

namespace Skuld.Events
{
    public class DiscordEvents : Bot
    {
        private static string SqlHost = Config.Load().SqlDBHost;
        private static Dictionary<int, string> StarEmotes = new Dictionary<int, string>()
        {
            {5, "⭐" },
            {10, "💫" },
            {15, "🌟" },
            {20, "✨" },
            {25, "🌠" }
        };
        public static void RegisterEvents()
        {
            /*All Events needed for running Skuld*/
            bot.MessageReceived += CommandManager.Bot_MessageReceived;
            bot.JoinedGuild += Bot_JoinedGuild;
            bot.LeftGuild += Bot_LeftGuild;
            bot.UserJoined += Bot_UserJoined;
            bot.UserLeft += Bot_UserLeft;
            bot.UserBanned += Bot_UserBanned;
            bot.ReactionAdded += Bot_ReactionAdded;
            bot.ReactionRemoved += Bot_ReactionRemoved;
            bot.RoleCreated += Bot_RoleCreated;
            bot.RoleUpdated += Bot_RoleUpdated;
            bot.RoleDeleted += Bot_RoleDeleted;
            bot.UserUnbanned += Bot_UserUnbanned;
            bot.ChannelCreated += Bot_ChannelCreated;
            bot.ChannelDestroyed += Bot_ChannelDestroyed;
            bot.ChannelUpdated += Bot_ChannelUpdated;
            bot.GuildUpdated += Bot_GuildUpdated;
            foreach(var shard in bot.Shards)
                shard.Connected += Shard_Connected;
        }
        public static void UnRegisterEvents()
        {
            bot.MessageReceived -= CommandManager.Bot_MessageReceived;
            bot.JoinedGuild -= Bot_JoinedGuild;
            bot.LeftGuild -= Bot_LeftGuild;
            bot.UserJoined -= Bot_UserJoined;
            bot.UserLeft -= Bot_UserLeft;
            bot.UserBanned -= Bot_UserBanned;
            bot.ReactionAdded -= Bot_ReactionAdded;
            bot.ReactionRemoved -= Bot_ReactionRemoved;
            bot.RoleCreated -= Bot_RoleCreated;
            bot.RoleUpdated -= Bot_RoleUpdated;
            bot.RoleDeleted -= Bot_RoleDeleted;
            bot.UserUnbanned -= Bot_UserUnbanned;
            bot.ChannelCreated -= Bot_ChannelCreated;
            bot.ChannelDestroyed -= Bot_ChannelDestroyed;
            bot.ChannelUpdated -= Bot_ChannelUpdated;
            bot.GuildUpdated -= Bot_GuildUpdated;
            foreach (var shard in bot.Shards)
                shard.Connected -= Shard_Connected;
        }

        private static async Task Bot_ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if(!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {

            }
        }

        private static async Task Bot_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                SocketGuild Gld = null;                
                foreach(var guild in bot.Guilds)
                {
                    if(guild.Channels.Where(x=>x.Id == arg2.Id).FirstOrDefault()!=null)
                    {
                        Gld = guild;
                        break;
                    }
                }
                if(Gld!=null)
                {
                    var Guild = await SqlTools.GetGuild(Gld.Id);
                    if (arg1.HasValue)
                    {
                        int StarboardThreshold = Config.Load().StarboardThreshold;
                        int StarboardReactions = 0;
                        int PinboardThreshold = Config.Load().PinboardThreshold;
                        int PinboardReactions = 0;
                        var Starboard = new Starboard();
                        var reader = await Sql.GetAsync(new MySqlCommand("SELECT * FROM `starboard` WHERE `MessageID` = " + arg1.Value.Id));
                        if(reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                Starboard.GuildID = Convert.ToUInt64(reader["guildid"]);
                                Starboard.ChannelID = Convert.ToUInt64(await Sql.GetSingleAsync(new MySqlCommand("SELECT `starboardchannel` FROM guild WHERE ID = "+Guild.ID)));
                                Starboard.MessageID = Convert.ToUInt64(reader["messageid"]);
                                Starboard.Stars = Convert.ToInt32(reader["stars"]);
                                Starboard.DateAdded = Convert.ToString(reader["added"]);
                                Starboard.Locked = Convert.ToBoolean(reader["locked"]);
                            }
                        }
                        reader.Close();
                        await Sql.getconn.CloseAsync();
                        foreach (var reactions in arg1.Value.Reactions)
                        {
                            if (reactions.Key.Name == "⭐")
                                StarboardReactions = reactions.Value.ReactionCount;
                            if (reactions.Key.Name == "📌")
                                PinboardReactions = reactions.Value.ReactionCount;
                        }
                        if(StarboardReactions >= StarboardThreshold)
                        {
                            var now = DateTime.UtcNow;
                            var dt = DateTime.UtcNow.AddDays(-Config.Load().StarboardDateLimit);
                            if ((now - dt).TotalDays < Config.Load().StarboardDateLimit)
                            {
                                var resp = await Sql.GetSingleAsync(new MySqlCommand("SELECT `messageid` FROM `starboard` WHERE `messageid` = "+arg1.Value.Id));
                                if (!String.IsNullOrEmpty(resp))
                                {
                                    if (arg1.Value.Id == Convert.ToUInt64(resp))
                                    {
                                        string react = "";
                                        foreach (var emote in StarEmotes)
                                            if (StarboardReactions >= emote.Key)
                                                react = emote.Value;
                                        string msg = react + " Channel: <#" + arg1.Value.Channel.Id + "> ID: " + arg1.Value.Id;
                                        var smsg = await Gld.GetTextChannel(Starboard.ChannelID).GetMessageAsync(Starboard.MessageID) as IUserMessage;
                                        var embd = smsg.Embeds.FirstOrDefault();
                                        await smsg.ModifyAsync(x =>
                                        {
                                            x.Content = msg;
                                        });
                                        await Sql.InsertAsync(new MySqlCommand($"UPDATE `starboard` `Stars` = {StarboardReactions}"));
                                    }
                                }
                                else
                                {
                                    string reaction = "";
                                    foreach (var emote in StarEmotes)
                                        if (StarboardReactions >= emote.Key)
                                            reaction = emote.Value;
                                    string message = reaction + " Channel: <#" + arg1.Value.Channel.Id + "> ID: " + arg1.Value.Id;
                                    var embed = new EmbedBuilder()
                                    {
                                        Author = new EmbedAuthorBuilder()
                                        {
                                            Name = arg1.Value.Author.Username,
                                            IconUrl = arg1.Value.Author.GetAvatarUrl()
                                        },
                                        Description = arg1.Value.Content,
                                        Timestamp = arg1.Value.CreatedAt
                                    };
                                    var messg = await MessageHandler.SendChannel(Gld.GetTextChannel(Starboard.ChannelID), message, embed);
                                    await Sql.InsertAsync(new MySqlCommand($"INSERT INTO `starboard` (`GuildID`,`MessageID`,`Stars`,`Added`,`Locked`) VALUES ({Gld.Id},{messg.Id},{StarboardReactions},\"{DateTime.UtcNow}\",0);"));
                                }
                            }
                            else
                            {
                                if (!String.IsNullOrEmpty(await Sql.GetSingleAsync(new MySqlCommand("SELECT `messageid` FROM `starboard` WHERE `messageid` = " + arg1.Value.Id))))
                                    await Sql.InsertAsync(new MySqlCommand("UPDATE `starboard` SET `locked` = 1 WHERE `messageid` = " + arg1.Value.Id));
                            }
                        }
                        if(PinboardReactions >= PinboardThreshold)
                        {
                            var now = DateTime.UtcNow;
                            var dt = DateTime.UtcNow.AddDays(-Config.Load().PinboardDateLimit);
                            if((now-dt).TotalDays<Config.Load().PinboardDateLimit)
                            {
                                if (!arg1.Value.IsPinned)
                                    await arg1.Value.PinAsync();
                            }
                        }
                    }
                }
            }
        }


        private static async Task Shard_Connected()
        {
            bool[] connected = new bool[bot.Shards.Count];
            foreach (var shard in bot.Shards)
            {
                if (shard.ConnectionState == ConnectionState.Connected)
                    connected[shard.ShardId] = true;
                else
                {
                    connected[shard.ShardId] = false;
                }
            }
            string game = Bot.bot.Game.ToString();
            string shardgame = $"{Config.Load().Prefix}help | {Bot.random.Next(0, bot.Shards.Count) + 1}/{bot.Shards.Count}";
            if (!String.IsNullOrEmpty(game)) { }
            else
            {
                await bot.SetGameAsync(shardgame);
            }
        }

        //Start Users
        private static async Task Bot_UserJoined(SocketGuildUser arg)
        {
            Logs.Add(new Models.LogMessage("UsrJoin", $"User {arg.Username} joined {arg.Guild.Name}", LogSeverity.Info));
            if (!string.IsNullOrEmpty(SqlHost))
            {
                MySqlCommand command = new MySqlCommand("INSERT IGNORE INTO `accounts` (`ID`, `username`, `description`) VALUES (@userid , @username, \"I have no description\");");
                command.Parameters.AddWithValue("@username", arg.Username.Replace("\"","\\").Replace("\'","\\'")+"#"+arg.DiscriminatorValue);
                command.Parameters.AddWithValue("@userid", arg.Id);
                await Sql.InsertAsync(command);

                var guild = arg.Guild;
                var Guild = await SqlTools.GetGuild(arg.Guild.Id);
                string autorole = Convert.ToString(Guild.AutoJoinRole);

                if (!String.IsNullOrEmpty(autorole))
                {

                    ulong joinroleid = Convert.ToUInt64(autorole);
                    var joinrole = guild.GetRole(joinroleid);
                    await arg.AddRoleAsync(joinrole);
                    Logs.Add(new Models.LogMessage("UsrJoin", $"Gave user {arg.Username}, the automatic role as per request of {guild.Name}.", LogSeverity.Info));

                }

                string welcomemessage = Guild.JoinMessage;
                if(!String.IsNullOrEmpty(welcomemessage))
                {

                    var channel = guild.GetTextChannel(Guild.UserLeaveChannel);
                    welcomemessage = welcomemessage.Replace("-m", "**"+arg.Mention+"**");
                    welcomemessage = welcomemessage.Replace("-s", "**" + guild.Name + "**");
                    welcomemessage = welcomemessage.Replace("-uc", Convert.ToString(guild.MemberCount));
                    welcomemessage = welcomemessage.Replace("-u", "**" + arg.Username + "**");
                    await MessageHandler.SendChannel(channel,welcomemessage);

                }
            }                
        }
        private static async Task Bot_UserLeft(SocketGuildUser arg)
        {
            Logs.Add(new Models.LogMessage("UsrLeft", $"User {arg.Username} just left {arg.Guild.Name}", LogSeverity.Info));
            if (!string.IsNullOrEmpty(SqlHost))
            {
                var Guild = await SqlTools.GetGuild(arg.Guild.Id);
                string leavemessage = Guild.LeaveMessage;
                if(!String.IsNullOrEmpty(Guild.LeaveMessage))
                {
                    var channel = arg.Guild.GetTextChannel(Guild.UserLeaveChannel);
                    leavemessage = leavemessage.Replace("-m", "**" + arg.Mention + "**");
                    leavemessage = leavemessage.Replace("-s", "**" + Guild.Name + "**");
                    leavemessage = leavemessage.Replace("-uc", Convert.ToString(arg.Guild.MemberCount));
                    leavemessage = leavemessage.Replace("-u", "**" + arg.Username + "**");
                    await MessageHandler.SendChannel(channel, leavemessage);
                }
            }
        }
        //End Users

        //Start Guilds
        private static async Task Bot_LeftGuild(SocketGuild arg)
        {
            if (!string.IsNullOrEmpty(SqlHost))
            {
                MySqlCommand command = new MySqlCommand("DELETE FROM guild WHERE id = @guildid");
                command.Parameters.AddWithValue("@guildid", arg.Id);
                await Sql.InsertAsync(command);
            }
            Logs.Add(new Models.LogMessage("LeftGld", $"Left a guild", LogSeverity.Info));
        }

        private static async Task Bot_JoinedGuild(SocketGuild arg)=>
            await PopulateSpecificGuild(arg);
        //End Guilds

        //Start ModLog Stuff
        private static async Task Bot_GuildUpdated(SocketGuild arg1, SocketGuild arg2)
        {

        }

        private static async Task Bot_ChannelUpdated(SocketChannel arg1, SocketChannel arg2)
        {
            if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                SocketGuild Gld = null;
                foreach (var guild in bot.Guilds)
                {
                    if ((guild.Channels.Where(x => x.Id == arg1.Id).FirstOrDefault() != null&& guild.Channels.Where(x => x.Id == arg1.Id).FirstOrDefault()!=null))
                    {
                        Gld = guild;
                        break;
                    }
                }
                var Guild = await SqlTools.GetGuild(Gld.Id);
                if (Guild.GuildSettings.Features.GuildChannelModification)
                {
                    var chan1 = Gld.GetChannel(arg1.Id);
                    var chan2 = Gld.GetChannel(arg2.Id);
                    string desc = $"Channel `{chan1.Name}` was modified. Modification(s):";
                    if (chan1.Name != chan2.Name)
                        desc += $"\nName: {chan1.Name} => {chan2.Name}";
                    if (chan1.Position != chan2.Position)
                        desc += $"\nPosition: {chan1.Position} => {chan2.Position}";
                    if (chan1.PermissionOverwrites != chan2.PermissionOverwrites)
                        desc += $"\nOverwrites were changed.";
                    var embed = new EmbedBuilder()
                    {
                        Title = "Channel Modified",
                        Description = desc,
                        Color = new Color(243, 255, 33),
                        Timestamp = DateTime.UtcNow
                    };
                    await SendModMessage(Gld.GetTextChannel(Guild.AuditChannel), "", embed);
                }
            }
        }

        private static async Task Bot_ChannelDestroyed(SocketChannel arg)
        {
            if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                SocketGuild Gld = null;
                SocketGuildChannel Chan = null;
                foreach (var guild in bot.Guilds)
                {
                    if (guild.Channels.Where(x => x.Id == arg.Id).FirstOrDefault() != null)
                    {
                        Gld = guild;
                        Chan = guild.Channels.Where(x => x.Id == arg.Id).FirstOrDefault();
                        break;
                    }
                }
                var Guild = await SqlTools.GetGuild(Gld.Id);
                if (Guild.GuildSettings.Features.GuildChannelModification)
                {
                    var embed = new EmbedBuilder()
                    {
                        Title = "Channel Deleted",
                        Description = $"Channel `{Chan.Name}` was deleted",
                        Color = new Color(255, 0, 0),
                        Timestamp = DateTime.UtcNow
                    };
                    await SendModMessage(Gld.GetTextChannel(Guild.AuditChannel), "", embed);
                }
            }
        }

        private static async Task Bot_ChannelCreated(SocketChannel arg)
        {
            if(!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                SocketGuild Gld = null;
                SocketGuildChannel Chan = null;
                foreach(var guild in bot.Guilds)
                {
                    if(guild.Channels.Where(x=>x.Id == arg.Id).FirstOrDefault() != null)
                    {
                        Gld = guild;
                        Chan = guild.Channels.Where(x => x.Id == arg.Id).FirstOrDefault();
                        break;
                    }
                }
                var Guild = await SqlTools.GetGuild(Gld.Id);
                if(Guild.GuildSettings.Features.GuildChannelModification)
                {
                    var embed = new EmbedBuilder()
                    {
                        Title = "Channel Created",
                        Description = $"Channel `{Chan.Name}` was created",
                        Color = new Color(243, 255, 33),
                        Timestamp = arg.CreatedAt
                    };
                    await SendModMessage(Gld.GetTextChannel(Guild.AuditChannel), "", embed);
                }
            }
        }

        private static async Task Bot_UserBanned(SocketUser arg1, SocketGuild arg2)
        {
            if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                var Guild = await SqlTools.GetGuild(arg2.Id);
                if (Guild.GuildSettings.Features.UserBanEvents)
                {
                    var bans = await arg2.GetBansAsync();
                    var audit = bans.Where(x => x.User.Id == arg1.Id).FirstOrDefault();
                    var embed = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder()
                        {
                            Name = arg1.Username + "#" + arg1.DiscriminatorValue,
                            IconUrl = arg1.GetAvatarUrl()
                        },
                        Title = "User Banned",
                        Description = $"{arg1.Username}#{arg1.DiscriminatorValue} ({arg1.Id}) has just been banned from {arg2.Name}.\nReason:\n`{audit.Reason}`",
                        Timestamp = DateTime.UtcNow,
                        Color = new Color(255, 0, 0)
                    };
                    await SendModMessage(arg2.GetTextChannel(Guild.AuditChannel), "", embed);
                }
            }
        }

        private static async Task Bot_UserUnbanned(SocketUser arg1, SocketGuild arg2)
        {
            if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                var Guild = await SqlTools.GetGuild(arg2.Id);
                if (Guild.GuildSettings.Features.UserBanEvents)
                {
                    var embed = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder()
                        {
                            Name = arg1.Username + "#" + arg1.DiscriminatorValue,
                            IconUrl = arg1.GetAvatarUrl()
                        },
                        Title = "User Unbanned",
                        Description = $"{arg1.Username}#{arg1.DiscriminatorValue} ({arg1.Id}) has just been unbanned from {arg2.Name}.",
                        Timestamp = DateTime.UtcNow,
                        Color = new Color(13, 229, 222)
                    };
                    await SendModMessage(arg2.GetTextChannel(Guild.AuditChannel), "", embed);
                }
            }
        }

        private static async Task Bot_RoleDeleted(SocketRole arg)
        {

        }

        private static async Task Bot_RoleUpdated(SocketRole arg1, SocketRole arg2)
        {

        }

        private static async Task Bot_RoleCreated(SocketRole arg)
        {

        }

        private static async Task SendModMessage(SocketTextChannel Channel, string Message, Embed Embed)=>
            await MessageHandler.SendChannel(Channel, Message, Embed);

        //End ModLog Stuff

        public static async Task PopulateGuilds()
        {
            foreach (var guild in bot.Guilds)
                await PopulateSpecificGuild(guild);
        }
        public static async Task PopulateSpecificGuild(SocketGuild guild)
        {
            var Guild = await SqlTools.GetGuild(guild.Id);
            if (Guild == null)
            {
                MySqlCommand gcmd = new MySqlCommand("INSERT IGNORE INTO `guild` (`ID`,`name`,`prefix`,`logenabled`) VALUES ");
                int nonbots = guild.Users.Where(x => x.IsBot == false).Count();
                gcmd.CommandText += $"( {guild.Id} , \"{guild.Name.Replace("\"", "\\").Replace("\'", "\\'")}\" ,\"{Config.Load().Prefix}\" , 0 )";
                await Sql.InsertAsync(gcmd);

                //Configures Modules
                gcmd = new MySqlCommand($"INSERT IGNORE INTO `guildcommandmodules` (`ID`,`Accounts`,`Actions`,`Admin`,`Fun`,`Help`,`Information`,`Search`,`Stats) VALUES ( {guild.Id} , 1, 1, 1, 1, 1, 1, 1, 1 )");
                await Sql.InsertAsync(gcmd);

                //Configures Settings
                gcmd = new MySqlCommand($"INSERT IGNORE INTO `guildfeaturemodules` (`ID`,`Starboard`,`Experience`,`UserJoinLeave`,`UserModification`,`UserBanEvents`,`GuildModification`,`GuildChannelModification`,`GuildRoleModification`) VALUES ( {guild.Id} , 0, 0, 0, 0, 0, 0, 0, 0 )");
                await Sql.InsertAsync(gcmd);

                Logs.Add(new Models.LogMessage("IsrtGld", $"Inserted {guild.Name}!", LogSeverity.Info));
                await PopulateEntireGuildUsers(guild);
            }
            else { }
        }
        private static async Task PopulateEntireGuildUsers(SocketGuild guild)
        {
            int usercount = 0;
            MySqlCommand ucmd = new MySqlCommand("INSERT IGNORE INTO `accounts` (`ID`, `username`, `description`) VALUES ");
            
            await guild.DownloadUsersAsync();
            foreach (var user in guild.Users)
            {
                if (user.IsBot == false && user != null)
                {
                    var cmd = new MySqlCommand("select ID from accounts where ID = @userid");
                    cmd.Parameters.AddWithValue("@userid", user.Id);
                    var resp = await Sql.GetSingleAsync(cmd);
                    if (String.IsNullOrEmpty(resp))
                    {
                        ucmd.CommandText += $"( {user.Id} , \"{user.Username.Replace("\"", "\\").Replace("\'", "\\'")}#{user.DiscriminatorValue}\", \"I have no description\" ), ";
                        Logs.Add(new Models.LogMessage("IsrtUsr", $"Added {user.Username} to queue", LogSeverity.Info));
                        usercount = usercount + 1;
                    }
                }
            }
            if (ucmd.CommandText.Contains("#"))
            {
                ucmd.CommandText = ucmd.CommandText.Substring(0, ucmd.CommandText.Length - 2);
                Logs.Add(new Models.LogMessage("IsrtUsr", $"Added {usercount} Users to the database", LogSeverity.Info));
                await Sql.InsertAsync(ucmd);
            }
            Logs.Add(new Models.LogMessage("IsrtUsr", $"Finished!", LogSeverity.Info));
        }
    }
}
