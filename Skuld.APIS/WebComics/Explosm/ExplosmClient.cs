using Skuld.APIS.Utilities;
using Skuld.APIS.WebComics.Explosm.Models;
using Skuld.Core.Services;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Skuld.APIS.WebComics.Explosm
{
    public class ExplosmClient : BaseClient
    {
        private readonly GenericLogger logger;
        private readonly RateLimiter rateLimiter;

        public ExplosmClient(GenericLogger log) : base(log)
        {
            logger = log;
            rateLimiter = new RateLimiter();
        }

        public async Task<CAHComic> GetComicAsync()
        {
            if (rateLimiter.IsRatelimited()) return null;

            var doc = await ScrapeUrlAsync(new Uri("http://explosm.net/comics/random"));
            var html = doc.DocumentNode.ChildNodes.FirstOrDefault(x => x.Name == "html");
            var body = html.ChildNodes.FirstOrDefault(x => x.Name == "body");

            var basepagecontent = body.ChildNodes
                .FirstOrDefault(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.Contains("off-canvas-wrap")).ChildNodes
                .FirstOrDefault(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.Contains("inner-wrap"));

            var page = basepagecontent.ChildNodes
                .FirstOrDefault(x => x.Attributes.Contains("id") && x.Attributes["id"].Value.Contains("main-content"));

            var comicarea = page.ChildNodes
                .FirstOrDefault(x => x.Attributes.Contains("id") && x.Attributes["id"].Value.Contains("main-left")).ChildNodes
                .FirstOrDefault(x => x.Attributes.Contains("id") && x.Attributes["id"].Value.Contains("comic-area"));

            var comicwrap = comicarea.ChildNodes
                .FirstOrDefault(x => x.Attributes.Contains("id") && x.Attributes["id"].Value.Contains("comic-wrap"));

            var comicimageurl = "http:" + comicwrap.ChildNodes.FirstOrDefault(x => x.Attributes.Contains("id") &&
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
            var avatarurl = "http:" + avatar.Attributes["src"].Value;

            return new CAHComic
            {
                Author = author,
                ImageURL = comicimageurl,
                AuthorAvatar = avatarurl,
                AuthorURL = authorurl,
                URL = comicurl
            };
        }
    }
}