using MySql.Data.MySqlClient;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skuld.Database.Extensions
{
    public static class ModelExtensions
    {
        //User
        public static MySqlCommand GetMySqlCommand(this SkuldUser user)
        {
            var command = new MySqlCommand(
                    "UPDATE `users` SET " +
                    "Banned = @banned, " +
                    "Title = @title, " +
                    "Username = @username, " +
                    "CanDM = @dmenabled, " +
                    "Money = @money, " +
                    "Language = @language, " +
                    "HP = @hp, " +
                    "Patted = @petted, Pats = @pets, " +
                    "LastDaily = @daily, " +
                    "RecurringBlock = @recurBlock, " +
                    "UnlockedCustBG = @unlockBG, " +
                    "Background = @bg " +
                    "WHERE UserID = @userID; "
                );

            command.Parameters.AddWithValue("@money", user.Money);
            command.Parameters.AddWithValue("@title", user.Title);
            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@daily", user.Daily);
            command.Parameters.AddWithValue("@language", user.Language);
            command.Parameters.AddWithValue("@dmenabled", user.CanDM);
            command.Parameters.AddWithValue("@petted", user.Patted);
            command.Parameters.AddWithValue("@pets", user.Pats);
            command.Parameters.AddWithValue("@hp", user.HP);
            command.Parameters.AddWithValue("@banned", user.Banned);
            command.Parameters.AddWithValue("@recurBlock", Convert.ToInt32(user.RecurringBlock));
            command.Parameters.AddWithValue("@unlockBG", user.UnlockedCustBG);
            command.Parameters.AddWithValue("@bg", user.Background);
            command.Parameters.AddWithValue("@userID", user.ID);

            return command;
        }
        public static async Task<EventResult> BlockUserActionsAsync(this SkuldUser user, ulong targetID)
        {
            if (await DatabaseClient.CheckConnectionAsync())
            {
                try
                {
                    var command = new MySqlCommand("INSERT INTO `blockedactions` (Blocker, Blockee) VALUES (@blocker, @blockee);");

                    command.Parameters.AddWithValue("@blocker", user.ID);
                    command.Parameters.AddWithValue("@blockee", targetID);

                    return await DatabaseClient.SingleQueryAsync(command).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return EventResult.FromFailureException(ex.Message, ex);
                }
            }
            return DatabaseClient.NoSqlConnection;
        }
        public static async Task<EventResult> UnblockUserActionsAsync(this SkuldUser user, ulong targetID)
        {
            if (await DatabaseClient.CheckConnectionAsync())
            {
                try
                {
                    var command = new MySqlCommand("DELETE FROM `blockedactions` WHERE `Blocker` = @blocker AND `Blockee` = @blockee;");

                    command.Parameters.AddWithValue("@blocker", user.ID);
                    command.Parameters.AddWithValue("@blockee", targetID);

                    return await DatabaseClient.SingleQueryAsync(command).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return EventResult.FromFailureException(ex.Message, ex);
                }
            }
            return DatabaseClient.NoSqlConnection;
        }
        public static async Task<EventResult> IsBlockedFromPerformingActions(this SkuldUser blocked, ulong BlockerID)
        {
            if (await DatabaseClient.CheckConnectionAsync())
            {
                try
                {
                    var command = new MySqlCommand("SELECT (EXISTS (SELECT * FROM `blockedactions` WHERE `Blocker` = @blocker AND `Blockee` = @blockee))");

                    command.Parameters.AddWithValue("@blocker", BlockerID);
                    command.Parameters.AddWithValue("@blockee", blocked.ID);

                    return await DatabaseClient.SingleQueryAsync(command);
                }
                catch (Exception ex)
                {
                    return EventResult.FromFailureException(ex.Message, ex);
                }
            }
            return DatabaseClient.NoSqlConnection;
        }
        public static async Task<EventResult> AddReputationAsync(this SkuldUser repee, ulong reper)
        {
            if (await DatabaseClient.CheckConnectionAsync())
            {
                try
                {
                    var command = new MySqlCommand("INSERT INTO `reputation` (Repee, Reper, Timestamp) VALUES (@repee, @reper, @timestamp);");

                    command.Parameters.AddWithValue("@repee", repee.ID);
                    command.Parameters.AddWithValue("@reper", reper);
                    command.Parameters.AddWithValue("@timestamp", DateTime.UtcNow.ToEpoch());

                    return await DatabaseClient.SingleQueryAsync(command).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return EventResult.FromFailureException(ex.Message, ex);
                }
            }
            return DatabaseClient.NoSqlConnection;
        }
        public static async Task<EventResult> RemoveReputationAsync(this SkuldUser repee, ulong reper)
        {
            if (await DatabaseClient.CheckConnectionAsync())
            {
                try
                {
                    var command = new MySqlCommand("DELETE FROM `reputation` WHERE `Repee` = @repee AND `Reper` = @reper;");

                    command.Parameters.AddWithValue("@repee", repee.ID);
                    command.Parameters.AddWithValue("@reper", reper);

                    return await DatabaseClient.SingleQueryAsync(command).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return EventResult.FromFailureException(ex.Message, ex);
                }
            }
            return DatabaseClient.NoSqlConnection;
        }
        public static async Task<EventResult> UpdateAsync(this SkuldUser user)
        {
            if (await DatabaseClient.CheckConnectionAsync())
            {
                return await DatabaseClient.SingleQueryAsync(user.GetMySqlCommand()).ConfigureAwait(false);
            }
            return DatabaseClient.NoSqlConnection;
        }
        public static async Task<EventResult> UpdateUserCommandCountAsync(this SkuldUser user, string command)
        {
            if (await DatabaseClient.CheckConnectionAsync())
            {
                var cmd = new MySqlCommand("SELECT `Usage` from `usercommandusage` WHERE UserID = @userID AND Command = @command");
                cmd.Parameters.AddWithValue("@userID", user.ID);
                cmd.Parameters.AddWithValue("@command", command);
                var resp = await DatabaseClient.SingleQueryAsync(cmd).ConfigureAwait(false);

                if (resp.Successful)
                {
                    if (resp.Data != null)
                    {
                        var cmdusg = Convert.ToInt32(resp.Data);
                        cmdusg = cmdusg + 1;
                        cmd = new MySqlCommand("UPDATE `usercommandusage` SET `Usage` = @userusg WHERE UserID = @userid AND Command = @command");
                        cmd.Parameters.AddWithValue("@userusg", cmdusg);
                        cmd.Parameters.AddWithValue("@userid", user.ID);
                        cmd.Parameters.AddWithValue("@command", command);
                        return await DatabaseClient.SingleQueryAsync(cmd).ConfigureAwait(false);
                    }
                    else
                    {
                        cmd = new MySqlCommand("INSERT INTO `usercommandusage` (`UserID`, `Usage`, `Command`) VALUES (@userid , @userusg , @command)");
                        cmd.Parameters.AddWithValue("@userusg", 1);
                        cmd.Parameters.AddWithValue("@userid", user.ID);
                        cmd.Parameters.AddWithValue("@command", command);
                        return await DatabaseClient.SingleQueryAsync(cmd).ConfigureAwait(false);
                    }
                }
                else
                {
                    return resp;
                }
            }
            return DatabaseClient.NoSqlConnection;
        }

        public static async Task<EventResult> CastPastaVoteAsync(this SkuldUser user, Pasta pasta, bool upvote)
        {
            if (await DatabaseClient.CheckConnectionAsync())
            {
                var command = new MySqlCommand("INSERT INTO `pastakarma` (PastaID,UserID,VoteType) VALUES (@pastaid,@userid,@votetype)");

                command.Parameters.AddWithValue("@pastaid", pasta.PastaID);
                command.Parameters.AddWithValue("@userid", user.ID);

                if (upvote)
                {
                    command.Parameters.AddWithValue("@votetype", 1);
                }
                else
                {
                    command.Parameters.AddWithValue("@votetype", 0);
                }

                var resp = await HasUserVotedOnPastaAsync(user, pasta);
                if (!resp.Successful) return resp;

                bool.TryParse(resp.Data as string, out bool usercasted);

                if (!usercasted)
                {
                    return await DatabaseClient.SingleQueryAsync(command);
                }
                else
                {
                    var tmpcom = new MySqlCommand("SELECT VoteType FROM `pastakarma` WHERE UserID = @userid AND PastaID = @pastaid");
                    tmpcom.Parameters.AddWithValue("@userid", user.ID);
                    tmpcom.Parameters.AddWithValue("@pastaid", pasta.PastaID);
                    var res = await DatabaseClient.SingleQueryAsync(tmpcom);

                    var datares = Convert.ToString(res.Data).ToBool();

                    if (datares == upvote)
                    {
                        return EventResult.FromFailure("User already voted");
                    }
                    if (!(datares == upvote))
                    {
                        return EventResult.FromFailure("User already voted");
                    }

                    return await ChangeUserVoteOnPastaAsync(user, pasta);
                }
            }
            return DatabaseClient.NoSqlConnection;
        }
        public static async Task<EventResult> GetUserVoteAsync(this SkuldUser user, Pasta pasta)
        {
            if (await DatabaseClient.CheckConnectionAsync())
            {
                var tmpcom = new MySqlCommand("SELECT VoteType FROM `pastakarma` WHERE UserID = @userid AND PastaID = @pastaid");
                tmpcom.Parameters.AddWithValue("@userid", user.ID);
                tmpcom.Parameters.AddWithValue("@pastaid", pasta.PastaID);
                var res = await DatabaseClient.SingleQueryAsync(tmpcom);

                var datares = Convert.ToString(res.Data).ToBool();

                return new EventResult
                {
                    Successful = true,
                    Data = datares
                };
            }
            return DatabaseClient.NoSqlConnection;
        }
        public static async Task<EventResult> HasUserVotedOnPastaAsync(this SkuldUser user, Pasta pasta)
        {
            if (await DatabaseClient.CheckConnectionAsync())
            {
                var command = new MySqlCommand("SELECT * FROM `pastakarma` WHERE PastaID = @pastaid AND UserID = @userid");

                command.Parameters.AddWithValue("@userid", user.ID);
                command.Parameters.AddWithValue("@pastaid", pasta.PastaID);

                var res = await DatabaseClient.SingleQueryAsync(command);

                if (res.Data == null)
                {
                    return new EventResult
                    {
                        Successful = true,
                        Data = false
                    };
                }
                else
                {
                    return new EventResult
                    {
                        Successful = true,
                        Data = true
                    };
                }
            }
            return DatabaseClient.NoSqlConnection;
        }
        public static async Task<EventResult> ChangeUserVoteOnPastaAsync(this SkuldUser user, Pasta pasta)
        {
            if (await DatabaseClient.CheckConnectionAsync())
            {
                bool vote = false;
                var hasresp = await HasUserVotedOnPastaAsync(user, pasta).ConfigureAwait(false);
                if (hasresp.Successful)
                {
                    var voteresp = await GetUserVoteAsync(user, pasta).ConfigureAwait(false);
                    if (voteresp.Successful)
                    {
                        bool.TryParse((string)voteresp.Data, out vote);
                    }
                }

                var command = new MySqlCommand("UPDATE `pastakarma` SET VoteType = @votetype WHERE UserID = @userID AND PastaID = @pastaid");
                command.Parameters.AddWithValue("@votetype", !vote);
                command.Parameters.AddWithValue("@UserID", user.ID
);
                command.Parameters.AddWithValue("@pastaid", pasta.PastaID);

                return await DatabaseClient.SingleQueryAsync(command);
            }
            return DatabaseClient.NoSqlConnection;
        }

        public static async Task<EventResult> UpdateGuildExperienceAsync(this SkuldUser user, GuildExperience guildexp, ulong GuildID)
        {
            if (await DatabaseClient.CheckConnectionAsync())
            {
                try
                {
                    var command = new MySqlCommand("UPDATE `userguildxp` SET " +
                    "`Level` = @lvl, " +
                    "`XP` = @xp, " +
                    "`TotalXP` = @txp " +
                    "WHERE GuildID = @guildID AND UserID = @userID");

                    command.Parameters.AddWithValue("@lvl", guildexp.Level);
                    command.Parameters.AddWithValue("@xp", guildexp.XP);
                    command.Parameters.AddWithValue("@txp", guildexp.TotalXP);
                    command.Parameters.AddWithValue("@guildID", GuildID);
                    command.Parameters.AddWithValue("@userID", user.ID);

                    return await DatabaseClient.SingleQueryAsync(command).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return EventResult.FromFailureException(ex.Message, ex);
                }
            }
            return DatabaseClient.NoSqlConnection;
        }
        public static async Task<EventResult> InsertGuildExperienceAsync(this SkuldUser user, ulong GuildID, GuildExperience experience)
        {
            if (await DatabaseClient.CheckConnectionAsync())
            {
                try
                {
                    var command = new MySqlCommand("INSERT INTO `userguildxp` (UserID, GuildID, Level, XP, TotalXP, LastGranted) VALUES (@userID, @guildID, @lvl, @xp, @txp, @lgrant);");

                    command.Parameters.AddWithValue("@guildID", GuildID);
                    command.Parameters.AddWithValue("@userID", user.ID);
                    command.Parameters.AddWithValue("@lvl", experience.Level);
                    command.Parameters.AddWithValue("@xp", experience.XP);
                    command.Parameters.AddWithValue("@txp", experience.TotalXP);
                    command.Parameters.AddWithValue("@lgrant", experience.LastGranted);

                    return await DatabaseClient.SingleQueryAsync(command).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return EventResult.FromFailureException(ex.Message, ex);
                }
            }
            return DatabaseClient.NoSqlConnection;
        }
        public static async Task<EventResult> AddExperienceAsync(this SkuldUser user, ulong amount, ulong GuildID)
        {
            if (await DatabaseClient.CheckConnectionAsync())
            {
                var userExperienceResp = await DatabaseClient.GetUserExperienceAsync(user.ID).ConfigureAwait(false);

                if (userExperienceResp.Data is UserExperience)
                {
                    var guildExp = (userExperienceResp.Data as UserExperience).GetGuildExperience(GuildID);

                    guildExp.XP += amount;
                    guildExp.TotalXP += amount;

                    return await user.UpdateGuildExperienceAsync(guildExp, GuildID);
                }
                else
                {
                    return EventResult.FromFailure(userExperienceResp.Error);
                }
            }
            return DatabaseClient.NoSqlConnection;
        }

        //Pasta
        public static MySqlCommand GetMySqlCommand(this Pasta pasta)
        {
            var command = new MySqlCommand(
                "UPDATE `pasta` SET " +
                "`Content` = @content, " +
                "`OwnerID` = @ownerid, " +
                "`Upvotes` = @upvotes, " +
                "`Downvotes` = @downvotes, " +
                "`Name` = @name, " +
                "`Created` = @created " +
                "WHERE PastaID = @pastaID ");

            command.Parameters.AddWithValue("@content", pasta.Content);
            command.Parameters.AddWithValue("@ownerid", pasta.OwnerID);
            command.Parameters.AddWithValue("@upvotes", pasta.Upvotes);
            command.Parameters.AddWithValue("@downvotes", pasta.Downvotes);
            command.Parameters.AddWithValue("@name", pasta.Name);
            command.Parameters.AddWithValue("@created", pasta.Created);
            command.Parameters.AddWithValue("@pastaID", pasta.PastaID);

            return command;
        }

        //Guild
        private static MySqlCommand GetBaseMySqlCommand(this SkuldGuild guild)
        {
            MySqlCommand command = null;
            if (guild.JoinableRoles != null)
            {
                command = new MySqlCommand(
                    "UPDATE `guilds` SET " +
                    "`Prefix` = @prefix, " +
                    "`MutedRole` = @mutedr, " +
                    "`JoinableRoles` = @joinableroles, " +
                    "`JoinRole` = @ajr, " +
                    "`JoinChannel` = @ujc, " +
                    "`LeaveChannel` = @ulc, " +
                    "`JoinMessage` = @jmsg, " +
                    "`LeaveMessage` = @lmsg, " +
                    "`LevelUpMessage` = @lupmsg, " +
                    "`LevelNotification` = @lNotif " +
                    "WHERE GuildID = @guildID;"
                );
                command.Parameters.AddWithValue("@joinableroles", string.Join(",", guild.JoinableRoles) ?? null);
            }
            else
            {
                command = new MySqlCommand(
                    "UPDATE `guilds` SET " +
                    "`Prefix` = @prefix, " +
                    "`MutedRole` = @mutedr, " +
                    "`JoinRole` = @ajr, " +
                    "`JoinChannel` = @ujc, " +
                    "`LeaveChannel` = @ulc, " +
                    "`JoinMessage` = @jmsg, " +
                    "`LeaveMessage` = @lmsg, " +
                    "`LevelUpMessage` = @lupmsg, " +
                    "`LevelUpChannel` = @lupchan, " +
                    "`LevelNotification` = @lNotif " +
                    "WHERE GuildID = @guildID;"
                );
            }

            command.Parameters.AddWithValue("@prefix", guild.Prefix);
            command.Parameters.AddWithValue("@mutedr", guild.MutedRole);
            command.Parameters.AddWithValue("@ajr", guild.JoinRole);
            command.Parameters.AddWithValue("@ujc", guild.UserJoinChannel);
            command.Parameters.AddWithValue("@ulc", guild.UserLeaveChannel);
            command.Parameters.AddWithValue("@jmsg", guild.JoinMessage);
            command.Parameters.AddWithValue("@lmsg", guild.LeaveMessage);
            command.Parameters.AddWithValue("@lupmsg", guild.LevelUpMessage);
            command.Parameters.AddWithValue("@lupchan", guild.LevelUpChannel);
            command.Parameters.AddWithValue("@lNotif", (int)guild.LevelNotification);
            command.Parameters.AddWithValue("@guildID", guild.ID);

            return command;
        }
        private static MySqlCommand GetFeaturesMySqlCommand(this SkuldGuild guild)
        {
            var command = new MySqlCommand("UPDATE `guildfeatures` SET " +
                "`Pinning` = @pin, " +
                "`Experience` = @xp " +
                "WHERE GuildID = @guildID");

            command.Parameters.AddWithValue("@pin", guild.Features.Pinning);
            command.Parameters.AddWithValue("@xp", guild.Features.Experience);
            command.Parameters.AddWithValue("@guildID", guild.ID);

            return command;
        }
        private static MySqlCommand GetModulesMySqlCommand(this SkuldGuild guild)
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
                "`Space` = @space, " +
                "`Stats` = @stats, " +
                "`Weeb` = @weeb " +
                "WHERE GuildID = @guildID;");

            command.Parameters.AddWithValue("@accounts", guild.Modules.AccountsEnabled);
            command.Parameters.AddWithValue("@actions", guild.Modules.ActionsEnabled);
            command.Parameters.AddWithValue("@admin", guild.Modules.AdminEnabled);
            command.Parameters.AddWithValue("@custom", guild.Modules.CustomEnabled);
            command.Parameters.AddWithValue("@fun", guild.Modules.FunEnabled);
            command.Parameters.AddWithValue("@info", guild.Modules.InformationEnabled);
            command.Parameters.AddWithValue("@lewd", guild.Modules.LewdEnabled);
            command.Parameters.AddWithValue("@search", guild.Modules.SearchEnabled);
            command.Parameters.AddWithValue("@space", guild.Modules.SpaceEnabled);
            command.Parameters.AddWithValue("@stats", guild.Modules.StatsEnabled);
            command.Parameters.AddWithValue("@weeb", guild.Modules.WeebEnabled);
            command.Parameters.AddWithValue("@guildID", guild.ID);

            return command;
        }
        public static async Task<IReadOnlyList<EventResult>> UpdateAsync(this SkuldGuild guild)
        {
            if (await DatabaseClient.CheckConnectionAsync())
            {
                return new List<EventResult>
                {
                    await DatabaseClient.SingleQueryAsync(guild.GetBaseMySqlCommand()).ConfigureAwait(false),
                    await DatabaseClient.SingleQueryAsync(guild.GetFeaturesMySqlCommand()).ConfigureAwait(false),
                    await DatabaseClient.SingleQueryAsync(guild.GetModulesMySqlCommand()).ConfigureAwait(false)
                }.AsReadOnly();
            }
            return new List<EventResult> { DatabaseClient.NoSqlConnection };
        }
        public static async Task<EventResult> RemoveRoleFromLevelRewardsAsync(this SkuldGuild guild, ulong roleID)
        {
            if(await DatabaseClient.CheckConnectionAsync())
            {
                return await DatabaseClient.SingleQueryAsync(new MySqlCommand($"DROP FROM guildlevelrewards WHERE `GuildID` = {guild.ID} AND `RoleID` = {roleID};"));
            }
            return DatabaseClient.NoSqlConnection;
        }
    }
}
