using Discord;
using Kitsu.Anime;
using Kitsu.Manga;
using Skuld.APIS.UrbanDictionary.Models;
using Skuld.APIS.Utilities;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Localisation;
using SteamStorefrontAPI;
using SteamStoreQuery;
using System;
using System.Globalization;
using System.Linq;
using System.Resources;
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
	}
}