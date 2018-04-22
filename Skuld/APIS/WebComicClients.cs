using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Skuld.Models.API;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Skuld.APIS
{
    public class WebComicClients
    {
		readonly Random random;

		public WebComicClients(Random ran) //depinj
		{
			random = ran;
		}

		//cah
		public async Task<CAHComic> GetCAHComicAsync()
		{
			var doc = await WebHandler.ScrapeUrlAsync(new Uri("http://explosm.net/comics/random"));
			var html = doc.DocumentNode.ChildNodes.FirstOrDefault(x=>x.Name == "html");
			var body = html.ChildNodes.FirstOrDefault(x => x.Name == "body");

			var basepagecontent = body.ChildNodes
				.FirstOrDefault(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.Contains("off-canvas-wrap")).ChildNodes
				.FirstOrDefault(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.Contains("inner-wrap"));

			var page = basepagecontent.ChildNodes
				.FirstOrDefault(x => x.Attributes.Contains("id") && x.Attributes["id"].Value.Contains("main-content"));

			var comicarea = page.ChildNodes
				.FirstOrDefault(x => x.Attributes.Contains("id") && x.Attributes["id"].Value.Contains("main-left")).ChildNodes
				.FirstOrDefault(x=>x.Attributes.Contains("id") && x.Attributes["id"].Value.Contains("comic-area"));

			var comicwrap = comicarea.ChildNodes
				.FirstOrDefault(x => x.Attributes.Contains("id") && x.Attributes["id"].Value.Contains("comic-wrap"));

			var comicimageurl = "http:"+comicwrap.ChildNodes.FirstOrDefault(x => x.Attributes.Contains("id") &&
				x.Attributes["id"].Value.Contains("main-comic") &&
				x.Attributes.Contains("src")).Attributes["src"].Value;

			var comicinfo = comicarea.ChildNodes
				.FirstOrDefault(x => x.Attributes.Contains("id") && x.Attributes["id"].Value.Contains("comic-under")).ChildNodes
				.FirstOrDefault(x => x.Attributes.Contains("id") && x.Attributes["id"].Value.Contains("comic-info"));

			var authorwrap = comicinfo.ChildNodes
				.FirstOrDefault(x => x.Attributes.Contains("id") && x.Attributes["id"].Value.Contains("comic-info-text"));

			var authorblock = authorwrap.ChildNodes
				.FirstOrDefault(x => x.Attributes.Contains("id") && x.Attributes["id"].Value.Contains("comic-author"));

			var socialblock = authorwrap.ChildNodes
				.FirstOrDefault(x => x.Attributes.Contains("id") && x.Attributes["id"].Value.Contains("comic-social"));

			var avatarblock = comicinfo.ChildNodes
				.FirstOrDefault(x => x.Attributes.Contains("id") && x.Attributes["id"].Value.Contains("comic-avatar"));

			var avatar = avatarblock.ChildNodes.FirstOrDefault(x => x.Name.Contains("img") && x.Attributes.Contains("src"));

			var author = authorblock.ChildNodes.FirstOrDefault(x => x.InnerText.Contains("by")).InnerText;
			author = Regex.Replace(author, @"\r\n?|\n", "");

			var comicurl = socialblock.ChildNodes
				.FirstOrDefault(
					x => 
						x.Attributes.Contains("id") && 
						x.Attributes["id"].Value.Contains("comic-social-link") && 
						x.Attributes.Contains("href")
				)
				.Attributes["href"].Value;

			var authorurl = "http://explosm.net" + avatarblock.Attributes["href"].Value;
			var avatarurl = "http:"+avatar.Attributes["src"].Value;

			return new CAHComic
			{
				Author = author,
				ImageURL = comicimageurl,
				AuthorAvatar = avatarurl,
				AuthorURL = authorurl,
				URL = comicurl
			};
		}

		//xkcd
		int? XKCDLastPage;
        public async Task<int?> GetXKCDLastPageAsync()
        {
            var rawresp = await WebHandler.ReturnStringAsync(new Uri("https://xkcd.com/info.0.json"));
            var jsonresp = JObject.Parse(rawresp);
            dynamic item = jsonresp;
            if (item["num"].ToString() != null)
            {
                int num = Convert.ToInt32(item["num"].ToString());
                return num;
            }
            else
                return null;
        }
		public async Task<XKCDComic> GetRandomXKCDComicAsync()
		{
			if(!XKCDLastPage.HasValue)
			{
				XKCDLastPage = await GetXKCDLastPageAsync();
				return await GetXKCDComicAsync(random.Next(0, XKCDLastPage.Value));
			}
			else
			{
				return await GetXKCDComicAsync(random.Next(0, XKCDLastPage.Value));
			}
		}
		
		public async Task<XKCDComic> GetXKCDComicAsync(int comicid)
        {
            if (XKCDLastPage.HasValue)
			{
				if(comicid<XKCDLastPage.Value&&comicid>0)				
					return JsonConvert.DeserializeObject<XKCDComic>((await WebHandler.ReturnStringAsync(new Uri($"https://xkcd.com/{comicid}/info.0.json"))));				
				else
					return JsonConvert.DeserializeObject<XKCDComic>((await WebHandler.ReturnStringAsync(new Uri($"https://xkcd.com/{XKCDLastPage.Value}/info.0.json"))));
			}                
            else
            {
                XKCDLastPage = await GetXKCDLastPageAsync().ConfigureAwait(false);		
				return null;
            }
        }
    }
}
