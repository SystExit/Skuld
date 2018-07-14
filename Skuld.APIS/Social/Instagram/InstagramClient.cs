using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Skuld.APIS.Social.Instagram.Models;
using Skuld.APIS.Utilities;
using Skuld.Core.Services;

namespace Skuld.APIS.Social.Instagram
{
    public class InstagramClient : BaseClient
    {
        private static string BaseURL = "https://www.instagram.com/{{USERNAME}}";
        private readonly RateLimiter rateLimiter;

        public InstagramClient(GenericLogger log) : base(log)
        {
            rateLimiter = new RateLimiter();
        }

        public async Task<InstagramUser> GetInstagramUserAsync(string username)
        {
            if (rateLimiter.IsRatelimited()) return null;

            var url = new Uri(BaseURL.Replace("{{USERNAME}}", username));
            var doc = await ScrapeUrlAsync(url);
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
