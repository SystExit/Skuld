using System;
using System.Threading.Tasks;
using Skuld.APIS.Utilities;
using Skuld.APIS.Wikipedia.Models;
using Newtonsoft.Json.Linq;
using Skuld.Core.Services;

namespace Skuld.APIS
{
    public class WikipediaClient : BaseClient
    {
        private readonly RateLimiter rateLimiter;

        public WikipediaClient(GenericLogger log) : base(log)
        {
            rateLimiter = new RateLimiter();
        }

        public async Task<WikipediaArticle> GetArticleAsync(string language, string name)
        {
            if (rateLimiter.IsRatelimited()) return null;

            var jsonresp = JObject.Parse((await ReturnStringAsync(new Uri($"https://{language}.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exintro=&explaintext=&titles={name}"))));
            dynamic item = jsonresp["query"]["pages"].First.First;
            string desc = Convert.ToString(item["extract"]);
            var article = new WikipediaArticle
            {
                Name = item["title"].ToString(),
                Description = desc.Remove(500) + "...\nRead more at the article.",
                Url = $"https://{language}.wikipedia.org/wiki/{name}"
            };
            return article;
        }
    }
}
