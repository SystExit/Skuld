using Skuld.APIS.Utilities;
using Skuld.APIS.WebComics.CAD.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.APIS.WebComics.CAD
{
    public class CADClient : BaseClient
    {
        private readonly RateLimiter rateLimiter;

        public CADClient() : base()
        {
            rateLimiter = new RateLimiter();
        }

        public async Task<CADComic> GetComicAsync()
        {
            if (rateLimiter.IsRatelimited()) return null;
            var doc = await ScrapeUrlAsync(new Uri("https://cad-comic.com/random")).ConfigureAwait(false);
            var html = doc.DocumentNode.ChildNodes.FirstOrDefault(x => x.Name == "html");
            var body = html.ChildNodes.FirstOrDefault(x => x.Name == "body");
            var container = body.ChildNodes.Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.Contains("container")).FirstOrDefault(z => z.ChildNodes.Any(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.Contains("col-md-8 main-content")));
            var container2 = container.ChildNodes.FirstOrDefault(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.Contains("col-md-8 main-content"));
            var content = container2.ChildNodes.FirstOrDefault(x => x.Name == "article");
            var comicview = content.ChildNodes.FirstOrDefault(x => x.HasClass("comicpage"));
            var comicholder = comicview.ChildNodes.LastOrDefault(x => x.Name == "a");

            var comicmeta = content.ChildNodes.FirstOrDefault(x => x.Attributes.Contains("id") && x.Attributes["id"].Value == "comicblog").ChildNodes.FirstOrDefault(x => x.HasClass("blog-wrap"));

            var comicinfo = comicmeta.ChildNodes.FirstOrDefault(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "blog-meta-wrap");

            var comicurl = comicinfo.ChildNodes.FirstOrDefault(x => x.Name == "a");

            var comicdate = comicinfo.ChildNodes.FirstOrDefault(x => x.Name == "p").InnerText.Replace(" by Tim", "");

            return new CADComic
            {
                ImageURL = comicholder.FirstChild.Attributes["src"].Value,
                Title = comicurl.InnerText,
                Uploaded = DateTime.Parse(comicdate).ToString("dd'/'MM'/'yyyy"),
                URL = comicurl.Attributes["href"].Value
            };
        }
    }
}