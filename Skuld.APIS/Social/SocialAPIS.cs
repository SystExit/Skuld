using Skuld.APIS.Social.Reddit;
using Skuld.APIS.Social.Reddit.Models;
using System.Threading.Tasks;

namespace Skuld.APIS
{
    public class SocialAPIS
    {
        private readonly RedditClient RedditClient;

        public SocialAPIS()
        {
            RedditClient = new RedditClient();
        }

        //reddit
        public async Task<SubReddit> GetSubRedditAsync(string subRedditName, int amountOfPosts = 10)
            => await RedditClient.GetSubRedditAsync(subRedditName, amountOfPosts).ConfigureAwait(false);
    }
}