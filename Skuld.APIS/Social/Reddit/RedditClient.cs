using Newtonsoft.Json;
using Skuld.APIS.Social.Reddit.Models;
using System;
using Skuld.APIS.Utilities;
using System.Threading.Tasks;
using Skuld.Core.Services;

namespace Skuld.APIS.Social.Reddit
{
    public class RedditClient : BaseClient
    {
        private readonly RateLimiter rateLimiter;

        public RedditClient(GenericLogger log) : base (log)
        {
            rateLimiter = new RateLimiter();
        }

        public async Task<SubReddit> GetSubRedditAsync(string subRedditName)
            => await GetSubRedditAsync(subRedditName, 10);

        public async Task<SubReddit> GetSubRedditAsync(string subRedditName, int amountOfPosts)
        {
            try
            {
                if (rateLimiter.IsRatelimited()) return null;

                await loggingService.AddToLogsAsync(new Core.Models.LogMessage("RedditGet", $"Attempting to access {subRedditName} for {amountOfPosts} posts", Discord.LogSeverity.Info));
                var uri = new Uri("https://www.reddit.com/" + subRedditName + "/.json?limit=" + amountOfPosts);
                var response = await ReturnStringAsync(uri);
                if (!string.IsNullOrEmpty(response) || !string.IsNullOrWhiteSpace(response))
                {
                    await loggingService.AddToLogsAsync(new Core.Models.LogMessage("RedditGet", "I got a response from " + subRedditName, Discord.LogSeverity.Verbose));
                    return JsonConvert.DeserializeObject<SubReddit>(response);
                }
                else
                {
                    throw new Exception("Empty response from " + uri);
                }
            }
            catch (Exception ex)
            {
                await loggingService.AddToLogsAsync(new Core.Models.LogMessage("RedditGet", ex.Message, Discord.LogSeverity.Error, ex));
                return null;
            }
        }
    }
}
