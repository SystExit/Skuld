using Skuld.APIS.Social.Instagram;
using Skuld.APIS.Social.Instagram.Models;
using Skuld.APIS.Social.Reddit;
using Skuld.APIS.Social.Reddit.Models;
using System.Threading.Tasks;

namespace Skuld.APIS
{
    public class SocialAPIS
    {
        private RedditClient RedditClient;
        private InstagramClient InstagramClient;

        public SocialAPIS()
        {
            RedditClient = new RedditClient();
            InstagramClient = new InstagramClient();
        }

        //reddit
        public async Task<SubReddit> GetSubRedditAsync(string subRedditName, int amountOfPosts = 10)
            => await RedditClient.GetSubRedditAsync(subRedditName, amountOfPosts).ConfigureAwait(false);

        //instagram
        public async Task<InstagramUser> GetInstagramUserAsync(string username)
            => await InstagramClient.GetInstagramUserAsync(username).ConfigureAwait(false);
    }
}