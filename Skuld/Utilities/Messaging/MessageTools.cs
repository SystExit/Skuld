using DiscordNet = Discord;
using Discord.WebSocket;
using Skuld.Core.Models;
using Skuld.Models.Database;
using Skuld.Services;
using System.Threading.Tasks;
using Discord.Commands;
using Skuld.Core.Services;
using System;
using Skuld.Models;
using Skuld.Extensions;

namespace Skuld.Utilities.Messaging
{
    public class MessageTools
    {
        public static async Task<bool> CheckEmbedPermission(DiscordNet.IGuildChannel channel)
            => (await channel.Guild.GetCurrentUserAsync()).GetPermissions(channel).EmbedLinks;

        public static async Task<CustomCommand> GetCustomCommandAsync(SocketGuild guild, string command, DatabaseService database)
        {
            if (database.CanConnect)
            {
                var cmd = await database.GetCustomCommandAsync(guild.Id, command).ConfigureAwait(false);
                if (cmd != null) return cmd;
            }
            return null;
        }

        public static string GetPrefixFromCommand(SkuldGuild guild, string command, SkuldConfig config)
        {
            if (guild != null) { if (command.StartsWith(guild.Prefix)) { return guild.Prefix; } }

            if (command.StartsWith(config.Discord.Prefix)) { return config.Discord.Prefix; }

            if (command.StartsWith(config.Discord.AltPrefix)) { return config.Discord.AltPrefix; }

            return null;
        }

        public static string GetCommandName(string prefix, int argpos, SocketMessage message)
        {
            string cmdname = message.Content;

            if (cmdname.StartsWith(prefix))
            { cmdname = cmdname.Remove(argpos, prefix.Length); }

            var content = cmdname.Split(' ');

            return content[0];
        }

        public static bool IsEnabledChannel(DiscordNet.IGuildUser user, DiscordNet.ITextChannel channel)
        {
            if (user.GuildPermissions.Administrator) return true;
            if (channel == null) return true;
            if (channel.Topic == null) return true;
            if (channel.Topic.Contains("-command")) return false;
            return true;
        }

        public static async Task<SkuldGuild> GetGuildAsync(DiscordNet.IGuild guild, DatabaseService database)
        {
            if (database.CanConnect)
            {
                var sguild = await database.GetGuildAsync(guild.Id).ConfigureAwait(false);
                if (sguild == null)
                {
                    await database.InsertGuildAsync(guild).ContinueWith(async x =>
                    {
                        sguild = await database.GetGuildAsync(guild.Id).ConfigureAwait(false);
                    });
                }
                return sguild;
            }
            else
            {
                return null;
            }
        }

        public static async Task<SkuldUser> GetUserAsync(DiscordNet.IUser user, DatabaseService database)
        {
            if (database.CanConnect)
            {
                var usr = await database.GetUserAsync(user.Id);
                if (usr == null) { await database.InsertUserAsync(user).ConfigureAwait(false); usr = await database.GetUserAsync(user.Id); }
                return usr;
            }
            else
            {
                return null;
            }
        }

        public static string GetCmdName(DiscordNet.IUserMessage arg, DiscordConfig config, DatabaseService database, SkuldGuild sguild = null)
        {
            string content = "";
            var contentsplit = arg.Content.Split(' ')[0];
            if (config.AltPrefix != null)
            {
                if (database.CanConnect)
                {
                    if (contentsplit.StartsWith(sguild.Prefix))
                        content = contentsplit.Replace(sguild.Prefix, "");
                    if (contentsplit.StartsWith(config.Prefix))
                        content = contentsplit.Replace(config.Prefix, "");
                    if (contentsplit.StartsWith(config.AltPrefix))
                        content = contentsplit.Replace(config.AltPrefix, "");
                }
                else
                {
                    if (contentsplit.StartsWith(config.Prefix))
                        content = contentsplit.Replace(config.Prefix, "");
                    if (contentsplit.StartsWith(config.AltPrefix))
                        content = contentsplit.Replace(config.AltPrefix, "");
                }
            }
            else
            {
                if (database.CanConnect)
                {
                    if (contentsplit.StartsWith(sguild.Prefix))
                        content = contentsplit.Replace(sguild.Prefix, "");
                    if (contentsplit.StartsWith(config.Prefix))
                        content = contentsplit.Replace(config.Prefix, "");
                }
                else
                {
                    if (contentsplit.StartsWith(config.Prefix))
                        content = contentsplit.Replace(config.Prefix, "");
                }
            }
            return content;
        }

        public static bool HasPrefix(DiscordNet.IUserMessage message, MessageServiceConfig config, string gprefix = null)
        {
            bool retn;
            if (gprefix != null)
            {
                retn = message.HasStringPrefix(gprefix, ref config.ArgPos) ||
                       message.HasStringPrefix(config.Prefix, ref config.ArgPos) ||
                       message.HasStringPrefix(config.AltPrefix, ref config.ArgPos);
            }
            else
            {
                retn = message.HasStringPrefix(config.Prefix, ref config.ArgPos) ||
                       message.HasStringPrefix(config.AltPrefix, ref config.ArgPos);
            }
            return retn;
        }

        public static bool ModuleDisabled(GuildCommandModules cmdmods, CommandInfo command)
        {
            if (!cmdmods.AccountsEnabled && command.Module.Name.ToLowerInvariant() == "accounts")
            { return true; }
            if (!cmdmods.ActionsEnabled && command.Module.Name.ToLowerInvariant() == "actions")
            { return true; }
            if (!cmdmods.AdminEnabled && command.Module.Name.ToLowerInvariant() == "admin")
            { return true; }
            if (!cmdmods.FunEnabled && command.Module.Name.ToLowerInvariant() == "fun")
            { return true; }
            if (!cmdmods.InformationEnabled && command.Module.Name.ToLowerInvariant() == "information")
            { return true; }
            if (!cmdmods.LewdEnabled && command.Module.Name.ToLowerInvariant() == "lewd")
            { return true; }
            if (!cmdmods.SearchEnabled && command.Module.Name.ToLowerInvariant() == "search")
            { return true; }
            if (!cmdmods.StatsEnabled && command.Module.Name.ToLowerInvariant() == "stats")
            { return true; }
            if (!cmdmods.WeebEnabled && command.Module.Name.ToLowerInvariant() == "weeb")
            { return true; }

            return false;
        }

        public static async Task<DiscordNet.IUserMessage> SendChannelAsync(DiscordNet.IChannel channel, string message, GenericLogger logger)
        {
            try
            {
                var textChan = (DiscordNet.ITextChannel)channel;
                var mesgChan = (DiscordNet.IMessageChannel)channel;
                if (channel == null || textChan == null || mesgChan == null) { return null; }
                await mesgChan.TriggerTypingAsync();
                await logger.AddToLogsAsync(new LogMessage("MsgDisp", $"Dispatched message to {(channel as DiscordNet.IGuildChannel).Guild} in {(channel as DiscordNet.IGuildChannel).Name}", DiscordNet.LogSeverity.Info));
                return await mesgChan.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", DiscordNet.LogSeverity.Warning, ex));
                return null;
            }
        }

        public static async Task<DiscordNet.IUserMessage> SendChannelAsync(DiscordNet.IChannel channel, string message, DiscordNet.Embed embed, GenericLogger logger)
        {
            try
            {
                var textChan = (DiscordNet.ITextChannel)channel;
                var mesgChan = (DiscordNet.IMessageChannel)channel;
                if (channel == null || textChan == null || mesgChan == null) { return null; }
                await mesgChan.TriggerTypingAsync();
                DiscordNet.IUserMessage msg;
                var perm = await CheckEmbedPermission(textChan);
                if (!perm)
                {
                    if (message != null)
                    {
                        msg = await mesgChan.SendMessageAsync(message + "\n" + embed.ToMessage());
                    }
                    else
                    {
                        msg = await mesgChan.SendMessageAsync(embed.ToMessage());
                    }
                }
                else
                {
                    msg = await mesgChan.SendMessageAsync(message, false, embed);
                }
                await logger.AddToLogsAsync(new LogMessage("MsgDisp", $"Dispatched message to {(channel as DiscordNet.IGuildChannel).Guild} in {(channel as DiscordNet.IGuildChannel).Name}", DiscordNet.LogSeverity.Info));
                return msg;
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Core.Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", DiscordNet.LogSeverity.Warning, ex));
                return null;
            }
        }
    }
}