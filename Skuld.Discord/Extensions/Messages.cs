using Discord;
using Discord.Commands;
using Skuld.Discord.Services;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.Discord.Extensions
{
    public static class Messages
    {
        public static Task QueueMessageAsync(this EmbedBuilder embed, ShardedCommandContext context, Models.MessageType type = Models.MessageType.Standard, string content = "", string filepath = null, Exception exception = null, double timeout = 0.0)
            => embed.Build().QueueMessageAsync(context, type, content, filepath, exception, timeout);

        public static Task QueueMessageAsync(this Embed embed, ShardedCommandContext context, Models.MessageType type = Models.MessageType.Standard, string content = "", string filepath = null, Exception exception = null, double timeout = 0.0)
        {
            MessageQueue.AddMessage(new Models.SkuldMessage
            {
                Channel = context.Channel,
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
                    User = context.User,
                    File = filepath
                }
            });
            return Task.CompletedTask;
        }

        public static Task QueueMessageAsync(this string content, ShardedCommandContext context, Models.MessageType type = Models.MessageType.Standard, string filepath = null, Exception exception = null, double timeout = 0.0)
        {
            MessageQueue.AddMessage(new Models.SkuldMessage
            {
                Channel = context.Channel,
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
                    User = context.User,
                    File = filepath
                }
            });
            return Task.CompletedTask;
        }

        public static Task QueueMessageAsync(this StringBuilder content, ShardedCommandContext context, Models.MessageType type = Models.MessageType.Standard, string filepath = null, Exception exception = null, double timeout = 0.0)
            => content.ToString().QueueMessageAsync(context, type, filepath, exception, timeout);

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