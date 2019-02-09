using Discord;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Utilities;
using System;
using System.Threading.Tasks;

namespace Skuld.Discord.Handlers
{
    public static class MessageSender
    {
        public static async Task<IUserMessage> ReplyAsync(IMessageChannel channel, string message, Embed embed = null)
        {
            if (channel == null) { return null; }

            await channel.TriggerTypingAsync();
            await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
            return await channel.SendMessageAsync(message, false, embed ?? null);
        }
        public static async Task<IUserMessage> ReplyWithFileAsync(IMessageChannel channel, string message, string filepath)
        {
            if (channel == null) return null;

            await channel.TriggerTypingAsync();
            await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
            return await channel.SendFileAsync(filepath, message);
        }

        public static async Task<IUserMessage> ReplyWithMentionAsync(IMessageChannel channel, IUser user, string message, Embed embed = null)
            => await ReplyAsync(channel, user.Mention + " " + message, embed).ConfigureAwait(false);

        public static async Task<IUserMessage> ReplyWithMentionAndFileAsync(IMessageChannel channel, IUser user, string message, string filepath)
            => await ReplyWithFileAsync(channel, user.Mention + " " + message, filepath).ConfigureAwait(false);

        public static async Task<IUserMessage> ReplyDMsAsync(IDMChannel channel, IMessageChannel backupchannel, string message, Embed embed = null)
        {
            IUserMessage msg = null;
            try
            {
                await channel.TriggerTypingAsync();
                await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {channel.Recipient} in DMs", LogSeverity.Info));
                msg = await backupchannel.SendMessageAsync(DiscordTools.Ok_Emoji + " Check your DMs");
                return await channel.SendMessageAsync(message, false, embed ?? null);
            }
            catch (Exception ex)
            {
                await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                await msg.ModifyAsync(x => { x.Embed = embed ?? null; x.Content = "I couldn't send the message to your DMs, so I sent it here instead\n\n" + message; });
                return msg;
            }
        }

        public static async Task<IUserMessage> ReplyDMFailableAsync(IDMChannel channel, string message, Embed embed = null)
        {
            await channel.TriggerTypingAsync();
            await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {channel.Recipient} in DMs", LogSeverity.Info));
            return await channel.SendMessageAsync(message, false, embed ?? null);
        }

        public static async Task ReplyWithTimedMessage(IMessageChannel channel, string message, Embed embed, double timeout)
        {
            if (channel == null) { return; }
            await channel.TriggerTypingAsync();
            await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Dispatched message to {channel}", LogSeverity.Info));
            var msg = await channel.SendMessageAsync(message, false, embed);
            await msg.DeleteAfterSecondsAsync((int)timeout);
        }

        public static async Task ReplyFailedAsync(IMessageChannel channel)
            => await ReplyAsync(channel, DiscordTools.Failed_Emoji + " Command Execution failed with reason: \"Unknown\"");

        public static async Task ReplyFailedAsync(IMessageChannel channel, string reason)
            => await ReplyAsync(channel, DiscordTools.Failed_Emoji + " Command Execution failed with reason: \"" + reason + "\"");

        public static async Task ReplySuccessAsync(IMessageChannel channel)
            => await ReplyAsync(channel, DiscordTools.Successful_Emoji + " Command Execution was successful");

        public static async Task ReplySuccessAsync(IMessageChannel channel, string reason)
            => await ReplyAsync(channel, DiscordTools.Successful_Emoji + " Command Execution was successful. Extra Information: \"" + reason + "\"");
    }
}
