using Discord;
using Discord.Commands;
using Skuld.Core;
using Skuld.Core.Utilities;
using System;

namespace Skuld.Bot.Extensions
{
	public static class EmbedExtensions
	{
		public static EmbedBuilder AddInlineField(this EmbedBuilder embed, string name, object value)
			=> embed.AddField(name, value, true);

		public static EmbedBuilder FromMessage(ICommandContext context)
			=> FromMessage("", "", context);

		public static EmbedBuilder FromMessage(string message, ICommandContext context)
			=> FromMessage("", message, context);

		public static EmbedBuilder FromMessage(string title, string message, ICommandContext context)
			=> FromMessage(title, message, Color.Teal, context);

		public static EmbedBuilder FromMessage(string title, string message, Color color, ICommandContext context)
			=> new EmbedBuilder()
				.WithTitle(title)
				.WithColor(color)
				.AddFooter(context)
				.WithCurrentTimestamp()
				.WithDescription(message)
				.AddAuthor();

		public static EmbedBuilder FromMessage(IUser sourceUser)
			=> FromMessage("", "", sourceUser);

		public static EmbedBuilder FromMessage(string message, IUser sourceUser)
			=> FromMessage("", message, sourceUser);

		public static EmbedBuilder FromMessage(string title, string message, IUser sourceUser)
			=> FromMessage(title, message, Color.Teal, sourceUser);

		public static EmbedBuilder FromMessage(string title, string message, Color color, IUser sourceUser)
			=> new EmbedBuilder()
				.WithTitle(title)
				.WithColor(color)
				.AddFooter(sourceUser)
				.WithCurrentTimestamp()
				.WithDescription(message)
				.AddAuthor();

		public static EmbedBuilder FromError(string title, string message, ICommandContext context)
			=> FromMessage(title, message, Color.Red, context);

		public static EmbedBuilder FromError(string message, ICommandContext context)
			=> FromMessage("⛔ Command Error! ⛔", message, Color.Red, context);

		public static EmbedBuilder FromError(string title, string message, IUser sourceUser)
			=> FromMessage(title, message, Color.Red, sourceUser);

		public static EmbedBuilder FromError(string message, IUser sourceUser)
			=> FromMessage("⛔ Command Error! ⛔", message, Color.Red, sourceUser);

		public static EmbedBuilder FromInfo(string title, string message, ICommandContext context)
			=> FromMessage(title, message, DiscordUtilities.Warning_Color, context);

		public static EmbedBuilder FromInfo(string message, ICommandContext context)
			=> FromMessage("⚠ Info ⚠", message, DiscordUtilities.Warning_Color, context);

		public static EmbedBuilder FromInfo(string title, string message, IUser sourceUser)
			=> FromMessage(title, message, DiscordUtilities.Warning_Color, sourceUser);

		public static EmbedBuilder FromInfo(string message, IUser sourceUser)
			=> FromMessage("⚠ Info ⚠", message, DiscordUtilities.Warning_Color, sourceUser);

		public static EmbedBuilder FromSuccess(ICommandContext context)
			=> FromMessage("✔ Success ✔", "", Color.Green, context);

		public static EmbedBuilder FromSuccess(string message, ICommandContext context)
			=> FromMessage("✔ Success ✔", message, Color.Green, context);

		public static EmbedBuilder FromSuccess(string title, string message, ICommandContext context)
			=> FromMessage(title, message, Color.Green, context);

		public static EmbedBuilder FromSuccess(IUser sourceUser)
			=> FromMessage("✔ Success ✔", "", Color.Green, sourceUser);

		public static EmbedBuilder FromSuccess(string message, IUser sourceUser)
			=> FromMessage("✔ Success ✔", message, Color.Green, sourceUser);

		public static EmbedBuilder FromSuccess(string title, string message, IUser sourceUser)
			=> FromMessage(title, message, Color.Green, sourceUser);

		public static EmbedBuilder FromImage(Uri image, Color color, ICommandContext context)
			=> FromImage(image.OriginalString, color, context);

		public static EmbedBuilder FromImage(Uri image, ICommandContext context)
			=> FromImage(image.OriginalString, context);


		public static EmbedBuilder FromImage(Uri image, string description, ICommandContext context)
			=> FromImage(image.OriginalString, description, context);

		public static EmbedBuilder FromImage(string imageUrl, Color color, ICommandContext context)
			=> new EmbedBuilder()
				.WithColor(color)
				.AddFooter(context)
				.WithImageUrl(imageUrl)
				.WithCurrentTimestamp()
				.AddAuthor();

		public static EmbedBuilder FromImage(string imageUrl, ICommandContext context)
			=> new EmbedBuilder()
				.WithRandomColor()
				.AddFooter(context)
				.WithCurrentTimestamp()
				.WithImageUrl(imageUrl)
				.AddAuthor();

		public static EmbedBuilder FromImage(string imageUrl, string description, ICommandContext context)
			=> new EmbedBuilder()
				.WithRandomColor()
				.AddFooter(context)
				.WithCurrentTimestamp()
				.WithImageUrl(imageUrl)
				.AddAuthor()
				.WithDescription(description);

		public static EmbedBuilder AddAuthor(this EmbedBuilder builder)
			=> builder.WithAuthor(SkuldApp.CurrentUser.Username,
								  SkuldApp.CurrentUser.GetAvatarUrl() ?? SkuldApp.CurrentUser.GetDefaultAvatarUrl(),
								  SkuldAppContext.Website)
				.WithCurrentTimestamp();

		public static EmbedBuilder AddFooter(this EmbedBuilder builder, ICommandContext context)
			=> builder.AddFooter(context.User);

		public static EmbedBuilder AddFooter(this EmbedBuilder builder, IUser user)
			=> builder.WithFooter($"Command executed for: {user.Username}#{user.Discriminator}",
								  user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
				.WithCurrentTimestamp();

		public static EmbedBuilder WithRandomColor(this EmbedBuilder builder)
			=> builder.WithColor(RandomEmbedColor());

		public static Color RandomEmbedColor()
		{
			var bytes = new byte[3];

			SkuldRandom.Fill(bytes);

			return new Color(bytes[0], bytes[1], bytes[2]);
		}
	}
}