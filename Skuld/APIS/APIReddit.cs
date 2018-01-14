using System.Threading.Tasks;
using Skuld.Models.API.Reddit;
using System;
using Newtonsoft.Json;
using Skuld.Exceptions;

namespace Skuld.APIS
{
	static class APIReddit
	{
        public static async Task<SubReddit> GetSubRedditAsync(string subRedditName)
            => await GetSubRedditAsync(subRedditName, 10);
        public static async Task<SubReddit> GetSubRedditAsync(string subRedditName, int amountOfPosts)
        {
            try
            {
                Bot.Logs.Add(new Models.LogMessage("RedditGet", $"Attempting to access {subRedditName} for {amountOfPosts} posts", Discord.LogSeverity.Info));
                var uri = new Uri("https://www.reddit.com/r/" + subRedditName + "/.json?limit=" + amountOfPosts);
                var response = await APIWebReq.ReturnString(uri);
                if (!string.IsNullOrEmpty(response) || !string.IsNullOrWhiteSpace(response))
                {
                    Bot.Logs.Add(new Models.LogMessage("RedditGet", "I got a response from r/" + subRedditName, Discord.LogSeverity.Verbose));
                    return JsonConvert.DeserializeObject<SubReddit>(response);
                }
                else
                {
                    throw new EmptyResponceException("Empty response from " + uri);
                }
            }
            catch (Exception ex)
            {
                Bot.Logs.Add(new Models.LogMessage("RedditGet", ex.Message, Discord.LogSeverity.Error, ex));
                return null;
            }
        }
	}
}
