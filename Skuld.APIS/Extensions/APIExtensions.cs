using Booru.Net;
using Discord;
using Discord.Commands;
using HtmlAgilityPack;
using Kitsu.Anime;
using Kitsu.Manga;
using Skuld.APIS.Social.Reddit.Models;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Verification;
using Skuld.Core.Models;
using SteamStoreQuery;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skuld.APIS.Extensions
{
	public static class APIExtensions
	{
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
			HtmlDocument htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(value);

			if (htmlDoc == null)
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

			StringBuilder pagetext = new StringBuilder();

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

		public static EventResult<IEnumerable<string>> ContainsBlacklistedTags(this IEnumerable<string> tags)
		{
			List<string> bannedTags = new List<string>();
			foreach (var tag in tags)
			{
				if (BlacklistedTags.Contains(tag.ToLowerInvariant()))
				{
					bannedTags.Add(tag);
				}
			}
			if (bannedTags.Any())
				return EventResult<IEnumerable<string>>.FromSuccess(bannedTags.AsEnumerable());

			return EventResult<IEnumerable<string>>.FromFailure("Banned Tags found");
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