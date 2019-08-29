using Discord.Commands;
using Discord.WebSocket;
using DiscordNet = Discord;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Core.Models.Skuld;
using Skuld.Database;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Skuld.Discord.Utilities
{
    public static class MessageTools
    {
        public static string ModAdminBypass = "-!{MA_commands}";
        public static string NoOneCommands = "-!commands";

        public static async Task<bool> CheckEmbedPermission(DiscordShardedClient client, DiscordNet.IChannel channel)
        {
            if(channel is DiscordNet.IDMChannel)
            {
                return true;
            }

            var gchan = channel as DiscordNet.ITextChannel;
            var gusr = await gchan.GetUserAsync(client.CurrentUser.Id).ConfigureAwait(false);
            return gusr.GetPermissions(gchan).EmbedLinks;
        }

        public static async Task<CustomCommand> GetCustomCommandAsync(SocketGuild guild, string command)
        {
            var resp = await DatabaseClient.GetCustomCommandAsync(guild.Id, command).ConfigureAwait(false);
            if (resp.Successful && resp.Data is CustomCommand) return resp.Data as CustomCommand;
            return null;
        }

        public static string GetPrefixFromCommand(SkuldGuild guild, string command, SkuldConfig config)
        {
            if (guild != null) { if (command.StartsWith(guild.Prefix)) { return guild.Prefix; } }

            if (command.StartsWith(config.Discord.Prefix)) { return config.Discord.Prefix; }

            if (command.StartsWith(config.Discord.AltPrefix)) { return config.Discord.AltPrefix; }

            return null;
        }

        public static string GetCommandName(string prefix, SocketMessage message)
        {
            string cmdname = message.Content;

            if (cmdname.StartsWith(prefix))
            { cmdname = cmdname.Remove(0, prefix.Length); }

            var content = cmdname.Split(' ');

            return content[0];
        }

        public static bool IsEnabledChannel(DiscordNet.IGuildUser user, DiscordNet.ITextChannel channel)
        {
            if (channel == null) return true;
            if (channel.Topic == null) return true;
            if (channel.Topic.Contains(ModAdminBypass))
            {
                if (user.GuildPermissions.Administrator) return true;
                else if (user.GuildPermissions.RawValue == DiscordUtilities.ModeratorPermissions.RawValue) return true;
                else return false;
            }
            if (channel.Topic.Contains(NoOneCommands)) return false;
            return true;
        }

        public static async Task<SkuldGuild> GetGuildOrInsertAsync(DiscordNet.IGuild guild)
        {
            var resp = await DatabaseClient.GetGuildAsync(guild.Id).ConfigureAwait(false);
            if(resp.Successful && resp.Data is SkuldGuild)
            {
                return resp.Data as SkuldGuild;
            }
            else
            {
                await DatabaseClient.InsertGuildAsync(guild.Id, SkuldConfig.Load().Discord.Prefix);
                resp = await DatabaseClient.GetGuildAsync(guild.Id);

                return resp.Data as SkuldGuild;
            }
        }

        public static async Task<SkuldUser> GetUserOrInsertAsync(DiscordNet.IUser user)
        {
            var resp = await DatabaseClient.GetUserAsync(user.Id).ConfigureAwait(false);
            if (resp.Successful && resp.Data is SkuldUser)
            {
                return resp.Data as SkuldUser;
            }
            else
            {
                await DatabaseClient.InsertUserAsync(user);
                resp = await DatabaseClient.GetUserAsync(user.Id);

                return resp.Data as SkuldUser;
            }
        }

        public static string GetCmdName(DiscordNet.IUserMessage arg, DiscordConfig config, DiscordShardedClient client, SkuldGuild sguild = null)
        {
            string content = "";
            var contentsplit = arg.Content.Split(' ')[0];
            if (config.AltPrefix != null)
            {
                if (sguild != null)
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
                if (SkuldConfig.Load().Discord.BotAdmins.Contains(arg.Author.Id) && contentsplit.StartsWith("ayo"))
                {
                    var split = arg.Content.Split(' ');

                    var botnamespace = 1;

                    if (split[1] != client.CurrentUser.Mention)
                    {
                        var splits = client.CurrentUser.Username.Split(' ').Length;
                        if (splits == 0)
                            botnamespace = 1;
                        else
                            botnamespace = splits;
                    }

                    content = split[1 + botnamespace + 1];
                }
            }
            else
            {
                if (sguild != null)
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
                if (SkuldConfig.Load().Discord.BotAdmins.Contains(arg.Author.Id) && contentsplit.StartsWith("ayo"))
                {
                    var split = arg.Content.Split(' ');

                    var botnamespace = 1;

                    if (split[1] != client.CurrentUser.Mention)
                    {
                        var splits = client.CurrentUser.Username.Split(' ').Length;
                        if (splits == 0)
                            botnamespace = 1;
                        else
                            botnamespace = splits;
                    }

                    content = split[1 + botnamespace + 1];
                }
            }
            return content;
        }

        public static bool HasPrefix(DiscordNet.IUserMessage message, DiscordShardedClient client, SkuldConfig sconf, MessageServiceConfig config, string gprefix = null)
        {
            if (gprefix != null)
            {
                if (sconf.Discord.BotAdmins.Contains(message.Author.Id))
                    return message.HasStringPrefix(gprefix, ref config.ArgPos) ||
                       message.HasStringPrefix(config.Prefix, ref config.ArgPos) ||
                       message.HasStringPrefix(config.AltPrefix, ref config.ArgPos) ||
                       message.HasStringPrefix($"ayo {client.CurrentUser.Username.ToLower()} do ", ref config.ArgPos) ||
                       message.HasStringPrefix($"ayo {client.CurrentUser.Mention} do ", ref config.ArgPos);

                return message.HasStringPrefix(gprefix, ref config.ArgPos) ||
                       message.HasStringPrefix(config.Prefix, ref config.ArgPos) ||
                       message.HasStringPrefix(config.AltPrefix, ref config.ArgPos);
            }
            else
            {
                if (sconf.Discord.BotAdmins.Contains(message.Author.Id))
                    return message.HasStringPrefix(config.Prefix, ref config.ArgPos) ||
                       message.HasStringPrefix(config.AltPrefix, ref config.ArgPos) ||
                       message.HasStringPrefix($"ayo {client.CurrentUser.Username.ToLower()} do ", ref config.ArgPos) ||
                       message.HasStringPrefix($"ayo {client.CurrentUser.Mention} do ", ref config.ArgPos);

                return message.HasStringPrefix(config.Prefix, ref config.ArgPos) ||
                       message.HasStringPrefix(config.AltPrefix, ref config.ArgPos);
            }
        }

        public static string GetPrefix(DiscordNet.IUserMessage message, MessageServiceConfig config, string gprefix = null)
        {
            if (message.Content.StartsWith(config.Prefix))
                return config.Prefix;
            if (message.Content.StartsWith(config.AltPrefix))
                return config.AltPrefix;

            if(gprefix != null)
                if (message.Content.StartsWith(gprefix))
                    return gprefix;

            return null;
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
            if (!cmdmods.SpaceEnabled && command.Module.Name.ToLowerInvariant() == "space")
            { return true; }
            if (!cmdmods.StatsEnabled && command.Module.Name.ToLowerInvariant() == "stats")
            { return true; }
            if (!cmdmods.WeebEnabled && command.Module.Name.ToLowerInvariant() == "weeb")
            { return true; }

            return false;
        }

#pragma warning disable IDE0060
        public static async Task<DiscordNet.IUserMessage> SendChannelAsync(this DiscordShardedClient client, DiscordNet.IChannel channel, string message)
        {
            try
            {
                var textChan = (DiscordNet.ITextChannel)channel;
                var mesgChan = (DiscordNet.IMessageChannel)channel;
                if (channel == null || textChan == null || mesgChan == null) { return null; }
                await mesgChan.TriggerTypingAsync();
                await GenericLogger.AddToLogsAsync(new LogMessage("MsgDisp", $"Dispatched message to {(channel as DiscordNet.IGuildChannel).Guild} in {(channel as DiscordNet.IGuildChannel).Name}", DiscordNet.LogSeverity.Info));
                return await mesgChan.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                await GenericLogger.AddToLogsAsync(new LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", DiscordNet.LogSeverity.Warning, ex));
                return null;
            }
        }
#pragma warning restore IDE0060

        public static async Task<DiscordNet.IUserMessage> SendChannelAsync(this DiscordShardedClient client, DiscordNet.IChannel channel, string message, DiscordNet.Embed embed)
        {
            try
            {
                var textChan = (DiscordNet.ITextChannel)channel;
                var mesgChan = (DiscordNet.IMessageChannel)channel;
                if (channel == null || textChan == null || mesgChan == null) { return null; }
                await mesgChan.TriggerTypingAsync();
                DiscordNet.IUserMessage msg;
                var perm = await CheckEmbedPermission(client, textChan);
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
                await GenericLogger.AddToLogsAsync(new LogMessage("MsgDisp", $"Dispatched message to {(channel as DiscordNet.IGuildChannel).Guild} in {(channel as DiscordNet.IGuildChannel).Name}", DiscordNet.LogSeverity.Info));
                return msg;
            }
            catch (Exception ex)
            {
                await GenericLogger.AddToLogsAsync(new LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", DiscordNet.LogSeverity.Warning, ex));
                return null;
            }
        }
    }
}