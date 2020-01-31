using Skuld.Discord.Models;
using System.Collections.Concurrent;

namespace Skuld.Discord.Services
{
    public static class MessageQueue
    {
        private const string Key = "MsgQueue";

        private static ConcurrentQueue<SkuldMessage> messageQueue = new ConcurrentQueue<SkuldMessage>();

        private const int messageDelay = 50;

        public static bool CheckForEmptyGuilds = false;
        /*
        private static async Task ExecuteAsync()
        {
            while (true)
            {
                try
                {
                    if (!messageQueue.IsEmpty)
                    {
                        if(CheckForEmptyGuilds)
                        {
                            var arr = messageQueue.ToArray();
                            var mssgQueue = new List<SkuldMessage>();
                            if (arr.Any(x=>x.Channel is IGuildChannel && (x.Channel as IGuildChannel).GuildId != 0))
                            {
                                foreach(var msg in arr)
                                {
                                    if(msg.Channel is IGuildChannel msgChan)
                                    {
                                        if(BotService.DiscordClient.GetGuild(msgChan.GuildId) != null)
                                        {
                                            mssgQueue.Add(msg);
                                        }
                                    }
                                }

                                messageQueue.Clear();
                                foreach (var item in mssgQueue)
                                    messageQueue.Enqueue(item);

                                CheckForEmptyGuilds = false;
                            }
                        }

                        if (messageQueue.TryDequeue(out SkuldMessage message))
                        {
                            try
                            {
                                switch (message.Meta.Type)
                                {
                                    case Models.MessageType.Standard:
                                        await MessageSender.ReplyAsync(message.Channel, message.Content.Message, message.Content.Embed).ConfigureAwait(false);
                                        break;

                                    case Models.MessageType.Mention:
                                        await MessageSender.ReplyWithMentionAsync(message.Channel, message.Content.User, message.Content.Message, message.Content.Embed).ConfigureAwait(false);
                                        break;

                                    case Models.MessageType.Success:
                                        if (!string.IsNullOrEmpty(message.Content.Message))
                                        {
                                            await MessageSender.ReplySuccessAsync(message.Channel, message.Content.Message).ConfigureAwait(false);
                                        }
                                        else
                                        {
                                            await MessageSender.ReplySuccessAsync(message.Channel).ConfigureAwait(false);
                                        }
                                        break;

                                    case Models.MessageType.Failed:
                                        if (!string.IsNullOrEmpty(message.Content.Message))
                                        {
                                            await MessageSender.ReplyFailedAsync(message.Channel, message.Content.Message).ConfigureAwait(false);
                                        }
                                        else
                                        {
                                            await MessageSender.ReplyFailedAsync(message.Channel).ConfigureAwait(false);
                                        }
                                        break;

                                    case Models.MessageType.DMS:
                                        await MessageSender.ReplyDMsAsync(await message.Content.User.GetOrCreateDMChannelAsync().ConfigureAwait(false), message.Channel, message.Content.Message, message.Content.Embed).ConfigureAwait(false);
                                        break;

                                    case Models.MessageType.DMFail:
                                        await MessageSender.ReplyDMFailableAsync(await message.Content.User.GetOrCreateDMChannelAsync().ConfigureAwait(false), message.Content.Message, message.Content.Embed).ConfigureAwait(false);
                                        break;

                                    case Models.MessageType.Timed:
                                        await MessageSender.ReplyWithTimedMessage(message.Channel, message.Content.Message, message.Content.Embed, message.Meta.Timeout).ConfigureAwait(false);
                                        break;

                                    case Models.MessageType.File:
                                        await MessageSender.ReplyWithFileAsync(message.Channel, message.Content.Message, message.Content.File, message.Content.FileName).ConfigureAwait(false);
                                        break;

                                    case Models.MessageType.MentionFile:
                                        await MessageSender.ReplyWithFileAsync(message.Channel, message.Content.Message, message.Content.File, message.Content.FileName).ConfigureAwait(false);
                                        break;
                                }

                                if (message.Content.File != null)
                                    await message.Content.File.DisposeAsync().ConfigureAwait(false);

                                await Task.Delay(messageDelay * messageQueue.Count).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                Log.Critical(Key, ex.Message, ex);
                                await MessageSender.ReplyFailedAsync(message.Channel, ex.Message).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            Log.Error(Key, "Error removing message from queue", null);
                        }
                    }
                    else
                    {
                        await Task.Delay(25).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    Log.Critical(Key, "Message Queue Service Failing with reason \"" + ex.Message + "\"", ex);
                }
            }
        }

        public static void AddMessage(SkuldMessage message)
        {
            messageQueue.Enqueue(message);

            Log.Debug(Key, $"Queued a response");
        }

        public static void Run()
            => Task.Run(async () => await ExecuteAsync().ConfigureAwait(false));
            */
    }
}