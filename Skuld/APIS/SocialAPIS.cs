using System.Threading.Tasks;
using Skuld.Models.API.Reddit;
using System;
using Newtonsoft.Json;
using Skuld.Exceptions;
using Skuld.Services;
using Skuld.Models.API.Social.Instagram;
using System.Linq;

namespace Skuld.APIS
{
    public class SocialAPIS
    {
		readonly LoggingService logger;
		public SocialAPIS(LoggingService log)//depinj
		{
			logger = log;
		}

		//reddit
		public async Task<SubReddit> GetSubRedditAsync(string subRedditName)
			=> await GetSubRedditAsync(subRedditName, 10);
		public async Task<SubReddit> GetSubRedditAsync(string subRedditName, int amountOfPosts)
		{
			try
			{
				await logger.AddToLogsAsync(new Models.LogMessage("RedditGet", $"Attempting to access {subRedditName} for {amountOfPosts} posts", Discord.LogSeverity.Info));
				var uri = new Uri("https://www.reddit.com/" + subRedditName + "/.json?limit=" + amountOfPosts);
				var response = await WebHandler.ReturnStringAsync(uri);
				if (!string.IsNullOrEmpty(response) || !string.IsNullOrWhiteSpace(response))
				{
					await logger.AddToLogsAsync(new Models.LogMessage("RedditGet", "I got a response from " + subRedditName, Discord.LogSeverity.Verbose));
					return JsonConvert.DeserializeObject<SubReddit>(response);
				}
				else
				{
					throw new EmptyResponceException("Empty response from " + uri);
				}
			}
			catch (Exception ex)
			{
				await logger.AddToLogsAsync(new Models.LogMessage("RedditGet", ex.Message, Discord.LogSeverity.Error, ex));
				return null;
			}
		}

		//instagram
		private static string BaseURL = "https://www.instagram.com/{{USERNAME}}";
		public async Task<InstagramUser> GetInstagramUserAsync(string username)
		{
			var url = new Uri(BaseURL.Replace("{{USERNAME}}", username));
			var doc = await WebHandler.ScrapeUrlAsync(url);
			var hook = doc.GetElementbyId("react-root");
			var script = hook.NextSibling.NextSibling;
			var scriptraw = script.InnerHtml;
			var json = scriptraw.Substring("window._sharedData = ".Length);
			json = json.Substring(0, json.Length - 1);

			if (json != null)
			{
				var data = JsonConvert.DeserializeObject<RootNode>(json);
				if (data != null)
				{
					return data.EntryData.ProfilePages.FirstOrDefault().Feeds.User;
				}
				else
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}
	}
}
