using Discord;
using Discord.WebSocket;
using MySql.Data.MySqlClient;
using Skuld.Core.Extensions;
using Skuld.Core.Globalization;
using Skuld.Core.Models;
using Skuld.Core.Services;
using Skuld.Models.Database;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Skuld.Core.Utilities;

namespace Skuld.Services
{
    public class DatabaseService
    {
        private SkuldConfig Configuration;
        private GenericLogger logger;
        private Locale local;
        private DiscordShardedClient client;

        string ConnectionString;

        public bool CanConnect = false;

        public DatabaseService(GenericLogger log, Locale loc, DiscordShardedClient cli, SkuldConfig config)
        {
            logger = log; local = loc; client = cli; Configuration = config;
            ConnectionString = $@"server={Configuration.SQL.Host};user={Configuration.SQL.Username};password={Configuration.SQL.Password};database={Configuration.SQL.Database};charset=utf8mb4;";
        }

        public async Task CheckConnectionAsync()
        {
            if (!Configuration.SQL.SSL && !ConnectionString.Contains("SslMode=None;"))
            {
                ConnectionString += "SslMode=None;";
            }

            bool canconnect = false;
            using (var conn = new MySqlConnection(ConnectionString))
            {
                try
                {
                    await conn.OpenAsync();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        canconnect = true;
                        await logger.AddToLogsAsync(new Core.Models.LogMessage("Database", "Connected Successfully", LogSeverity.Info));
                    }
                    else
                    {
                        canconnect = false;
                        await logger.AddToLogsAsync(new Core.Models.LogMessage("Database", "Can't Connect", LogSeverity.Error));
                    }
                }
                catch (Exception ex)
                {
                    await logger.AddToLogsAsync(new Core.Models.LogMessage("Database", ex.Message, LogSeverity.Error, ex));
                    canconnect = false;
                }
                finally
                {
                    await conn.CloseAsync();
                }
            }
            CanConnect = canconnect;
        }

        public async Task<SqlResult> SingleQueryAsync(MySqlCommand command)
        {
            if (CanConnect)
            {
                using (var conn = new MySqlConnection(ConnectionString))
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
                            await logger.AddToLogsAsync(new Core.Models.LogMessage("SQLSngl", "Error with SQL Command", Discord.LogSeverity.Error, ex));
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
            if (CanConnect)
            {
                var user = new SkuldUser();
                var command = new MySqlCommand("SELECT * FROM `users` WHERE UserID = @userid");
                command.Parameters.AddWithValue("@userid", id);

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

                                user.Money = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["Money"]));

                                user.Description = Convert.ToString(reader["Description"]);

                                user.Language = Convert.ToString(reader["Language"]);

                                user.Daily = ConversionTools.ParseUInt64OrDefault(reader["LastDaily"].ToString());

                                user.CanDM = Convert.ToBoolean(reader["CanDM"]);

                                user.Banned = Convert.ToBoolean(reader["Banned"]);

                                user.Patted = ConversionTools.ParseUInt32OrDefault(Convert.ToString(reader["Patted"]));

                                user.Pats = ConversionTools.ParseUInt32OrDefault(Convert.ToString(reader["Pats"]));

                                user.HP = ConversionTools.ParseUInt32OrDefault(Convert.ToString(reader["HP"]));

                                user.GlaredAt = ConversionTools.ParseUInt32OrDefault(Convert.ToString(reader["GlaredAt"]));

                                user.Glares = ConversionTools.ParseUInt32OrDefault(Convert.ToString(reader["Glares"]));

                                user.AvatarUrl = Convert.ToString(reader["AvatarUrl"]);
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

                command = new MySqlCommand("SELECT * FROM `usercommandusage` WHERE UserID = @userid ORDER BY `Usage` DESC LIMIT 1");
                command.Parameters.AddWithValue("@userid", id);

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

                                user.FavCmd = Convert.ToString(reader["command"]);

                                user.FavCmdUsg = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["Usage"]));
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
            if (CanConnect)
            {
                var pasta = new Pasta();
                var command = new MySqlCommand("SELECT * FROM `pasta` WHERE Name = @pastaname");
                command.Parameters.AddWithValue("@pastaname", name);

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
                if (!String.IsNullOrEmpty(pasta.Name))
                { return pasta; }
            }
            return null;
        }

        public async Task<IReadOnlyList<Pasta>> GetAllPastasAsync()
        {
            if (CanConnect)
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

                                var pasta = new Pasta
                                {
                                    PastaID = Convert.ToUInt32(reader["PastaID"].ToString()),
                                    Name = reader["Name"].ToString(),
                                    Content = reader["Content"].ToString(),
                                    OwnerID = Convert.ToUInt64(reader["OwnerID"].ToString()),
                                    Created = Convert.ToUInt64(reader["Created"].ToString()),
                                    Upvotes = Convert.ToUInt32(reader["UpVotes"].ToString()),
                                    Downvotes = Convert.ToUInt32(reader["DownVotes"].ToString())
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

        public async Task<SqlResult> CastPastaVoteAsync(IUser user, string title, bool upvote)
        {
            if (CanConnect)
            {
                var pasta = await GetPastaAsync(title);
                if (pasta != null)
                {
                    var command = new MySqlCommand("INSERT INTO `pastakarma` (PastaID,UserID,VoteType) VALUES (@pastaid,@userid,@votetype)");

                    command.Parameters.AddWithValue("@pastaid", pasta.PastaID);
                    command.Parameters.AddWithValue("@userid", user.Id);

                    if (upvote)
                    {
                        command.Parameters.AddWithValue("@votetype", 1);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@votetype", 0);
                    }

                    var usercasted = await HasUserVotedOnPastaAsync(user, title);

                    if (!usercasted)
                    {
                        return await SingleQueryAsync(command);
                    }
                    else
                    {
                        var tmpcom = new MySqlCommand("SELECT VoteType FROM `pastakarma` WHERE UserID = @userid AND PastaID = @pastaid");
                        tmpcom.Parameters.AddWithValue("@userid", user.Id);
                        tmpcom.Parameters.AddWithValue("@pastaid", pasta.PastaID);
                        var res = await SingleQueryAsync(tmpcom);

                        var datares = Convert.ToString(res.Data).ToBool();

                        if (datares == upvote)
                        {
                            return new SqlResult()
                            {
                                Successful = false,
                                Error = "User already upvoted"
                            };
                        }
                        if ((datares == upvote) == false)
                        {
                            return new SqlResult()
                            {
                                Successful = false,
                                Error = "User already downvoted"
                            };
                        }

                        return await ChangeUserVoteOnPastaAsync(user, title);
                    }
                }
                else
                {
                    return new SqlResult()
                    {
                        Successful = false,
                        Error = "Pasta \"" + title + "\" doesn't exist"
                    };
                }
            }
            return new SqlResult()
            {
                Successful = false,
                Error = "Database not enabled"
            };
        }

        public async Task<bool> HasUserVotedOnPastaAsync(IUser user, string title)
        {
            if (CanConnect)
            {
                var pasta = await GetPastaAsync(title);
                if (pasta != null)
                {
                    var command = new MySqlCommand("SELECT * FROM `pastakarma` WHERE PastaID = @pastaid AND UserID = @userid");

                    command.Parameters.AddWithValue("@userid", user.Id);
                    command.Parameters.AddWithValue("@pastaid", pasta.PastaID);

                    var res = await SingleQueryAsync(command);

                    if (res.Data == null)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<SqlResult> ChangeUserVoteOnPastaAsync(IUser user, string title)
        {
            if (CanConnect)
            {
                var pasta = await GetPastaAsync(title);
                if (pasta != null)
                {
                    var command = new MySqlCommand("UPDATE `pastakarma` SET VoteType = @votetype WHERE UserID = @userID AND PastaID = @pastaid");
                    command.Parameters.AddWithValue("@UserID", user.Id);
                    command.Parameters.AddWithValue("@pastaid", pasta.PastaID);

                    return await SingleQueryAsync(command);
                }
                else
                {
                    return new SqlResult()
                    {
                        Successful = false,
                        Error = "Pasta \"" + title + "\" doesn't exist"
                    };
                }
            }
            return new SqlResult()
            {
                Successful = false,
                Error = "Database not enabled"
            };
        }

        public async Task<CustomCommand> GetCustomCommandAsync(ulong GuildID, string Command)
        {
            if (CanConnect)
            {
                var cmd = new CustomCommand();
                var command = new MySqlCommand("SELECT * FROM `guildcustomcommands` WHERE GuildID = @guildID AND CommandName = @command");

                command.Parameters.AddWithValue("@guildID", GuildID);
                command.Parameters.AddWithValue("@command", Command);

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

                                cmd.GuildID = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["GuildID"]));

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
                var guild = await GetBaseGuildAsync(id).ConfigureAwait(false);
                if (guild != null)
                {
                    var guildSetts = new GuildSettings();

                    var comSetts = await GetGuildCommandModulesAsync(id).ConfigureAwait(false);
                    var featSetts = await GetFeatureModulesAsync(id).ConfigureAwait(false);

                    guildSetts.Modules = comSetts;
                    guildSetts.Features = featSetts;
                    guild.GuildSettings = guildSetts;

                    return guild;
                }
            }
            return null;
        }

        private async Task<SkuldGuild> GetBaseGuildAsync(ulong id)
        {
            if (CanConnect)
            {
                try
                {
                    var guild = new SkuldGuild();
                    var command = new MySqlCommand("SELECT * FROM `guilds` WHERE `GuildID` = @guildid");
                    command.Parameters.AddWithValue("@guildid", id);

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

                                    if(!string.IsNullOrWhiteSpace(rawroles))
                                    {
                                        var roles = rawroles.Split(',');

                                        if (roles.Length > 0)
                                        {
                                            var rolelist = new ulong[roles.Length];
                                            for (int x = 0; x < roles.Length; x++)
                                            {
                                                rolelist[x] = Convert.ToUInt64(roles[x]);
                                            }
                                            guild.JoinableRoles = rolelist;
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
                                }

                                DogStatsd.Increment("mysql.rows_ret", rows);

                                await conn.CloseAsync();

                                guild.GuildSettings = new GuildSettings();
                            }
                        }
                    }
                    if (guild.ID != 0)
                    { return guild; }
                }
                catch (Exception ex)
                {
                    await logger.AddToLogsAsync(new Core.Models.LogMessage("DBService", ex.Message, LogSeverity.Error, ex));
                }
            }
            return null;
        }

        private async Task<GuildCommandModules> GetGuildCommandModulesAsync(ulong id)
        {
            var comSetts = new GuildCommandModules();

            var command = new MySqlCommand("SELECT * FROM `guildmodules` WHERE GuildID = @guildid");
            command.Parameters.AddWithValue("@guildid", id);

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

                            comSetts.StatsEnabled = Convert.ToBoolean(reader["Stats"]);

                            comSetts.WeebEnabled = Convert.ToBoolean(reader["Weeb"]);
                        }
                        DogStatsd.Increment("mysql.rows_ret", rows);

                        await conn.CloseAsync();
                    }
                }
            }

            return comSetts;
        }

        private async Task<GuildFeatureModules> GetFeatureModulesAsync(ulong id)
        {
            var featSetts = new GuildFeatureModules();

            var command = new MySqlCommand("SELECT * FROM `guildfeatures` WHERE GuildID = @guildid");
            command.Parameters.AddWithValue("@guildid", id);

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
            return featSetts;
        }

        public async Task<SqlResult> InsertAdvancedSettingsAsync(bool feature, IGuild guild)
        {
            if (CanConnect)
            {
                var command = new MySqlCommand("INSERT IGNORE INTO ");
                if (feature)
                {
                    command.CommandText += "`guildfeatures` (`GuildID`,`Pinning`,`Experience`) VALUES (@guildid,0,0);";
                    command.Parameters.AddWithValue("@guildid", guild.Id);
                }
                else
                {
                    command.CommandText += "`guildmodules` (`GuildID`,`Accounts`, `Actions`,`Admin`,`Custom`,`Fun`,`Information`,`Lewd`,`Search`,`Stats`,`Weeb`) VALUES (@guildid,1,1,1,1,1,1,1);";
                    command.Parameters.AddWithValue("@guildid", guild.Id);
                }
                return await SingleQueryAsync(command).ConfigureAwait(false);
            }
            return new SqlResult
            {
                Error = "Database not available.",
                Successful = false
            };
        }

        public async Task<SqlResult> InsertUserAsync(IUser user)
        {
            if (CanConnect)
            {
                if (!user.IsBot)
                {
                    var command = new MySqlCommand("INSERT IGNORE INTO `users` (`UserID`, `Description`, `Language`, `AvatarUrl`) VALUES (@userid , \"I have no description\", @locale, @avatarurl);");
                    command.Parameters.AddWithValue("@userid", user.Id);
                    command.Parameters.AddWithValue("@locale", local.defaultLocale);
                    command.Parameters.AddWithValue("@avatarurl", user.GetAvatarUrl());
                    return await SingleQueryAsync(command).ConfigureAwait(false);
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
                var cmd = new MySqlCommand("INSERT INTO `guildcustomcommands` ( `Content`, `GuildID`, `CommandName` ) VALUES ( @newcontent , @guildID , @commandName ) ;");
                cmd.Parameters.AddWithValue("@newcontent", content);
                cmd.Parameters.AddWithValue("@guildID", guild.Id);
                cmd.Parameters.AddWithValue("@commandName", command);
                return await SingleQueryAsync(cmd).ConfigureAwait(false);
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

                var gcmd = new MySqlCommand("INSERT IGNORE INTO `guilds` (`GuildID`,`prefix`) VALUES ");
                gcmd.CommandText += $"( {guild.Id} , \"{Configuration.Discord.Prefix}\" )";

                results.Add(await SingleQueryAsync(gcmd).ConfigureAwait(false));
                results.Add(await InsertAdvancedSettingsAsync(false, guild).ConfigureAwait(false));
                results.Add(await InsertAdvancedSettingsAsync(true, guild).ConfigureAwait(false));

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

                var command = new MySqlCommand("INSERT INTO `pasta` (Content,OwnerID,Created,Name) VALUES ( @content , @ownerid , @created , @pastatitle )");
                command.Parameters.AddWithValue("@content", content);
                command.Parameters.AddWithValue("@ownerid", user.Id);
                command.Parameters.AddWithValue("@created", DateTime.UtcNow.ToEpoch());
                command.Parameters.AddWithValue("@pastatitle", pastaname);

                return await SingleQueryAsync(command).ConfigureAwait(false);
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
                return await SingleQueryAsync(new MySqlCommand($"DELETE FROM `guilds` WHERE `GuildID` = {guild.Id}; DELETE FROM `guildmodules` WHERE `GuildID` = {guild.Id}; DELETE FROM `guildfeatures` WHERE `GuildID` = {guild.Id}; DELETE FROM `guildcustomcommands` WHERE `GuildID` = {guild.Id}; DELETE FROM `guildlevelrewards` WHERE `GuildID` = {guild.Id}; DELETE FROM `userguildxp` WHERE `GuildID` = {guild.Id};")).ConfigureAwait(false);
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
                return await SingleQueryAsync(new MySqlCommand($"DELETE FROM `users` WHERE `UserID` = {user.Id}; DELETE FROM `usercommandusage` WHERE `UserID` = {user.Id}; DELETE FROM `pasta` WHERE `OwnerID` = {user.Id}; DELETE FROM `userglobalxp` WHERE `UserID` = {user.Id}; DELETE FROM `userguildxp` WHERE `UserID` = {user.Id};")).ConfigureAwait(false);
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
                var cmd = new MySqlCommand("DELETE FROM `guildcustomcommands` WHERE `GuildID` = @guildID AND `CommandName` = @commandName ;");
                cmd.Parameters.AddWithValue("@guildID", guild.Id);
                cmd.Parameters.AddWithValue("@commandName", command);
                return await SingleQueryAsync(cmd).ConfigureAwait(false);
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
                var cmd = new MySqlCommand("DELETE FROM `pasta` WHERE Name = @pastaname;");
                cmd.Parameters.AddWithValue("@pastaname", title);
                return await SingleQueryAsync(cmd).ConfigureAwait(false);
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
                var command = new MySqlCommand(
                    "UPDATE `users` SET " +
                    "`Banned = @banned, " +
                    "`Description` = @description, " +
                    "`CanDM` = @dmenabled," +
                    "`Money` = @money, " +
                    "`Language` = @language, " +
                    "`HP` = @hp, " +
                    "`Patted` = @petted, `Pats` = @pets, " +
                    "`GlaredAt` = @glaredat, `Glares` = @glares " +
                    "`LastDaily` = @daily, " +
                    "WHERE UserID = @userID "
                );

                command.Parameters.AddWithValue("@money", user.Money);
                command.Parameters.AddWithValue("@description", user.Description);
                command.Parameters.AddWithValue("@daily", user.Daily);
                command.Parameters.AddWithValue("@language", user.Language);
                command.Parameters.AddWithValue("@dmenabled", user.CanDM);
                command.Parameters.AddWithValue("@petted", user.Patted);
                command.Parameters.AddWithValue("@pets", user.Pats);
                command.Parameters.AddWithValue("@hp", user.HP);
                command.Parameters.AddWithValue("@glaredat", user.GlaredAt);
                command.Parameters.AddWithValue("@glares", user.Glares);
                command.Parameters.AddWithValue("@banned", user.Banned);
                command.Parameters.AddWithValue("@userID", user.ID);

                return await SingleQueryAsync(command).ConfigureAwait(false);
            }
            return new SqlResult
            {
                Error = "Database not available.",
                Successful = false
            };
        }

        public async Task<SqlResult> UpdateUserUsageAsync(SkuldUser user, string command)
        {
            if (CanConnect)
            {
                var cmd = new MySqlCommand("SELECT `Usage` from `usercommandusage` WHERE UserID = @userID AND Command = @command");
                cmd.Parameters.AddWithValue("@userID", user.ID);
                cmd.Parameters.AddWithValue("@command", command);
                var resp = await SingleQueryAsync(cmd).ConfigureAwait(false);

                if (resp.Successful)
                {
                    if (!String.IsNullOrEmpty(Convert.ToString(resp.Data)))
                    {
                        var cmdusg = Convert.ToInt32(resp.Data);
                        cmdusg = cmdusg + 1;
                        cmd = new MySqlCommand("UPDATE `usercommandusage` SET `Usage` = @userusg WHERE UserID = @userid AND Command = @command");
                        cmd.Parameters.AddWithValue("@userusg", cmdusg);
                        cmd.Parameters.AddWithValue("@userid", user.ID);
                        cmd.Parameters.AddWithValue("@command", command);
                        return await SingleQueryAsync(cmd).ConfigureAwait(false);
                    }
                    else
                    {
                        cmd = new MySqlCommand("INSERT INTO `usercommandusage` (`UserID`, `Usage`, `Command`) VALUES (@userid , @userusg , @command)");
                        cmd.Parameters.AddWithValue("@userusg", 1);
                        cmd.Parameters.AddWithValue("@userid", user.ID);
                        cmd.Parameters.AddWithValue("@command", command);
                        return await SingleQueryAsync(cmd).ConfigureAwait(false);
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
                var cmd = new MySqlCommand("UPDATE `guildcustomcommands` SET `Content` = @newcontent WHERE `GuildID` = @guildID AND `CommandName` = @commandName ;");
                cmd.Parameters.AddWithValue("@newcontent", content);
                cmd.Parameters.AddWithValue("@guildID", guild.Id);
                cmd.Parameters.AddWithValue("@commandName", command);
                return await SingleQueryAsync(cmd).ConfigureAwait(false);
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
                if (user == null)
                {
                    var command = new MySqlCommand("UPDATE `pasta` SET Content = @content, UpVotes = @upvotes, DownVotes = @downvotes WHERE Name = @title");
                    command.Parameters.AddWithValue("@content", pasta.Content);
                    command.Parameters.AddWithValue("@downvotes", pasta.Downvotes);
                    command.Parameters.AddWithValue("@upvotes", pasta.Upvotes);
                    command.Parameters.AddWithValue("@title", pasta.Name);

                    return await SingleQueryAsync(command).ConfigureAwait(false);
                }
                else
                {
                    var command = new MySqlCommand("UPDATE `pasta` SET Content = @content, UpVotes = @upvotes, DownVotes = @downvotes, OwnerID = @ownerid WHERE Name = @title");
                    command.Parameters.AddWithValue("@content", pasta.Content);
                    command.Parameters.AddWithValue("@downvotes", pasta.Downvotes);
                    command.Parameters.AddWithValue("@upvotes", pasta.Upvotes);
                    command.Parameters.AddWithValue("@ownerid", user.Id);
                    command.Parameters.AddWithValue("@title", pasta.Name);

                    return await SingleQueryAsync(command).ConfigureAwait(false);
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
                    await UpdateBaseGuildAsync(guild).ConfigureAwait(false),
                    await UpdateFeaturesAsync(guild.ID, guild.GuildSettings.Features).ConfigureAwait(false),
                    await UpdateCommandModulesAsync(guild.ID, guild.GuildSettings.Modules).ConfigureAwait(false)
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

        private async Task<SqlResult> UpdateBaseGuildAsync(SkuldGuild guild)
        {
            if (CanConnect)
            {
                var command = new MySqlCommand(
                    "UPDATE `guilds` SET " +
                    "`Prefix` = @prefix, " +
                    "`MutedRole` = @mutedr, " +
                    "`JoinableRoles` = @joinableroles " +
                    "`JoinRole` = @ajr, " +
                    "`JoinChannel` = @ujc, " +
                    "`LeaveChannel` = @ulc " +
                    "`JoinMessage` = @jmsg, " +
                    "`LeaveMessage` = @lmsg, " +
                    "WHERE GuildID = @guildID;"
                );

                command.Parameters.AddWithValue("@prefix", guild.Prefix);
                command.Parameters.AddWithValue("@mutedr", guild.MutedRole);
                command.Parameters.AddWithValue("@joinableroles", string.Join(",", guild.JoinableRoles));
                command.Parameters.AddWithValue("@ajr", guild.JoinRole);
                command.Parameters.AddWithValue("@ujc", guild.UserJoinChannel);
                command.Parameters.AddWithValue("@ulc", guild.UserLeaveChannel);
                command.Parameters.AddWithValue("@jmsg", guild.JoinMessage);
                command.Parameters.AddWithValue("@lmsg", guild.LeaveMessage);
                command.Parameters.AddWithValue("@guildID", guild.ID);

                return await SingleQueryAsync(command).ConfigureAwait(false);
            }
            return new SqlResult
            {
                Error = "Database not available.",
                Successful = false
            };
        }

        private async Task<SqlResult> UpdateCommandModulesAsync(ulong id, GuildCommandModules modules)
        {
            if (CanConnect)
            {
                var command = new MySqlCommand("UPDATE `guildmodules` SET " +
                        "`Accounts` = @accounts, " +
                        "`Actions` = @actions, " +
                        "`Admin` = @admin, " +
                        "`Custom` = @custom, " +
                        "`Fun` = @fun, " +
                        "`Information` = @info, " +
                        "`Lewd` = @lewd, " +
                        "`Search` = @search, " +
                        "`Stats` = @stats, " +
                        "`Weeb` = @weeb " +
                        "WHERE GuildID = @guildID;");

                command.Parameters.AddWithValue("@accounts", modules.AccountsEnabled);
                command.Parameters.AddWithValue("@actions", modules.ActionsEnabled);
                command.Parameters.AddWithValue("@admin", modules.AdminEnabled);
                command.Parameters.AddWithValue("@custom", modules.CustomEnabled);
                command.Parameters.AddWithValue("@fun", modules.FunEnabled);
                command.Parameters.AddWithValue("@info", modules.InformationEnabled);
                command.Parameters.AddWithValue("@lewd", modules.LewdEnabled);
                command.Parameters.AddWithValue("@search", modules.SearchEnabled);
                command.Parameters.AddWithValue("@stats", modules.StatsEnabled);
                command.Parameters.AddWithValue("@weeb", modules.WeebEnabled);
                command.Parameters.AddWithValue("@guildID", id);

                return await SingleQueryAsync(command).ConfigureAwait(false);
            }
            return new SqlResult
            {
                Error = "Database not available.",
                Successful = false
            };
        }

        private async Task<SqlResult> UpdateFeaturesAsync(ulong id, GuildFeatureModules features)
        {
            if (CanConnect)
            {
                var command = new MySqlCommand("UPDATE `guildfeatures` SET " +
                    "`Pinning` = @pin, " +
                    "`Experience` = @xp " +
                    "WHERE GuildID = @guildID");

                command.Parameters.AddWithValue("@pin", features.Pinning);
                command.Parameters.AddWithValue("@xp", features.Experience);
                command.Parameters.AddWithValue("@guildID", id);

                return await SingleQueryAsync(command).ConfigureAwait(false);
            }
            return new SqlResult
            {
                Error = "Database not available.",
                Successful = false
            };
        }

        public async Task<object> GetUserExperienceAsync(IUser user)
        {
            if(CanConnect)
            {
                var command = new MySqlCommand("SELECT * FROM `userguildxp` WHERE UserID = @userID");

                command.Parameters.AddWithValue("@userID", user.Id);

                var userExperience = new UserExperience();

                userExperience.UserID = user.Id;

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

                                var localExperience = new GuildExperience();

                                localExperience.GuildID = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["GuildID"]));

                                localExperience.Level = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["Level"]));

                                localExperience.XP = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["XP"]));

                                localExperience.TotalXP = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["TotalXP"]));

                                localExperience.LastGranted = ConversionTools.ParseUInt64OrDefault(Convert.ToString(reader["LastGranted"]));

                                userExperience.GuildExperiences.Add(localExperience);
                            }

                            DogStatsd.Increment("mysql.rows_ret", rows);

                            await conn.CloseAsync();
                        }
                        else
                        {
                            return new SqlResult
                            {
                                Error = "Unknown Error",
                                Successful = false
                            };
                        }
                    }
                }

                return userExperience;
            }
            return new SqlResult
            {
                Error = "Database not available.",
                Successful = false
            };
        }

        public async Task<SqlResult> UpdateGuildExperienceAsync(IUser user, GuildExperience guildexp, IGuild guild)
        {
            if (CanConnect)
            {
                var command = new MySqlCommand("UPDATE `userguildxp` SET " +
                    "`Level` = @lvl, " +
                    "`XP` = @xp, " +
                    "`TotalXP` = @txp " +
                    "WHERE GuildID = @guildID AND UserID = @userID");

                command.Parameters.AddWithValue("@lvl", guildexp.Level);
                command.Parameters.AddWithValue("@xp", guildexp.XP);
                command.Parameters.AddWithValue("@txp", guildexp.TotalXP);
                command.Parameters.AddWithValue("@guildID", guild.Id);
                command.Parameters.AddWithValue("@userID", user.Id);

                return await SingleQueryAsync(command).ConfigureAwait(false);
            }
            return new SqlResult
            {
                Error = "Database not available.",
                Successful = false
            };
        }

        public async Task<SqlResult> InsertGuildExperienceAsync(IUser user, IGuild guild, GuildExperience experience)
        {
            if (CanConnect)
            {
                var command = new MySqlCommand("INSERT INTO `userguildxp` (UserID, GuildID, Level, XP, TotalXP, LastGranted) VALUES (@userID, @guildID, @lvl, @xp, @txp, @lgrant);");

                command.Parameters.AddWithValue("@guildID", guild.Id);
                command.Parameters.AddWithValue("@userID", user.Id);
                command.Parameters.AddWithValue("@lvl", experience.Level);
                command.Parameters.AddWithValue("@xp", experience.XP);
                command.Parameters.AddWithValue("@txp", experience.TotalXP);
                command.Parameters.AddWithValue("@lgrant", experience.LastGranted);

                return await SingleQueryAsync(command).ConfigureAwait(false);
            }
            return new SqlResult
            {
                Error = "Database not available.",
                Successful = false
            };
        }

        public async Task<SqlResult> AddExperienceAsync(IUser user, ulong amount, IGuild guild)
        {
            if(CanConnect)
            {
                var userExperience = await GetUserExperienceAsync(user);

                if(userExperience is UserExperience)
                {
                    var guildExp = (userExperience as UserExperience).GuildExperiences.First(x => x.GuildID == guild.Id);

                    guildExp.XP += amount;
                    guildExp.TotalXP += amount;

                    return await UpdateGuildExperienceAsync(user, guildExp, guild);
                }
                else
                {
                    return (SqlResult)userExperience;
                }
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
                    var db = await GetUserAsync(user.Id).ConfigureAwait(false);
                    if (discord == null && db != null)
                    {
                        results.Add(await DropUserAsync(user).ConfigureAwait(false));
                    }
                    else if (discord != null && db == null)
                    {
                        results.Add(await InsertUserAsync(discord).ConfigureAwait(false));
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

                results.AddRange(await InsertGuildAsync(guild));

                results.AddRange(await CheckGuildUsersAsync(guild).ConfigureAwait(false));

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
            if (CanConnect)
            {
                var results = new List<SqlResult>();
                foreach (var guild in client.Guilds)
                {
                    results.AddRange(await CheckGuildUsersAsync(guild).ConfigureAwait(false));
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