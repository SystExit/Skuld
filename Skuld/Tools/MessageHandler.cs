using System;
using System.Threading.Tasks;
using Discord;
using System.Timers;
using System.Diagnostics;

namespace Skuld.Tools
{
    public class MessageHandler
    {
        public static async Task<IUserMessage> SendChannel(IChannel channel, string message)
        {
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                await mesgChan.TriggerTypingAsync();
                Bot.Logs.Add(new Models.LogMessage("MsgDisp", $"Dispactched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                return await mesgChan.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                Bot.Logs.Add(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }
        public static async Task<IUserMessage> SendChannel(IChannel channel, string message, Embed embed)
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
                    msg = await mesgChan.SendMessageAsync(message, isTTS: false, embed: embed);
                }
                Bot.Logs.Add(new Models.LogMessage("MsgDisp", $"Dispactched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                return msg;
            }
            catch (Exception ex)
            {
                Bot.Logs.Add(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }
        public static async Task<IUserMessage> SendChannel(IChannel channel, string message, string filename)
        {
            var textChan = (ITextChannel)channel;
            var mesgChan = (IMessageChannel)channel;
            IUserMessage msg;
            await mesgChan.TriggerTypingAsync();
            try
            {
                msg = await mesgChan.SendFileAsync(filename, message);
                Bot.Logs.Add(new Models.LogMessage("MsgDisp", $"Dispactched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                return msg;
            }
            catch (Exception ex)
            {
                Bot.Logs.Add(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }

        static IUserMessage dmsg = null;
        public static async Task SendChannel(IChannel channel, string message, double timeout)
        {
            try
            {
                var textChan = (ITextChannel)channel;
                var mesgChan = (IMessageChannel)channel;
                await mesgChan.TriggerTypingAsync();
                Bot.Logs.Add(new Models.LogMessage("MsgDisp", $"Dispactched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                var timer = new Stopwatch();
                dmsg = await mesgChan.SendMessageAsync(message);
                timer.Start();
                while (timer.IsRunning)
                {
                    if (timer.ElapsedMilliseconds == timeout * 1000)
                    {
                        await dmsg.DeleteAsync();
                        timer.Stop();
                        Bot.Logs.Add(new Models.LogMessage("MsgDisp", $"Deleted a timed message in channel: {(channel as IGuildChannel)} in guild: {(channel as IGuildChannel).Guild}", LogSeverity.Info));
                    }
                }
            }
            catch (Exception ex)
            {
                Bot.Logs.Add(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
            }
        }
        public static async Task SendChannel(IChannel channel, string message, double timeout, Embed embed)
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
                        dmsg = await mesgChan.SendMessageAsync(message + "\n" + EmbedToText.ConvertEmbedToText(embed));
                    }
                    else
                    {
                        dmsg = await mesgChan.SendMessageAsync(EmbedToText.ConvertEmbedToText(embed));
                    }
                }
                else
                {
                    dmsg = await mesgChan.SendMessageAsync(message, isTTS: false, embed: embed);
                }
                Bot.Logs.Add(new Models.LogMessage("MsgDisp", $"Dispactched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                var timer = new Stopwatch();
                timer.Start();
                while (timer.IsRunning)
                {
                    if (timer.ElapsedMilliseconds == timeout * 1000)
                    {
                        await dmsg.DeleteAsync();
                        timer.Stop();
                        Bot.Logs.Add(new Models.LogMessage("MsgDisp", $"Deleted a timed message in channel: {(channel as IGuildChannel)} in guild: {(channel as IGuildChannel).Guild}", LogSeverity.Info));
                    }
                }
            }
            catch (Exception ex)
            {
                Bot.Logs.Add(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
            }
        }
        public static async Task SendChannel(IChannel channel, string message, double timeout, string filename)
        {
            var textChan = (ITextChannel)channel;
            var mesgChan = (IMessageChannel)channel;
            IUserMessage msg;
            await mesgChan.TriggerTypingAsync();
            try
            {
                dmsg = await mesgChan.SendFileAsync(filename, message);
                Bot.Logs.Add(new Models.LogMessage("MsgDisp", $"Dispactched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                var timer = new Stopwatch();
                timer.Start();
                while (timer.IsRunning)
                {
                    if (timer.ElapsedMilliseconds == timeout * 1000)
                    {
                        await dmsg.DeleteAsync();
                        timer.Stop();
                        Bot.Logs.Add(new Models.LogMessage("MsgDisp", $"Deleted a timed message in channel: {(channel as IGuildChannel)} in guild: {(channel as IGuildChannel).Guild}", LogSeverity.Info));
                    }
                }
            }
            catch (Exception ex)
            {
                Bot.Logs.Add(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
            }
        }

        private static async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(dmsg != null)
            {
                await dmsg.DeleteAsync();
                dmsg = null;
            }            
        }

        public static async Task<IUserMessage> SendDMs(IMessageChannel channel, IDMChannel user, string message)
        {
            try
            {
                IUserMessage dmsg = null;
                dmsg = await user.SendMessageAsync(message);
                await SendChannel(channel, $":ok_hand: <@{user.Recipient.Id}> Check your DMs.");
                return dmsg;
            }
            catch (Exception ex)
            {
                Bot.Logs.Add(new Models.LogMessage("MsgH-DM", "Error dispatching Direct Message to user, sending to channel instead. Printed exception to logs.", LogSeverity.Warning, ex));
                await SendChannel(channel, "I couldn't send it to your DMs, so I sent it here instead... I hope you're not mad. <:blobcry:350681079415439361> " + message);
                return null;
            }
        }
        public static async Task<IUserMessage> SendDMs(IMessageChannel channel, IDMChannel user, string message, Embed embed)
        {
            try
            {
                IUserMessage dmsg = null;
                dmsg = await user.SendMessageAsync(message, isTTS: false, embed: embed);
                await SendChannel(channel, $":ok_hand: <@{user.Recipient.Id}> Check your DMs.");
                return dmsg;
            }
            catch (Exception ex)
            {
                Bot.Logs.Add(new Models.LogMessage("MsgH-DM", "Error dispatching Direct Message to user, sending to channel instead. Printed exception to logs.", LogSeverity.Warning, ex));
                await SendChannel(channel, "I couldn't send it to your DMs, so I sent it here instead... I hope you're not mad. <:blobcry:350681079415439361> " + message, embed);
                return null;
            }
        }
    }
}
