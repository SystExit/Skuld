using System;

namespace Skuld.APIS.Giphy.Models
{
	public class GiphyGif
	{
		public string ID { get; set; }

		public Uri Url
		{
			get
			{
				return new("https://i.giphy.com/media/" + ID + "/giphy.gif");
			}
		}
	}
}