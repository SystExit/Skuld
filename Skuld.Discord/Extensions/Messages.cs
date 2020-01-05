using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Skuld.Core;
using Skuld.Core.Utilities;
using Skuld.Discord.Handlers;
using Skuld.Discord.Services;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.Discord.Extensions
{
    public static class Messages
    {
        public static async Task<IUserMessage> QueueMessageAsync(this string content,
                                             ShardedCommandContext context,
                                             Models.MessageType type = Models.MessageType.Standard,
                                             Stream imageStream = null,
                                             string fileName = "image.png",
                                             Exception exception = null,
                                             double timeout = 0.0)
        {
            switch (type)
            {
                case Models.MessageType.Standard:
                    return await MessageSender.ReplyAsync(context.Channel, content).ConfigureAwait(false);

                case Models.MessageType.Mention:
                    return await MessageSender.ReplyWithMentionAsync(context.Channel, context.User, content).ConfigureAwait(false);

                case Models.MessageType.DMS:
                    return await MessageSender.ReplyDMsAsync(await context.User.GetOrCreateDMChannelAsync().ConfigureAwait(false), context.Channel, content).ConfigureAwait(false);

                case Models.MessageType.DMFail:
                    return await MessageSender.ReplyDMFailableAsync(await context.User.GetOrCreateDMChannelAsync().ConfigureAwait(false), content, null).ConfigureAwait(false);

                case Models.MessageType.Timed:
                    await MessageSender.ReplyWithTimedMessage(context.Channel, content, null, timeout).ConfigureAwait(false);
                    return null;

                case Models.MessageType.File:
                    {
                        var msg = await MessageSender.ReplyWithFileAsync(context.Channel, content, imageStream, fileName).ConfigureAwait(false);

                        if (imageStream != null)
                            await imageStream.DisposeAsync().ConfigureAwait(false);

                        return msg;
                    }

                case Models.MessageType.MentionFile:
                    {
                        var msg = await MessageSender.ReplyWithFileAsync(context.Channel, content, imageStream, fileName).ConfigureAwait(false);

                        if (imageStream != null)
                            await imageStream.DisposeAsync().ConfigureAwait(false);

                        return msg;
                    }
            }

            return null;
        }

        public static async Task<IUserMessage> QueueMessageAsync(this StringBuilder content,
                                             ShardedCommandContext context,
                                             Models.MessageType type = Models.MessageType.Standard,
                                             Stream imageStream = null,
                                             string fileName = "image.png",
                                             Exception exception = null,
                                             double timeout = 0.0)
            => await content.ToString().QueueMessageAsync(context, type, imageStream, fileName, exception, timeout);

        public static async Task<IUserMessage> QueueMessageAsync(this Embed embed,
                                             ShardedCommandContext context,
                                             Models.MessageType type = Models.MessageType.Standard,
                                             string content = "",
                                             Stream imageStream = null,
                                             string fileName = "image.png",
                                             Exception exception = null,
                                             double timeout = 0.0)
        {
            switch (type)
            {
                case Models.MessageType.Standard:
                    return await MessageSender.ReplyAsync(context.Channel, content, embed).ConfigureAwait(false);

                case Models.MessageType.Mention:
                    return await MessageSender.ReplyWithMentionAsync(context.Channel, context.User, content, embed).ConfigureAwait(false);

                case Models.MessageType.DMS:
                    return await MessageSender.ReplyDMsAsync(await context.User.GetOrCreateDMChannelAsync().ConfigureAwait(false), context.Channel, content, embed).ConfigureAwait(false);

                case Models.MessageType.DMFail:
                    return await MessageSender.ReplyDMFailableAsync(await context.User.GetOrCreateDMChannelAsync().ConfigureAwait(false), content, embed).ConfigureAwait(false);

                case Models.MessageType.Timed:
                    await MessageSender.ReplyWithTimedMessage(context.Channel, content, embed, timeout).ConfigureAwait(false);
                    return null;

                case Models.MessageType.File:
                    {
                        var msg = await MessageSender.ReplyWithFileAsync(context.Channel, content, imageStream, fileName, embed).ConfigureAwait(false);

                        if (imageStream != null)
                            await imageStream.DisposeAsync().ConfigureAwait(false);

                        return msg;
                    }

                case Models.MessageType.MentionFile:
                    {
                        var msg = await MessageSender.ReplyWithFileAsync(context.Channel, content, imageStream, fileName, embed).ConfigureAwait(false);

                        if (imageStream != null)
                            await imageStream.DisposeAsync().ConfigureAwait(false);

                        return msg;
                    }
            }

            return null;
        }

        public static async Task<IUserMessage> QueueMessageAsync(this EmbedBuilder embed,
                                             ShardedCommandContext context,
                                             Models.MessageType type = Models.MessageType.Standard,
                                             string content = "",
                                             Stream imageStream = null,
                                             string fileName = "image.png",
                                             Exception exception = null,
                                             double timeout = 0.0)
            => await embed.Build().QueueMessageAsync(context, type, content, imageStream, fileName, exception, timeout);

        public static string TrimEmbedHiders(this string message)
        {
            string s = message;

            if (s.StartsWith("<"))
                s = s.Substring(1);
            if (s.EndsWith(">"))
                s = s[0..^1];

            return s;
        }

        public static string ReplaceGuildEventMessage(this string message, IUser user, SocketGuild guild)
        {
            var msg = message;
            msg = msg.Replace("-m", "**" + user.Mention + "**");
            msg = msg.Replace("-s", "**" + guild.Name + "**");
            msg = msg.Replace("-uc", Convert.ToString(guild.MemberCount));
            msg = msg.Replace("-u", "**" + user.Username + "**");
            return msg;
        }

        public static string PruneMention(this string message, ulong id)
        {
            if (message == null) return message;

            var msg = message;

            msg = msg.Replace($"<@{id}> ", "", StringComparison.InvariantCultureIgnoreCase);
            msg = msg.Replace($"<@{id}>", "", StringComparison.InvariantCultureIgnoreCase);
            msg = msg.Replace($"<@!{id}> ", "", StringComparison.InvariantCultureIgnoreCase);
            msg = msg.Replace($"<@!{id}>", "", StringComparison.InvariantCultureIgnoreCase);

            return msg;
        }

        public static string ToMessage(this Embed embed)
        {
            string message = "";

            if (embed.Author.HasValue) message += $"**__{embed.Author.Value.Name}__**\n";
            if (!string.IsNullOrEmpty(embed.Title)) message += $"**{embed.Title}**\n";
            if (!string.IsNullOrEmpty(embed.Description)) message += embed.Description + "\n";

            foreach (var field in embed.Fields) message += $"__{field.Name}__\n{field.Value}\n\n";

            if (embed.Video.HasValue) message += embed.Video.Value.Url + "\n";
            if (embed.Thumbnail.HasValue) message += embed.Thumbnail.Value.Url + "\n";
            if (embed.Image.HasValue) message += embed.Image.Value.Url + "\n";
            if (embed.Footer.HasValue) message += $"`{embed.Footer.Value.Text}`";
            if (embed.Timestamp.HasValue) message += " | " + embed.Timestamp.Value.ToString("dd'/'MM'/'yyyy hh:mm:ss tt");

            return message;
        }

        public static async Task DeleteAfterSecondsAsync(this IUserMessage message, int timeout)
        {
            await Task.Delay((timeout * 1000));
            await message.DeleteAsync();
            Log.Debug("MsgDisp", "Deleted a timed message");
        }

        public static EmbedBuilder AddInlineField(this EmbedBuilder embed, string name, object value)
            => embed.AddField(name, value, true);

        public static async Task<bool> CanEmbedAsync(this IMessageChannel channel, IGuild guild = null)
        {
            if (guild == null) return true;
            else
            {
                var curr = await guild.GetCurrentUserAsync();
                var chan = await guild.GetChannelAsync(channel.Id);
                var perms = curr.GetPermissions(chan);
                return perms.EmbedLinks;
            }
        }

        public static EmbedBuilder FromMessage(ICommandContext context)
            => FromMessage("", "", context);

        public static EmbedBuilder FromMessage(string title, string message, ICommandContext context)
            => FromMessage(title, message, Color.Teal, context);

        public static EmbedBuilder FromMessage(string title, string message, Color color, ICommandContext context)
            => new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = BotService.DiscordClient.CurrentUser.Username,
                    IconUrl = BotService.DiscordClient.CurrentUser.GetAvatarUrl() ?? BotService.DiscordClient.CurrentUser.GetDefaultAvatarUrl(),
                    Url = SkuldAppContext.Website
                },
                Title = title,
                Description = message,
                Color = color,
                Timestamp = DateTime.UtcNow,
                Footer = new EmbedFooterBuilder
                {
                    IconUrl = context.User.GetAvatarUrl() ?? context.User.GetDefaultAvatarUrl(),
                    Text = $"Command executed for: {context.User.Username}#{context.User.Discriminator}"
                }
            };

        public static EmbedBuilder FromError(string title, string message, ICommandContext context)
            => FromMessage(title, message, Color.Red, context);

        public static EmbedBuilder FromError(string message, ICommandContext context)
            => FromMessage("⛔Command Error!⛔", message, Color.Red, context);

        public static EmbedBuilder FromInfo(string title, string message, ICommandContext context)
            => FromMessage(title, message, DiscordTools.Warning_Color, context);

        public static EmbedBuilder FromInfo(string message, ICommandContext context)
            => FromMessage("⚠Info⚠", message, DiscordTools.Warning_Color, context);

        public static EmbedBuilder FromSuccess(ICommandContext context)
            => FromMessage("✔Success✔", "", Color.Green, context);

        public static EmbedBuilder FromSuccess(string message, ICommandContext context)
            => FromMessage("✔Success✔", message, Color.Green, context);

        public static EmbedBuilder FromSuccess(string title, string message, ICommandContext context)
            => FromMessage(title, message, Color.Green, context);

        public static EmbedBuilder FromImage(string imageUrl, Color color, ICommandContext context)
            => new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = BotService.DiscordClient.CurrentUser.Username,
                    IconUrl = BotService.DiscordClient.CurrentUser.GetAvatarUrl() ?? BotService.DiscordClient.CurrentUser.GetDefaultAvatarUrl(),
                    Url = SkuldAppContext.Website
                },
                Color = color,
                Timestamp = DateTime.UtcNow,
                ImageUrl = imageUrl,
                Footer = new EmbedFooterBuilder
                {
                    IconUrl = context.User.GetAvatarUrl() ?? context.User.GetDefaultAvatarUrl(),
                    Text = $"Command executed for: {context.User.Username}#{context.User.Discriminator}"
                }
            };
    }
}