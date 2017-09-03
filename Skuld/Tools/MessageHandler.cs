using System;
using System.Threading.Tasks;
using Discord;
using System.Timers;
using System.Diagnostics;
using System.Linq;

namespace Skuld.Tools
{
    public partial class MessageHandler
    {
        public static async Task<IUserMessage> SendChannel(IChannel channel, string message, Embed embed = null)
        {
            var TextChan = (ITextChannel)channel;
            var MesgChan = (IMessageChannel)channel;
            IUserMessage Message;
            await MesgChan.TriggerTypingAsync();
            try
            {
                if (embed == null)
                    Message = await MesgChan.SendMessageAsync(message);
                else
                {
                    var perm = (await TextChan.Guild.GetUserAsync(Bot.bot.CurrentUser.Id)).GetPermissions(TextChan).EmbedLinks;
                    if (!perm)
                    {
                        if (message != null)
                            Message = await MesgChan.SendMessageAsync(message + "\n" + ConvertEmbedToText(embed));
                        else
                            Message = await MesgChan.SendMessageAsync(ConvertEmbedToText(embed));             
                    }
                    else
                        Message = await MesgChan.SendMessageAsync(message, false, embed);                    
                }
                Bot.Logs.Add(new Models.LogMessage("MsgDisp", $"Dispactched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                return Message;
            }
            catch (Exception ex)
            {
                Bot.Logs.Add(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                return null;
            }
        }
        private static IUserMessage dmsg = null;
        public static async Task SendChannel(IChannel channel, string message, double Timeout, Embed embed = null)
        {
            var TextChan = (ITextChannel)channel;
            var MesgChan = (IMessageChannel)channel;
            await MesgChan.TriggerTypingAsync();
            try
            {
                if (embed == null)
                    dmsg = await MesgChan.SendMessageAsync(message);
                else
                {
                    var perm = (await TextChan.Guild.GetUserAsync(Bot.bot.CurrentUser.Id)).GetPermissions(TextChan).EmbedLinks;
                    if (!perm)
                    {
                        if (message != null)
                            dmsg = await MesgChan.SendMessageAsync(message + "\n" + ConvertEmbedToText(embed));
                        else
                            dmsg = await MesgChan.SendMessageAsync(ConvertEmbedToText(embed));
                    }
                    else if (perm)
                        dmsg = await MesgChan.SendMessageAsync(message, false, embed);
                }
                Bot.Logs.Add(new Models.LogMessage("MsgDisp", $"Dispactched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                Stopwatch timer = new Stopwatch();
                timer.Start();
                while (timer.IsRunning)
                {
                    if (timer.ElapsedMilliseconds == Timeout * 1000)
                    {
                        await dmsg.DeleteAsync();
                        timer.Stop();
                        Bot.Logs.Add(new Models.LogMessage("MsgDisp", $"Deleted a timed message in channel: {(channel as IGuildChannel)} in guild: {(channel as IGuildChannel).Guild}", LogSeverity.Info));
                    }
                }
            }
            catch (Exception ex)
            {
                Bot.Logs.Add(new Models.LogMessage("MH-ChHV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
            }
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(dmsg != null)
            {
                dmsg.DeleteAsync().Wait();
                dmsg = null;
            }            
        }

        public static async Task<IUserMessage> SendDMs(IMessageChannel channel, IDMChannel user, string message, Embed embed = null)
        {
            try
            {
                IUserMessage dmsg = null;
                if (embed == null)
                    dmsg = await user.SendMessageAsync(message);
                else
                    dmsg = await user.SendMessageAsync(message, false, embed);
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
