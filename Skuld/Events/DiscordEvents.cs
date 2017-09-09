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
using System.Diagnostics;

namespace Skuld.Events
{
    public class DiscordEvents : Bot
    {
        private static string SqlHost = Config.Load().SqlDBHost;
        private static Dictionary<int, string> StarEmotes = new Dictionary<int, string>()
        {
            {1, "⭐" },
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
            if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                SocketGuild Gld = bot.Guilds.Where(x => x.TextChannels.Where(z => z.Id == arg2.Id).FirstOrDefault() != null).FirstOrDefault();
                var Guild = await SqlTools.GetGuild(Gld.Id);
                if(Guild.GuildSettings.Features.Starboard)
                {
                    var Dldedmsg = await arg1.GetOrDownloadAsync();
                    int StarboardThreshold = Config.Load().StarboardThreshold;
                    var Starboard = new Starboard();
                    var reader = await SqlTools.GetAsync(new MySqlCommand("SELECT * FROM `starboard` WHERE `MessageID` = " + Dldedmsg.Id));
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            if (reader["guildid"] != DBNull.Value)
                                Starboard.GuildID = Convert.ToUInt64(reader["guildid"]);
                            else
                                Starboard.GuildID = 0;
                            if (reader["messageid"] != DBNull.Value)
                                Starboard.MessageID = Convert.ToUInt64(reader["messageid"]);
                            else
                                Starboard.MessageID = 0;
                            if (reader["originalmessageid"] != DBNull.Value)
                                Starboard.OriginalMessageID = Convert.ToUInt64(reader["originalmessageid"]);
                            else
                                Starboard.OriginalMessageID = 0;
                            if (reader["stars"] != DBNull.Value)
                                Starboard.Stars = Convert.ToInt32(reader["stars"]);
                            else
                                Starboard.Stars = 0;
                            if (reader["added"] != DBNull.Value)
                                Starboard.DateAdded = Convert.ToString(reader["added"]);
                            else
                                Starboard.DateAdded = null;
                        }
                    }
                    reader.Close();
                    await SqlTools.getconn.CloseAsync();
                    Starboard.ChannelID = Convert.ToUInt64(await SqlTools.GetSingleAsync(new MySqlCommand("SELECT `starboardchannel` FROM `guild` WHERE ID = " + Guild.ID)));
                    if (arg3.Emote.Name == new Emoji("⭐").Name)
                        Starboard.Stars = Starboard.Stars - 1;
                    if (Dldedmsg.Id == Starboard.MessageID)
                    {
                        string react = StarEmotes.OrderByDescending(x => x.Key).Where(x => x.Key <= Starboard.Stars).FirstOrDefault().Value;
                        var smsg = await Gld.GetTextChannel(Starboard.ChannelID).GetMessageAsync(Starboard.MessageID) as IUserMessage;
                        var content = smsg.Content.Split(' ').Skip(2);
                        string msg = react + " " + Starboard.Stars + " " + string.Join(" ", content);
                        var embd = smsg.Embeds.FirstOrDefault();
                        await smsg.ModifyAsync(x =>
                        {
                            x.Content = msg;
                        });
                        await SqlTools.InsertAsync(new MySqlCommand($"UPDATE `starboard` SET `Stars` = {Starboard.Stars} WHERE `messageid` = " + Dldedmsg.Id));
                    }
                }
            }
        }

        private static async Task Bot_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                SocketGuild Gld = bot.Guilds.Where(x => x.TextChannels.Where(z => z.Id == arg2.Id).FirstOrDefault() != null).FirstOrDefault();
                var Guild = await SqlTools.GetGuild(Gld.Id);
                if (Guild != null && Gld != null)
                {
                    if (Guild.GuildSettings.Features.Starboard)
                    {
                        var Dldedmsg = await arg1.GetOrDownloadAsync();
                        int StarboardThreshold = Config.Load().StarboardThreshold;
                        int StarboardReactions = Dldedmsg.Reactions.Where(x => x.Key.Name == "⭐").FirstOrDefault().Value.ReactionCount;
                        var temp = await SqlTools.GetSingleAsync(new MySqlCommand("SELECT OriginalMessageID FROM starboard WHERE OriginalMessageID = " + Dldedmsg.Id));
                        var temp2 = await SqlTools.GetSingleAsync(new MySqlCommand("SELECT MessageID FROM starboard WHERE MessageID = " + Dldedmsg.Id));
                        if (String.IsNullOrEmpty(temp)&&String.IsNullOrEmpty(temp2))
                        {
                            if (StarboardReactions >= StarboardThreshold)
                            {
                                var now = Dldedmsg.CreatedAt;
                                var dt = DateTime.UtcNow.AddDays(-Config.Load().StarboardDateLimit);
                                if ((now - dt).TotalDays > 0)
                                {
                                    string react = StarEmotes.OrderByDescending(x => x.Key).Where(x => x.Key <= StarboardReactions).FirstOrDefault().Value;
                                    string message = react+" " + StarboardReactions + " Channel: <#" + Dldedmsg.Channel.Id + "> ID: " + Dldedmsg.Id;
                                    var embed = new EmbedBuilder()
                                    {
                                        Author = new EmbedAuthorBuilder()
                                        {
                                            Name = Dldedmsg.Author.Username,
                                            IconUrl = Dldedmsg.Author.GetAvatarUrl()
                                        },
                                        Description = Dldedmsg.Content,
                                        Timestamp = Dldedmsg.CreatedAt
                                    };
                                    var attachment = Dldedmsg.Attachments.FirstOrDefault();
                                    if (attachment != null)
                                    {
                                        if (IsImageExtension(attachment.Url))
                                            embed.ImageUrl = attachment.Url;
                                        else
                                            embed.Description = embed.Description + "\n" + attachment.Url;
                                    }
                                    var messg = await MessageHandler.SendChannel(bot.GetGuild(Guild.ID).GetTextChannel(Guild.StarboardChannel), message, embed.Build());
                                    await SqlTools.InsertAsync(new MySqlCommand($"INSERT INTO `starboard` (`GuildID`,`MessageID`,`OriginalMessageID`,`Stars`,`Added`) VALUES ({Gld.Id},{messg.Id},{Dldedmsg.Id},{StarboardReactions},\"{DateTime.UtcNow}\");"));
                                }
                            }
                        }
                        else
                        {
                            var Starboard = new Starboard();
                            var reader = await SqlTools.GetAsync(new MySqlCommand("SELECT * FROM `starboard` WHERE `MessageID` = " + temp2));
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    if (reader["guildid"] != DBNull.Value)
                                        Starboard.GuildID = Convert.ToUInt64(reader["guildid"]);
                                    else
                                        Starboard.GuildID = 0;
                                    if (reader["messageid"] != DBNull.Value)
                                        Starboard.MessageID = Convert.ToUInt64(reader["messageid"]);
                                    else
                                        Starboard.MessageID = 0;
                                    if (reader["originalmessageid"] != DBNull.Value)
                                        Starboard.OriginalMessageID = Convert.ToUInt64(reader["originalmessageid"]);
                                    else
                                        Starboard.OriginalMessageID = 0;
                                    if (reader["stars"] != DBNull.Value)
                                        Starboard.Stars = Convert.ToInt32(reader["stars"]);
                                    else
                                        Starboard.Stars = 0;
                                    if (reader["added"] != DBNull.Value)
                                        Starboard.DateAdded = Convert.ToString(reader["added"]);
                                    else
                                        Starboard.DateAdded = null;
                                }
                            }
                            reader.Close();
                            await SqlTools.getconn.CloseAsync();
                            Starboard.ChannelID = Guild.StarboardChannel;
                            string react = StarEmotes.OrderByDescending(x => x.Key).Where(x => x.Key <= Starboard.Stars).FirstOrDefault().Value;
                            var stars = Starboard.Stars;
                            var returnstars = stars + 1;
                            var smsg = await Gld.GetTextChannel(Starboard.ChannelID).GetMessageAsync(Starboard.MessageID) as IUserMessage;
                            var content = smsg.Content.Split(' ').Skip(2);
                            string msg = react + " " + returnstars + " " + string.Join(" ", content);
                            var embd = smsg.Embeds.FirstOrDefault();
                            await smsg.ModifyAsync(x =>
                            {
                                x.Content = msg;
                            });
                            await SqlTools.InsertAsync(new MySqlCommand($"UPDATE `starboard` SET `Stars` = {returnstars} WHERE `messageid` = " + temp2));
                        }
                    }
                    if (Guild.GuildSettings.Features.Pinning)
                    {
                        var Dldedmsg = await arg1.GetOrDownloadAsync();
                        int PinboardThreshold = Config.Load().PinboardThreshold;
                        int PinboardReactions = 0;
                        if (arg3.Emote.Name == "📌")
                            PinboardReactions = Dldedmsg.Reactions.Where(x => x.Key.Name == "📌").FirstOrDefault().Value.ReactionCount;
                        if (PinboardReactions >= PinboardThreshold)
                        {
                            var now = Dldedmsg.CreatedAt;
                            var dt = DateTime.UtcNow.AddDays(-Config.Load().PinboardDateLimit);
                            if ((now - dt).TotalDays > 0)
                            {
                                if (!Dldedmsg.IsPinned)
                                {
                                    await Dldedmsg.PinAsync();
                                    Logs.Add(new Models.LogMessage("PinBrd", $"Reached or Over Threshold, pinned a message in: {Dldedmsg.Channel.Name} from: {Gld.Name}", LogSeverity.Info));
                                }
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
                await SqlTools.InsertAsync(command);
                
                string autorole = Convert.ToString(await SqlTools.GetSingleAsync(new MySqlCommand("SELECT `autojoinrole` FROM `guild` WHERE `id` = "+arg.Guild.Id)));

                if (!String.IsNullOrEmpty(autorole))
                {

                    ulong joinroleid = Convert.ToUInt64(autorole);
                    var joinrole = arg.Guild.GetRole(joinroleid);
                    await arg.AddRoleAsync(joinrole);
                    Logs.Add(new Models.LogMessage("UsrJoin", $"Gave user {arg.Username}, the automatic role as per request of {arg.Guild.Name}.", LogSeverity.Info));
                }

                string welcomemessage = await SqlTools.GetSingleAsync(new MySqlCommand("SELECT `joinmessage` FROM `guild` WHERE `id` = " + arg.Guild.Id));
                if (!String.IsNullOrEmpty(welcomemessage))
                {

                    var channel = arg.Guild.GetTextChannel(Convert.ToUInt64(await SqlTools.GetSingleAsync(new MySqlCommand("SELECT `userjoinchan` FROM `guild` WHERE `id` = " + arg.Guild.Id))));
                    welcomemessage = welcomemessage.Replace("-m", "**"+arg.Mention+"**");
                    welcomemessage = welcomemessage.Replace("-s", "**" + arg.Guild.Name + "**");
                    welcomemessage = welcomemessage.Replace("-uc", Convert.ToString(arg.Guild.MemberCount));
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
                await SqlTools.InsertAsync(command);
            }
            Logs.Add(new Models.LogMessage("LeftGld", $"Left a guild", LogSeverity.Info));
        }

        private static async Task Bot_JoinedGuild(SocketGuild arg)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                PopulateSpecificGuild(arg).Wait();
            }).Start();
        }
            
    //End Guilds

    //Start ModLog Stuff
    private static async Task Bot_GuildUpdated(SocketGuild arg1, SocketGuild arg2)
        {
            if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                SocketGuild Gld = bot.Guilds.Where(x => x.TextChannels.Where(z => z.Id == arg2.Id).FirstOrDefault() != null).FirstOrDefault();
                var Guild = await SqlTools.GetGuild(Gld.Id);
                if (Guild.GuildSettings.Features.GuildModification)
                {
                    string desc = $"The server was modified. Modification(s):";
                    if (arg1.Name != arg2.Name)
                        desc += $"\nName: {arg1.Name} => {arg2.Name}";
                    if (arg1.IconId != arg2.IconId)
                        desc += $"\nIconUrl: {arg1.IconUrl} => {arg2.IconUrl}\nIconUrl: {arg1.IconId} => {arg2.IconId}";
                    if (arg1.MfaLevel != arg2.MfaLevel)
                        desc += $"\nServer MFA Level: {arg1.MfaLevel} => {arg2.MfaLevel}";
                    if (arg1.AFKChannel != arg2.AFKChannel)
                        desc += $"\nAFK Channel: {arg1.AFKChannel} => {arg2.AFKChannel}";
                    if (arg1.AFKTimeout != arg2.AFKTimeout)
                        desc += $"\nAFK Timeout: {arg1.AFKTimeout} => {arg2.AFKTimeout}";
                    if (arg1.OwnerId!=arg2.OwnerId)
                        desc += $"\nOwner changed: {arg1.Owner} => {arg2.Owner}";
                    if (arg1.SplashId!=arg2.SplashId)
                        desc += $"\nSplash Url: {arg1.SplashUrl} => {arg2.SplashUrl}\nSplash ID: {arg1.SplashId} => {arg2.SplashId}";
                    if (arg1.VoiceRegionId!= arg2.VoiceRegionId)
                        desc += $"\nVoice Region: {arg1.VoiceRegionId} => {arg2.VoiceRegionId}";
                    if (arg1.VerificationLevel!=arg2.VerificationLevel)
                        desc += $"\nVerification Level: {arg1.VerificationLevel} => {arg2.VerificationLevel}";
                    var embed = new EmbedBuilder()
                    {
                        Title = "Server Modified",
                        Description = desc,
                        Color = new Color(243, 255, 33),
                        Timestamp = DateTime.UtcNow
                    };
                    await SendModMessage(Gld.GetTextChannel(Guild.AuditChannel), "", embed.Build());
                }
            }
        }

        private static async Task Bot_ChannelUpdated(SocketChannel arg1, SocketChannel arg2)
        {
            if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                SocketGuild Gld = bot.Guilds.Where(x => x.TextChannels.Where(z => z.Id == arg2.Id).FirstOrDefault() != null).FirstOrDefault();
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
                    await SendModMessage(Gld.GetTextChannel(Guild.AuditChannel), "", embed.Build());
                }
            }
        }

        private static async Task Bot_ChannelDestroyed(SocketChannel arg)
        {
            if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                SocketGuild Gld = bot.Guilds.Where(x => x.TextChannels.Where(z => z.Id == arg.Id).FirstOrDefault() != null).FirstOrDefault();
                SocketGuildChannel Chan = null;
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
                    await SendModMessage(Gld.GetTextChannel(Guild.AuditChannel), "", embed.Build());
                }
            }
        }

        private static async Task Bot_ChannelCreated(SocketChannel arg)
        {
            if(!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                SocketGuild Gld = bot.Guilds.Where(x => x.TextChannels.Where(z => z.Id == arg.Id).FirstOrDefault() != null).FirstOrDefault();
                SocketGuildChannel Chan = null;
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
                    await SendModMessage(Gld.GetTextChannel(Guild.AuditChannel), "", embed.Build());
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
                    await SendModMessage(arg2.GetTextChannel(Guild.AuditChannel), "", embed.Build());
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
                    await SendModMessage(arg2.GetTextChannel(Guild.AuditChannel), "", embed.Build());
                }
            }
        }

        private static async Task Bot_RoleDeleted(SocketRole arg)
        {
            if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                SocketGuild Gld = bot.Guilds.Where(x => x.Roles.Where(z => z.Id == arg.Id).FirstOrDefault() != null).FirstOrDefault();
                var Guild = await SqlTools.GetGuild(Gld.Id);
                if (Guild.GuildSettings.Features.GuildRoleModification)
                {
                    string desc = $"Role `{arg.Name}` was deleted";
                    var embed = new EmbedBuilder()
                    {
                        Title = "Role deleted",
                        Description = desc,
                        Color = new Color(255, 0, 0),
                        Timestamp = DateTime.UtcNow
                    };
                    await SendModMessage(Gld.GetTextChannel(Guild.AuditChannel), "", embed.Build());
                }
            }
        }

        private static async Task Bot_RoleUpdated(SocketRole arg1, SocketRole arg2)
        {
            if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                SocketGuild Gld = bot.Guilds.Where(x => x.TextChannels.Where(z => z.Id == arg1.Id).FirstOrDefault() != null).FirstOrDefault();
                var Guild = await SqlTools.GetGuild(Gld.Id);
                if (Guild.GuildSettings.Features.GuildRoleModification)
                {
                    string desc = $"Role `{arg1.Name}` was modified. Modification(s):";
                    if (arg1.Name != arg2.Name)
                        desc += $"\nName: {arg1.Name} => {arg2.Name}";
                    if (arg1.Position != arg2.Position)
                        desc += $"\nPosition: {arg1.Position} => {arg2.Position}";
                    if (arg1.IsHoisted != arg2.IsHoisted)
                        desc += $"\nIt is now hoisted.";
                    if (arg1.IsMentionable != arg2.IsMentionable)
                        desc += $"\nIt is now mentionable.";
                    if (arg1.Color.RawValue != arg2.Color.RawValue)
                        desc += $"\nThe colour has changed.";
                    if (arg1.Permissions.RawValue != arg2.Permissions.RawValue)
                        desc += $"\nPermissions have changed.";
                    var embed = new EmbedBuilder()
                    {
                        Title = "Role Modified",
                        Description = desc,
                        Color = new Color(243, 255, 33),
                        Timestamp = DateTime.UtcNow
                    };
                    await SendModMessage(Gld.GetTextChannel(Guild.AuditChannel), "", embed.Build());
                }
            }
        }

        private static async Task Bot_RoleCreated(SocketRole arg)
        {
            if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                SocketGuild Gld = bot.Guilds.Where(x => x.TextChannels.Where(z => z.Id == arg.Id).FirstOrDefault() != null).FirstOrDefault();
                var Guild = await SqlTools.GetGuild(Gld.Id);
                if (Guild.GuildSettings.Features.GuildRoleModification)
                {
                    string desc = $"Role `{arg.Name}` was created";
                    var embed = new EmbedBuilder()
                    {
                        Title = "Role created",
                        Description = desc,
                        Color = new Color(0, 255, 0),
                        Timestamp = DateTime.UtcNow
                    };
                    await SendModMessage(Gld.GetTextChannel(Guild.AuditChannel), "", embed.Build());
                }
            }
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
            MySqlCommand gcmd = new MySqlCommand("INSERT IGNORE INTO `guild` (`ID`,`name`,`prefix`) VALUES ");
            gcmd.CommandText += $"( {guild.Id} , \"{guild.Name.Replace("\"", "\\").Replace("\'", "\\'")}\" ,\"{Config.Load().Prefix}\" )";
            await SqlTools.InsertAsync(gcmd);

            //Configures Modules
            gcmd = new MySqlCommand($"INSERT IGNORE INTO `guildcommandmodules` (`ID`,`Accounts`,`Actions`,`Admin`,`Fun`,`Help`,`Information`,`Search`,`Stats`) VALUES ( {guild.Id} , 1, 1, 1, 1, 1, 1, 1, 1 )");
            await SqlTools.InsertAsync(gcmd);

            //Configures Settings
            gcmd = new MySqlCommand($"INSERT IGNORE INTO `guildfeaturemodules` (`ID`,`Starboard`,`Experience`,`UserJoinLeave`,`UserModification`,`UserBanEvents`,`GuildModification`,`GuildChannelModification`,`GuildRoleModification`) VALUES ( {guild.Id} , 0, 0, 0, 0, 0, 0, 0, 0 )");
            await SqlTools.InsertAsync(gcmd);

            Logs.Add(new Models.LogMessage("IsrtGld", $"Populated {guild.Name} if it doesn't exist!", LogSeverity.Info));
            await PopulateEntireGuildUsers(guild);
        }
        public static async Task PopulateEntireGuildUsers(SocketGuild guild)
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
                    var resp = await SqlTools.GetSingleAsync(cmd);
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
                await SqlTools.InsertAsync(ucmd);
            }
        }

        private static readonly string[] _validExtensions = { "jpg", "bmp", "gif", "png" };
        public static bool IsImageExtension(string ext)
        {
            return _validExtensions.Contains(ext);
        }
    }
}
