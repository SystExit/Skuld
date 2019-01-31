using Discord;
using Skuld.Core;
using Skuld.Discord.Handlers;
using Skuld.Discord.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.Discord.Services
{
    public class MessageQueue
    {
        private static ConcurrentQueue<SkuldMessage> messageQueue = new ConcurrentQueue<SkuldMessage>();

        private static async Task ExecuteAsync()
        {
            while(true)
            {
                if(!messageQueue.IsEmpty)
                {
                    if(messageQueue.TryDequeue(out SkuldMessage message))
                    {
                        try
                        {
                            switch(message.Meta.Type)
                            {
                                case Models.MessageType.Standard:
                                    await MessageSender.ReplyAsync(message.Channel, message.Content.Message, message.Content.Embed);
                                    break;
                                case Models.MessageType.Mention:
                                    await MessageSender.ReplyWithMentionAsync(message.Channel, message.Content.User, message.Content.Message, message.Content.Embed);
                                    break;
                                case Models.MessageType.Success:
                                    await MessageSender.ReplySuccessAsync(message.Channel, message.Content.Message);
                                    break;
                                case Models.MessageType.Failed:
                                    await MessageSender.ReplyFailedAsync(message.Channel, message.Content.Message);
                                    break;
                                case Models.MessageType.DMS:
                                    await MessageSender.ReplyDMsAsync(await message.Content.User.GetOrCreateDMChannelAsync(), message.Channel, message.Content.Message, message.Content.Embed);
                                    break;
                                case Models.MessageType.DMFail:
                                    await MessageSender.ReplyDMFailableAsync(await message.Content.User.GetOrCreateDMChannelAsync(), message.Content.Message, message.Content.Embed);
                                    break;
                                case Models.MessageType.Timed:
                                    await MessageSender.ReplyWithTimedMessage(message.Channel, message.Content.Message, message.Content.Embed, message.Meta.Timeout);
                                    break;
                                case Models.MessageType.File:
                                    await MessageSender.ReplyWithFileAsync(message.Channel, message.Content.Message, message.Content.File);
                                    break;
                                case Models.MessageType.MentionFile:
                                    await MessageSender.ReplyWithMentionAndFileAsync(message.Channel, message.Content.User, message.Content.Message, message.Content.File);
                                    break;
                            }
                            await Task.Delay(250);
                        }
                        catch(Exception ex)
                        {
                            await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("MsgQueue", ex.Message, LogSeverity.Critical, ex));
                            await MessageSender.ReplyFailedAsync(message.Channel, ex.Message);
                        }
                    }
                }
                else
                {
                    await Task.Delay(50);
                }
            }
        }

        public static void AddMessage(SkuldMessage message)
            => messageQueue.Enqueue(message);

        public static void Run()
            => Task.Run(async () => await ExecuteAsync());
    }
}
