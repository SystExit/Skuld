using Discord;
using Skuld.Core.Utilities;
using Skuld.Discord.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Skuld.Discord.Handlers
{
    public static class MessageSender
    {
        private const string Key = "MsgDisp";

        public static async Task<IUserMessage> ReplyAsync(IMessageChannel channel, string message, Embed embed = null)
        {
            if (channel == null) { return null; }

            await channel.TriggerTypingAsync().ConfigureAwait(false);

            if (channel is IGuildChannel)
            {
                if ((channel as IGuildChannel).Guild != null)
                {
                    Log.Info(Key, $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}");
                }
            }
            else
            {
                Log.Info(Key, $"Dispatched message to {channel.Name}");
            }

            return await channel.SendMessageAsync(message, false, embed ?? null).ConfigureAwait(false);
        }

        public static async Task<IUserMessage> ReplyWithFileAsync(IMessageChannel channel, string message, Stream file, string fileName, Embed embed = null)
        {
            if (channel == null) return null;

            await channel.TriggerTypingAsync().ConfigureAwait(false);
            Log.Info(Key, $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}");
            return await channel.SendFileAsync(file, fileName, message, false, embed).ConfigureAwait(false);
        }

        public static async Task<IUserMessage> ReplyWithMentionAsync(IMessageChannel channel, IUser user, string message, Embed embed = null)
            => await ReplyAsync(channel, user.Mention + " " + message, embed).ConfigureAwait(false);

        public static async Task<IUserMessage> ReplyWithMentionAndFileAsync(IMessageChannel channel, IUser user, string message, Stream file, string fileName)
            => await ReplyWithFileAsync(channel, user.Mention + " " + message, file, fileName).ConfigureAwait(false);

        public static async Task<IUserMessage> ReplyDMsAsync(IDMChannel channel, IMessageChannel backupchannel, string message, Embed embed = null)
        {
            IUserMessage msg = null;
            try
            {
                await channel.TriggerTypingAsync().ConfigureAwait(false);
                Log.Info(Key, $"Dispatched message to {channel.Recipient} in DMs");
                if (backupchannel != channel) msg = await backupchannel.SendMessageAsync(DiscordTools.Ok_Emoji + " Check your DMs").ConfigureAwait(false);
                return await channel.SendMessageAsync(message, false, embed ?? null).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Warning(Key, "Error dispatching Message, printed exception to logs.", ex);
                await msg.ModifyAsync(x => { x.Embed = embed ?? null; x.Content = "I couldn't send the message to your DMs, so I sent it here instead\n\n" + message; }).ConfigureAwait(false);
                return msg;
            }
        }

        public static async Task<IUserMessage> ReplyDMFailableAsync(IDMChannel channel, string message, Embed embed = null)
        {
            await channel.TriggerTypingAsync().ConfigureAwait(false);
            Log.Info(Key, $"Dispatched message to {channel.Recipient} in DMs");
            return await channel.SendMessageAsync(message, false, embed ?? null).ConfigureAwait(false);
        }

        public static async Task ReplyWithTimedMessage(IMessageChannel channel, string message, Embed embed, double timeout)
        {
            if (channel == null) { return; }
            await channel.TriggerTypingAsync().ConfigureAwait(false);
            Log.Info(Key, $"Dispatched message to {channel}");
            var msg = await channel.SendMessageAsync(message, false, embed).ConfigureAwait(false);
            await msg.DeleteAfterSecondsAsync((int)timeout).ConfigureAwait(false);
        }

        public static async Task<IUserMessage> ReplyFailedAsync(IMessageChannel channel)
            => await ReplyAsync(channel, DiscordTools.Failed_Emoji + " Command Execution failed with reason: \"Unknown\"").ConfigureAwait(false);

        public static async Task<IUserMessage> ReplyFailedAsync(IMessageChannel channel, string reason)
            => await ReplyAsync(channel, DiscordTools.Failed_Emoji + " Command Execution failed with reason: \"" + reason + "\"").ConfigureAwait(false);

        public static async Task<IUserMessage> ReplySuccessAsync(IMessageChannel channel)
            => await ReplyAsync(channel, DiscordTools.Successful_Emoji + " Command Execution was successful").ConfigureAwait(false);

        public static async Task<IUserMessage> ReplySuccessAsync(IMessageChannel channel, string reason)
            => await ReplyAsync(channel, DiscordTools.Successful_Emoji + " Command Execution was successful. Extra Information: \"" + reason + "\"").ConfigureAwait(false);
    }
}