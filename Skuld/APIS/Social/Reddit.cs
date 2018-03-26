using System.Threading.Tasks;
using Skuld.Models.API.Reddit;
using System;
using Newtonsoft.Json;
using Skuld.Exceptions;

namespace Skuld.APIS.Social
{
	static class Reddit
	{
        public static async Task<SubReddit> GetSubRedditAsync(string subRedditName)
            => await GetSubRedditAsync(subRedditName, 10);
        public static async Task<SubReddit> GetSubRedditAsync(string subRedditName, int amountOfPosts)
        {
            try
            {
                await Bot.Logger.AddToLogs(new Models.LogMessage("RedditGet", $"Attempting to access {subRedditName} for {amountOfPosts} posts", Discord.LogSeverity.Info));
                var uri = new Uri("https://www.reddit.com/" + subRedditName + "/.json?limit=" + amountOfPosts);
                var response = await WebHandler.ReturnStringAsync(uri);
                if (!string.IsNullOrEmpty(response) || !string.IsNullOrWhiteSpace(response))
                {
                    await Bot.Logger.AddToLogs(new Models.LogMessage("RedditGet", "I got a response from " + subRedditName, Discord.LogSeverity.Verbose));
                    return JsonConvert.DeserializeObject<SubReddit>(response);
                }
                else
                {
                    throw new EmptyResponceException("Empty response from " + uri);
                }
            }
            catch (Exception ex)
            {
                await Bot.Logger.AddToLogs(new Models.LogMessage("RedditGet", ex.Message, Discord.LogSeverity.Error, ex));
                return null;
            }
        }
	}
}
