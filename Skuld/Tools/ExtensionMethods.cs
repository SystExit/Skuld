using System;
using System.Collections.Generic;
using System.Text;
using Skuld.Models.API.Booru;
using Microsoft.Extensions.DependencyInjection;

namespace Skuld.Extensions
{
    public static class ExtensionMethods
    {
		public static Discord.LogSeverity FromNTwitch(this NTwitch.LogSeverity logSeverity)
		{
			if (logSeverity == NTwitch.LogSeverity.Critical)
				return Discord.LogSeverity.Critical;
			if (logSeverity == NTwitch.LogSeverity.Debug)
				return Discord.LogSeverity.Debug;
			if (logSeverity == NTwitch.LogSeverity.Error)
				return Discord.LogSeverity.Error;
			if (logSeverity == NTwitch.LogSeverity.Info)
				return Discord.LogSeverity.Info;
			if (logSeverity == NTwitch.LogSeverity.Verbose)
				return Discord.LogSeverity.Verbose;
			if (logSeverity == NTwitch.LogSeverity.Warning)
				return Discord.LogSeverity.Warning;

			return Discord.LogSeverity.Verbose;
		}
		
		public static DanbooruImage GetRandomImage(this IReadOnlyList<DanbooruImage> posts)
			=> posts[Bot.services.GetRequiredService<Random>().Next(0, posts.Count)];
		
		public static Rule34Image GetRandomImage(this IReadOnlyList<Rule34Image> posts)
			=> posts[Bot.services.GetRequiredService<Random>().Next(0, posts.Count)];
		
		public static RealbooruImage GetRandomImage(this IReadOnlyList<RealbooruImage> posts)
			=> posts[Bot.services.GetRequiredService<Random>().Next(0, posts.Count)];

		public static SafebooruImage GetRandomImage(this IReadOnlyList<SafebooruImage> posts)
			=> posts[Bot.services.GetRequiredService<Random>().Next(0, posts.Count)];
		
		public static GelbooruImage GetRandomImage(this IReadOnlyList<GelbooruImage> posts)
			=> posts[Bot.services.GetRequiredService<Random>().Next(0, posts.Count)];

		public static KonaChanImage GetRandomImage(this IReadOnlyList<KonaChanImage> posts)
			=> posts[Bot.services.GetRequiredService<Random>().Next(0, posts.Count)];

		public static E621Image GetRandomImage(this IReadOnlyList<E621Image> posts)
			=> posts[Bot.services.GetRequiredService<Random>().Next(0, posts.Count)];

		public static YandereImage GetRandomImage(this IReadOnlyList<YandereImage> posts)
			=> posts[Bot.services.GetRequiredService<Random>().Next(0, posts.Count)];
	}
}
