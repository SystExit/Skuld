using Newtonsoft.Json;
using Skuld.APIS.Social.Reddit.Models;
using Skuld.APIS.Utilities;
using Skuld.Core.Utilities;
using System;
using System.Threading.Tasks;

namespace Skuld.APIS.Social.Reddit
{
    public class RedditClient
    {
        private readonly RateLimiter rateLimiter;

        public RedditClient()
        {
            rateLimiter = new RateLimiter();
        }

        public async Task<SubReddit> GetSubRedditAsync(string subRedditName)
            => await GetSubRedditAsync(subRedditName, 10).ConfigureAwait(false);

        public async Task<SubReddit> GetSubRedditAsync(string subRedditName, int amountOfPosts)
        {
            try
            {
                if (rateLimiter.IsRatelimited()) return null;

                Log.Verbose("RedditGet", $"Attempting to access {subRedditName} for {amountOfPosts} posts");
                var uri = new Uri("https://www.reddit.com/" + subRedditName + "/.json?limit=" + amountOfPosts);
                var response = await HttpWebClient.ReturnStringAsync(uri);
                if (!string.IsNullOrEmpty(response) || !string.IsNullOrWhiteSpace(response))
                {
                    Log.Verbose("RedditGet", "I got a response from " + subRedditName);
                    return JsonConvert.DeserializeObject<SubReddit>(response);
                }
                else
                {
                    throw new Exception("Empty response from " + uri);
                }
            }
            catch (Exception ex)
            {
                Log.Error("RedditGet", ex.Message, ex);
                return null;
            }
        }
    }
}