using System;
using Skuld.Models.API.Booru;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Skuld.APIS
{
    public class BooruClient
	{
		readonly Uri DanbooruBaseUri = new Uri("https://danbooru.donmai.us/posts.json?tags=");
		readonly Uri GelbooruBaseUri = new Uri("https://gelbooru.com/index.php?page=dapi&s=post&q=index&json=1&tags=");
		readonly Uri Rule34BaseUri = new Uri("https://rule34.xxx/index.php?page=dapi&s=post&q=index&json=1&tags=");
		readonly Uri E621BaseUri = new Uri("https://e621.net/post/index.json?tags=");
		readonly Uri YandereBaseUri = new Uri("https://yande.re/post.json?tags=");
		readonly Uri SafebooruBaseUri = new Uri("https://safebooru.org/index.php?page=dapi&s=post&q=index&json=1&tags=");
		readonly Uri KonaChanBaseUri = new Uri("https://konachan.com/post.json?tags=");

		readonly List<string> blacklistedTags = new List<string>
		{
			"loli",
			"shota",
			"gore",
			"vore",
			"death"
		};

		public async Task<IReadOnlyList<SafebooruImage>> GetSafebooruImagesAsync(params string[] tags)
		{
			IList<string> newtags = tags.ToList();
			AddblacklistedTags(newtags);
			var tagstring = String.Join("%20", newtags);

			Uri requesturl = requesturl = new Uri(SafebooruBaseUri.OriginalString + tagstring);

			var data = await WebHandler.ReturnStringAsync(requesturl);

			var posts = JsonConvert.DeserializeObject<List<SafebooruImage>>(data);
			if (posts != null)
			{
				return posts;
			}

			else return null;
		}
		public async Task<IReadOnlyList<Rule34Image>> GetRule34ImagesAsync(params string[] tags)
		{
			IList<string> newtags = tags.ToList();
			AddblacklistedTags(newtags);
			var tagstring = String.Join("%20", newtags);

			Uri requesturl = requesturl = new Uri(Rule34BaseUri.OriginalString + tagstring);

			var data = await WebHandler.ReturnStringAsync(requesturl);

			var posts = JsonConvert.DeserializeObject<List<Rule34Image>>(data);
			if (posts != null)
			{
				return posts;
			}

			else return null;
		}
		public async Task<IReadOnlyList<DanbooruImage>> GetDanbooruImagesAsync(params string[] tags)
		{
			IList<string> newtags = tags.ToList();
			AddblacklistedTags(newtags);
			var tagstring = String.Join("%20", newtags);

			Uri requesturl = requesturl = new Uri(DanbooruBaseUri.OriginalString + tagstring);

			var data = await WebHandler.ReturnStringAsync(requesturl);

			var posts = JsonConvert.DeserializeObject<List<DanbooruImage>>(data);
			if (posts != null)
			{
				return posts;
			}

			else return null;
		}
		public async Task<IReadOnlyList<GelbooruImage>> GetGelbooruImagesAsync(params string[] tags)
		{
			IList<string> newtags = tags.ToList();
			AddblacklistedTags(newtags);
			var tagstring = String.Join("%20", newtags);

			Uri requesturl = requesturl = new Uri(GelbooruBaseUri.OriginalString + tagstring);

			var data = await WebHandler.ReturnStringAsync(requesturl);

			var posts = JsonConvert.DeserializeObject<List<GelbooruImage>>(data);
			if (posts != null)
			{
				return posts;
			}

			else return null;
		}
		public async Task<IReadOnlyList<KonaChanImage>> GetKonaChanImagesAsync(params string[] tags)
		{
			IList<string> newtags = tags.ToList();
			AddblacklistedTags(newtags);
			var tagstring = String.Join("%20", newtags);

			Uri requesturl = requesturl = new Uri(KonaChanBaseUri.OriginalString + tagstring);

			var data = await WebHandler.ReturnStringAsync(requesturl);

			var posts = JsonConvert.DeserializeObject<List<KonaChanImage>>(data);
			if (posts != null)
			{
				return posts;
			}

			else return null;
		}
		public async Task<IReadOnlyList<E621Image>> GetE621ImagesAsync(params string[] tags)
		{
			IList<string> newtags = tags.ToList();
			AddblacklistedTags(newtags);
			var tagstring = String.Join("%20", newtags);

			Uri requesturl = requesturl = new Uri(E621BaseUri.OriginalString + tagstring);

			var data = await WebHandler.ReturnStringAsync(requesturl);

			var posts = JsonConvert.DeserializeObject<List<E621Image>>(data);
			if (posts != null)
			{
				return posts;
			}

			else return null;
		}
		public async Task<IReadOnlyList<YandereImage>> GetYandereImagesAsync(params string[] tags)
		{
			IList<string> newtags = tags.ToList();
			AddblacklistedTags(newtags);
			var tagstring = String.Join("%20", newtags);

			Uri requesturl = requesturl = new Uri(YandereBaseUri.OriginalString + tagstring);

			var data = await WebHandler.ReturnStringAsync(requesturl);

			var posts = JsonConvert.DeserializeObject<List<YandereImage>>(data);
			if (posts != null)
			{
				return posts;
			}

			else return null;
		}

		void AddblacklistedTags(IList<string> tags)
		{
			blacklistedTags.ForEach(x => tags.Add("-" + x));
		}

		public bool ContainsBlacklistedTags(string[] tags)
		{
			bool returnvalue=false;
			foreach(var tag in tags)
			{
				if (blacklistedTags.Contains(tag))
				{
					returnvalue = true;
				}
				continue;
			}
			return returnvalue;
		}
    }
}
