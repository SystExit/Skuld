using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using System;
using Skuld.Models;
using Discord.WebSocket;
using System.Collections.Generic;
using Skuld.Tools;
using Discord;
using StatsdClient;
using System.Threading;

namespace Skuld.Services
{
    public class DatabaseService
    {
		readonly LoggingService logger;
		readonly Locale local;
		readonly DiscordShardedClient client;

        private static string cs = $@"server={Bot.Configuration.SQL.Host};user={Bot.Configuration.SQL.Username};password={Bot.Configuration.SQL.Password};database={Bot.Configuration.SQL.Database};charset=utf8mb4";

		public bool CanConnect = false;

		public DatabaseService(LoggingService log,
			DiscordShardedClient cli,
			Locale loc) //inherits from depinjection
		{
			logger = log;
			client = cli;
			local = loc;
		}

		public async Task CheckConnectionAsync()
		{
			bool canconnect = false;
			using (var conn = new MySqlConnection(cs))
			{
				try
				{
					await conn.OpenAsync();
					if (conn.State == System.Data.ConnectionState.Open)
						canconnect = true;
					else
						canconnect = false;
				}
				catch
				{
					canconnect = false;
				}
				finally
				{
					await conn.CloseAsync();
				}
			}
			CanConnect = canconnect;
		}
		
        async Task<SqlResult> SingleQueryAsync(MySqlCommand command)
        {
			if(CanConnect)
			{
				using (var conn = new MySqlConnection(cs))
				{
					await conn.OpenAsync();
					if (conn.State == System.Data.ConnectionState.Open)
					{
						command.Connection = conn;
						try
						{
							var resp = await command.ExecuteScalarAsync();
							DogStatsd.Increment("mysql.queries");
							if (resp == null)
							{
								DogStatsd.Increment("mysql.insert");
								await conn.CloseAsync();
								return new SqlResult
								{
									Successful = true
								};
							}
							else
							{
								await conn.CloseAsync();
								return new SqlResult
								{
									Data = Convert.ToString(resp),
									Successful = true
								};
							}
						}
						catch (Exception ex)
						{
							await logger.AddToLogsAsync(new Models.LogMessage("SQLSngl", "Error with SQL Command", Discord.LogSeverity.Error, ex));
							await conn.CloseAsync();
							return new SqlResult
							{
								Error = "Error with SQL Command",
								Successful = false,
								Exception = ex
							};
						}
					}
					else
					{
						await conn.CloseAsync();
						return new SqlResult
						{
							Error = "Couldn't open connection",
							Successful = false
						};
					}
				}
			}
			return new SqlResult
			{
				Error = "Database not available.",
				Successful = false
			};
		}

		public async Task<SkuldUser> GetUserAsync(ulong id)
        {
			if(CanConnect)
			{
				var user = new SkuldUser();
				var command = new MySqlCommand("SELECT * FROM `accounts` WHERE ID = @userid");
				command.Parameters.AddWithValue("@userid", id);

				using (var conn = new MySqlConnection(cs))
				{
					await conn.OpenAsync();
					if (conn.State == System.Data.ConnectionState.Open)
					{
						command.Connection = conn;

						DogStatsd.Increment("mysql.queries");

						var reader = await command.ExecuteReaderAsync();

						int rows = 0;

						if (reader.HasRows)
						{
							while (await reader.ReadAsync())
							{
								rows++;

								user.ID = Tools.Tools.ParseUInt64OrDefault(Convert.ToString(reader["ID"]));

								user.Money = Tools.Tools.ParseUInt64OrDefault(Convert.ToString(reader["money"]));

								user.Description = Convert.ToString(reader["description"]);

								user.Language = Convert.ToString(reader["language"]);

								user.Daily = Convert.ToString(reader["daily"]);

								user.LuckFactor = Convert.ToDouble(reader["luckfactor"]);

								user.DMEnabled = Convert.ToBoolean(reader["dmenabled"]);

								user.Petted = Tools.Tools.ParseUInt32OrDefault(Convert.ToString(reader["petted"]));

								user.Pets = Tools.Tools.ParseUInt32OrDefault(Convert.ToString(reader["pets"]));

								user.HP = Tools.Tools.ParseUInt32OrDefault(Convert.ToString(reader["hp"]));

								user.GlaredAt = Tools.Tools.ParseUInt32OrDefault(Convert.ToString(reader["glaredat"]));

								user.Glares = Tools.Tools.ParseUInt32OrDefault(Convert.ToString(reader["glares"]));
							}

							DogStatsd.Increment("mysql.rows_ret", rows);

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
					if (conn.State == System.Data.ConnectionState.Open)
					{
						command.Connection = conn;

						DogStatsd.Increment("mysql.queries");

						var reader = await command.ExecuteReaderAsync();

						int rows = 0;


						if (reader.HasRows)
						{
							while (await reader.ReadAsync())
							{
								rows++;

								user.FavCmd = Convert.ToString(reader["command"]);

								user.FavCmdUsg = Tools.Tools.ParseUInt64OrDefault(Convert.ToString(reader["UserUsage"]));
							}
							DogStatsd.Increment("mysql.rows_ret", rows);

							await conn.CloseAsync();
						}
					}
				}

				return user;
			}
			return null;
        }

        public async Task<Pasta> GetPastaAsync(string name)
		{
			if(CanConnect)
			{
				var pasta = new Pasta();
				var command = new MySqlCommand("SELECT * FROM pasta WHERE pastaname = @pastaname");
				command.Parameters.AddWithValue("@pastaname", name);

				using (var conn = new MySqlConnection(cs))
				{
					await conn.OpenAsync();
					if (conn.State == System.Data.ConnectionState.Open)
					{
						command.Connection = conn;

						DogStatsd.Increment("mysql.queries");

						var reader = await command.ExecuteReaderAsync();

						int rows = 0;

						if (reader.HasRows)
						{
							while (await reader.ReadAsync())
							{
								rows++;
								pasta.Name = reader["pastaname"].ToString();
								pasta.Content = reader["content"].ToString();
								pasta.OwnerID = Convert.ToUInt64(reader["ownerid"].ToString());
								pasta.Created = reader["created"].ToString();
								pasta.Upvotes = Convert.ToUInt32(reader["upvotes"].ToString());
								pasta.Downvotes = Convert.ToUInt32(reader["downvotes"].ToString());
							}

							DogStatsd.Increment("mysql.rows_ret", rows);

							await conn.CloseAsync();
						}
					}
				}
				if (!String.IsNullOrEmpty(pasta.Name))
					return pasta;
				else
					return null;
			}
			return null;
		}
		public async Task<IReadOnlyList<Pasta>> GetAllPastasAsync()
		{
			if (CanConnect)
			{
				var command = new MySqlCommand("SELECT * FROM `pasta`;");
				var allpasta = new List<Pasta>();
				using (var conn = new MySqlConnection(cs))
				{
					await conn.OpenAsync();
					if (conn.State == System.Data.ConnectionState.Open)
					{
						command.Connection = conn;

						DogStatsd.Increment("mysql.queries");

						var reader = await command.ExecuteReaderAsync();

						int rows = 0;

						if (reader.HasRows)
						{
							while (await reader.ReadAsync())
							{
								rows++;

								var pasta = new Pasta
								{
									ID = Tools.Tools.ParseUInt32OrDefault(Convert.ToString(reader["ID"])),

									OwnerID = Tools.Tools.ParseUInt64OrDefault(Convert.ToString(reader["OwnerID"])),

									Name = Convert.ToString(reader["PastaName"]),

									Created = Convert.ToString(reader["Created"]),

									Content = Convert.ToString(reader["Content"]),

									Downvotes = Tools.Tools.ParseUInt32OrDefault(Convert.ToString(reader["Downvotes"])),

									Upvotes = Tools.Tools.ParseUInt32OrDefault(Convert.ToString(reader["Upvotes"]))
								};

								allpasta.Add(pasta);

								pasta = null;
							}

							DogStatsd.Increment("mysql.rows_ret", rows);

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
			return null;
		}

		public async Task<CustomCommand> GetCustomCommandAsync(ulong GuildID, string Command)
		{
			if(CanConnect)
			{
				var cmd = new CustomCommand();
				var command = new MySqlCommand("SELECT * FROM `customcommand` WHERE GuildID = @guildID AND CommandName = @command");

				command.Parameters.AddWithValue("@guildID", GuildID);
				command.Parameters.AddWithValue("@command", Command);


				using (var conn = new MySqlConnection(cs))
				{
					await conn.OpenAsync();
					if (conn.State == System.Data.ConnectionState.Open)
					{
						command.Connection = conn;

						DogStatsd.Increment("mysql.queries");

						var reader = await command.ExecuteReaderAsync();

						int rows = 0;

						if (reader.HasRows)
						{
							while (await reader.ReadAsync())
							{
								rows++;

								cmd.GuildID = Tools.Tools.ParseUInt64OrDefault(Convert.ToString(reader["GuildID"]));

								cmd.Content = Convert.ToString(reader["Content"]);

								cmd.CommandName = Convert.ToString(reader["CommandName"]);
							}
							DogStatsd.Increment("mysql.rows_ret", rows);

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
			return null;
		}

		public async Task<SkuldGuild> GetGuildAsync(ulong id)
		{
			if (CanConnect)
			{
				var guild = await GetBaseGuildAsync(id);
				if(guild != null)
				{

					var guildSetts = new GuildSettings();

					var comSetts = await GetGuildCommandModulesAsync(id);
					var featSetts = await GetFeatureModulesAsync(id);

					guildSetts.Modules = comSetts;
					guildSetts.Features = featSetts;
					guild.GuildSettings = guildSetts;

					return guild;
				}
			}
			return null;
		}
		async Task<SkuldGuild> GetBaseGuildAsync(ulong id)
		{
			if (CanConnect)
			{
				try
				{
					var guild = new SkuldGuild();
					var command = new MySqlCommand("SELECT * FROM `guild` WHERE ID = @guildid");
					command.Parameters.AddWithValue("@guildid", id);

					using (var conn = new MySqlConnection(cs))
					{
						await conn.OpenAsync();
						if (conn.State == System.Data.ConnectionState.Open)
						{
							command.Connection = conn;

							DogStatsd.Increment("mysql.queries");

							var reader = await command.ExecuteReaderAsync();

							int rows = 0;

							if (reader.HasRows)
							{
								while (await reader.ReadAsync())
								{
									rows++;

									guild.ID = Tools.Tools.ParseUInt64OrDefault(Convert.ToString(reader["ID"]));

									guild.Name = Convert.ToString(reader["name"]);

									guild.JoinMessage = Convert.ToString(reader["joinmessage"]);

									guild.LeaveMessage = Convert.ToString(reader["leavemessage"]);

									guild.AutoJoinRole = Tools.Tools.ParseUInt64OrDefault(Convert.ToString(reader["autojoinrole"]));

									guild.Prefix = Convert.ToString(reader["prefix"]);

									guild.Language = Convert.ToString(reader["language"]);

									//guild.JoinableRoles = Convert.ToString(reader["joinableroles"]).Split(',');

									guild.TwitchNotifChannel = Tools.Tools.ParseUInt64OrDefault(Convert.ToString(reader["twitchnotifchannel"]));

									guild.TwitterLogChannel = Tools.Tools.ParseUInt64OrDefault(Convert.ToString(reader["twitterlogchannel"]));

									guild.MutedRole = Tools.Tools.ParseUInt64OrDefault(Convert.ToString(reader["mutedrole"]));

									guild.AuditChannel = Tools.Tools.ParseUInt64OrDefault(Convert.ToString(reader["auditchannel"]));

									guild.UserJoinChannel = Tools.Tools.ParseUInt64OrDefault(Convert.ToString(reader["userjoinchan"]));

									guild.UserLeaveChannel = Tools.Tools.ParseUInt64OrDefault(Convert.ToString(reader["userleavechan"]));
								}

								DogStatsd.Increment("mysql.rows_ret", rows);

								await conn.CloseAsync();

								guild.GuildSettings = new GuildSettings();
							}
						}
					}
					if(guild.ID != 0)
						return guild;
				}
				catch(Exception ex)
				{
					await logger.AddToLogsAsync(new Models.LogMessage("DBService", ex.Message, LogSeverity.Error, ex));
				}				
			}
			return null;
		}
		async Task<GuildCommandModules> GetGuildCommandModulesAsync(ulong id)
		{
			var comSetts = new GuildCommandModules();

			var command = new MySqlCommand("SELECT * FROM `guildcommandmodules` WHERE ID = @guildid");
			command.Parameters.AddWithValue("@guildid", id);

			using (var conn = new MySqlConnection(cs))
			{
				await conn.OpenAsync();
				if (conn.State == System.Data.ConnectionState.Open)
				{
					command.Connection = conn;

					DogStatsd.Increment("mysql.queries");

					var reader = await command.ExecuteReaderAsync();

					int rows = 0;

					if (reader.HasRows)
					{
						while (await reader.ReadAsync())
						{
							rows++;
							comSetts.AccountsEnabled = Convert.ToBoolean(reader["accounts"]);

							comSetts.ActionsEnabled = Convert.ToBoolean(reader["actions"]);

							comSetts.AdminEnabled = Convert.ToBoolean(reader["admin"]);

							comSetts.FunEnabled = Convert.ToBoolean(reader["fun"]);

							comSetts.HelpEnabled = Convert.ToBoolean(reader["help"]);

							comSetts.InformationEnabled = Convert.ToBoolean(reader["information"]);

							comSetts.SearchEnabled = Convert.ToBoolean(reader["search"]);

							comSetts.StatsEnabled = Convert.ToBoolean(reader["stats"]);
						}
						DogStatsd.Increment("mysql.rows_ret", rows);

						await conn.CloseAsync();
					}
				}
			}

			return comSetts;
		}
		async Task<GuildFeatureModules> GetFeatureModulesAsync(ulong id)
		{
			var featSetts = new GuildFeatureModules();

			var command = new MySqlCommand("SELECT * FROM `guildfeaturemodules` WHERE ID = @guildid");
			command.Parameters.AddWithValue("@guildid", id);

			using (var conn = new MySqlConnection(cs))
			{
				await conn.OpenAsync();
				if (conn.State == System.Data.ConnectionState.Open)
				{
					command.Connection = conn;

					DogStatsd.Increment("mysql.queries");

					var reader = await command.ExecuteReaderAsync();

					int rows = 0;

					if (reader.HasRows)
					{
						while (await reader.ReadAsync())
						{
							rows++;

							featSetts.Pinning = Convert.ToBoolean(reader["pinning"]);

							featSetts.Experience = Convert.ToBoolean(reader["experience"]);
						}
						DogStatsd.Increment("mysql.rows_ret", rows);

						await conn.CloseAsync();
					}
				}
			}
			return featSetts;
		}

		public async Task<SqlResult> InsertAdvancedSettingsAsync(bool feature, IGuild guild)
		{
			if(CanConnect)
			{
				var command = new MySqlCommand("INSERT INTO ");
				if (feature)
				{
					command.CommandText += "`guildfeaturemodules` (`ID`,`Experience`,`UserJoinLeave`) VALUES (@guildid,0,0);";
					command.Parameters.AddWithValue("@guildid", guild.Id);
				}
				else
				{
					command.CommandText += "`guildcommandmodules` (`ID`,`Accounts`, `Actions`,`Admin`,`Fun`,`Help`,`Information`,`Search`,`Stats`) VALUES (@guildid,1,1,1,1,1,1,1,1);";
					command.Parameters.AddWithValue("@guildid", guild.Id);
				}
				return await SingleQueryAsync(command);
			}
			return new SqlResult
			{
				Error = "Database not available.",
				Successful = false
			};
		}
        public async Task<SqlResult> InsertUserAsync(IUser user)
		{
			if(CanConnect)
			{
				if (!user.IsBot)
				{
					var command = new MySqlCommand("INSERT IGNORE INTO `accounts` (`ID`, `description`, `language`) VALUES (@userid , \"I have no description\", @locale);");
					command.Parameters.AddWithValue("@userid", user.Id);
					command.Parameters.AddWithValue("@locale", local.defaultLocale);
					return await SingleQueryAsync(command);
				}
				else
				{
					return new SqlResult
					{
						Error = "Robots are not supported.",
						Successful = false
					};
				}
			}
			return new SqlResult
			{
				Error = "Database not available.",
				Successful = false
			};
		}
		public async Task<SqlResult> InsertCustomCommand(IGuild guild, string command, string content)
		{
			if (CanConnect)
			{
				var cmd = new MySqlCommand("INSERT INTO `customcommand` ( `Content`, `GuildID`, `CommandName` ) VALUES ( @newcontent , @guildID , @commandName ) ;");
				cmd.Parameters.AddWithValue("@newcontent", content);
				cmd.Parameters.AddWithValue("@guildID", guild.Id);
				cmd.Parameters.AddWithValue("@commandName", command);
				return await SingleQueryAsync(cmd);
			}
			return new SqlResult
			{
				Error = "Database not available.",
				Successful = false
			};
		}
		public async Task<IReadOnlyList<SqlResult>> InsertGuildAsync(IGuild guild)
		{
			if (CanConnect)
			{
				List<SqlResult> results = new List<SqlResult>();

				var gcmd = new MySqlCommand("INSERT IGNORE INTO `guild` (`ID`,`name`,`prefix`) VALUES ");
				gcmd.CommandText += $"( {guild.Id} , \"{guild.Name.Replace("\"", "\\").Replace("\'", "\\'")}\" ,\"{Bot.Configuration.Discord.Prefix}\" )";

				results.Add(await SingleQueryAsync(gcmd));
				results.Add(await InsertAdvancedSettingsAsync(false, guild));
				results.Add(await InsertAdvancedSettingsAsync(true, guild));

				return results;
			}
			return new List<SqlResult>
			{
				new SqlResult
				{
					Error = "Database not available.",
					Successful = false
				}
			};
		}
		public async Task<SqlResult> InsertPastaAsync(IUser user, string pastaname, string content)
		{
			if (CanConnect)
			{
				content = content.Replace("\'", "\\\'");
				content = content.Replace("\"", "\\\"");

				var command = new MySqlCommand("INSERT INTO pasta (content,ownerid,created,pastaname) VALUES ( @content , @ownerid , @created , @pastatitle )");
				command.Parameters.AddWithValue("@content", content);
				command.Parameters.AddWithValue("@ownerid", user.Id);
				command.Parameters.AddWithValue("@created", DateTime.UtcNow);
				command.Parameters.AddWithValue("@pastatitle", pastaname);

				return await SingleQueryAsync(command);
			}
			return new SqlResult
			{
				Error = "Database not available.",
				Successful = false
			};
		}

		public async Task<SqlResult> DropGuildAsync(IGuild guild)
		{
			if (CanConnect)
			{
				return await SingleQueryAsync(new MySqlCommand($"DELETE FROM `guild` WHERE `ID` = {guild.Id}; DELETE FROM `guildcommandmodules` WHERE `ID` = {guild.Id}; DELETE FROM `guildfeaturemodules` WHERE `ID` = {guild.Id}; DELETE FROM `customcommand` WHERE `GuildID` = {guild.Id};"));
			}
			return new SqlResult
			{
				Error = "Database not available.",
				Successful = false
			};
		}
        public async Task<SqlResult> DropUserAsync(IUser user)
        {
			if (CanConnect)
			{
				return await SingleQueryAsync(new MySqlCommand($"DELETE FROM `accounts` WHERE `ID` = {user.Id}; DELETE FROM `commandusage` WHERE `UserID` = {user.Id}"));
			}
			return new SqlResult
			{
				Error = "Database not available.",
				Successful = false
			};
		}
		public async Task<SqlResult> DropCustomCommand(IGuild guild, string command)
		{
			if (CanConnect)
			{
				var cmd = new MySqlCommand("DELETE FROM `customcommand` WHERE `GuildID` = @guildID AND `CommandName` = @commandName ;");
				cmd.Parameters.AddWithValue("@guildID", guild.Id);
				cmd.Parameters.AddWithValue("@commandName", command);
				return await SingleQueryAsync(cmd);
			}
			return new SqlResult
			{
				Error = "Database not available.",
				Successful = false
			};
		}
		public async Task<SqlResult> DropPastaAsync(string title)
		{
			if (CanConnect)
			{
				var cmd = new MySqlCommand("DELETE FROM `pasta` WHERE pastaname = @pastaname;");
				cmd.Parameters.AddWithValue("@pastaname", title);
				return await SingleQueryAsync(cmd);
			}
			return new SqlResult
			{
				Error = "Database not available.",
				Successful = false
			};
		}

        public async Task<SqlResult> UpdateUserAsync(SkuldUser user)
        {
			if (CanConnect)
			{
				var command = new MySqlCommand("UPDATE `accounts` " +
					"SET `Money` = @money, " +
					"`Description` = @description, `Daily` = @daily, " +
					"`LuckFactor` = @luckfactor, `Language` = @language, " +
					"`DMEnabled` = @dmenabled, `Petted` = @petted, " +
					"`Pets` = @pets, `HP` = @hp, `GlaredAt` = @glaredat, " +
					"`Glares` = @glares WHERE ID = @userID ");
				
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

				return await SingleQueryAsync(command);
			}
			return new SqlResult
			{
				Error = "Database not available.",
				Successful = false
			};
		}
		public async Task<SqlResult> UpdateUserUsageAsync(SkuldUser user, string command)
		{
			if(CanConnect)
			{
				var cmd = new MySqlCommand("SELECT UserUsage from commandusage WHERE UserID = @userID AND Command = @command");
				cmd.Parameters.AddWithValue("@userID", user.ID);
				cmd.Parameters.AddWithValue("@command", command);
				var resp = await SingleQueryAsync(cmd);

				if(resp.Successful)
				{
					if (!String.IsNullOrEmpty(Convert.ToString(resp.Data)))
					{
						var cmdusg = Convert.ToInt32(resp.Data);
						cmdusg = cmdusg + 1;
						cmd = new MySqlCommand("UPDATE commandusage SET UserUsage = @userusg WHERE UserID = @userid AND Command = @command");
						cmd.Parameters.AddWithValue("@userusg", cmdusg);
						cmd.Parameters.AddWithValue("@userid", user.ID);
						cmd.Parameters.AddWithValue("@command", command);
						return await SingleQueryAsync(cmd);
					}
					else
					{
						cmd = new MySqlCommand("INSERT INTO commandusage (`UserID`, `UserUsage`, `Command`) VALUES (@userid , @userusg , @command)");
						cmd.Parameters.AddWithValue("@userusg", 1);
						cmd.Parameters.AddWithValue("@userid", user.ID);
						cmd.Parameters.AddWithValue("@command", command);
						return await SingleQueryAsync(cmd);
					}
				}
				else
				{
					return resp;
				}
			}
			return new SqlResult
			{
				Error = "Database not available.",
				Successful = false
			};
		}

		public async Task<SqlResult> UpdateCustomCommand(IGuild guild, string command, string content)
		{
			if (CanConnect)
			{
				var cmd = new MySqlCommand("UPDATE `customcommand` SET `Content` = @newcontent WHERE `GuildID` = @guildID AND `CommandName` = @commandName ;");
				cmd.Parameters.AddWithValue("@newcontent", content);
				cmd.Parameters.AddWithValue("@guildID", guild.Id);
				cmd.Parameters.AddWithValue("@commandName", command);
				return await SingleQueryAsync(cmd);
			}
			return new SqlResult
			{
				Error = "Database not available.",
				Successful = false
			};
		}

		public async Task<SqlResult> UpdatePastaAsync(Pasta pasta, IUser user = null)
		{
			if (CanConnect)
			{
				if(user == null)
				{
					var command = new MySqlCommand("UPDATE pasta SET content = @content, upvotes = @upvotes, downvotes = @downvotes WHERE pastaname = @title");
					command.Parameters.AddWithValue("@content", pasta.Content);
					command.Parameters.AddWithValue("@downvotes", pasta.Downvotes);
					command.Parameters.AddWithValue("@upvotes", pasta.Upvotes);
					command.Parameters.AddWithValue("@title", pasta.Name);

					return await SingleQueryAsync(command);
				}
				else
				{
					var command = new MySqlCommand("UPDATE pasta SET content = @content, upvotes = @upvotes, downvotes = @downvotes, ownerID = @ownerid WHERE pastaname = @title");
					command.Parameters.AddWithValue("@content", pasta.Content);
					command.Parameters.AddWithValue("@downvotes", pasta.Downvotes);
					command.Parameters.AddWithValue("@upvotes", pasta.Upvotes);
					command.Parameters.AddWithValue("@ownerid", user.Id);
					command.Parameters.AddWithValue("@title", pasta.Name);

					return await SingleQueryAsync(command);
				}
			}
			return new SqlResult
			{
				Error = "Database not available.",
				Successful = false
			};
		}

		public async Task<IReadOnlyList<SqlResult>> UpdateGuildAsync(SkuldGuild guild)
		{
			if (CanConnect)
			{
				var results = new List<SqlResult>
				{
					await UpdateBaseGuildAsync(guild),
					await UpdateFeaturesAsnyc(guild.ID, guild.GuildSettings.Features),
					await UpdateCommandModulesAsync(guild.ID, guild.GuildSettings.Modules)
				};

				return results;
			}
			return new List<SqlResult>
			{
				new SqlResult
				{
					Error = "Database not available.",
					Successful = false
				}
			};
		}
		async Task<SqlResult> UpdateBaseGuildAsync(SkuldGuild guild)
		{
			if(CanConnect)
			{
				var command = new MySqlCommand("UPDATE `guild` " +
					"SET `Name` = @name, `JoinMessage` = @jmsg, " +
					"`LeaveMessage` = @lmsg, `AutoJoinRole` = @ajr, " +
					"`Prefix` = @prefix, `Language` = @language, " +
					"`TwitchNotifChannel` = @twitchchan, `TwitterLogChannel` = @twitchan, " +
					"`MutedRole` = @mutedr, `UserJoinChan` = @ujc, `UserLeaveChan` = @ulc " +
					"WHERE ID = @guildID;");

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
				command.Parameters.AddWithValue("@guildID", guild.ID);

				return await SingleQueryAsync(command);
			}
			return new SqlResult
			{
				Error = "Database not available.",
				Successful = false
			};
		}
		async Task<SqlResult> UpdateCommandModulesAsync(ulong id, GuildCommandModules modules)
		{
			if (CanConnect)
			{
				var command = new MySqlCommand("UPDATE `guildcommandmodules` " +
						"SET `Accounts` = @accounts, `Actions` = @actions, " +
						"`Admin` = @admin, `Fun` = @fun, " +
						"`Help` = @help, `Information` = @info, " +
						"`Search` = @search, `Stats` = @stats " +
						"WHERE ID = @guildID;");

				command.Parameters.AddWithValue("@accounts", modules.AccountsEnabled);
				command.Parameters.AddWithValue("@actions", modules.ActionsEnabled);
				command.Parameters.AddWithValue("@admin", modules.AdminEnabled);
				command.Parameters.AddWithValue("@fun", modules.FunEnabled);
				command.Parameters.AddWithValue("@help", modules.HelpEnabled);
				command.Parameters.AddWithValue("@info", modules.InformationEnabled);
				command.Parameters.AddWithValue("@search", modules.SearchEnabled);
				command.Parameters.AddWithValue("@stats", modules.StatsEnabled);
				command.Parameters.AddWithValue("@guildID", id);

				return await SingleQueryAsync(command);
			}
			return new SqlResult
			{
				Error = "Database not available.",
				Successful = false
			};
		}
		async Task<SqlResult> UpdateFeaturesAsnyc(ulong id, GuildFeatureModules features)
		{
			if (CanConnect)
			{
				var command = new MySqlCommand(" UPDATE `guildfeaturemodules` " +
					"SET `Pinning` = @pin, `Experience` = @xp " +
					"WHERE ID = @guildID");

				command.Parameters.AddWithValue("@pin", features.Pinning);
				command.Parameters.AddWithValue("@xp", features.Experience);
				command.Parameters.AddWithValue("@guildID", id);

				return await SingleQueryAsync(command);
			}
			return new SqlResult
			{
				Error = "Database not available.",
				Successful = false
			};
		}
		
		public async Task<IReadOnlyList<SqlResult>> CheckGuildUsersAsync(IGuild guild)
		{
			if (CanConnect)
			{
				var results = new List<SqlResult>();
				await guild.DownloadUsersAsync();
				foreach (var user in await guild.GetUsersAsync())
				{
					var discord = client.GetUser(user.Id);
					var db = await GetUserAsync(user.Id);
					if (discord == null && db != null)
					{
						results.Add(await DropUserAsync(user));
					}
					else if (discord != null && db == null)
					{
						results.Add(await InsertUserAsync(discord));
					}
				}
				return results;
			}
			return new List<SqlResult>
			{
				new SqlResult
				{
					Error = "Database not available.",
					Successful = false
				}
			};
		}
		public async Task<IReadOnlyList<SqlResult>> RebuildGuildAsync(IGuild guild)
		{
			if (CanConnect)
			{
				var results = new List<SqlResult>();
				var gcmd = new MySqlCommand("INSERT IGNORE INTO `guild` (`ID`,`name`,`prefix`) VALUES ");
				gcmd.CommandText += $"( {guild.Id} , \"{guild.Name.Replace("\"", "\\").Replace("\'", "\\'")}\" ,\"{Bot.Configuration.Discord.Prefix}\" )";
				results.Add(await SingleQueryAsync(gcmd));

				//Configures Modules
				gcmd = new MySqlCommand("INSERT IGNORE INTO `guildcommandmodules` " +
					"(`ID`,`Accounts`,`Actions`,`Admin`,`Fun`,`Help`,`Information`,`Search`,`Stats`) " +
					$"VALUES ( {guild.Id} , 1, 1, 1, 1, 1, 1, 1, 1 )");
				results.Add(await SingleQueryAsync(gcmd));

				//Configures Settings
				gcmd = new MySqlCommand("INSERT IGNORE INTO `guildfeaturemodules` " + "(`ID`,`Starboard`,`Pinning`,`Experience`,`UserJoinLeave`,`UserModification`,`UserBanEvents`,`GuildModification`,`GuildChannelModification`,`GuildRoleModification`) " +
					$"VALUES ( {guild.Id} , 0, 0, 0, 0, 0, 0, 0, 0, 0 )");
				results.Add(await SingleQueryAsync(gcmd));

				results.AddRange(await CheckGuildUsersAsync(guild));

				return results;
			}
			return new List<SqlResult>
			{
				new SqlResult
				{
					Error = "Database not available.",
					Successful = false
				}
			};
		}
		public async Task<IReadOnlyList<SqlResult>> PopulateGuildsAsync()
		{
			if(CanConnect)
			{
				var results = new List<SqlResult>();
				foreach (var guild in client.Guilds)
				{
					results.AddRange(await CheckGuildUsersAsync(guild));
				}
				return results;
			}
			return new List<SqlResult>
			{
				new SqlResult
				{
					Error = "Database not available.",
					Successful = false
				}
			};
		}
	}
}
