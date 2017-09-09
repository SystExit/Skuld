using System;
using System.Threading.Tasks;
using Skuld.Models;
using MySql.Data.MySqlClient;
using Discord.WebSocket;

namespace Skuld.Tools
{
    public partial class SqlTools
    {
        public static async Task<SkuldUser> GetUser(ulong ID)
        {
            var User = new SkuldUser();
            var command = new MySqlCommand("SELECT * FROM `accounts` WHERE ID = @userid");
            command.Parameters.AddWithValue("@userid", ID);
            var reader = await GetAsync(command);
            while(await reader.ReadAsync())
            {
                User.ID = Convert.ToUInt64(reader["ID"]);
                User.Username = Convert.ToString(reader["username"]);
                User.Money = Convert.ToUInt64(reader["money"]);
                User.Description = Convert.ToString(reader["description"]);
                User.LuckFactor = Convert.ToDouble(reader["luckfactor"]);
                User.DMEnabled = Convert.ToBoolean(reader["dmenabled"]);
                User.Petted = Convert.ToUInt32(reader["petted"]);
                User.Pets = Convert.ToUInt32(reader["pets"]);
                User.HP = Convert.ToUInt32(reader["hp"]);
                User.GlaredAt = Convert.ToUInt32(reader["glaredat"]);
                User.Glares = Convert.ToUInt32(reader["glares"]);
            }
            reader.Close();
            await getconn.CloseAsync();
            command = new MySqlCommand("SELECT * FROM commandusage WHERE UserID = @userid ORDER BY UserUsage DESC LIMIT 1");
            command.Parameters.AddWithValue("@userid", ID);
            reader = await GetAsync(command);
            while (await reader.ReadAsync())
            {
                User.FavCmd = Convert.ToString(reader["command"]);
                User.FavCmdUsg = Convert.ToUInt64(reader["UserUsage"]);
            }
            reader.Close();
            await getconn.CloseAsync();
            return User;
        }
        public static async Task<SkuldGuild> GetGuild(ulong ID)
        {
            var Guild = new SkuldGuild();
            var command = new MySqlCommand("SELECT * FROM `guild` WHERE ID = @guildid");
            command.Parameters.AddWithValue("@guildid", ID);
            var reader = await GetAsync(command);
            if(reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    if (reader["ID"] != DBNull.Value)
                        Guild.ID = Convert.ToUInt64(reader["ID"]);
                    else
                        Guild.ID = 0;

                    if (reader["name"] != DBNull.Value)
                        Guild.Name = Convert.ToString(reader["name"]);
                    else
                        Guild.Name = null;

                    if (reader["joinmessage"] != DBNull.Value)
                        Guild.JoinMessage = Convert.ToString(reader["joinmessage"]);
                    else
                        Guild.JoinMessage = null;

                    if (reader["leavemessage"] != DBNull.Value)
                        Guild.LeaveMessage = Convert.ToString(reader["leavemessage"]);
                    else
                        Guild.LeaveMessage = null;

                    if (reader["autojoinrole"]!=DBNull.Value)
                        Guild.AutoJoinRole = Convert.ToUInt64(reader["autojoinrole"]);
                    else
                        Guild.AutoJoinRole = 0;

                    if (reader["prefix"] != DBNull.Value)
                        Guild.Prefix = Convert.ToString(reader["prefix"]);
                    else
                        Guild.Prefix = null;

                    if (reader["joinableroles"] != DBNull.Value)
                        Guild.JoinableRoles = Convert.ToString(reader["joinableroles"]).Split(',');
                    else
                        Guild.JoinableRoles = null;

                    if (reader["twitchnotifchannel"] != DBNull.Value)
                        Guild.TwitchNotifChannel = Convert.ToUInt64(reader["twitchnotifchannel"]);
                    else
                        Guild.TwitchNotifChannel = 0;

                    if (reader["twitterlogchannel"] != DBNull.Value)
                        Guild.TwitterLogChannel = Convert.ToUInt64(reader["twitterlogchannel"]);
                    else
                        Guild.TwitterLogChannel = 0;

                    if (reader["mutedrole"] != DBNull.Value)
                        Guild.MutedRole = Convert.ToUInt64(reader["mutedrole"]);
                    else
                        Guild.MutedRole = 0;

                    if (reader["auditchannel"] != DBNull.Value)
                        Guild.AuditChannel = Convert.ToUInt64(reader["auditchannel"]);
                    else
                        Guild.AuditChannel = 0;

                    if (reader["userjoinchan"] != DBNull.Value)
                        Guild.UserJoinChannel = Convert.ToUInt64(reader["userjoinchan"]);
                    else
                        Guild.UserJoinChannel = 0;

                    if (reader["userleavechan"] != DBNull.Value)
                        Guild.UserLeaveChannel = Convert.ToUInt64(reader["userleavechan"]);
                    else
                        Guild.UserLeaveChannel = 0;

                    if (reader["starboardchannel"] != DBNull.Value)
                        Guild.StarboardChannel = Convert.ToUInt64(reader["starboardchannel"]);
                    else
                        Guild.StarboardChannel = 0;
                }
                reader.Close();
                await getconn.CloseAsync();
                var GuildSetts = new GuildSettings();
                var ComSetts = new GuildCommandModules();
                var FeatSetts = new GuildFeatureModules();
                command = new MySqlCommand("SELECT * FROM `guildcommandmodules` WHERE ID = @guildid");
                command.Parameters.AddWithValue("@guildid", ID);
                reader = await GetAsync(command);
                while (await reader.ReadAsync())
                {
                    ComSetts.AccountsEnabled = Convert.ToBoolean(reader["accounts"]);
                    ComSetts.ActionsEnabled = Convert.ToBoolean(reader["actions"]);
                    ComSetts.AdminEnabled = Convert.ToBoolean(reader["admin"]);
                    ComSetts.FunEnabled = Convert.ToBoolean(reader["fun"]);
                    ComSetts.HelpEnabled = Convert.ToBoolean(reader["help"]);
                    ComSetts.InformationEnabled = Convert.ToBoolean(reader["information"]);
                    ComSetts.SearchEnabled = Convert.ToBoolean(reader["search"]);
                    ComSetts.StatsEnabled = Convert.ToBoolean(reader["stats"]);
                }
                reader.Close();
                await getconn.CloseAsync();
                command = new MySqlCommand("SELECT * FROM `guildfeaturemodules` WHERE ID = @guildid");
                command.Parameters.AddWithValue("@guildid", ID);
                reader = await GetAsync(command);
                while (await reader.ReadAsync())
                {
                    FeatSetts.Starboard = Convert.ToBoolean(reader["starboard"]);
                    FeatSetts.Pinning = Convert.ToBoolean(reader["pinning"]);
                    FeatSetts.Experience = Convert.ToBoolean(reader["experience"]);
                    FeatSetts.UserJoinLeave = Convert.ToBoolean(reader["userjoinleave"]);
                    FeatSetts.UserModification = Convert.ToBoolean(reader["usermodification"]);
                    FeatSetts.UserBanEvents = Convert.ToBoolean(reader["userbanevents"]);
                    FeatSetts.GuildModification = Convert.ToBoolean(reader["guildmodification"]);
                    FeatSetts.GuildChannelModification = Convert.ToBoolean(reader["guildchannelmodification"]);
                    FeatSetts.GuildRoleModification = Convert.ToBoolean(reader["guildrolemodification"]);
                }
                reader.Close();
                await getconn.CloseAsync();
                GuildSetts.Modules = ComSetts;
                GuildSetts.Features = FeatSetts;
                Guild.GuildSettings = GuildSetts;
                return Guild;
            }
            else
            {
                return null;
            }            
        }
        public static async Task<Pasta> GetPasta(string Name)
        {
            var Pasta = new Pasta();
            var command = new MySqlCommand("SELECT * FROM pasta WHERE pastaname = @pastaname");
            command.Parameters.AddWithValue("@pastaname", Name);
            var reader = await GetAsync(command);
            while (reader.Read())
            {
                Pasta.PastaName = reader["pastaname"].ToString();
                Pasta.Content = reader["content"].ToString();
                Pasta.OwnerID = Convert.ToUInt64(reader["ownerid"].ToString());
                Pasta.Username = reader["username"].ToString();
                Pasta.Created = reader["created"].ToString();
                Pasta.Upvotes = Convert.ToUInt32(reader["upvotes"].ToString());
                Pasta.Downvotes = Convert.ToUInt32(reader["downvotes"].ToString());
            }
            reader.Close();
            await getconn.CloseAsync();
            return Pasta;
        }
        public static async Task InsertAdvancedSettings(bool feature, SocketGuild Guild)
        {
            var Command = new MySqlCommand("INSERT INTO ");
            if (feature)
            {
                Command.CommandText += "`guildfeaturemodules` (`ID`,`Starboard`,`Experience`,`UserJoinLeave`,`UserModification`,`UserBanEvents`,`GuildModification`,`GuildChannelModification`,`GuildRoleModification`) VALUES (" + Guild.Id + "0,0,0,0,0,0,0,0);";
            }
            else
            {
                Command.CommandText += "`guildfeaturemodules` (`ID`,`Accounts`, `Actions`,`Admin`,`Fun`,`Help`,`Information`,`Search`,`Stats`) VALUES (" + Guild.Id + ",1,1,1,1,1,1,1,1);";
            }
            await InsertAsync(Command);
        }
    }
}
