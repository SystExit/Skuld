using System;
using System.Threading.Tasks;
using Skuld.Models;
using MySql.Data.MySqlClient;
using Discord.WebSocket;

namespace Skuld.Tools
{
    public partial class SqlTools
    {
        public static async Task<SkuldUser> GetUser(ulong id)
        {
            var user = new SkuldUser();
            var command = new MySqlCommand("SELECT * FROM `accounts` WHERE ID = @userid");
            command.Parameters.AddWithValue("@userid", id);
            var reader = await GetAsync(command);
            while(await reader.ReadAsync())
            {
                user.ID = Convert.ToUInt64(reader["ID"]);
                user.Username = Convert.ToString(reader["username"]);
                user.Money = Convert.ToUInt64(reader["money"]);
                user.Description = Convert.ToString(reader["description"]);
                user.LuckFactor = Convert.ToDouble(reader["luckfactor"]);
                user.DMEnabled = Convert.ToBoolean(reader["dmenabled"]);
                user.Petted = Convert.ToUInt32(reader["petted"]);
                user.Pets = Convert.ToUInt32(reader["pets"]);
                user.HP = Convert.ToUInt32(reader["hp"]);
                user.GlaredAt = Convert.ToUInt32(reader["glaredat"]);
                user.Glares = Convert.ToUInt32(reader["glares"]);
            }
            reader.Close();
            await getconn.CloseAsync();
            command = new MySqlCommand("SELECT * FROM commandusage WHERE UserID = @userid ORDER BY UserUsage DESC LIMIT 1");
            command.Parameters.AddWithValue("@userid", id);
            reader = await GetAsync(command);
            while (await reader.ReadAsync())
            {
                user.FavCmd = Convert.ToString(reader["command"]);
                user.FavCmdUsg = Convert.ToUInt64(reader["UserUsage"]);
            }
            reader.Close();
            await getconn.CloseAsync();
            return user;
        }
        public static async Task<SkuldGuild> GetGuild(ulong id)
        {
            var guild = new SkuldGuild();
            var command = new MySqlCommand("SELECT * FROM `guild` WHERE ID = @guildid");
            command.Parameters.AddWithValue("@guildid", id);
            var reader = await GetAsync(command);
            if(reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    if (reader["ID"] != DBNull.Value)
                        guild.ID = Convert.ToUInt64(reader["ID"]);
                    else
                        guild.ID = 0;

                    if (reader["name"] != DBNull.Value)
                        guild.Name = Convert.ToString(reader["name"]);
                    else
                        guild.Name = null;

                    if (reader["joinmessage"] != DBNull.Value)
                        guild.JoinMessage = Convert.ToString(reader["joinmessage"]);
                    else
                        guild.JoinMessage = null;

                    if (reader["leavemessage"] != DBNull.Value)
                        guild.LeaveMessage = Convert.ToString(reader["leavemessage"]);
                    else
                        guild.LeaveMessage = null;

                    if (reader["autojoinrole"]!=DBNull.Value)
                        guild.AutoJoinRole = Convert.ToUInt64(reader["autojoinrole"]);
                    else
                        guild.AutoJoinRole = 0;

                    if (reader["prefix"] != DBNull.Value)
                        guild.Prefix = Convert.ToString(reader["prefix"]);
                    else
                        guild.Prefix = null;

                    if (reader["joinableroles"] != DBNull.Value)
                        guild.JoinableRoles = Convert.ToString(reader["joinableroles"]).Split(',');
                    else
                        guild.JoinableRoles = null;

                    if (reader["twitchnotifchannel"] != DBNull.Value)
                        guild.TwitchNotifChannel = Convert.ToUInt64(reader["twitchnotifchannel"]);
                    else
                        guild.TwitchNotifChannel = 0;

                    if (reader["twitterlogchannel"] != DBNull.Value)
                        guild.TwitterLogChannel = Convert.ToUInt64(reader["twitterlogchannel"]);
                    else
                        guild.TwitterLogChannel = 0;

                    if (reader["mutedrole"] != DBNull.Value)
                        guild.MutedRole = Convert.ToUInt64(reader["mutedrole"]);
                    else
                        guild.MutedRole = 0;

                    if (reader["auditchannel"] != DBNull.Value)
                        guild.AuditChannel = Convert.ToUInt64(reader["auditchannel"]);
                    else
                        guild.AuditChannel = 0;

                    if (reader["userjoinchan"] != DBNull.Value)
                        guild.UserJoinChannel = Convert.ToUInt64(reader["userjoinchan"]);
                    else
                        guild.UserJoinChannel = 0;

                    if (reader["userleavechan"] != DBNull.Value)
                        guild.UserLeaveChannel = Convert.ToUInt64(reader["userleavechan"]);
                    else
                        guild.UserLeaveChannel = 0;

                    if (reader["starboardchannel"] != DBNull.Value)
                        guild.StarboardChannel = Convert.ToUInt64(reader["starboardchannel"]);
                    else
                        guild.StarboardChannel = 0;
                }
                reader.Close();
                await getconn.CloseAsync();
                var guildSetts = new GuildSettings();
                var comSetts = new GuildCommandModules();
                var featSetts = new GuildFeatureModules();
                command = new MySqlCommand("SELECT * FROM `guildcommandmodules` WHERE ID = @guildid");
                command.Parameters.AddWithValue("@guildid", id);
                reader = await GetAsync(command);
                while (await reader.ReadAsync())
                {
                    comSetts.AccountsEnabled = Convert.ToBoolean(reader["accounts"]);
                    comSetts.ActionsEnabled = Convert.ToBoolean(reader["actions"]);
                    comSetts.AdminEnabled = Convert.ToBoolean(reader["admin"]);
                    comSetts.FunEnabled = Convert.ToBoolean(reader["fun"]);
                    comSetts.HelpEnabled = Convert.ToBoolean(reader["help"]);
                    comSetts.InformationEnabled = Convert.ToBoolean(reader["information"]);
                    comSetts.SearchEnabled = Convert.ToBoolean(reader["search"]);
                    comSetts.StatsEnabled = Convert.ToBoolean(reader["stats"]);
                }
                reader.Close();
                await getconn.CloseAsync();
                command = new MySqlCommand("SELECT * FROM `guildfeaturemodules` WHERE ID = @guildid");
                command.Parameters.AddWithValue("@guildid", id);
                reader = await GetAsync(command);
                while (await reader.ReadAsync())
                {
                    featSetts.Starboard = Convert.ToBoolean(reader["starboard"]);
                    featSetts.Pinning = Convert.ToBoolean(reader["pinning"]);
                    featSetts.Experience = Convert.ToBoolean(reader["experience"]);
                    featSetts.UserJoinLeave = Convert.ToBoolean(reader["userjoinleave"]);
                    featSetts.UserModification = Convert.ToBoolean(reader["usermodification"]);
                    featSetts.UserBanEvents = Convert.ToBoolean(reader["userbanevents"]);
                    featSetts.GuildModification = Convert.ToBoolean(reader["guildmodification"]);
                    featSetts.GuildChannelModification = Convert.ToBoolean(reader["guildchannelmodification"]);
                    featSetts.GuildRoleModification = Convert.ToBoolean(reader["guildrolemodification"]);
                }
                reader.Close();
                await getconn.CloseAsync();
                guildSetts.Modules = comSetts;
                guildSetts.Features = featSetts;
                guild.GuildSettings = guildSetts;
                return guild;
            }
            else
            {
                return null;
            }            
        }
        public static async Task<Pasta> GetPasta(string name)
        {
            var pasta = new Pasta();
            var command = new MySqlCommand("SELECT * FROM pasta WHERE pastaname = @pastaname");
            command.Parameters.AddWithValue("@pastaname", name);
            var reader = await GetAsync(command);
            while (reader.Read())
            {
                pasta.PastaName = reader["pastaname"].ToString();
                pasta.Content = reader["content"].ToString();
                pasta.OwnerID = Convert.ToUInt64(reader["ownerid"].ToString());
                pasta.Username = reader["username"].ToString();
                pasta.Created = reader["created"].ToString();
                pasta.Upvotes = Convert.ToUInt32(reader["upvotes"].ToString());
                pasta.Downvotes = Convert.ToUInt32(reader["downvotes"].ToString());
            }
            reader.Close();
            await getconn.CloseAsync();
            return pasta;
        }
        public static async Task InsertAdvancedSettings(bool feature, SocketGuild guild)
        {
            var command = new MySqlCommand("INSERT INTO ");
            if (feature)
            {
                command.CommandText += "`guildfeaturemodules` (`ID`,`Starboard`,`Experience`,`UserJoinLeave`,`UserModification`,`UserBanEvents`,`GuildModification`,`GuildChannelModification`,`GuildRoleModification`) VALUES (" + guild.Id + "0,0,0,0,0,0,0,0);";
            }
            else
            {
                command.CommandText += "`guildfeaturemodules` (`ID`,`Accounts`, `Actions`,`Admin`,`Fun`,`Help`,`Information`,`Search`,`Stats`) VALUES (" + guild.Id + ",1,1,1,1,1,1,1,1);";
            }
            await InsertAsync(command);
        }
    }
}
