using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Skuld.Models.API.Social.Instagram;

namespace Skuld.APIS.Social
{
    public class Instagram
    {
		private static string BaseURL = "https://www.instagram.com/{{USERNAME}}/?__a=1";
		public static async Task<InstagramUser> GetInstagramUserAsync(string username)
		{
			var url = new Uri(BaseURL.Replace("{{USERNAME}}", username));
			var rawresp = await WebHandler.ReturnStringAsync(url);
			if (rawresp != null)
			{
				var data = JsonConvert.DeserializeObject<RootNode>(rawresp);
				if(data!=null)
				{
					return data.Feed.User;
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
