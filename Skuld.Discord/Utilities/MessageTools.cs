using Discord.Commands;
using Discord.WebSocket;
using Skuld.Core.Extensions;
using Skuld.Core.Generic.Models;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Discord.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using DiscordNet = Discord;

namespace Skuld.Discord.Utilities
{
    public static class MessageTools
    {
        private static readonly string Key = "MsgTools";
        public static string ModAdminBypass = "-!{MA_commands}";
        public static string NoOneCommands = "-!commands";

        public static async Task<bool> CheckEmbedPermission(DiscordShardedClient client, DiscordNet.IChannel channel)
        {
            if (channel is DiscordNet.IDMChannel)
            {
                return true;
            }

            var gchan = channel as DiscordNet.ITextChannel;
            var gusr = await gchan.GetUserAsync(client.CurrentUser.Id).ConfigureAwait(false);
            return gusr.GetPermissions(gchan).EmbedLinks;
        }

        public static string GetPrefixFromCommand(Guild guild, string command, SkuldConfig config)
        {
            if (guild != null) { if (command.StartsWith(guild.Prefix)) { return guild.Prefix; } }

            if (command.StartsWith(config.Prefix)) { return config.Prefix; }

            if (command.StartsWith(config.AltPrefix)) { return config.AltPrefix; }

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

        public static string GetCmdName(DiscordNet.IUserMessage arg, SkuldConfig config, DiscordShardedClient client, User initiator, Guild sguild = null)
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
                if ((initiator.Flags & Utils.BotAdmin) != 0 && contentsplit.StartsWith("ayo"))
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
                if ((initiator.Flags & Utils.BotAdmin) != 0 && contentsplit.StartsWith("ayo"))
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

        public static bool HasPrefix(DiscordNet.IUserMessage message, DiscordShardedClient client, MessageServiceConfig config, User initiator, string gprefix = null)
        {
            if (gprefix != null)
            {
                if ((initiator.Flags & Utils.BotAdmin) != 0)
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
                if ((initiator.Flags & Utils.BotAdmin) != 0)
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

            if (gprefix != null)
                if (message.Content.StartsWith(gprefix))
                    return gprefix;

            return null;
        }

        public static bool ModuleDisabled(GuildModules cmdmods, CommandInfo command)
        {
            if (!cmdmods.Accounts && command.Module.Name.ToLowerInvariant() == "accounts")
            { return true; }
            if (!cmdmods.Actions && command.Module.Name.ToLowerInvariant() == "actions")
            { return true; }
            if (!cmdmods.Admin && command.Module.Name.ToLowerInvariant() == "admin")
            { return true; }
            if (!cmdmods.Fun && command.Module.Name.ToLowerInvariant() == "fun")
            { return true; }
            if (!cmdmods.Information && command.Module.Name.ToLowerInvariant() == "information")
            { return true; }
            if (!cmdmods.Lewd && command.Module.Name.ToLowerInvariant() == "lewd")
            { return true; }
            if (!cmdmods.Search && command.Module.Name.ToLowerInvariant() == "search")
            { return true; }
            if (!cmdmods.Space && command.Module.Name.ToLowerInvariant() == "space")
            { return true; }
            if (!cmdmods.Stats && command.Module.Name.ToLowerInvariant() == "stats")
            { return true; }
            if (!cmdmods.Weeb && command.Module.Name.ToLowerInvariant() == "weeb")
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
                Log.Info(Key, $"Dispatched message to {(channel as DiscordNet.IGuildChannel).Guild} in {(channel as DiscordNet.IGuildChannel).Name}");
                return await mesgChan.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                Log.Error(Key, $"Error dispatching Message - {ex.Message}", ex);
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
                Log.Info(Key, $"Dispatched message to {(channel as DiscordNet.IGuildChannel).Guild} in {(channel as DiscordNet.IGuildChannel).Name}");
                return msg;
            }
            catch (Exception ex)
            {
                Log.Error(Key, $"Error dispatching Message - {ex.Message}", ex);
                return null;
            }
        }
    }
}