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
                var gldLocal = bot.Guilds.FirstOrDefault(x => x.TextChannels.Where(z => z.Id == arg2.Id) != null);
                var guild = await SqlTools.GetGuildAsync(gldLocal.Id);
                if(guild.GuildSettings.Features.Starboard)
                {
                    var dldedmsg = await arg1.GetOrDownloadAsync();
                    int starboardThreshold = Config.Load().StarboardThreshold;
                    var starboard = new Starboard();
                    var reader = await SqlTools.GetAsync(new MySqlCommand("SELECT * FROM `starboard` WHERE `MessageID` = " + dldedmsg.Id));
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            if (reader["guildid"] != DBNull.Value)
                                starboard.GuildID = Convert.ToUInt64(reader["guildid"]);
                            else
                                starboard.GuildID = 0;
                            if (reader["messageid"] != DBNull.Value)
                                starboard.MessageID = Convert.ToUInt64(reader["messageid"]);
                            else
                                starboard.MessageID = 0;
                            if (reader["originalmessageid"] != DBNull.Value)
                                starboard.OriginalMessageID = Convert.ToUInt64(reader["originalmessageid"]);
                            else
                                starboard.OriginalMessageID = 0;
                            if (reader["stars"] != DBNull.Value)
                                starboard.Stars = Convert.ToInt32(reader["stars"]);
                            else
                                starboard.Stars = 0;
                            if (reader["added"] != DBNull.Value)
                                starboard.DateAdded = Convert.ToString(reader["added"]);
                            else
                                starboard.DateAdded = null;
                        }
                    }
                    reader.Close();
                    await SqlTools.getconn.CloseAsync();
                    starboard.ChannelID = Convert.ToUInt64(await SqlTools.GetSingleAsync(new MySqlCommand("SELECT `starboardchannel` FROM `guild` WHERE ID = " + guild.ID)));
                    var stars = (await gldLocal.GetTextChannel(starboard.ChannelID).GetMessageAsync(starboard.MessageID) as IUserMessage).Reactions.FirstOrDefault(x => x.Key.Name == ":star:" && !x.Value.IsMe).Value.ReactionCount + (await gldLocal.GetTextChannel(starboard.ChannelID).GetMessageAsync(starboard.OriginalMessageID) as IUserMessage).Reactions.FirstOrDefault(x => x.Key.Name == ":star:" && !x.Value.IsMe).Value.ReactionCount;
                    if (dldedmsg.Id == starboard.MessageID)
                    {
                        string react = StarEmotes.OrderByDescending(x => x.Key).FirstOrDefault(x => x.Key <= starboard.Stars).Value;
                        var smsg = await gldLocal.GetTextChannel(starboard.ChannelID).GetMessageAsync(starboard.MessageID) as IUserMessage;
                        var content = smsg.Content.Split(' ').Skip(2);
                        string msg = react + " " + stars + " " + string.Join(" ", content);
                        var embd = smsg.Embeds.FirstOrDefault();
                        await smsg.ModifyAsync(x =>
                        {
                            x.Content = msg;
                        });
                        await SqlTools.InsertAsync(new MySqlCommand($"UPDATE `starboard` SET `Stars` = {starboard.Stars} WHERE `messageid` = " + dldedmsg.Id));
                    }
                    else if (dldedmsg.Id == starboard.OriginalMessageID)
                    {
                        string react = StarEmotes.OrderByDescending(x => x.Key).FirstOrDefault(x => x.Key <= starboard.Stars).Value;
                        var smsg = await gldLocal.GetTextChannel(starboard.ChannelID).GetMessageAsync(starboard.MessageID) as IUserMessage;
                        var content = smsg.Content.Split(' ').Skip(2);
                        string msg = react + " " + stars + " " + string.Join(" ", content);
                        var embd = smsg.Embeds.FirstOrDefault();
                        await smsg.ModifyAsync(x =>
                        {
                            x.Content = msg;
                        });
                        await SqlTools.InsertAsync(new MySqlCommand($"UPDATE `starboard` SET `Stars` = {starboard.Stars} WHERE `messageid` = " + dldedmsg.Id));
                    }
                }
            }
        }

        private static async Task Bot_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            if (!string.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                var gld = bot.Guilds.FirstOrDefault(x => x.TextChannels.FirstOrDefault(z => z.Id == arg2.Id) != null);
                var guild = await SqlTools.GetGuildAsync(gld.Id);
                if (guild != null && gld != null)
                {
                    if (guild.GuildSettings.Features.Starboard)
                    {
                        var dldedmsg = await arg1.GetOrDownloadAsync();
                        int starboardThreshold = Config.Load().StarboardThreshold;
                        int starboardReactions = dldedmsg.Reactions.FirstOrDefault(x => x.Key.Name == "⭐").Value.ReactionCount;
                        var temp = await SqlTools.GetSingleAsync(new MySqlCommand("SELECT OriginalMessageID FROM starboard WHERE OriginalMessageID = " + dldedmsg.Id));
                        var temp2 = await SqlTools.GetSingleAsync(new MySqlCommand("SELECT MessageID FROM starboard WHERE MessageID = " + dldedmsg.Id));
                        if (String.IsNullOrEmpty(temp)&&String.IsNullOrEmpty(temp2))
                        {
                            if (starboardReactions >= starboardThreshold)
                            {
                                var now = dldedmsg.CreatedAt;
                                var dt = DateTime.UtcNow.AddDays(-Config.Load().StarboardDateLimit);
                                if ((now - dt).TotalDays > 0)
                                {
                                    string react = StarEmotes.OrderByDescending(x => x.Key).FirstOrDefault(x => x.Key <= starboardReactions).Value;
                                    string message = react+" " + starboardReactions + " Channel: <#" + dldedmsg.Channel.Id + "> ID: " + dldedmsg.Id;
                                    var embed = new EmbedBuilder()
                                    {
                                        Author = new EmbedAuthorBuilder()
                                        {
                                            Name = dldedmsg.Author.Username,
                                            IconUrl = dldedmsg.Author.GetAvatarUrl()
                                        },
                                        Description = dldedmsg.Content,
                                        Timestamp = dldedmsg.CreatedAt
                                    };
                                    var attachment = dldedmsg.Attachments.FirstOrDefault();
                                    if (attachment != null)
                                    {
                                        if (Tools.Tools.IsImageExtension(attachment.Url))
                                            embed.ImageUrl = attachment.Url;
                                        else
                                            embed.Description = embed.Description + "\n" + attachment.Url;
                                    }
                                    var messg = await MessageHandler.SendChannel(bot.GetGuild(guild.ID).GetTextChannel(guild.StarboardChannel), message, embed);
                                    await SqlTools.InsertAsync(new MySqlCommand($"INSERT INTO `starboard` (`GuildID`,`MessageID`,`OriginalMessageID`,`Stars`,`Added`) VALUES ({gld.Id},{messg.Id},{dldedmsg.Id},{starboardReactions},\"{DateTime.UtcNow}\");"));
                                }
                            }
                        }
                        else if (!String.IsNullOrEmpty(temp2))
                        {
                            var starboard = new Starboard();
                            var reader = await SqlTools.GetAsync(new MySqlCommand("SELECT * FROM `starboard` WHERE `MessageID` = " + temp2));
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    if (reader["guildid"] != DBNull.Value)
                                        starboard.GuildID = Convert.ToUInt64(reader["guildid"]);
                                    else
                                        starboard.GuildID = 0;
                                    if (reader["messageid"] != DBNull.Value)
                                        starboard.MessageID = Convert.ToUInt64(reader["messageid"]);
                                    else
                                        starboard.MessageID = 0;
                                    if (reader["originalmessageid"] != DBNull.Value)
                                        starboard.OriginalMessageID = Convert.ToUInt64(reader["originalmessageid"]);
                                    else
                                        starboard.OriginalMessageID = 0;
                                    if (reader["stars"] != DBNull.Value)
                                        starboard.Stars = Convert.ToInt32(reader["stars"]);
                                    else
                                        starboard.Stars = 0;
                                    if (reader["added"] != DBNull.Value)
                                        starboard.DateAdded = Convert.ToString(reader["added"]);
                                    else
                                        starboard.DateAdded = null;
                                }
                            }
                            reader.Close();
                            await SqlTools.getconn.CloseAsync();
                            starboard.ChannelID = guild.StarboardChannel;
                            string react = StarEmotes.OrderByDescending(x => x.Key).FirstOrDefault(x => x.Key <= starboard.Stars).Value;
                            var stars = starboard.Stars;
                            var returnstars = stars + 1;
                            var smsg = await gld.GetTextChannel(starboard.ChannelID).GetMessageAsync(starboard.MessageID) as IUserMessage;
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
                    if (guild.GuildSettings.Features.Pinning)
                    {
                        var dldedmsg = await arg1.GetOrDownloadAsync();
                        int pinboardThreshold = Config.Load().PinboardThreshold;
                        int pinboardReactions = 0;
                        if (arg3.Emote.Name == "📌")
                            pinboardReactions = dldedmsg.Reactions.FirstOrDefault(x => x.Key.Name == "📌").Value.ReactionCount;
                        if (pinboardReactions >= pinboardThreshold)
                        {
                            var now = dldedmsg.CreatedAt;
                            var dt = DateTime.UtcNow.AddDays(-Config.Load().PinboardDateLimit);
                            if ((now - dt).TotalDays > 0)
                            {
                                if (!dldedmsg.IsPinned)
                                {
                                    await dldedmsg.PinAsync();
                                    Logs.Add(new Models.LogMessage("PinBrd", $"Reached or Over Threshold, pinned a message in: {dldedmsg.Channel.Name} from: {gld.Name}", LogSeverity.Info));
                                }
                            }
                        }
                    }
                }
            }                
        }

        private static async Task Shard_Connected()
        {
            var connected = new bool[bot.Shards.Count];
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
                var command = new MySqlCommand("INSERT IGNORE INTO `accounts` (`ID`, `username`, `description`) VALUES (@userid , @username, \"I have no description\");");
                command.Parameters.AddWithValue("@username", arg.Username.Replace("\"","\\").Replace("\'","\\'")+"#"+arg.DiscriminatorValue);
                command.Parameters.AddWithValue("@userid", arg.Id);
                await SqlTools.InsertAsync(command);
                
                string autorole = Convert.ToString(await SqlTools.GetSingleAsync(new MySqlCommand("SELECT `autojoinrole` FROM `guild` WHERE `id` = "+arg.Guild.Id)));

                if (!String.IsNullOrEmpty(autorole))
                {

                    var joinroleid = Convert.ToUInt64(autorole);
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
                var guild = await SqlTools.GetGuildAsync(arg.Guild.Id);
                string leavemessage = guild.LeaveMessage;
                if(!String.IsNullOrEmpty(guild.LeaveMessage))
                {
                    var channel = arg.Guild.GetTextChannel(guild.UserLeaveChannel);
                    leavemessage = leavemessage.Replace("-m", "**" + arg.Mention + "**");
                    leavemessage = leavemessage.Replace("-s", "**" + guild.Name + "**");
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
                var command = new MySqlCommand("DELETE FROM guild WHERE id = @guildid");
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
                var gld = bot.Guilds.FirstOrDefault(x => x.TextChannels.FirstOrDefault(z => z.Id == arg2.Id) != null);
                var guild = await SqlTools.GetGuildAsync(gld.Id);
                if (guild.GuildSettings.Features.GuildModification)
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
                    await SendModMessage(gld.GetTextChannel(guild.AuditChannel), "", embed);
                }
            }
        }

        private static async Task Bot_ChannelUpdated(SocketChannel arg1, SocketChannel arg2)
        {
            if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                var gld = bot.Guilds.FirstOrDefault(x => x.TextChannels.FirstOrDefault(z => z.Id == arg2.Id) != null);
                var guild = await SqlTools.GetGuildAsync(gld.Id);
                if (guild.GuildSettings.Features.GuildChannelModification)
                {
                    var chan1 = gld.GetChannel(arg1.Id);
                    var chan2 = gld.GetChannel(arg2.Id);
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
                    await SendModMessage(gld.GetTextChannel(guild.AuditChannel), "", embed);
                }
            }
        }

        private static async Task Bot_ChannelDestroyed(SocketChannel arg)
        {
            if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                var gld = bot.Guilds.FirstOrDefault(x => x.TextChannels.FirstOrDefault(z => z.Id == arg.Id) != null);
                SocketGuildChannel chan = null;
                var guild = await SqlTools.GetGuildAsync(gld.Id);
                if (guild.GuildSettings.Features.GuildChannelModification)
                {
                    var embed = new EmbedBuilder()
                    {
                        Title = "Channel Deleted",
                        Description = $"Channel `{chan.Name}` was deleted",
                        Color = new Color(255, 0, 0),
                        Timestamp = DateTime.UtcNow
                    };
                    await SendModMessage(gld.GetTextChannel(guild.AuditChannel), "", embed);
                }
            }
        }

        private static async Task Bot_ChannelCreated(SocketChannel arg)
        {
            if(!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                var gld = bot.Guilds.FirstOrDefault(x => x.TextChannels.FirstOrDefault(z => z.Id == arg.Id) != null);
                SocketGuildChannel chan = null;
                var guild = await SqlTools.GetGuildAsync(gld.Id);
                if(guild.GuildSettings.Features.GuildChannelModification)
                {
                    var embed = new EmbedBuilder()
                    {
                        Title = "Channel Created",
                        Description = $"Channel `{chan.Name}` was created",
                        Color = new Color(243, 255, 33),
                        Timestamp = arg.CreatedAt
                    };
                    await SendModMessage(gld.GetTextChannel(guild.AuditChannel), "", embed);
                }
            }
        }

        private static async Task Bot_UserBanned(SocketUser arg1, SocketGuild arg2)
        {
            if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                var guild = await SqlTools.GetGuildAsync(arg2.Id);
                if (guild.GuildSettings.Features.UserBanEvents)
                {
                    var bans = await arg2.GetBansAsync();
                    var audit = bans.FirstOrDefault(x => x.User.Id == arg1.Id);
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
                    await SendModMessage(arg2.GetTextChannel(guild.AuditChannel), "", embed);
                }
            }
        }

        private static async Task Bot_UserUnbanned(SocketUser arg1, SocketGuild arg2)
        {
            if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                var guild = await SqlTools.GetGuildAsync(arg2.Id);
                if (guild.GuildSettings.Features.UserBanEvents)
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
                    await SendModMessage(arg2.GetTextChannel(guild.AuditChannel), "", embed);
                }
            }
        }

        private static async Task Bot_RoleDeleted(SocketRole arg)
        {
            if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                var gld = bot.Guilds.FirstOrDefault(x => x.Roles.FirstOrDefault(z => z.Id == arg.Id) != null);
                var guild = await SqlTools.GetGuildAsync(gld.Id);
                if (guild.GuildSettings.Features.GuildRoleModification)
                {
                    string desc = $"Role `{arg.Name}` was deleted";
                    var embed = new EmbedBuilder()
                    {
                        Title = "Role deleted",
                        Description = desc,
                        Color = new Color(255, 0, 0),
                        Timestamp = DateTime.UtcNow
                    };
                    await SendModMessage(gld.GetTextChannel(guild.AuditChannel), "", embed);
                }
            }
        }

        private static async Task Bot_RoleUpdated(SocketRole arg1, SocketRole arg2)
        {
            if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                var gld = bot.Guilds.FirstOrDefault(x => x.TextChannels.FirstOrDefault(z => z.Id == arg1.Id) != null);
                var guild = await SqlTools.GetGuildAsync(gld.Id);
                if (guild.GuildSettings.Features.GuildRoleModification)
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
                    await SendModMessage(gld.GetTextChannel(guild.AuditChannel), "", embed);
                }
            }
        }

        private static async Task Bot_RoleCreated(SocketRole arg)
        {
            if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                var gld = bot.Guilds.FirstOrDefault(x => x.TextChannels.FirstOrDefault(z => z.Id == arg.Id) != null);
                var guild = await SqlTools.GetGuildAsync(gld.Id);
                if (guild.GuildSettings.Features.GuildRoleModification)
                {
                    string desc = $"Role `{arg.Name}` was created";
                    var embed = new EmbedBuilder()
                    {
                        Title = "Role created",
                        Description = desc,
                        Color = new Color(0, 255, 0),
                        Timestamp = DateTime.UtcNow
                    };
                    await SendModMessage(gld.GetTextChannel(guild.AuditChannel), "", embed);
                }
            }
        }

        private static async Task SendModMessage(SocketTextChannel channel, string message, Embed embed)=>
            await MessageHandler.SendChannel(channel, message, embed);

        //End ModLog Stuff

        public static async Task PopulateGuilds()
        {
            foreach (var guild in bot.Guilds)
                await PopulateSpecificGuild(guild);
        }
        public static async Task PopulateSpecificGuild(SocketGuild guild)
        {
            var gcmd = new MySqlCommand("INSERT IGNORE INTO `guild` (`ID`,`name`,`prefix`) VALUES ");
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
            var ucmd = new MySqlCommand("INSERT IGNORE INTO `accounts` (`ID`, `username`, `description`) VALUES ");
            
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
    }
}
