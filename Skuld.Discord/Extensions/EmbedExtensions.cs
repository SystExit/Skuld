using Discord;
using Discord.Commands;
using Skuld.Core;
using Skuld.Core.Utilities;
using System;

namespace Skuld.Discord.Extensions
{
    public static class EmbedExtensions
    {
        public static EmbedBuilder AddInlineField(this EmbedBuilder embed, string name, object value)
            => embed.AddField(name, value, true);

        public static EmbedBuilder FromMessage(ICommandContext context)
            => FromMessage("", "", context);

        public static EmbedBuilder FromMessage(string title, string message, ICommandContext context)
            => FromMessage(title, message, Color.Teal, context);

        public static EmbedBuilder FromMessage(string title, string message, Color color, ICommandContext context)
            => new EmbedBuilder()
                .WithDescription(message)
                .WithCurrentTimestamp()
                .AddFooter(context)
                .WithTitle(title)
                .WithColor(color)
                .AddAuthor(context.Client);

        public static EmbedBuilder FromError(string title, string message, ICommandContext context)
            => FromMessage(title, message, Color.Red, context);

        public static EmbedBuilder FromError(string message, ICommandContext context)
            => FromMessage("⛔Command Error!⛔", message, Color.Red, context);

        public static EmbedBuilder FromInfo(string title, string message, ICommandContext context)
            => FromMessage(title, message, DiscordUtilities.Warning_Color, context);

        public static EmbedBuilder FromInfo(string message, ICommandContext context)
            => FromMessage("⚠Info⚠", message, DiscordUtilities.Warning_Color, context);

        public static EmbedBuilder FromSuccess(ICommandContext context)
            => FromMessage("✔Success✔", "", Color.Green, context);

        public static EmbedBuilder FromSuccess(string message, ICommandContext context)
            => FromMessage("✔Success✔", message, Color.Green, context);

        public static EmbedBuilder FromSuccess(string title, string message, ICommandContext context)
            => FromMessage(title, message, Color.Green, context);

        public static EmbedBuilder FromImage(string imageUrl, Color color, ICommandContext context)
            => new EmbedBuilder()
                .WithImageUrl(imageUrl)
                .WithCurrentTimestamp()
                .AddFooter(context)
                .WithColor(color)
                .AddAuthor(context.Client);

        public static EmbedBuilder AddAuthor(this EmbedBuilder builder, IDiscordClient client)
            => builder.WithAuthor(client.CurrentUser.Username,
                                  client.CurrentUser.GetAvatarUrl() ?? client.CurrentUser.GetDefaultAvatarUrl(),
                                  SkuldAppContext.Website)
                .WithCurrentTimestamp();

        public static EmbedBuilder AddFooter(this EmbedBuilder builder, ICommandContext context)
            => builder.WithFooter($"Command executed for: {context.User.Username}#{context.User.Discriminator}",
                                  context.User.GetAvatarUrl() ?? context.User.GetDefaultAvatarUrl())
                .WithCurrentTimestamp();

        public static EmbedBuilder WithRandomColor(this EmbedBuilder builder)
            => builder.WithColor(RandomEmbedColor());

        public static Color RandomEmbedColor(int seed = 0)
        {
            var bytes = new byte[3];

            if (seed == 0)
                seed = new Random().Next(1, int.MaxValue);

            new Random(seed).NextBytes(bytes);

            return new Color(bytes[0], bytes[1], bytes[2]);
        }
    }
}