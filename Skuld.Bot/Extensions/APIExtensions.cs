using Booru.Net;
using Discord;
using Discord.Commands;
using HtmlAgilityPack;
using Kitsu.Anime;
using Kitsu.Manga;
using Skuld.APIS.Social.Reddit.Models;
using Skuld.APIS.UrbanDictionary.Models;
using Skuld.APIS.Utilities;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Localisation;
using Skuld.Core.Extensions.Verification;
using Skuld.Core.Models;
using SteamStorefrontAPI;
using SteamStoreQuery;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.Bot.Extensions
{
	public static class APIExtensions
	{
		public static EmbedBuilder ToEmbed(this AnimeDataModel model, ResourceManager loc)
		{
			var attr = model.Attributes;

			var embed = new EmbedBuilder()
				.WithColor(Color.Purple)
				.WithImageUrl(attr.PosterImage.Large)
				.WithTitle(attr.CanonicalTitle.CheckEmptyWithLocale(loc));

			if (attr.AbbreviatedTitles is not null && attr.AbbreviatedTitles.Any())
			{
				embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_SYNON", CultureInfo.InvariantCulture), attr.AbbreviatedTitles.CheckEmptyWithLocale(", ", loc));
			}

			embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_EPS", CultureInfo.InvariantCulture), attr.EpisodeCount.CheckEmptyWithLocale(loc));
			embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_SDATE", CultureInfo.InvariantCulture), attr.StartDate.CheckEmptyWithLocale(loc));
			embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_EDATE", CultureInfo.InvariantCulture), attr.EndDate.CheckEmptyWithLocale(loc));
			embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_SCORE", CultureInfo.InvariantCulture), attr.RatingRank.CheckEmptyWithLocale(loc));

			if (attr.Synopsis.CheckEmptyWithLocale(loc) != loc.GetString("SKULD_GENERIC_EMPTY", CultureInfo.InvariantCulture))
			{
				var syno = attr.Synopsis;

				if (syno.Length > 1024)
				{
					embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_SYNOP", CultureInfo.InvariantCulture), attr.Synopsis.Substring(0, 1021) + "...");
				}
				else
				{
					embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_SYNOP", CultureInfo.InvariantCulture), attr.Synopsis);
				}
			}

			return embed;
		}

		public static EmbedBuilder ToEmbed(this MangaDataModel model, ResourceManager loc)
		{
			var attr = model.Attributes;

			var embed = new EmbedBuilder
			{
				Title = attr.CanonicalTitle.CheckEmptyWithLocale(loc),
				ImageUrl = attr.PosterImage.Large
			};

			if (attr.AbbreviatedTitles is not null && attr.AbbreviatedTitles.Any())
				embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_SYNON", CultureInfo.InvariantCulture), attr.AbbreviatedTitles.CheckEmptyWithLocale(", ", loc));

			embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_EPS", CultureInfo.InvariantCulture), attr.ChapterCount.CheckEmptyWithLocale(loc));
			embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_SDATE", CultureInfo.InvariantCulture), attr.StartDate.CheckEmptyWithLocale(loc));
			embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_EDATE", CultureInfo.InvariantCulture), attr.EndDate.CheckEmptyWithLocale(loc));
			embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_SCORE", CultureInfo.InvariantCulture), attr.RatingRank.CheckEmptyWithLocale(loc));

			if (attr.Synopsis.CheckEmptyWithLocale(loc) != loc.GetString("SKULD_GENERIC_EMPTY", CultureInfo.InvariantCulture))
			{
				var syno = attr.Synopsis;
				if (syno.Length > 1024)
					embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_SYNOP", CultureInfo.InvariantCulture), attr.Synopsis.Substring(0, 1021) + "...");
				else
					embed.AddInlineField(loc.GetString("SKULD_SEARCH_WEEB_SYNOP", CultureInfo.InvariantCulture), attr.Synopsis);
			}

			embed.WithColor(Color.Purple);

			return embed;
		}

		public static async Task<EmbedBuilder> GetEmbedAsync(this Listing game)
		{
			var app = await AppDetails.GetAsync(game.AppId).ConfigureAwait(false);

			string releasetext = app.ReleaseDate.ComingSoon ? "Coming soon! (Date Number may not be accurate)" : "Released on:";

			var embed =
				new EmbedBuilder()
					.WithAuthor(
						new EmbedAuthorBuilder()
						.WithName(string.Join(", ", app.Developers))
					)
					.WithDescription(SteamUtilities.GetSteamGameDescription(game, app))
					.WithImageUrl(app.Screenshots.Random().PathFull)
					.WithRandomColor()
					.WithUrl($"https://store.steampowered.com/app/{game.AppId}/")
					.WithTitle(game.Name)
					.WithFooter(
						new EmbedFooterBuilder()
						.WithText(releasetext)
					);

			if (int.TryParse(app.ReleaseDate.Date[0].ToString(), out int _))
			{
				embed.WithTimestamp(DateTime.ParseExact(app.ReleaseDate.Date, "dd MMM, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces));
			}
			else
			{
				embed.WithTimestamp(DateTime.ParseExact(app.ReleaseDate.Date, "MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces));
			}

			return embed;
		}

		public static EmbedBuilder ToEmbed(this UrbanWord word)
		{
			return
				new EmbedBuilder()
				.WithAuthor(new EmbedAuthorBuilder()
					.WithUrl(word.PermaLink)
					.WithName(word.Word)
				)
				.WithDescription(word.Definition)
				.WithRandomColor()
				.AddField("Author", word.Author)
				.AddField("Example", word.Example)
				.AddField("Upvotes", word.UpVotes)
				.AddInlineField("Downvotes", word.DownVotes);
		}

		public static List<string> BlacklistedTags { get; } = new List<string>
		{
			"loli",
			"shota",
			"cub",
			"gore",
			"guro",
			"death",
			"decapitation",
			"murder",
			"necrophilia",
			"gutted",
			"disemboweled",
			"disembowelment",
			"wound_fucking",
			"dead",
			"corpse",
			"child",
			"baby",
			"kid",
			"kiddo"
		};

		//https://gist.github.com/starquake/8d72f1e55c0176d8240ed336f92116e3
		public static string StripHtml(this string value)
		{
			HtmlDocument htmlDoc = new();
			htmlDoc.LoadHtml(value);

			if (htmlDoc is null)
			{
				return value;
			}

			return htmlDoc.DocumentNode.InnerText;
		}

		#region Pagination

		public static IList<string> PaginateList(this IReadOnlyList<AnimeDataModel> list, int maxrows = 10)
		{
			var pages = new List<string>();
			string pagetext = "";

			for (int x = 0; x < list.Count; x++)
			{
				var obj = list[x];

				pagetext += $"{x + 1}. {obj.Attributes.CanonicalTitle}\n";

				if ((x + 1) % maxrows == 0 || (x + 1) == list.Count)
				{
					pages.Add(pagetext);
					pagetext = "";
				}
			}

			return pages;
		}

		public static IList<string> PaginateList(this IReadOnlyList<MangaDataModel> list, int maxrows = 10)
		{
			var pages = new List<string>();
			string pagetext = "";

			for (int x = 0; x < list.Count; x++)
			{
				var obj = list[x];

				pagetext += $"{x + 1}. {obj.Attributes.CanonicalTitle}\n";

				if ((x + 1) % maxrows == 0 || (x + 1) == list.Count)
				{
					pages.Add(pagetext);
					pagetext = "";
				}
			}

			return pages;
		}

		public static IList<string> PaginateList(this IReadOnlyList<Listing> list, int maxrows = 10)
		{
			var pages = new List<string>();
			string pagetext = "";

			for (int x = 0; x < list.Count; x++)
			{
				var obj = list[x];

				pagetext += $"{x + 1}. {obj.Name}\n";

				if ((x + 1) % maxrows == 0 || (x + 1) == list.Count)
				{
					pages.Add(pagetext);
					pagetext = "";
				}
			}

			return pages;
		}

		public static IList<string> PaginatePosts(this Post[] posts, ITextChannel channel, int maxrows = 10)
		{
			var Pages = new List<string>();

			StringBuilder pagetext = new();

			for (int x = 0; x < posts.Length; x++)
			{
				var post = posts[x];

				string text = $"[{post.Data.Title}](https://reddit.com{post.Data.Permalink})\n";

				if (post.Data.Over18 && channel.IsNsfw)
				{
					pagetext.Append("**NSFW** " + text);
				}
				else
				{
					pagetext.Append(text);
				}
				pagetext.Append('\n');

				if ((x + 1) % maxrows == 0 || (x + 1) == posts.Length)
				{
					Pages.Add(pagetext.ToString());
					pagetext.Clear();
				}
			}
			return Pages;
		}

		#endregion

		#region Booru

		public static IList<string> AddBlacklistedTags(this IList<string> tags)
		{
			var newtags = new List<string>();
			newtags.AddRange(tags);
			BlacklistedTags.ForEach(x => newtags.Add("-" + x));
			return newtags;
		}

		public static EventResult ContainsBlacklistedTags(this IEnumerable<string> tags)
		{
			List<string> bannedTags = new();
			foreach (var tag in tags)
			{
				if (BlacklistedTags.Contains(tag.ToLowerInvariant()))
				{
					bannedTags.Add(tag);
				}
			}
			if (bannedTags.Count > 0)
			{
				return EventResult.FromSuccess(bannedTags.AsEnumerable());
			}

			return EventResult.FromFailure(bannedTags.AsEnumerable(), "Banned Tags found");
		}

		public static object GetMessage(this DanbooruImage image, ICommandContext context, bool forceString = false)
		{
			string message = $"`Score: {image.Score}` <{image.PostUrl}>";
			if (!image.ImageUrl.IsVideoFile())
			{
				if (forceString)
					message += $"\n{image.ImageUrl}";
			}
			else
			{
				message += $"\n{image.ImageUrl} (Video)";
			}

			if (!image.ImageUrl.IsVideoFile() && !forceString)
			{
				return
					EmbedExtensions.FromImage(image.ImageUrl, EmbedExtensions.RandomEmbedColor(), context)
				.WithDescription(message);
			}
			else
			{
				return message;
			}
		}

		public static object GetMessage(this GelbooruImage image, ICommandContext context, bool forceString = false)
		{
			string message = $"`Score: {image.Score}` <{image.PostUrl}>";
			if (!image.ImageUrl.IsVideoFile())
			{
				if (forceString)
					message += $"\n{image.ImageUrl}";
			}
			else
			{
				message += $"\n{image.ImageUrl} (Video)";
			}

			if (!image.ImageUrl.IsVideoFile() && !forceString)
			{
				return
					EmbedExtensions.FromImage(image.ImageUrl, EmbedExtensions.RandomEmbedColor(), context)
				.WithDescription(message);
			}
			else
			{
				return message;
			}
		}

		public static object GetMessage(this SafebooruImage image, ICommandContext context, bool forceString = false)
		{
			string message = $"`Score: {image.Score}` <{image.PostUrl}>";
			if (!image.ImageUrl.IsVideoFile())
			{
				if (forceString)
					message += $"\n{image.ImageUrl}";
			}
			else
			{
				message += $"\n{image.ImageUrl} (Video)";
			}

			if (!image.ImageUrl.IsVideoFile() && !forceString)
			{
				return
					EmbedExtensions.FromImage(image.ImageUrl, EmbedExtensions.RandomEmbedColor(), context)
				.WithDescription(message);
			}
			else
			{
				return message;
			}
		}

		public static object GetMessage(this E621Image image, ICommandContext context, bool forceString = false)
		{
			string message = $"`Score: 👍 {image.Score.Up} 👎 {image.Score.Down}` <{image.PostUrl}>";
			if (!image.ImageUrl.IsVideoFile())
			{
				if (forceString)
					message += $"\n{image.ImageUrl}";
			}
			else
			{
				message += $"\n{image.ImageUrl} (Video)";
			}

			if (!image.ImageUrl.IsVideoFile() && !forceString)
			{
				return
					EmbedExtensions.FromImage(image.ImageUrl, EmbedExtensions.RandomEmbedColor(), context)
				.WithDescription(message);
			}
			else
			{
				return message;
			}
		}

		#endregion Booru
	}
}