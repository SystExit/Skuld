using System;
using System.Threading.Tasks;
using Skuld.Models;
using MySql.Data.MySqlClient;
using Discord.WebSocket;
using StatsdClient;

namespace Skuld.Tools
{
    public class SqlTools : SqlConnection
    {
        public static async Task<SkuldUser> GetUserAsync(ulong id)
        {
            var user = new SkuldUser();
            var command = new MySqlCommand("SELECT * FROM `accounts` WHERE ID = @userid");
            command.Parameters.AddWithValue("@userid", id);
            var reader = await GetAsync(command);
            while (await reader.ReadAsync())
            {
                if (reader["ID"] != DBNull.Value) { user.ID = Convert.ToUInt64(reader["ID"]); }
                else { user.ID = 0; }

                if (reader["username"] != DBNull.Value) { user.Username = Convert.ToString(reader["username"]); }
                else { user.Username = null; }

                if (reader["money"] != DBNull.Value){ user.Money = Convert.ToUInt64(reader["money"]); }
                else { user.Money = null; }

				if (reader["description"] != DBNull.Value) { user.Description = Convert.ToString(reader["description"]); }
				else { user.Description = null; }

				if (reader["language"] != DBNull.Value) { user.Language = Convert.ToString(reader["language"]); }
				else { user.Language = Locale.defaultLocale; }

				if (reader["daily"] != DBNull.Value) { user.Daily = Convert.ToString(reader["daily"]); }
                else { user.Daily = null; }

                if (reader["luckfactor"] != DBNull.Value){ user.LuckFactor = Convert.ToDouble(reader["luckfactor"]); }
                else { user.LuckFactor = 0; }

                if (reader["dmenabled"] != DBNull.Value){ user.DMEnabled = Convert.ToBoolean(reader["dmenabled"]); }
                else { user.DMEnabled = false; }

                if (reader["petted"] != DBNull.Value){ user.Petted = Convert.ToUInt32(reader["petted"]); }
                else { user.Petted = 0; }

                if (reader["pets"] != DBNull.Value){ user.Pets = Convert.ToUInt32(reader["pets"]); }
                else { user.Pets = 0; }

                if (reader["hp"] != DBNull.Value){ user.HP = Convert.ToUInt32(reader["hp"]); }
                else { user.HP = 0; }

                if (reader["glaredat"] != DBNull.Value){ user.GlaredAt = Convert.ToUInt32(reader["glaredat"]); }
                else { user.GlaredAt = 0; }

                if (reader["glares"] != DBNull.Value){ user.Glares = Convert.ToUInt32(reader["glares"]); }
                else {user.Glares = 0; }
            }
            reader.Close();
            await getconn.CloseAsync();
            command = new MySqlCommand("SELECT * FROM commandusage WHERE UserID = @userid ORDER BY UserUsage DESC LIMIT 1");
            command.Parameters.AddWithValue("@userid", id);
            reader = await GetAsync(command);
            while (await reader.ReadAsync())
            {
                if (reader["command"] != DBNull.Value){ user.FavCmd = Convert.ToString(reader["command"]); }
                else { user.FavCmd = null; }

                if (reader["UserUsage"] != DBNull.Value){ user.FavCmdUsg = Convert.ToUInt64(reader["UserUsage"]); }
                else { user.FavCmdUsg = 0; }
            }
            reader.Close();
            await getconn.CloseAsync();
            return user;
        }
        public static async Task<SkuldGuild> GetGuildAsync(ulong id)
        {
            var guild = new SkuldGuild();
            var command = new MySqlCommand("SELECT * FROM `guild` WHERE ID = @guildid");
            command.Parameters.AddWithValue("@guildid", id);
            var reader = await GetAsync(command);
            if (reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    if (reader["ID"] != DBNull.Value) { guild.ID = Convert.ToUInt64(reader["ID"]); }
                    else { guild.ID = 0; }
                    if (reader["name"] != DBNull.Value) { guild.Name = Convert.ToString(reader["name"]); }
                    else { guild.Name = null; }
                    if (reader["joinmessage"] != DBNull.Value) { guild.JoinMessage = Convert.ToString(reader["joinmessage"]); }
                    else { guild.JoinMessage = null; }
                    if (reader["leavemessage"] != DBNull.Value) { guild.LeaveMessage = Convert.ToString(reader["leavemessage"]); }
                    else { guild.LeaveMessage = null; }
                    if (reader["autojoinrole"] != DBNull.Value) { guild.AutoJoinRole = Convert.ToUInt64(reader["autojoinrole"]); }
                    else { guild.AutoJoinRole = 0; }
                    if (reader["prefix"] != DBNull.Value) { guild.Prefix = Convert.ToString(reader["prefix"]); }
                    else { guild.Prefix = null; }
					if (reader["language"] != DBNull.Value) { guild.Language = Convert.ToString(reader["language"]); }
					else { guild.Language = null; }
					if (reader["joinableroles"] != DBNull.Value) { guild.JoinableRoles = Convert.ToString(reader["joinableroles"]).Split(','); }
                    else { guild.JoinableRoles = null; }
                    if (reader["twitchnotifchannel"] != DBNull.Value) { guild.TwitchNotifChannel = Convert.ToUInt64(reader["twitchnotifchannel"]); }
                    else { guild.TwitchNotifChannel = 0; }
                    if (reader["twitterlogchannel"] != DBNull.Value) { guild.TwitterLogChannel = Convert.ToUInt64(reader["twitterlogchannel"]); }
                    else { guild.TwitterLogChannel = 0; }
                    if (reader["mutedrole"] != DBNull.Value) { guild.MutedRole = Convert.ToUInt64(reader["mutedrole"]); }
                    else { guild.MutedRole = 0; }
                    if (reader["auditchannel"] != DBNull.Value) { guild.AuditChannel = Convert.ToUInt64(reader["auditchannel"]); }
                    else { guild.AuditChannel = 0; }
                    if (reader["userjoinchan"] != DBNull.Value) { guild.UserJoinChannel = Convert.ToUInt64(reader["userjoinchan"]); }
                    else { guild.UserJoinChannel = 0; }
                    if (reader["userleavechan"] != DBNull.Value) { guild.UserLeaveChannel = Convert.ToUInt64(reader["userleavechan"]); }
                    else { guild.UserLeaveChannel = 0; }
                    if (reader["starboardchannel"] != DBNull.Value) { guild.StarboardChannel = Convert.ToUInt64(reader["starboardchannel"]); }
                    else { guild.StarboardChannel = 0; }
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
                    if (reader["accounts"] != DBNull.Value) { comSetts.AccountsEnabled = Convert.ToBoolean(reader["accounts"]); }
                    else { comSetts.AccountsEnabled = true; }
                    if (reader["actions"] != DBNull.Value) { comSetts.ActionsEnabled = Convert.ToBoolean(reader["actions"]); }
                    else { comSetts.ActionsEnabled = true; }
                    if (reader["admin"] != DBNull.Value) { comSetts.AdminEnabled = Convert.ToBoolean(reader["admin"]); }
                    else { comSetts.AdminEnabled = true; }
                    if (reader["fun"] != DBNull.Value) { comSetts.FunEnabled = Convert.ToBoolean(reader["fun"]); }
                    else { comSetts.FunEnabled = true; }
                    if (reader["help"] != DBNull.Value) { comSetts.HelpEnabled = Convert.ToBoolean(reader["help"]); }
                    else { comSetts.HelpEnabled = true; }
                    if (reader["information"] != DBNull.Value) { comSetts.InformationEnabled = Convert.ToBoolean(reader["information"]); }
                    else { comSetts.InformationEnabled = true; }
                    if (reader["search"] != DBNull.Value) { comSetts.SearchEnabled = Convert.ToBoolean(reader["search"]); }
                    else { comSetts.SearchEnabled = true; }
                    if (reader["stats"] != DBNull.Value) { comSetts.StatsEnabled = Convert.ToBoolean(reader["stats"]); }
                    else { comSetts.StatsEnabled = true; }
                }
                reader.Close();
                await getconn.CloseAsync();
                command = new MySqlCommand("SELECT * FROM `guildfeaturemodules` WHERE ID = @guildid");
                command.Parameters.AddWithValue("@guildid", id);
                reader = await GetAsync(command);
                while (await reader.ReadAsync())
                {
                    if (reader["starboard"] != DBNull.Value) { featSetts.Starboard = Convert.ToBoolean(reader["starboard"]); }
                    else { featSetts.Starboard = false; }
                    if (reader["pinning"] != DBNull.Value) { featSetts.Pinning = Convert.ToBoolean(reader["pinning"]); }
                    else { featSetts.Pinning = false; }
                    if (reader["experience"] != DBNull.Value) { featSetts.Experience = Convert.ToBoolean(reader["experience"]); }
                    else { featSetts.Experience = false; }
                    if (reader["userjoinleave"] != DBNull.Value) { featSetts.UserJoinLeave = Convert.ToBoolean(reader["userjoinleave"]); }
                    else { featSetts.UserJoinLeave = false; }
                    if (reader["usermodification"] != DBNull.Value) { featSetts.UserModification = Convert.ToBoolean(reader["usermodification"]); }
                    else { featSetts.UserModification = false; }
                    if (reader["userbanevents"] != DBNull.Value) { featSetts.UserBanEvents = Convert.ToBoolean(reader["userbanevents"]); }
                    else { featSetts.UserBanEvents = false; }
                    if (reader["guildmodification"] != DBNull.Value) { featSetts.GuildModification = Convert.ToBoolean(reader["guildmodification"]); }
                    else { featSetts.GuildModification = false; }
                    if (reader["guildchannelmodification"] != DBNull.Value) { featSetts.GuildChannelModification = Convert.ToBoolean(reader["guildchannelmodification"]); }
                    else { featSetts.GuildChannelModification = false; }
                    if (reader["guildrolemodification"] != DBNull.Value) { featSetts.GuildRoleModification = Convert.ToBoolean(reader["guildrolemodification"]); }
                    else { featSetts.GuildRoleModification = false; }
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
        public static async Task<SqlError> InsertAdvancedSettings(bool feature, SocketGuild guild)
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
            return new SqlError(await InsertAsync(command));
        }
        public static async Task<SqlError> InsertUserAsync(SocketUser user)
        {
            if(!user.IsBot)
            {
                var command = new MySqlCommand("INSERT IGNORE INTO `accounts` (`ID`, `username`, `description`) VALUES (@userid , @username, \"I have no description\");");
                command.Parameters.AddWithValue("@username", $"{user.Username.Replace("\"", "\\").Replace("\'", "\\'")}");
                command.Parameters.AddWithValue("@userid", user.Id);
                return new SqlError(await InsertAsync(command));
            }
            else { return new SqlError(false,"Robots are not supported."); }
        }
        public static async Task<SqlError> ModifyUserAsync(SocketUser user, string column, string value)
        {
            if(!user.IsBot)
            {
                return new SqlError(await InsertAsync(new MySqlCommand($"UPDATE `accounts` SET `{column}`='{value}' WHERE ID='{user.Id}'")));
            }
            else { return new SqlError(false, "Robots are not supported."); }
        }
    }
}
