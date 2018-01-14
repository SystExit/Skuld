using System;
using System.Threading.Tasks;
using Discord;
using System.Timers;
using System.Diagnostics;

namespace Skuld.Tools
{
    public partial class MessageHandler
    {
        public static async Task<IUserMessage> SendChannel(IChannel channel, string message, Embed embed = null, string filename = null)
        {
            var textChan = (ITextChannel)channel;
            var mesgChan = (IMessageChannel)channel;
            IUserMessage msg;
            await mesgChan.TriggerTypingAsync();
            try
            {
                if (embed == null)
                {
                    if(filename == null)
                        msg = await mesgChan.SendMessageAsync(message);
                    else
                        msg = await mesgChan.SendFileAsync(filename, message);
                }
                else
                {
                    var perm = (await textChan.Guild.GetUserAsync(Bot.bot.CurrentUser.Id)).GetPermissions(textChan).EmbedLinks;
                    if (!perm)
                    {
                        if (message != null)
                            msg = await mesgChan.SendMessageAsync(message + "\n" + ConvertEmbedToText(embed));
                        else
                            msg = await mesgChan.SendMessageAsync(ConvertEmbedToText(embed));             
                    }
                    else
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
        private static IUserMessage dmsg = null;
        public static async Task SendChannel(IChannel channel, string message, double timeout, EmbedBuilder embed = null, string filename = null)
        {
            var cmdstopwatch = CommandManager.CommandStopWatch;
            cmdstopwatch.Stop();

            embed.Footer.Text = "Command took " + cmdstopwatch.ElapsedMilliseconds + "ms";
            var textChan = (ITextChannel)channel;
            var mesgChan = (IMessageChannel)channel;
            await mesgChan.TriggerTypingAsync();
            try
            {
                if (embed == null)
                {
                    if (filename == null)
                        dmsg = await mesgChan.SendMessageAsync(message);
                    else
                        dmsg = await mesgChan.SendFileAsync(filename, message);
                }
                else
                {
                    var perm = (await textChan.Guild.GetUserAsync(Bot.bot.CurrentUser.Id)).GetPermissions(textChan).EmbedLinks;
                    if (!perm)
                    {
                        if (message != null)
                            dmsg = await mesgChan.SendMessageAsync(message + "\n" + ConvertEmbedToText(embed.Build()));
                        else
                            dmsg = await mesgChan.SendMessageAsync(ConvertEmbedToText(embed.Build()));
                    }
                    else if (perm)
                        dmsg = await mesgChan.SendMessageAsync(message, isTTS: false, embed: embed.Build());
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
                Bot.Logs.Add(new Models.LogMessage("MH-ChHV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
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

        public static async Task<IUserMessage> SendDMs(IMessageChannel channel, IDMChannel user, string message, Embed embed = null)
        {
            try
            {
                IUserMessage dmsg = null;
                if (embed == null)
                    dmsg = await user.SendMessageAsync(message);
                else
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
