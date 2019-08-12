using Discord;
using Discord.WebSocket;
using MySql.Data.MySqlClient;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Globalization;
using Skuld.Core.Models;
using Skuld.Core.Models.Skuld;
using Skuld.Core.Utilities;
using Skuld.Database.Extensions;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skuld.Database
{
    public static class DatabaseClient
    {
        private static string ConnectionString;
        private static DateTime LastCheck;
        private static bool LastResult;
        public static EventResult NoSqlConnection = new EventResult
        {
                Successful = false,
                Error = "MySQL Connection unsuccessful"
        };
        public static EventResult ResultParseError = new EventResult
        {
            Successful = false,
            Error = "Error Parsing Data"
        };
        public static EventResult NoResultsAvailable = new EventResult
        {
            Successful = false,
            Error = "No Results Available"
        };
        public static bool Initialized = false;
        public static bool Enabled = false;

        public static void Initialize(SkuldConfig conf)
        {
            if (conf.SQL.Enabled)
            {
                ConnectionString = $@"server={conf.SQL.Host};user={conf.SQL.Username};password={conf.SQL.Password};database={conf.SQL.Database};charset=utf8mb4;";
                if(!conf.SQL.SSL)
                {
                    ConnectionString += "SslMode=None;";
                }
                Enabled = true;
            }
            else
            {
                LastResult = false;
            }
            Initialized = true;
            CheckConnectionAsync().ConfigureAwait(false);
        }

        public static async Task<bool> CheckConnectionAsync()
        {
            if (Enabled)
            {
                if (DateTime.UtcNow >= LastCheck.AddMinutes(30))
                {
                    bool canconnect = false;
                    using (var conn = new MySqlConnection(ConnectionString))
                    {
                        try
                        {
                            await conn.OpenAsync();
                            if (conn.State == System.Data.ConnectionState.Open)
                            {
                                canconnect = true;
                                await conn.CloseAsync();
                            }
                        }
                        catch
                        {
                            canconnect = false;
                        }
                        LastCheck = DateTime.UtcNow;
                    }
                    LastResult = canconnect;
                }
            }
            else LastResult = false;

            return LastResult;
        }

        public static async Task<EventResult> SingleQueryAsync(MySqlCommand command)
        {
            if (await CheckConnectionAsync())
            {
                using var conn = new MySqlConnection(ConnectionString);
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
                            if (command.CommandText.StartsWith("INSERT INTO"))
                                DogStatsd.Increment("mysql.insert");

                            await conn.CloseAsync();
                            return EventResult.FromSuccess();
                        }
                        else
                        {
                            await conn.CloseAsync();
                            return EventResult.FromSuccess(resp);
                        }
                    }
                    catch (Exception ex)
                    {
                        await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("SQLSngl", "Error with SQL Statement", LogSeverity.Error, ex));
                        await conn.CloseAsync();
                        return EventResult.FromFailureException("Error with SQL Statement", ex);
                    }
                }
            }
            return NoSqlConnection;
        }

        public static async Task<EventResult> InsertUserAsync(IUser user, string locale = null)
        {
            if (await CheckConnectionAsync())
            {
                if (!user.IsBot && !user.IsWebhook)
                {
                    var command = new MySqlCommand("INSERT IGNORE INTO `users` (`UserID`, `Username`, `Title`, `Language`, `AvatarUrl`) VALUES (@userid , @username, \"\", @locale, @avatarurl);");
                    command.Parameters.AddWithValue("@userid", user.Id);
                    command.Parameters.AddWithValue("@username", user.Username);
                    command.Parameters.AddWithValue("@locale", locale??Locale.defaultLocale);
                    command.Parameters.AddWithValue("@avatarurl", user.GetAvatarUrl());
                    return await SingleQueryAsync(command).ConfigureAwait(false);
                }
                else
                {
                    return EventResult.FromFailure(DiscordTools.NoBotsString);
                }
            }
            return NoSqlConnection;
        }
        public static async Task<IReadOnlyList<EventResult>> InsertGuildAsync(ulong GuildID, string prefix)
        {
            if (await CheckConnectionAsync())
            {
                var results = new List<EventResult>();

                var gcmd = new MySqlCommand("INSERT IGNORE INTO `guilds` (`GuildID`,`prefix`) VALUES ( @GuildID , @prefix );");
                gcmd.Parameters.AddWithValue("@GuildID", GuildID);
                gcmd.Parameters.AddWithValue("@prefix", prefix);

                results.Add(await SingleQueryAsync(gcmd).ConfigureAwait(false));

                gcmd = new MySqlCommand("INSERT IGNORE INTO `guildfeatures` (`GuildID`,`Pinning`,`Experience`) VALUES ( @guildid, 0, 0);");
                gcmd.Parameters.AddWithValue("@GuildID", GuildID);
                results.Add(await SingleQueryAsync(gcmd).ConfigureAwait(false));

                gcmd = new MySqlCommand("INSERT IGNORE INTO `guildmodules` (`GuildID`,`Accounts`, `Actions`,`Admin`,`Custom`,`Fun`,`Information`,`Lewd`,`Search`,`Stats`,`Weeb`) VALUES (@guildid,1,1,1,1,1,1,1,1,1,1);");
                gcmd.Parameters.AddWithValue("@GuildID", GuildID);
                results.Add(await SingleQueryAsync(gcmd).ConfigureAwait(false));

                return results;
            }
            return new List<EventResult> { NoSqlConnection };
        }
        public static async Task<EventResult> InsertPastaAsync(IUser user, string pastaname, string content)
        {
            if (await CheckConnectionAsync())
            {
                content = content.Replace("\'", "\\\'");
                content = content.Replace("\"", "\\\"");

                var command = new MySqlCommand("INSERT INTO `pasta` (Content,OwnerID,Created,Name) VALUES ( @content , @ownerid , @created , @pastatitle )");
                command.Parameters.AddWithValue("@content", content);
                command.Parameters.AddWithValue("@ownerid", user.Id);
                command.Parameters.AddWithValue("@created", DateTime.UtcNow.ToEpoch());
                command.Parameters.AddWithValue("@pastatitle", pastaname);

                return await SingleQueryAsync(command).ConfigureAwait(false);
            }
            return NoSqlConnection;
        }
        public static async Task<EventResult> InsertCustomCommandAsync(IGuild guild, string command, string content)
        {
            if (await CheckConnectionAsync())
            {
                var cmd = new MySqlCommand("INSERT INTO `guildcustomcommands` ( `Content`, `GuildID`, `CommandName` ) VALUES ( @newcontent , @guildID , @commandName ) ;");
                cmd.Parameters.AddWithValue("@newcontent", content);
                cmd.Parameters.AddWithValue("@guildID", guild.Id);
                cmd.Parameters.AddWithValue("@commandName", command);
                return await SingleQueryAsync(cmd).ConfigureAwait(false);
            }
            return NoSqlConnection;
        }

        public static async Task<EventResult> GetUserAsync(ulong UserID)
        {
            if (await CheckConnectionAsync())
            {
                var user = new SkuldUser();
                var command = new MySqlCommand("SELECT * FROM `users` WHERE UserID = @userid");
                command.Parameters.AddWithValue("@userid", UserID);

                using (var conn = new MySqlConnection(ConnectionString))
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

                                user.ID = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["UserID"]));

                                user.Username = Convert.ToString(reader["Username"]);

                                user.Money = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["Money"]));

                                user.Title = Convert.ToString(reader["Title"]);

                                user.Language = Convert.ToString(reader["Language"]);

                                user.Daily = ConversionTools.ParseUInt64OrDefault(reader["LastDaily"].ToString());

                                user.CanDM = Convert.ToBoolean(reader["CanDM"]);

                                user.Banned = Convert.ToBoolean(reader["Banned"]);

                                user.Patted = ConversionTools.ParseUInt32OrDefault(Convert.ToString(reader["Patted"]));

                                user.Pats = ConversionTools.ParseUInt32OrDefault(Convert.ToString(reader["Pats"]));

                                user.HP = ConversionTools.ParseUInt32OrDefault(Convert.ToString(reader["HP"]));

                                user.RecurringBlock = Convert.ToBoolean(reader["RecurringBlock"]);

                                user.UnlockedCustBG = Convert.ToBoolean(reader["UnlockedCustBG"]);

                                user.Background = Convert.ToString(reader["Background"]);

                                user.AvatarUrl = Convert.ToString(reader["AvatarUrl"]);
                            }

                            DogStatsd.Increment("mysql.rows_ret", rows);

                            await conn.CloseAsync();
                        }
                        else
                        {
                            return NoResultsAvailable;
                        }
                    }
                }

                command = new MySqlCommand("SELECT * FROM `usercommandusage` WHERE UserID = @userid ORDER BY `Usage` DESC LIMIT 1");
                command.Parameters.AddWithValue("@userid", UserID);

                using (var conn = new MySqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        command.Connection = conn;

                        DogStatsd.Increment("mysql.queries");

                        var reader = await command.ExecuteReaderAsync();

                        int rows = 0;

                        List<CommandUsage> cmds = new List<CommandUsage>();

                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                rows++;

                                cmds.Add(new CommandUsage
                                {
                                    Command = Convert.ToString(reader["command"]),
                                    Usage = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["Usage"]))
                                });
                            }
                            DogStatsd.Increment("mysql.rows_ret", rows);

                            await conn.CloseAsync();
                        }

                        user.CommandUsage = cmds;
                    }
                }

                command = new MySqlCommand("SELECT * FROM `reputation` WHERE Repee = @userid");
                command.Parameters.AddWithValue("@userid", UserID);

                using (var conn = new MySqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        command.Connection = conn;

                        DogStatsd.Increment("mysql.queries");

                        var reader = await command.ExecuteReaderAsync();

                        int rows = 0;

                        List<Reputation> reps = new List<Reputation>();

                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                rows++;

                                reps.Add(new Reputation
                                {
                                    Reper = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["Reper"])),
                                    Timestamp = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["Timestamp"]))
                                });
                            }
                            DogStatsd.Increment("mysql.rows_ret", rows);

                            await conn.CloseAsync();
                        }

                        user.Reputation = reps;
                    }
                }

                return EventResult.FromSuccess(user);
            }
            return NoSqlConnection;
        }
        public static async Task<EventResult> GetGuildAsync(ulong GuildID)
        {
            if (await CheckConnectionAsync())
            {
                SkuldGuild guild = new SkuldGuild();

                if (await CheckConnectionAsync())
                {
                    var command = new MySqlCommand("SELECT * FROM `guilds` WHERE `GuildID` = @guildid");
                    command.Parameters.AddWithValue("@guildid", GuildID);

                    using (var conn = new MySqlConnection(ConnectionString))
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

                                    guild.ID = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["GuildID"]));

                                    guild.JoinMessage = Convert.ToString(reader["JoinMessage"]);

                                    guild.LeaveMessage = Convert.ToString(reader["LeaveMessage"]);

                                    guild.JoinRole = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["JoinRole"]));

                                    guild.Prefix = Convert.ToString(reader["Prefix"]);

                                    var rawroles = Convert.ToString(reader["JoinableRoles"]);

                                    if (!string.IsNullOrWhiteSpace(rawroles))
                                    {
                                        guild.JoinableRoles = new List<ulong>();

                                        var roles = rawroles.Split(',');

                                        if (roles.Length > 0)
                                        {
                                            for (int x = 0; x < roles.Length; x++)
                                            {
                                                guild.JoinableRoles.Add(Convert.ToUInt64(roles[x]));
                                            }
                                        }
                                        else
                                        {
                                            guild.JoinableRoles = null;
                                        }
                                    }
                                    else
                                    {
                                        guild.JoinableRoles = null;
                                    }

                                    guild.MutedRole = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["MutedRole"]));

                                    guild.UserJoinChannel = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["JoinChannel"]));

                                    guild.UserLeaveChannel = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["LeaveChannel"]));

                                    guild.LevelUpMessage = Convert.ToString(reader["LevelUpMessage"]);

                                    guild.LevelUpChannel = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["LevelUpChannel"]));

                                    var level = ConversionTools.ParseInt32OrDefault(reader["LevelNotification"]);

                                    Enum.TryParse(Convert.ToString(level), out LevelNotification lupnotif);

                                    guild.LevelNotification = lupnotif;
                                }

                                DogStatsd.Increment("mysql.rows_ret", rows);

                                await conn.CloseAsync();
                            }
                        }
                    }
                    if (guild.ID == 0) return NoResultsAvailable;
                }

                if (guild != null)
                {
                    GuildCommandModules comSetts = new GuildCommandModules();
                    var command = new MySqlCommand("SELECT * FROM `guildmodules` WHERE GuildID = @guildid");
                    command.Parameters.AddWithValue("@guildid", GuildID);

                    using (var conn = new MySqlConnection(ConnectionString))
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
                                    comSetts.AccountsEnabled = Convert.ToBoolean(reader["Accounts"]);

                                    comSetts.ActionsEnabled = Convert.ToBoolean(reader["Actions"]);

                                    comSetts.AdminEnabled = Convert.ToBoolean(reader["Admin"]);

                                    comSetts.CustomEnabled = Convert.ToBoolean(reader["Custom"]);

                                    comSetts.FunEnabled = Convert.ToBoolean(reader["Fun"]);

                                    comSetts.InformationEnabled = Convert.ToBoolean(reader["Information"]);

                                    comSetts.LewdEnabled = Convert.ToBoolean(reader["Lewd"]);

                                    comSetts.SearchEnabled = Convert.ToBoolean(reader["Search"]);

                                    comSetts.SpaceEnabled = Convert.ToBoolean(reader["Space"]);

                                    comSetts.StatsEnabled = Convert.ToBoolean(reader["Stats"]);

                                    comSetts.WeebEnabled = Convert.ToBoolean(reader["Weeb"]);
                                }
                                DogStatsd.Increment("mysql.rows_ret", rows);

                                await conn.CloseAsync();
                            }
                        }
                    }

                    GuildFeatureModules featSetts = new GuildFeatureModules();
                    command = new MySqlCommand("SELECT * FROM `guildfeatures` WHERE GuildID = @guildid");
                    command.Parameters.AddWithValue("@guildid", GuildID);

                    using (var conn = new MySqlConnection(ConnectionString))
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

                                    featSetts.Pinning = Convert.ToBoolean(reader["Pinning"]);

                                    featSetts.Experience = Convert.ToBoolean(reader["Experience"]);
                                }
                                DogStatsd.Increment("mysql.rows_ret", rows);

                                await conn.CloseAsync();
                            }
                        }
                    }

                    guild.Modules = comSetts;
                    guild.Features = featSetts;

                    return EventResult.FromSuccess(guild);
                }
            }
            return NoSqlConnection;
        }
        public static async Task<EventResult> GetPastaAsync(string PastaName)
        {
            if (await CheckConnectionAsync())
            {
                var pasta = new Pasta();
                var command = new MySqlCommand("SELECT * FROM `pasta` WHERE Name = @pastaname");
                command.Parameters.AddWithValue("@pastaname", PastaName);

                using (var conn = new MySqlConnection(ConnectionString))
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
                                pasta.Name = reader["Name"].ToString();
                                pasta.Content = reader["Content"].ToString();
                                pasta.OwnerID = Convert.ToUInt64(reader["OwnerID"].ToString());
                                pasta.Created = Convert.ToUInt64(reader["Created"].ToString());
                                pasta.Upvotes = Convert.ToUInt32(reader["UpVotes"].ToString());
                                pasta.Downvotes = Convert.ToUInt32(reader["DownVotes"].ToString());
                            }

                            DogStatsd.Increment("mysql.rows_ret", rows);

                            await conn.CloseAsync();
                        }
                    }
                }
                if (string.IsNullOrEmpty(pasta.Name)) return NoResultsAvailable;

                return EventResult.FromSuccess(pasta);
            }
            return NoSqlConnection;
        }
        public static async Task<EventResult> GetAllPastasAsync()
        {
            if (await CheckConnectionAsync())
            {
                var command = new MySqlCommand("SELECT * FROM `pasta`;");
                var allpasta = new List<Pasta>();
                using (var conn = new MySqlConnection(ConnectionString))
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

                                allpasta.Add(new Pasta
                                {
                                    PastaID = Convert.ToUInt32(reader["PastaID"].ToString()),
                                    Name = reader["Name"].ToString(),
                                    Content = reader["Content"].ToString(),
                                    OwnerID = Convert.ToUInt64(reader["OwnerID"].ToString()),
                                    Created = Convert.ToUInt64(reader["Created"].ToString()),
                                    Upvotes = Convert.ToUInt32(reader["UpVotes"].ToString()),
                                    Downvotes = Convert.ToUInt32(reader["DownVotes"].ToString())
                                });
                            }

                            DogStatsd.Increment("mysql.rows_ret", rows);

                            await conn.CloseAsync();
                        }
                        else
                        {
                            await conn.CloseAsync();
                            return ResultParseError;
                        }
                    }
                }
                return EventResult.FromSuccess(allpasta.AsReadOnly());
            }
            return NoSqlConnection;
        }
        public static async Task<EventResult> GetCustomCommandAsync(ulong GuildID, string CommandName)
        {
            if (await CheckConnectionAsync())
            {
                var cmd = new CustomCommand();
                var command = new MySqlCommand("SELECT * FROM `guildcustomcommands` WHERE GuildID = @guildID AND CommandName = @command");

                command.Parameters.AddWithValue("@guildID", GuildID);
                command.Parameters.AddWithValue("@command", CommandName);

                using (var conn = new MySqlConnection(ConnectionString))
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

                                //cmd.GuildID = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["GuildID"]));

                                cmd.Content = Convert.ToString(reader["Content"]);

                                cmd.CommandName = Convert.ToString(reader["CommandName"]);
                            }
                            DogStatsd.Increment("mysql.rows_ret", rows);

                            await conn.CloseAsync();
                        }
                        else
                        {
                            return NoResultsAvailable;
                        }
                    }
                }

                return EventResult.FromSuccess(cmd);
            }
            return NoSqlConnection;
        }
        public static async Task<EventResult> GetAllCustomCommandsAsync(ulong GuildID)
        {
            if (await CheckConnectionAsync())
            {
                var command = new MySqlCommand("SELECT * FROM `guildcustomcommands` WHERE GuildID = @guildID");

                command.Parameters.AddWithValue("@guildID", GuildID);

                var commands = new List<CustomCommand>();

                using (var conn = new MySqlConnection(ConnectionString))
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

                                commands.Add(new CustomCommand
                                {
//                                    GuildID = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["GuildID"])),
                                    Content = Convert.ToString(reader["Content"]),
                                    CommandName = Convert.ToString(reader["CommandName"])
                                });
                            }
                            DogStatsd.Increment("mysql.rows_ret", rows);

                            await conn.CloseAsync();
                        }
                        else
                        {
                            return NoResultsAvailable;
                        }
                    }
                }

                return EventResult.FromSuccess(commands.AsReadOnly());
            }
            return NoSqlConnection;
        }

        public static async Task<EventResult> UpdateUserAsync(SkuldUser user)
            => await user.UpdateAsync();

        public static async Task<IReadOnlyList<EventResult>> UpdateGuildAsync(SkuldGuild guild)
            => await guild.UpdateAsync();

        public static async Task<EventResult> UpdateCustomCommand(ulong GuildID, string command, string content)
        {
            if (await CheckConnectionAsync())
            {
                var cmd = new MySqlCommand("UPDATE `guildcustomcommands` SET `Content` = @newcontent WHERE `GuildID` = @guildID AND `CommandName` = @commandName ;");
                cmd.Parameters.AddWithValue("@newcontent", content);
                cmd.Parameters.AddWithValue("@guildID", GuildID);
                cmd.Parameters.AddWithValue("@commandName", command);
                return await SingleQueryAsync(cmd).ConfigureAwait(false);
            }
            return NoSqlConnection;
        }
        public static async Task<EventResult> UpdatePastaAsync(Pasta pasta, IUser user = null)
        {
            if (await CheckConnectionAsync())
            {
                if (user == null)
                {
                    return await SingleQueryAsync(pasta.GetMySqlCommand()).ConfigureAwait(false);
                }
                else
                {
                    pasta.OwnerID = user.Id;

                    return await SingleQueryAsync(pasta.GetMySqlCommand()).ConfigureAwait(false);
                }
            }
            return NoSqlConnection;
        }

        public static async Task<EventResult> DropGuildAsync(IGuild guild)
            => await DropGuildAsync(guild.Id);
        public static async Task<EventResult> DropGuildAsync(ulong ID)
        {
            if (await CheckConnectionAsync())
            {
                return await SingleQueryAsync(new MySqlCommand($"DELETE FROM `guilds` WHERE `GuildID` = {ID}; DELETE FROM `guildmodules` WHERE `GuildID` = {ID}; DELETE FROM `guildfeatures` WHERE `GuildID` = {ID}; DELETE FROM `guildcustomcommands` WHERE `GuildID` = {ID}; DELETE FROM `guildlevelrewards` WHERE `GuildID` = {ID}; DELETE FROM `userguildxp` WHERE `GuildID` = {ID};")).ConfigureAwait(false);
            }
            return NoSqlConnection;
        }
        public static async Task<EventResult> DropUserAsync(ulong ID)
        {
            if (await CheckConnectionAsync())
            {
                return await SingleQueryAsync(new MySqlCommand($"DELETE FROM `users` WHERE `UserID` = {ID}; DELETE FROM `usercommandusage` WHERE `UserID` = {ID}; DELETE FROM `pasta` WHERE `OwnerID` = {ID}; DELETE FROM `userguildxp` WHERE `UserID` = {ID};")).ConfigureAwait(false);
            }
            return NoSqlConnection;
        }
        public static async Task<EventResult> DropCustomCommand(IGuild guild, string command)
            => await DropCustomCommand(guild.Id, command);
        public static async Task<EventResult> DropCustomCommand(ulong guildID, string command)
        {
            if (await CheckConnectionAsync())
            {
                var cmd = new MySqlCommand("DELETE FROM `guildcustomcommands` WHERE `GuildID` = @guildID AND `CommandName` = @commandName ;");
                cmd.Parameters.AddWithValue("@guildID", guildID);
                cmd.Parameters.AddWithValue("@commandName", command);
                return await SingleQueryAsync(cmd).ConfigureAwait(false);
            }
            return NoSqlConnection;
        }
        public static async Task<EventResult> DropPastaAsync(string title)
        {
            if (await CheckConnectionAsync())
            {
                var cmd = new MySqlCommand("DELETE FROM `pasta` WHERE Name = @pastaname;");
                cmd.Parameters.AddWithValue("@pastaname", title);
                return await SingleQueryAsync(cmd).ConfigureAwait(false);
            }
            return NoSqlConnection;
        }

        public static async Task<EventResult> GetUserExperienceAsync(ulong UserID)
        {
            if (await CheckConnectionAsync())
            {
                try
                {
                    var command = new MySqlCommand("SELECT * FROM `userguildxp` WHERE UserID = @userID");

                    command.Parameters.AddWithValue("@userID", UserID);

                    var userExperience = new UserExperience
                    {
                        UserID = UserID
                    };

                    using (var conn = new MySqlConnection(ConnectionString))
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

                                    var gID = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["GuildID"]));
                                    var gLv = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["Level"]));
                                    var gXP = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["XP"]));
                                    var gTx = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["TotalXP"]));
                                    var gLg = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["LastGranted"]));

                                    userExperience.GuildExperiences.Add(new GuildExperience
                                    {

                                        GuildID = gID,

                                        Level = gLv,

                                        XP = gXP,

                                        TotalXP = gTx,

                                        LastGranted = gLg
                                    });
                                }

                                DogStatsd.Increment("mysql.rows_ret", rows);

                                await conn.CloseAsync();
                            }
                        }
                    }

                    return new EventResult
                    {
                        Successful = true,
                        Data = userExperience
                    };
                }
                catch (Exception ex)
                {
                    return EventResult.FromFailureException(ex.Message, ex);
                }
            }
            return NoSqlConnection;
        }
        public static async Task<EventResult> GetGuildExperienceCountAsync(ulong GuildID)
        {
            if (await CheckConnectionAsync())
            {
                try
                {
                    var command = new MySqlCommand("SELECT COUNT(*) FROM `userguildxp` WHERE GuildID = @guildID");
                    command.Parameters.AddWithValue("@guildID", GuildID);

                    int count = 0;

                    using (var conn = new MySqlConnection(ConnectionString))
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

                                    count = ConversionTools.ParseInt32OrDefault(Convert.ToString(reader["COUNT(*)"]));
                                }

                                DogStatsd.Increment("mysql.rows_ret", rows);

                                await conn.CloseAsync();
                            }
                        }
                    }

                    return EventResult.FromSuccess(count);
                }
                catch (Exception ex)
                {
                    return EventResult.FromFailureException(ex.Message, ex);
                }
            }
            return NoSqlConnection;
        }
        public static async Task<EventResult> GetUserGuildPositionAsync(ulong UserID, ulong GuildID)
        {
            if (await CheckConnectionAsync())
            {
                try
                {
                    var command = new MySqlCommand("SELECT UserID FROM `userguildxp` WHERE GuildID = @guildID ORDER BY TotalXP DESC");
                    command.Parameters.AddWithValue("@guildID", GuildID);

                    List<ulong> Users = new List<ulong>();

                    using (var conn = new MySqlConnection(ConnectionString))
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

                                    Users.Add(ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["UserID"])));
                                }

                                DogStatsd.Increment("mysql.rows_ret", rows);

                                await conn.CloseAsync();
                            }
                        }
                    }

                    return EventResult.FromSuccess(Users.IndexOf(UserID));
                }
                catch (Exception ex)
                {
                    return EventResult.FromFailureException(ex.Message, ex);
                }
            }
            return NoSqlConnection;
        }
        public static async Task<EventResult> GetGuildExperienceAsync(ulong GuildID)
        {
            if (await CheckConnectionAsync())
            {
                try
                {
                    var command = new MySqlCommand("SELECT * FROM `userguildxp` WHERE GuildID = @guildID ORDER BY TotalXP DESC");
                    command.Parameters.AddWithValue("@guildID", GuildID);

                    var results = new List<ExperienceLeaderboardEntry>();

                    using (var conn = new MySqlConnection(ConnectionString))
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

                                    results.Add(new ExperienceLeaderboardEntry
                                    {
                                        ID = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["UserID"])),
                                        Level = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["Level"])),
                                        TotalXP = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["TotalXP"])),
                                        XP = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["XP"]))
                                    });
                                }

                                DogStatsd.Increment("mysql.rows_ret", rows);

                                await conn.CloseAsync();
                            }
                        }
                    }

                    return EventResult.FromSuccess(results.AsReadOnly());
                }
                catch (Exception ex)
                {
                    return EventResult.FromFailureException(ex.Message, ex);
                }
            }
            return NoSqlConnection;
        }
        public static async Task<EventResult> GetMoneyLeaderboardAsync(int limit = 25)
        {
            if (await CheckConnectionAsync())
            {
                try
                {
                    var command = new MySqlCommand("SELECT * FROM `users` ORDER BY Money DESC LIMIT "+limit);

                    var results = new List<MoneyLeaderboardEntry>();

                    using (var conn = new MySqlConnection(ConnectionString))
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

                                    results.Add(new MoneyLeaderboardEntry
                                    {
                                        ID = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["UserID"])),
                                        Money = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["Money"]))
                                    });
                                }

                                DogStatsd.Increment("mysql.rows_ret", rows);

                                await conn.CloseAsync();
                            }
                        }
                    }

                    return EventResult.FromSuccess(results.AsReadOnly());
                }
                catch (Exception ex)
                {
                    return EventResult.FromFailureException(ex.Message, ex);
                }
            }
            return NoSqlConnection;
        }

        public static async Task<EventResult> GetGlobalRankAsync(ulong userID)
        {
            if(await CheckConnectionAsync())
            {
                var command = new MySqlCommand("SELECT UserID FROM `userguildxp` GROUP BY UserID ORDER BY SUM(TotalXP) DESC");

                var entries = new List<string>();

                using (var conn = new MySqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        command.Connection = conn;

                        DogStatsd.Increment("mysql.queries");

                        var reader = await command.ExecuteReaderAsync();

                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                entries.Add(Convert.ToString(reader["UserID"]));
                            }

                            DogStatsd.Increment("mysql.rows_ret", entries.Count);

                            await conn.CloseAsync();
                        }
                        else
                        {
                            return NoResultsAvailable;
                        }
                    }
                }

                return EventResult.FromSuccess(new Rank(entries.IndexOf(Convert.ToString(userID))+1, entries.Count));
            }
            else
            {
                return NoSqlConnection;
            }
        }
        public static async Task<EventResult> GetGuildRankAsync(ulong userID, ulong guildID)
        {
            if (await CheckConnectionAsync())
            {
                var command = new MySqlCommand("SELECT UserID FROM `userguildxp` WHERE GuildID = @guildID GROUP BY UserID ORDER BY SUM(TotalXP) DESC");

                command.Parameters.AddWithValue("@guildID", guildID);

                var entries = new List<string>();

                using (var conn = new MySqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        command.Connection = conn;

                        DogStatsd.Increment("mysql.queries");

                        var reader = await command.ExecuteReaderAsync();

                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                entries.Add(Convert.ToString(reader["UserID"]));
                            }

                            DogStatsd.Increment("mysql.rows_ret", entries.Count);

                            await conn.CloseAsync();
                        }
                        else
                        {
                            return NoResultsAvailable;
                        }
                    }
                }

                var index = entries.FindIndex(x => x == Convert.ToString(userID));

                if(index < 0)
                {
                    return EventResult.FromSuccess(new Rank(index, entries.Count));
                }

                return EventResult.FromSuccess(new Rank(index+1, entries.Count));
            }
            else
            {
                return NoSqlConnection;
            }
        }

        public static async Task<EventResult> AddGuildAssignRoleAsync(IGuild guild, ulong roleID, GuildRoleConfig roleConfig)
        {
            throw new NotImplementedException();
            //TODO

            if (await CheckConnectionAsync())
            {

            }
            return NoSqlConnection;
        }
        public static async Task<EventResult> AddGuildLevelRewardAsync(IGuild guild, ulong roleID, GuildRoleConfig roleConfig)
        {
            throw new NotImplementedException();
            //TODO

            if (await CheckConnectionAsync())
            {

            }
            return NoSqlConnection;
        }
        public static async Task<EventResult> RemoveGuildRewardRoleAsync(SkuldGuild guild, ulong roleID)
        {
            if (await CheckConnectionAsync())
            {
                foreach(var role in guild.LevelRewards)
                {
                    if(role.RoleID == roleID)
                    {
                        guild.LevelRewards.Remove(role);
                        await guild.RemoveRoleFromLevelRewardsAsync(role.RoleID);
                    }
                }

            }
            return NoSqlConnection;
        }

        public static async Task<IReadOnlyList<EventResult>> CheckGuildUsersAsync(DiscordShardedClient client, IGuild guild)
        {
            if (await CheckConnectionAsync())
            {
                var results = new List<EventResult>();
                await guild.DownloadUsersAsync();
                foreach (var user in await guild.GetUsersAsync())
                {
                    var discord = client.GetUser(user.Id);
                    var db = await GetUserAsync(user.Id).ConfigureAwait(false);
                    if (discord == null && db != null)
                    {
                        results.Add(await DropUserAsync(user.Id).ConfigureAwait(false));
                    }
                    else if (discord != null && db == null)
                    {
                        results.Add(await InsertUserAsync(discord).ConfigureAwait(false));
                    }
                }
                return results;
            }
            return new List<EventResult> { NoSqlConnection };
        }
        public static async Task<IReadOnlyList<EventResult>> RebuildGuildAsync(DiscordShardedClient client, SkuldConfig config, IGuild guild)
        {
            if (await CheckConnectionAsync())
            {
                var results = new List<EventResult>();

                results.AddRange(await InsertGuildAsync(guild.Id, config.Discord.Prefix));

                results.AddRange(await CheckGuildUsersAsync(client, guild).ConfigureAwait(false));

                return results;
            }
            return new List<EventResult> { NoSqlConnection };
        }
        public static async Task<IReadOnlyList<EventResult>> PopulateGuildsAsync(DiscordShardedClient client)
        {
            if (await CheckConnectionAsync())
            {
                var results = new List<EventResult>();
                foreach (var guild in client.Guilds)
                {
                    results.AddRange(await CheckGuildUsersAsync(client, guild).ConfigureAwait(false));
                }
                return results;
            }
            return new List<EventResult> { NoSqlConnection };
        }
    }
}
