using Discord;
using Skuld.Discord.Services;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.Discord.Extensions
{
    public static class Messages
    {
        public static Task QueueMessage(this Embed embed, Models.MessageType type, IUser user, IMessageChannel channel, string content = "", string filepath = null, Exception exception = null, double timeout = 0.0)
        {
            MessageQueue.AddMessage(new Models.SkuldMessage
            {
                Channel = channel,
                Meta = new Models.SkuldMessageMeta
                {
                    Exception = exception,
                    Timeout = timeout,
                    Type = type
                },
                Content = new Models.SkuldMessageContent
                {
                    Embed = embed,
                    Message = content,
                    User = user,
                    File = filepath
                }
            });
            return Task.CompletedTask;
        }
        public static Task QueueMessage(this EmbedBuilder embed, Models.MessageType type, IUser user, IMessageChannel channel, string content = "", string filepath = null, Exception exception = null, double timeout = 0.0)
        {
            MessageQueue.AddMessage(new Models.SkuldMessage
            {
                Channel = channel,
                Meta = new Models.SkuldMessageMeta
                {
                    Exception = exception,
                    Timeout = timeout,
                    Type = type
                },
                Content = new Models.SkuldMessageContent
                {
                    Embed = embed.Build(),
                    Message = content,
                    User = user,
                    File = filepath
                }
            });
            return Task.CompletedTask;
        }
        public static Task QueueMessage(this string content, Models.MessageType type, IUser user, IMessageChannel channel, string filepath = null, Exception exception = null, double timeout = 0.0)
        {
            MessageQueue.AddMessage(new Models.SkuldMessage
            {
                Channel = channel,
                Meta = new Models.SkuldMessageMeta
                {
                    Exception = exception,
                    Timeout = timeout,
                    Type = type
                },
                Content = new Models.SkuldMessageContent
                {
                    Message = content,
                    Embed = null,
                    User = user,
                    File = filepath
                }
            });
            return Task.CompletedTask;
        }

        public static Task QueueMessage(this StringBuilder content, Models.MessageType type, IUser user, IMessageChannel channel, string filepath = null, Exception exception = null, double timeout = 0.0)
            => content.ToString().QueueMessage(type, user, channel, filepath, exception, timeout);

        public static string TrimEmbedHiders(this string message)
        {
            string s = message;

            if (s.StartsWith("<"))
                s = s.Substring(1);
            if (s.EndsWith(">"))
                s = s[0..^1];

            return s;
        }
    }
}