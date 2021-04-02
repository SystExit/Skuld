using Newtonsoft.Json.Linq;
using Skuld.APIS.Wikipedia.Models;
using Skuld.Core.Extensions.Conversion;
using Skuld.Core.Utilities;
using System.Linq;
using System.Threading.Tasks;
using WikiClientLibrary;
using WikiClientLibrary.Client;
using WikiClientLibrary.Pages;
using WikiClientLibrary.Pages.Queries;
using WikiClientLibrary.Pages.Queries.Properties;
using WikiClientLibrary.Sites;

namespace Skuld.APIS
{
	public class WikipediaClient
	{
		private readonly RateLimiter rateLimiter;
		private readonly WikiClient wikiClient;
		private readonly WikiSite wikipediaSite;

		public WikipediaClient()
		{
			rateLimiter = new RateLimiter();
			wikiClient = new()
			{
				ClientUserAgent = HttpWebClient.UAGENT
			};
			wikipediaSite = new(wikiClient, "https://en.wikipedia.org/w/api.php");
		}

		public async Task<WikipediaArticle> GetArticleAsync(string language, string name)
		{
			if (rateLimiter.IsRatelimited()) return null;

			var article = new WikipediaArticle
			{
				Url = $"https://{language}.wikipedia.org/wiki/{name}"
			};

			var page = new WikiPage(wikipediaSite, name);

			await page.RefreshAsync(new WikiPageQueryProvider
			{
				Properties =
				{
					new ExtractsPropertyProvider
					{
						MaxCharacters = 1024,
						AsPlainText = true,
						IntroductionOnly = true
					}
				}
			});

			var extractGroup = page.GetPropertyGroup<ExtractsPropertyGroup>();

			article.Name = page.Title;
			article.Url = WikiLink.Parse(wikipediaSite, name).TargetUrl;

			article.Description = extractGroup.Extract;

			if (article.Description.Length >= 1024)
			{
				var split = article.Description.Split(". ").ToList();
				article.Description = string.Join(". ", split.Take(4)) + ".";
			}

			var response = await HttpWebClient.ReturnStringAsync(new System.Uri($"https://www.wikidata.org/w/api.php?action=wbgetentities&format=json&sites={language}wiki&props=claims&titles={name}"));
			var jsonresp = JObject.Parse(response);
			var container = (JObject)jsonresp["entities"].First.Value<JProperty>().Value;
			var claims = container["claims"];

			//P18/P154/P242/P109/P1621
			JToken snak = null;
			if (claims["P18"] is not null)
			{
				snak = claims["P18"];
			}
			else if (claims["P154"] is not null)
			{
				snak = claims["P154"];
			}
			else if (claims["P242"] is not null)
			{
				snak = claims["P242"];
			}
			else if (claims["P109"] is not null)
			{
				snak = claims["P109"];
			}
			else if (claims["P1621"] is not null)
			{
				snak = claims["P1621"];
			}

			if (snak is not null)
			{
				var val = snak.First["mainsnak"]["datavalue"]["value"].ToObject<string>();

				val = val.Replace(" ", "_");

				var md5 = val.CreateMD5(true); ;

				article.ImageUrl = $"https://upload.wikimedia.org/wikipedia/commons/{md5[0]}/{md5[0]}{md5[1]}/{val}";
			}

			return article;
		}
	}
}