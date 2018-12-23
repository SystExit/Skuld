using MySql.Data.MySqlClient;
using Skuld.Core.Models;

namespace Skuld.Database.Extensions
{
    public static class ModelExtensions
    {
        public static MySqlCommand GetMySqlCommand(this SkuldUser user)
        {
            var command = new MySqlCommand(
                    "UPDATE `users` SET " +
                    "Banned = @banned, " +
                    "Description = @description, " +
                    "CanDM = @dmenabled, " +
                    "Money = @money, " +
                    "Language = @language, " +
                    "HP = @hp, " +
                    "Patted = @petted, Pats = @pets, " +
                    "GlaredAt = @glaredat, Glares = @glares, " +
                    "LastDaily = @daily " +
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

            return command;
        }

        public static MySqlCommand GetBaseMySqlCommand(this SkuldGuild guild)
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
                    "`LevelUpMessage` = @lupmsg " +
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
                    "`LevelUpMessage` = @lupmsg " +
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
            command.Parameters.AddWithValue("@guildID", guild.ID);

            return command;
        }

        public static MySqlCommand GetFeaturesMySqlCommand(this SkuldGuild guild)
        {
            var command = new MySqlCommand("UPDATE `guildfeatures` SET " +
                "`Pinning` = @pin, " +
                "`Experience` = @xp " +
                "WHERE GuildID = @guildID");

            /*command.Parameters.AddWithValue("@pin", guild.GuildSettings.Features.Pinning);
            command.Parameters.AddWithValue("@xp", guild.GuildSettings.Features.Experience);*/
            command.Parameters.AddWithValue("@guildID", guild.ID);

            return command;
        }

        public static MySqlCommand GetModulesMySqlCommand(this SkuldGuild guild)
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

            /*command.Parameters.AddWithValue("@accounts", guild.GuildSettings.Modules.AccountsEnabled);
            command.Parameters.AddWithValue("@actions", guild.GuildSettings.Modules.ActionsEnabled);
            command.Parameters.AddWithValue("@admin", guild.GuildSettings.Modules.AdminEnabled);
            command.Parameters.AddWithValue("@custom", guild.GuildSettings.Modules.CustomEnabled);
            command.Parameters.AddWithValue("@fun", guild.GuildSettings.Modules.FunEnabled);
            command.Parameters.AddWithValue("@info", guild.GuildSettings.Modules.InformationEnabled);
            command.Parameters.AddWithValue("@lewd", guild.GuildSettings.Modules.LewdEnabled);
            command.Parameters.AddWithValue("@search", guild.GuildSettings.Modules.SearchEnabled);
            command.Parameters.AddWithValue("@stats", guild.GuildSettings.Modules.StatsEnabled);
            command.Parameters.AddWithValue("@weeb", guild.GuildSettings.Modules.WeebEnabled);*/
            command.Parameters.AddWithValue("@guildID", guild.ID);

            return command;
        }

        public static MySqlCommand GetMySqlCommand(this Pasta pasta)
        {
            var command = new MySqlCommand(
                "UPDATE `pasta`  SET " +
                "`Content` = @content, " +
                "`OwnerID` = @ownerid, " +
                "`Upvotes` = @upvotes, " +
                "`Downvotes` = @downvotes, " +
                "`Name` = @name, " +
                "`Created` = @created");

            command.Parameters.AddWithValue("@content", pasta.Content);
            command.Parameters.AddWithValue("@ownerid", pasta.OwnerID);
            command.Parameters.AddWithValue("@upvotes", pasta.Upvotes);
            command.Parameters.AddWithValue("@downvotes", pasta.Downvotes);
            command.Parameters.AddWithValue("@name", pasta.Name);
            command.Parameters.AddWithValue("@created", pasta.Created);

            return command;
        }
    }
}
