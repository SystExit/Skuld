using Discord;
using Skuld.Core.Utilities;
using Skuld.Discord.Handlers;
using Skuld.Discord.Models;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Skuld.Discord.Services
{
    public static class MessageQueue
    {
        private static ConcurrentQueue<SkuldMessage> messageQueue = new ConcurrentQueue<SkuldMessage>();

        private const int messageDelay = 50;

        private static async Task ExecuteAsync()
        {
            while (true)
            {
                if (!messageQueue.IsEmpty)
                {
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
                                    await MessageSender.ReplyWithFileAsync(message.Channel, message.Content.Message, message.Content.File).ConfigureAwait(false);
                                    break;

                                case Models.MessageType.MentionFile:
                                    await MessageSender.ReplyWithMentionAndFileAsync(message.Channel, message.Content.User, message.Content.Message, message.Content.File).ConfigureAwait(false);
                                    break;
                            }

                            if (message.Meta.Type == (Models.MessageType.File | Models.MessageType.MentionFile))
                            {
                                File.Delete(message.Content.File);
                            }

                            await Task.Delay(messageDelay * messageQueue.Count).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Log.Critical("MsgQueue", ex.Message, ex);
                            await MessageSender.ReplyFailedAsync(message.Channel, ex.Message).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        Log.Error("MsgQueue", "Error removing message from queue", null);
                    }
                }
                else
                {
                    await Task.Delay(25).ConfigureAwait(false);
                }
            }
        }

        public static void AddMessage(SkuldMessage message)
        {
            messageQueue.Enqueue(message);

            string location = "";

            if (message.Channel is IGuildChannel)
            {
                location = $"{message.Channel}/{((IGuildChannel)message.Channel).Guild}";
            }
            else
            {
                location = $"{message.Channel.Name}";
            }

            if (message.Meta.Type != (Models.MessageType.DMS | Models.MessageType.DMFail))
            {
                Log.Info("MQ-Queue", $"Queued a command in: {location} for {message.Content.User}");
            }
            else
            {
                Log.Info("MQ-Queue", $"Queued a command in: {message.Content.User}/DMs");
            }
        }

        public static void Run()
            => Task.Run(async () => await ExecuteAsync().ConfigureAwait(false));
    }
}