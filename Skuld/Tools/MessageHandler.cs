using System;
using System.Threading.Tasks;
using Discord;
using System.Timers;
using System.Diagnostics;

namespace Skuld.Tools
{
    public static class MessageHandler
    {
        public static async Task<IUserMessage> SendChannelAsync(IChannel channel, string message)
        {
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                await mesgChan.TriggerTypingAsync();
                Bot.Logger.AddToLogs(new Models.LogMessage("MsgDisp", $"Dispactched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                return await mesgChan.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                Bot.Logger.AddToLogs(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }
        public static async Task<IUserMessage> SendChannelAsync(IChannel channel, string message, Embed embed)
        {
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                IUserMessage msg;
                await mesgChan.TriggerTypingAsync();
                var perm = (await textChan.Guild.GetUserAsync(Bot.bot.CurrentUser.Id)).GetPermissions(textChan).EmbedLinks;
                if (!perm)
                {
                    if (message != null)
                    {
                        msg = await mesgChan.SendMessageAsync(message + "\n" + EmbedToText.ConvertEmbedToText(embed));
                    }
                    else
                    {
                        msg = await mesgChan.SendMessageAsync(EmbedToText.ConvertEmbedToText(embed));
                    }
                }
                else
                {
                    msg = await mesgChan.SendMessageAsync(message, false, embed);
                }
                Bot.Logger.AddToLogs(new Models.LogMessage("MsgDisp", $"Dispactched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                return msg;
            }
            catch (Exception ex)
            {
                Bot.Logger.AddToLogs(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }
        public static async Task<IUserMessage> SendChannelAsync(IChannel channel, string message, string filename)
        {
            var textChan = (ITextChannel)channel;
            var mesgChan = (IMessageChannel)channel;
            IUserMessage msg;
            await mesgChan.TriggerTypingAsync();
            try
            {
                msg = await mesgChan.SendFileAsync(filename, message);
                Bot.Logger.AddToLogs(new Models.LogMessage("MsgDisp", $"Dispactched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                return msg;
            }
            catch (Exception ex)
            {
                Bot.Logger.AddToLogs(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }
        
        public static async Task SendChannelAsync(IChannel channel, string message, double timeout)
        {
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                await mesgChan.TriggerTypingAsync();
                IUserMessage msg = await mesgChan.SendMessageAsync(message);
                Bot.Logger.AddToLogs(new Models.LogMessage("MsgDisp", $"Dispactched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                await Task.Delay((int)(timeout * 1000));
                DeleteMessage(msg);
            }
            catch (Exception ex)
            {
                Bot.Logger.AddToLogs(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
            }
        }
        public static async Task SendChannelAsync(IChannel channel, string message, double timeout, Embed embed)
        {
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                await mesgChan.TriggerTypingAsync();
                var perm = (await textChan.Guild.GetUserAsync(Bot.bot.CurrentUser.Id)).GetPermissions(textChan).EmbedLinks;
                IUserMessage msg = null;
                if (!perm)
                {
                    if (message != null)
                    {
                        msg = await mesgChan.SendMessageAsync(message + "\n" + EmbedToText.ConvertEmbedToText(embed));
                    }
                    else
                    {
                        msg = await mesgChan.SendMessageAsync(EmbedToText.ConvertEmbedToText(embed));
                    }
                }
                else
                {
                    msg = await mesgChan.SendMessageAsync(message, isTTS: false, embed: embed);
                }
                Bot.Logger.AddToLogs(new Models.LogMessage("MsgDisp", $"Dispactched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                await Task.Delay((int)(timeout * 1000));
                DeleteMessage(msg);
            }
            catch (Exception ex)
            {
                Bot.Logger.AddToLogs(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
            }
        }
        public static async Task SendChannelAsync(IChannel channel, string message, double timeout, string filename)
        {
            var textChan = (ITextChannel)channel;
            var mesgChan = (IMessageChannel)channel;
            await mesgChan.TriggerTypingAsync();
            try
            {
                IUserMessage msg = await mesgChan.SendFileAsync(filename, message);
                Bot.Logger.AddToLogs(new Models.LogMessage("MsgDisp", $"Dispactched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                await Task.Delay((int)(timeout * 1000));
                DeleteMessage(msg);
            }
            catch (Exception ex)
            {
                Bot.Logger.AddToLogs(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
            }
        }

        private static async void DeleteMessage(IUserMessage msg)
        {
            if(msg != null)
            {
                await msg.DeleteAsync();
                Bot.Logger.AddToLogs(new Models.LogMessage("MsgDisp", $"Deleted a timed message", LogSeverity.Info));
            }            
        }

        public static async Task<IUserMessage> SendDMsAsync(IMessageChannel channel, IDMChannel user, string message)
        {
            try
            {
                IUserMessage msg = null;
                msg = await user.SendMessageAsync(message);
                await SendChannelAsync(channel, $":ok_hand: <@{user.Recipient.Id}> Check your DMs.");
                return msg;
            }
            catch (Exception ex)
            {
                Bot.Logger.AddToLogs(new Models.LogMessage("MsgH-DM", "Error dispatching Direct Message to user, sending to channel instead. Printed exception to logs.", LogSeverity.Warning, ex));
                await SendChannelAsync(channel, "I couldn't send it to your DMs, so I sent it here instead... I hope you're not mad. <:blobcry:350681079415439361> " + message);
                return null;
            }
        }
        public static async Task<IUserMessage> SendDMsAsync(IMessageChannel channel, IDMChannel user, string message, Embed embed)
        {
            try
            {
                IUserMessage msg = null;
                msg = await user.SendMessageAsync(message, isTTS: false, embed: embed);
                await SendChannelAsync(channel, $":ok_hand: <@{user.Recipient.Id}> Check your DMs.");
                return msg;
            }
            catch (Exception ex)
            {
                Bot.Logger.AddToLogs(new Models.LogMessage("MsgH-DM", "Error dispatching Direct Message to user, sending to channel instead. Printed exception to logs.", LogSeverity.Warning, ex));
                await SendChannelAsync(channel, "I couldn't send it to your DMs, so I sent it here instead... I hope you're not mad. <:blobcry:350681079415439361> " + message, embed);
                return null;
            }
        }
    }
}
