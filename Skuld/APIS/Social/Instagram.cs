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
			var rawresp = await APIWebReq.ReturnString(url);
			if (rawresp != null)
			{
				var data = JsonConvert.DeserializeObject<UserFeed>(rawresp).Feed;
				if(data!=null)
				{
					return data;
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
