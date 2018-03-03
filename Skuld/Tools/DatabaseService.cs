using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using System;
using System.Data.Common;
using Skuld.Models;
using Discord.WebSocket;
using System.Collections.Generic;
using Discord.Commands;

namespace Skuld.Tools
{
    public class DatabaseService
    {
        private static string cs = $@"server={Bot.Configuration.SqlDBHost};user={Bot.Configuration.SqlUser};password={Bot.Configuration.SqlPass};database={Bot.Configuration.SqlDB};charset=utf8mb4";

        static DiscordShardedClient SClient;

        public DatabaseService(DiscordShardedClient client)
        {
            SClient = client;
        }
		
        public async Task<bool> NonQueryAsync(MySqlCommand command)
        {
            using (var conn = new MySqlConnection(cs))
            {
                await conn.OpenAsync();
                if (conn.State == ConnectionState.Open)
                {
                    command.Connection = conn;
                    try
                    {
                        await command.ExecuteNonQueryAsync();
                        StatsdClient.DogStatsd.Increment("mysql.queries");
                        StatsdClient.DogStatsd.Increment("mysql.insert");
                        await conn.CloseAsync();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await Bot.Logger.AddToLogs(new LogMessage("SQL-Ins", "Error with SQL Command", Discord.LogSeverity.Error, ex));
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task<string> GetSingleAsync(MySqlCommand command)
        {
            using (var conn = new MySqlConnection(cs))
            {
                await conn.OpenAsync();
                if (conn.State == ConnectionState.Open)
                {
                    command.Connection = conn;
                    try
                    {
                        var result = Convert.ToString(await command.ExecuteScalarAsync());
                        StatsdClient.DogStatsd.Increment("mysql.queries");
                        StatsdClient.DogStatsd.Set("mysql.rows-ret", 1);
                        await conn.CloseAsync();
                        return result;
                    }
                    catch (Exception ex)
                    {
                        await Bot.Logger.AddToLogs(new LogMessage("SQL-Get", "Error with SQL Command", Discord.LogSeverity.Error, ex));
                    }
                }
                return null;
            }
        }

        public async Task<SkuldUser> GetUserAsync(ulong id)
        {
            var user = new SkuldUser();
            var command = new MySqlCommand("SELECT * FROM `accounts` WHERE ID = @userid");
            command.Parameters.AddWithValue("@userid", id);

            using (var conn = new MySqlConnection(cs))
            {
                await conn.OpenAsync();
                if (conn.State == ConnectionState.Open)
                {
                    command.Connection = conn;

                    StatsdClient.DogStatsd.Increment("mysql.queries");

                    var reader = await command.ExecuteReaderAsync();

                    int rows = 0;

                    rows = reader.Depth + 1;

                    StatsdClient.DogStatsd.Set("mysql.rows-ret", rows);

                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            user.ID = Tools.ParseUInt64OrDefault(Convert.ToString(reader["ID"]));

                            user.Username = Convert.ToString(reader["username"]);

                            user.Money = Tools.ParseUInt64OrDefault(Convert.ToString(reader["money"]));

                            user.Description = Convert.ToString(reader["description"]);

                            user.Language = Convert.ToString(reader["language"]);

                            user.Daily = Convert.ToString(reader["daily"]);

                            user.LuckFactor = Convert.ToDouble(reader["luckfactor"]);

                            user.DMEnabled = Convert.ToBoolean(reader["dmenabled"]);

                            user.Petted = Tools.ParseUInt32OrDefault(Convert.ToString(reader["petted"]));

                            user.Pets = Tools.ParseUInt32OrDefault(Convert.ToString(reader["pets"]));

                            user.HP = Tools.ParseUInt32OrDefault(Convert.ToString(reader["hp"]));

                            user.GlaredAt = Tools.ParseUInt32OrDefault(Convert.ToString(reader["glaredat"]));

                            user.Glares = Tools.ParseUInt32OrDefault(Convert.ToString(reader["glares"]));
                        }

                        await conn.CloseAsync();
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            command = new MySqlCommand("SELECT * FROM commandusage WHERE UserID = @userid ORDER BY UserUsage DESC LIMIT 1");
            command.Parameters.AddWithValue("@userid", id);

            using (var conn = new MySqlConnection(cs))
            {
                await conn.OpenAsync();
                if (conn.State == ConnectionState.Open)
                {
                    command.Connection = conn;

                    StatsdClient.DogStatsd.Increment("mysql.queries");

                    var reader = await command.ExecuteReaderAsync();

                    int rows = 0;

                    rows = reader.Depth + 1;

                    StatsdClient.DogStatsd.Set("mysql.rows-ret", rows);

                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            user.FavCmd = Convert.ToString(reader["command"]);

                            user.FavCmdUsg = Tools.ParseUInt64OrDefault(Convert.ToString(reader["UserUsage"]));
                        }

                        await conn.CloseAsync();
                    }
                }
            }

            return user;
        }
        public async Task<SkuldGuild> GetGuildAsync(ulong id)
        {
            var guild = new SkuldGuild();
            var command = new MySqlCommand("SELECT * FROM `guild` WHERE ID = @guildid");
            command.Parameters.AddWithValue("@guildid", id);

            using (var conn = new MySqlConnection(cs))
            {
                await conn.OpenAsync();
                if (conn.State == ConnectionState.Open)
                {
                    command.Connection = conn;

                    StatsdClient.DogStatsd.Increment("mysql.queries");

                    var reader = await command.ExecuteReaderAsync();

                    int rows = 0;

                    rows = reader.Depth + 1;

                    StatsdClient.DogStatsd.Set("mysql.rows-ret", rows);

                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            guild.ID = Tools.ParseUInt64OrDefault(Convert.ToString(reader["ID"]));

                            guild.Name = Convert.ToString(reader["name"]);

                            guild.JoinMessage = Convert.ToString(reader["joinmessage"]);

                            guild.LeaveMessage = Convert.ToString(reader["leavemessage"]);

                            guild.AutoJoinRole = Tools.ParseUInt64OrDefault(Convert.ToString(reader["autojoinrole"]));

                            guild.Prefix = Convert.ToString(reader["prefix"]);

                            guild.Language = Convert.ToString(reader["language"]);

                            //guild.JoinableRoles = Convert.ToString(reader["joinableroles"]).Split(',');

                            guild.TwitchNotifChannel = Tools.ParseUInt64OrDefault(Convert.ToString(reader["twitchnotifchannel"]));

                            guild.TwitterLogChannel = Tools.ParseUInt64OrDefault(Convert.ToString(reader["twitterlogchannel"]));

                            guild.MutedRole = Tools.ParseUInt64OrDefault(Convert.ToString(reader["mutedrole"]));

                            guild.AuditChannel = Tools.ParseUInt64OrDefault(Convert.ToString(reader["auditchannel"]));

                            guild.UserJoinChannel = Tools.ParseUInt64OrDefault(Convert.ToString(reader["userjoinchan"]));

                            guild.UserLeaveChannel = Tools.ParseUInt64OrDefault(Convert.ToString(reader["userleavechan"]));
                        }

                        await conn.CloseAsync();
                    }
                }
            }

            var guildSetts = new GuildSettings();
            var comSetts = new GuildCommandModules();
            var featSetts = new GuildFeatureModules();

            command = new MySqlCommand("SELECT * FROM `guildcommandmodules` WHERE ID = @guildid");
            command.Parameters.AddWithValue("@guildid", id);

            using (var conn = new MySqlConnection(cs))
            {
                await conn.OpenAsync();
                if (conn.State == ConnectionState.Open)
                {
                    command.Connection = conn;

                    StatsdClient.DogStatsd.Increment("mysql.queries");

                    var reader = await command.ExecuteReaderAsync();

                    int rows = 0;

                    rows = reader.Depth + 1;

                    StatsdClient.DogStatsd.Set("mysql.rows-ret", rows);

                    if (reader.HasRows)
                    {
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

                        await conn.CloseAsync();
                    }
                }
            }

            command = new MySqlCommand("SELECT * FROM `guildfeaturemodules` WHERE ID = @guildid");
            command.Parameters.AddWithValue("@guildid", id);

            using (var conn = new MySqlConnection(cs))
            {
                await conn.OpenAsync();
                if (conn.State == ConnectionState.Open)
                {
                    command.Connection = conn;

                    StatsdClient.DogStatsd.Increment("mysql.queries");

                    var reader = await command.ExecuteReaderAsync();

                    int rows = 0;

                    rows = reader.Depth + 1;

                    StatsdClient.DogStatsd.Set("mysql.rows-ret", rows);

                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            featSetts.Pinning = Convert.ToBoolean(reader["pinning"]);

                            featSetts.Experience = Convert.ToBoolean(reader["experience"]);

                            featSetts.UserJoinLeave = Convert.ToBoolean(reader["userjoinleave"]);
                        }

                        await conn.CloseAsync();
                    }
                }
            }

            guildSetts.Modules = comSetts;
            guildSetts.Features = featSetts;
            guild.GuildSettings = guildSetts;

            return guild;
        }
        public async Task<Pasta> GetPastaAsync(string name)
        {
            var pasta = new Pasta();
            var command = new MySqlCommand("SELECT * FROM pasta WHERE pastaname = @pastaname");
            command.Parameters.AddWithValue("@pastaname", name);

            using (var conn = new MySqlConnection(cs))
            {
                await conn.OpenAsync();
                if (conn.State == ConnectionState.Open)
                {
                    command.Connection = conn;

                    StatsdClient.DogStatsd.Increment("mysql.queries");

                    var reader = await command.ExecuteReaderAsync();

                    int rows = 0;

                    rows = reader.Depth + 1;

                    StatsdClient.DogStatsd.Set("mysql.rows-ret", rows);

                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            pasta.Name = reader["pastaname"].ToString();
                            pasta.Content = reader["content"].ToString();
                            pasta.OwnerID = Convert.ToUInt64(reader["ownerid"].ToString());
                            pasta.Created = reader["created"].ToString();
                            pasta.Upvotes = Convert.ToUInt32(reader["upvotes"].ToString());
                            pasta.Downvotes = Convert.ToUInt32(reader["downvotes"].ToString());
                        }

                        await conn.CloseAsync();
                    }
                }
            }

            return pasta;
        }
		public async Task<CustomCommand> GetCustomCommandAsync(ulong GuildID, string Command)
		{
			var cmd = new CustomCommand();
			var command = new MySqlCommand("SELECT * FROM `customcommand` WHERE GuildID = @guildID AND CommandName = @command");

			command.Parameters.AddWithValue("@guildID", GuildID);
			command.Parameters.AddWithValue("@command", Command);


			using (var conn = new MySqlConnection(cs))
			{
				await conn.OpenAsync();
				if (conn.State == ConnectionState.Open)
				{
					command.Connection = conn;

					StatsdClient.DogStatsd.Increment("mysql.queries");

					var reader = await command.ExecuteReaderAsync();

					int rows = 0;

					rows = reader.Depth + 1;

					StatsdClient.DogStatsd.Set("mysql.rows-ret", rows);

					if (reader.HasRows)
					{
						while (await reader.ReadAsync())
						{
							cmd.GuildID = Tools.ParseUInt64OrDefault(Convert.ToString(reader["GuildID"]));

							cmd.Content = Convert.ToString(reader["Content"]);

							cmd.CommandName = Convert.ToString(reader["CommandName"]);
						}

						await conn.CloseAsync();
					}
					else
					{
						return null;
					}
				}
			}

			return cmd;
		}

		public async Task<SqlError> InsertAdvancedSettingsAsync(bool feature, SocketGuild guild)
        {
            var command = new MySqlCommand("INSERT INTO ");
            if (feature)
            {
                command.CommandText += "`guildfeaturemodules` (`ID`,`Experience`,`UserJoinLeave`) VALUES (@guildid,0,0);";
                command.Parameters.AddWithValue("@guildid", guild.Id);
            }
            else
            {
                command.CommandText += "`guildfeaturemodules` (`ID`,`Accounts`, `Actions`,`Admin`,`Fun`,`Help`,`Information`,`Search`,`Stats`) VALUES (@guildid,1,1,1,1,1,1,1,1);";
                command.Parameters.AddWithValue("@guildid", guild.Id);
            }
            return new SqlError(await NonQueryAsync(command));
        }
        public async Task<SqlError> InsertUserAsync(SocketUser user)
        {
            if (!user.IsBot)
            {
                var command = new MySqlCommand("INSERT IGNORE INTO `accounts` (`ID`, `username`, `description`, `language`) VALUES (@userid , @username, \"I have no description\", @locale);");
                command.Parameters.AddWithValue("@username", $"{user.Username.Replace("\"", "\\").Replace("\'", "\\'")}");
                command.Parameters.AddWithValue("@userid", user.Id);
                command.Parameters.AddWithValue("@locale", Locale.defaultLocale);
                return new SqlError(await NonQueryAsync(command));
            }
            else { return new SqlError(false, "Robots are not supported."); }
        }
        public async Task<SqlError> ModifyUserAsync(SocketUser user, string column, string value)
        {
            if (!user.IsBot)
            {
                return new SqlError(await NonQueryAsync(new MySqlCommand($"UPDATE `accounts` SET `{column}`='{value}' WHERE ID='{user.Id}'")));
            }
            else { return new SqlError(false, "Robots are not supported."); }
        }

        public async Task DropUserAsync(SocketUser user)
        {
            await NonQueryAsync(new MySqlCommand($"DELETE FROM `accounts` WHERE `ID` = {user.Id}; DELETE FROM `commandusage` WHERE `UserID` = {user.Id}"));
        }
        public async Task UpdateUserAsync(SkuldUser user)
        {
            var command = new MySqlCommand("UPDATE `accounts` " +
                "SET `Username` = @username, `Money` = @money, " +
                "`Description` = @description, `Daily` = @daily, " +
                "`LuckFactor` = @luckfactor, `Language` = @language, " +
                "`DMEnabled` = @dmenabled, `Petted` = @petted, " +
                "`Pets` = @pets, `HP` = @hp, `GlaredAt` = @glaredat, " +
                "`Glares` = @glares WHERE ID = @userID ");

            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@money", user.Money);
            command.Parameters.AddWithValue("@description", user.Description);
            command.Parameters.AddWithValue("@daily", user.Daily);
            command.Parameters.AddWithValue("@luckfactor", user.LuckFactor);
            command.Parameters.AddWithValue("@language", user.Language);
            command.Parameters.AddWithValue("@dmenabled", user.DMEnabled);
            command.Parameters.AddWithValue("@petted", user.Petted);
            command.Parameters.AddWithValue("@pets", user.Pets);
            command.Parameters.AddWithValue("@hp", user.HP);
            command.Parameters.AddWithValue("@glaredat", user.GlaredAt);
            command.Parameters.AddWithValue("@glares", user.Glares);
            command.Parameters.AddWithValue("@userID", user.ID);

            using (var conn = new MySqlConnection(cs))
            {
                await conn.OpenAsync();
                if (conn.State == ConnectionState.Open)
                {
                    command.Connection = conn;

                    await command.ExecuteNonQueryAsync();

                    await conn.CloseAsync();
                }
            }
        }
        public async Task UpdateGuildAsync(SkuldGuild guild)
        {
            var command = new MySqlCommand("UPDATE `guild` " +
                "SET `Name` = @name, `JoinMessage` = @jmsg, " +
                "`LeaveMessage` = @lmsg, `AutoJoinRole` = @ajr, " +
                "`Prefix` = @prefix, `Language` = @language, " +
                "`TwitchNotifChannel` = @twitchchan, `TwitterLogChannel` = @twitchan, " +
                "`MutedRole` = @mutedr, `UserJoinChan` = @ujc, `UserLeaveChan` = @ulc " +
                "WHERE ID = @guildID; UPDATE `guildcommandmodules` " +
                "SET `Accounts` = @accounts, `Actions` = @actions, " +
                "`Admin` = @admin, `Fun` = @fun, " +
                "`Help` = @help, `Information` = @info, " +
                "`Search` = @search, `Stats` = @stats " +
                "WHERE ID = @guildID; UPDATE `guildfeaturemodules` " +
                "SET `Pinning` = @pin, `Experience` = @xp " +
                "WHERE ID = @guildID");

            command.Parameters.AddWithValue("@name", guild.Name);
            command.Parameters.AddWithValue("@jmsg", guild.JoinMessage);
            command.Parameters.AddWithValue("@lmsg", guild.LeaveMessage);
            command.Parameters.AddWithValue("@ajr", guild.AutoJoinRole);
            command.Parameters.AddWithValue("@prefix", guild.Prefix);
            command.Parameters.AddWithValue("@language", guild.Language);
            command.Parameters.AddWithValue("@twitchchan", guild.TwitchNotifChannel);
            command.Parameters.AddWithValue("@twitchan", guild.TwitterLogChannel);
            command.Parameters.AddWithValue("@mutedr", guild.MutedRole);
            command.Parameters.AddWithValue("@ujc", guild.UserJoinChannel);
            command.Parameters.AddWithValue("@ulc", guild.UserLeaveChannel);
            command.Parameters.AddWithValue("@accounts", guild.GuildSettings.Modules.AccountsEnabled);
            command.Parameters.AddWithValue("@actions", guild.GuildSettings.Modules.ActionsEnabled);
            command.Parameters.AddWithValue("@admin", guild.GuildSettings.Modules.AdminEnabled);
            command.Parameters.AddWithValue("@fun", guild.GuildSettings.Modules.FunEnabled);
            command.Parameters.AddWithValue("@help", guild.GuildSettings.Modules.HelpEnabled);
            command.Parameters.AddWithValue("@info", guild.GuildSettings.Modules.InformationEnabled);
            command.Parameters.AddWithValue("@search", guild.GuildSettings.Modules.SearchEnabled);
            command.Parameters.AddWithValue("@stats", guild.GuildSettings.Modules.StatsEnabled);
            command.Parameters.AddWithValue("@pin", guild.GuildSettings.Features.Pinning);
            command.Parameters.AddWithValue("@xp", guild.GuildSettings.Features.Experience);
            command.Parameters.AddWithValue("@guildID", guild.ID);

            using (var conn = new MySqlConnection(cs))
            {
                await conn.OpenAsync();
                if (conn.State == ConnectionState.Open)
                {
                    command.Connection = conn;
                    
                    await command.ExecuteNonQueryAsync();

                    await conn.CloseAsync();
                }
            }
        }		

		public async Task<IReadOnlyList<Pasta>> GetAllPastasAsync()
		{
			var command = new MySqlCommand("SELECT * FROM `pasta`;");
			var allpasta = new List<Pasta>();
			using (var conn = new MySqlConnection(cs))
			{
				await conn.OpenAsync();
				if (conn.State == ConnectionState.Open)
				{
					command.Connection = conn;

					StatsdClient.DogStatsd.Increment("mysql.queries");

					var reader = await command.ExecuteReaderAsync();

					int rows = 0;

					rows = reader.Depth + 1;

					StatsdClient.DogStatsd.Set("mysql.rows-ret", rows);

					if (reader.HasRows)
					{
						while (await reader.ReadAsync())
						{
							var pasta = new Pasta
							{
								ID = Tools.ParseUInt32OrDefault(Convert.ToString(reader["ID"])),

								OwnerID = Tools.ParseUInt64OrDefault(Convert.ToString(reader["OwnerID"])),

								Name = Convert.ToString(reader["PastaName"]),

								Created = Convert.ToString(reader["Created"]),

								Content = Convert.ToString(reader["Content"]),

								Downvotes = Tools.ParseUInt32OrDefault(Convert.ToString(reader["Downvotes"])),

								Upvotes = Tools.ParseUInt32OrDefault(Convert.ToString(reader["Upvotes"]))
							};

							allpasta.Add(pasta);

							pasta = null;
						}

						await conn.CloseAsync();
					}
					else
					{
						await conn.CloseAsync();
						return null;
					}
				}
			}
			return allpasta;
		}
    }
}
