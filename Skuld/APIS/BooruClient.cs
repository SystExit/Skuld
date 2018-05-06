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

			Uri requesturl = requesturl = new Uri("https://safebooru.org/index.php?page=dapi&s=post&q=index&json=1&tags=" + tagstring);

			var data = await WebHandler.ReturnStringAsync(requesturl);

			if (data != null)
			{
				var posts = JsonConvert.DeserializeObject<List<SafebooruImage>>(data);
				return posts;
			}

			return null;
		}
		public async Task<IReadOnlyList<Rule34Image>> GetRule34ImagesAsync(params string[] tags)
		{
			IList<string> newtags = tags.ToList();
			AddblacklistedTags(newtags);
			var tagstring = String.Join("%20", newtags);

			Uri requesturl = requesturl = new Uri("https://rule34.xxx/index.php?page=dapi&s=post&q=index&json=1&tags=" + tagstring);

			var data = await WebHandler.ReturnStringAsync(requesturl);
			if (data != null)
			{
				var posts = JsonConvert.DeserializeObject<List<Rule34Image>>(data);
				return posts;
			}

			return null;
		}
		public async Task<IReadOnlyList<DanbooruImage>> GetDanbooruImagesAsync(params string[] tags)
		{
			IList<string> newtags = tags.ToList();
			AddblacklistedTags(newtags);
			var tagstring = String.Join("%20", newtags);

			Uri requesturl = requesturl = new Uri("https://danbooru.donmai.us/posts.json?tags=" + tagstring);

			var data = await WebHandler.ReturnStringAsync(requesturl);
			if(data!=null)
			{
				var posts = JsonConvert.DeserializeObject<List<DanbooruImage>>(data);
				return posts;
			}

			return null;
		}
		public async Task<IReadOnlyList<GelbooruImage>> GetGelbooruImagesAsync(params string[] tags)
		{
			IList<string> newtags = tags.ToList();
			AddblacklistedTags(newtags);
			var tagstring = String.Join("%20", newtags);

			Uri requesturl = requesturl = new Uri("https://gelbooru.com/index.php?page=dapi&s=post&q=index&json=1&tags=" + tagstring);

			var data = await WebHandler.ReturnStringAsync(requesturl);

			if (data != null)
			{
				var posts = JsonConvert.DeserializeObject<List<GelbooruImage>>(data);
				return posts;
			}
			else return null;
		}
		public async Task<IReadOnlyList<KonaChanImage>> GetKonaChanImagesAsync(params string[] tags)
		{
			IList<string> newtags = tags.ToList();
			AddblacklistedTags(newtags);
			var tagstring = String.Join("%20", newtags);

			Uri requesturl = requesturl = new Uri("https://konachan.com/post.json?tags=" + tagstring);

			var data = await WebHandler.ReturnStringAsync(requesturl);
			
			if (data != null)
			{
				var posts = JsonConvert.DeserializeObject<List<KonaChanImage>>(data);
				return posts;
			}
			else return null;
		}
		public async Task<IReadOnlyList<E621Image>> GetE621ImagesAsync(params string[] tags)
		{
			IList<string> newtags = tags.ToList();
			AddblacklistedTags(newtags);
			var tagstring = String.Join("%20", newtags);

			Uri requesturl = requesturl = new Uri("https://e621.net/post/index.json?tags=" + tagstring);

			var data = await WebHandler.ReturnStringAsync(requesturl);
			
			if (data != null)
			{
				var posts = JsonConvert.DeserializeObject<List<E621Image>>(data);
				return posts;
			}
			else return null;
		}
		public async Task<IReadOnlyList<YandereImage>> GetYandereImagesAsync(params string[] tags)
		{
			IList<string> newtags = tags.ToList();
			AddblacklistedTags(newtags);
			var tagstring = String.Join("%20", newtags);

			Uri requesturl = requesturl = new Uri("https://yande.re/post.json?tags=" + tagstring);

			var data = await WebHandler.ReturnStringAsync(requesturl);
			
			if (data != null)
			{
				var posts = JsonConvert.DeserializeObject<List<YandereImage>>(data);
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
			}
			return returnvalue;
		}
    }
}
