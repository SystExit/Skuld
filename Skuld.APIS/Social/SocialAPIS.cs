using System.Threading.Tasks;
using Skuld.APIS.Social.Reddit;
using Skuld.APIS.Social.Reddit.Models;
using Skuld.APIS.Social.Instagram;
using Skuld.APIS.Social.Instagram.Models;
using Skuld.Core.Services;

namespace Skuld.APIS
{
    public class SocialAPIS
    {
        private RedditClient RedditClient;
        private InstagramClient InstagramClient;

        public SocialAPIS(GenericLogger log)
        {
            RedditClient = new RedditClient(log);
            InstagramClient = new InstagramClient(log);
        }

        //reddit
        public async Task<SubReddit> GetSubRedditAsync(string subRedditName, int amountOfPosts = 10)
            => await RedditClient.GetSubRedditAsync(subRedditName, amountOfPosts);

        //instagram
        public async Task<InstagramUser> GetInstagramUserAsync(string username)
            => await InstagramClient.GetInstagramUserAsync(username);
    }
}