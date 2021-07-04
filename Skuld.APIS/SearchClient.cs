using Google.Apis.Customsearch.v1;
using Google.Apis.Customsearch.v1.Data;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using Imgur.API.Models.Impl;
using Skuld.Core.Extensions;
using Skuld.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;

namespace Skuld.APIS
{
	public static class SearchClient
	{
		private static CustomsearchService GoogleSearchService;
		private static YoutubeClient Youtube;
		private static ImgurClient ImgurClient;
		private static string GoogleCxKey;
		private static GalleryEndpoint galleryEndpoint;

		public static void Configure(string GoogleAPIKey, string GoogleCx, string imgurClientID, string imgurClientSecret)
		{
			GoogleCxKey = GoogleCx;
			GoogleSearchService = new CustomsearchService();

			Youtube = new YoutubeClient();

			if (!string.IsNullOrEmpty(imgurClientID) && !string.IsNullOrEmpty(imgurClientSecret))
			{
				ImgurClient = new ImgurClient(imgurClientID, imgurClientSecret);
			}

			if (!string.IsNullOrEmpty(GoogleAPIKey))
			{
				GoogleSearchService = new CustomsearchService(new Google.Apis.Services.BaseClientService.Initializer { ApiKey = GoogleAPIKey, ApplicationName = "Skuld" });
			}
		}

		public static async Task<string> SearchImgurAsync(string query)
		{
			if (ImgurClient is null) return null;

			if (galleryEndpoint is null)
			{
				galleryEndpoint = new GalleryEndpoint(ImgurClient);
			}

			var images = await galleryEndpoint.SearchGalleryAsync(query).ConfigureAwait(false);

			if (images is not null)
			{
				return GetAlbumLink(images);
			}

			return "I found nothing sorry. :/";
		}

		static bool CheckNSFWState(dynamic album, bool isChannelNSFW)
			=> isChannelNSFW && album.Nsfw;

		private static string GetAlbumLink(IEnumerable<IGalleryItem> images, bool isChannelNSFW = false)
		{
			var albm = images.Random();
			dynamic album = null;

			if (albm is GalleryImage)
			{
				album = albm as IGalleryImage;
			}
			if (albm is GalleryAlbum)
			{
				album = albm as IGalleryAlbum;
			}

			if (album is not null && CheckNSFWState(album, isChannelNSFW))
			{
				return "I found this:\n" + album.Link;
			}

			return null;
		}

		public static async Task<string> SearchYoutubeAsync(string query)
		{
			try
			{
				var items = await Youtube.Search.GetVideosAsync(query).ToListAsync();

				if (items.Count > 0)
				{
					var video = await Youtube.Videos.GetAsync(items[0].Id);

					var totalreactions = video.Engagement.LikeCount + video.Engagement.DislikeCount;
					double ratiog = ((double)video.Engagement.LikeCount / totalreactions) * 100;
					double ratiob = ((double)video.Engagement.DislikeCount / totalreactions) * 100;

					return $"<:youtube:314349922885566475> | http://youtu.be/{video.Id}\n" +
						$"`👀: {video.Engagement.ViewCount.ToFormattedString()}`\n" +
						$"`👍: {video.Engagement.LikeCount.ToFormattedString()} ({ratiog:0.0}%)\t👎: {video.Engagement.DislikeCount:N0} ({ratiob:0.0}%)`\n" +
						$"`Duration: {video.Duration}`";
				}

				return "Nothing found";
			}
			catch (Exception ex)
			{
				Log.Error("YOUTUBE", ex.Message, null, ex);
				return null;
			}
		}

		public static async Task<IReadOnlyCollection<Result>> SearchGoogleAsync(string query)
		{
			if (GoogleSearchService is null) return null;
			try
			{
				var listRequest = GoogleSearchService.Cse.List();
				listRequest.Q = query;
				listRequest.Cx = GoogleCxKey;
				listRequest.Safe = CseResource.ListRequest.SafeEnum.High;
				var search = await listRequest.ExecuteAsync().ConfigureAwait(false);
				var items = search.Items;
				if (items is not null)
				{
					var item = items.FirstOrDefault();
					var item2 = items.ElementAtOrDefault(1);
					var item3 = items.ElementAtOrDefault(2);

					return new List<Result>
					{
						item,
						item2,
						item3
					}.AsReadOnly();
				}
				else
				{
					return null;
				}
			}
			catch (Exception ex)
			{
				Log.Error("Google", ex.Message, null, ex);
				return null;
			}
		}
	}
}