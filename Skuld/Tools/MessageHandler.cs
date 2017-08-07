using System;
using System.Threading.Tasks;
using Discord;
using System.Timers;

namespace Skuld.Tools
{
    public class MessageHandler
    {
        private static IUserMessage dmsg = null;
        public static async Task SendChannel(IMessageChannel channel, string message, Embed embed = null, double? Timeout = null)
        {
            await channel.TriggerTypingAsync();
            if (Timeout.HasValue)
            {
                try
                {                    
                    if (embed == null)
                        dmsg = await channel.SendMessageAsync(message);
                    else
                    {
                        dmsg = await channel.SendMessageAsync(message, false, embed);
                    }                        
                    Bot.Logs.Add(new Models.LogMessage("MsgDisp", $"Dispactched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                    Timer timer = new Timer(Timeout.Value*1000);
                    timer.Elapsed += Timer_Elapsed;
                    timer.Start();
                }
                catch (Exception ex)
                {
                    Bot.Logs.Add(new Models.LogMessage("MH-ChHV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                }
            }
            else
            {
                try
                {
                    if (embed == null)
                        await channel.SendMessageAsync(message);
                    else
                    {
                        await channel.SendMessageAsync(message, false, embed);
                    }                        
                    Bot.Logs.Add(new Models.LogMessage("MsgDisp", $"Dispactched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
                }
                catch (Exception ex)
                {
                    Bot.Logs.Add(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
                }
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

        public static async Task SendDMs(IMessageChannel channel, IDMChannel user, string message, Embed embed = null)
        {
            try
            {
                await channel.TriggerTypingAsync();
                await channel.SendMessageAsync(":ok_hand: Check your DMs.");
                if (embed == null)
                    await user.SendMessageAsync(message);
                else
                    await user.SendMessageAsync(message, false, embed);
            }
            catch (Exception ex)
            {
                Bot.Logs.Add(new Models.LogMessage("MsgH-DM", "Error dispatching Direct Message to user, printed exception to logs.", LogSeverity.Warning, ex));
            }
        }
    }
}
