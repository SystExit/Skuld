using Newtonsoft.Json.Linq;
using Skuld.APIS.Wikipedia.Models;
using Skuld.Core.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.APIS
{
    public class WikipediaClient
    {
        private readonly RateLimiter rateLimiter;

        public WikipediaClient()
        {
            rateLimiter = new RateLimiter();
        }

        public async Task<WikipediaArticle> GetArticleAsync(string language, string name)
        {
            if (rateLimiter.IsRatelimited()) return null;

            var article = new WikipediaArticle
            {
                Url = $"https://{language}.wikipedia.org/wiki/{name}"
            };

            {
                var pageInfo = await HttpWebClient.ReturnStringAsync(new Uri($"https://{language}.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exintro=&explaintext=&titles={name}")).ConfigureAwait(false);

                var jsonresp = JObject.Parse(pageInfo);
                dynamic item = jsonresp["query"]["pages"].First.First;

                string desc = Convert.ToString(item["extract"]);

                article.Name = item["title"].ToString();

                if (desc.Length >= 1024)
                {
                    var split = desc.Split(". ").ToList();
                    article.Description = string.Join(". ", split.Take(4))+".";
                }
                else
                {
                    article.Description = desc;
                }
            }

            {
                var pageImage = await HttpWebClient.ReturnStringAsync(new Uri($"https://{language}.wikipedia.org/w/api.php?format=json&action=query&prop=pageimages&piprop=original&titles={name}")).ConfigureAwait(false);

                var jsonresp = JObject.Parse(pageImage);
                dynamic item = jsonresp["query"]["pages"].First.First["original"];

                if(item != null)
                {
                    article.Original = new WikipediaImage
                    {
                        Source = item["source"].ToString(),
                        Width = Convert.ToInt32(item["width"]),
                        Height = Convert.ToInt32(item["height"])
                    };
                }
            }

            return article;
        }
    }
}