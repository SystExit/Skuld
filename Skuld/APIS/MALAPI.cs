using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.Linq;
using Skuld.APIS;
using Skuld.Tools;
using System.Text;
using System.Collections.Generic;
using System.Web;
using Discord.Addons.Interactive;
using Skuld.Models.API.MAL;

namespace Skuld.APIS
{
	public class MALAPI
	{
		private static async Task<string> GetResultAsync(bool isAnime, string Search)
		{
			string url = "";
			if (!isAnime)
				url = $"https://myanimelist.net/api/manga/search.xml?q={Search}";
			else
				url = $"https://myanimelist.net/api/anime/search.xml?q={Search}";
			Search = Search.Replace(" ", "+");
			var byteArray = new UTF8Encoding().GetBytes($"{Config.Load().MALUName}:{Config.Load().MALPassword}");
			return await APIWebReq.ReturnString(new Uri(url), byteArray);
		}
		private static XmlDocument ConvertStringToXmlDoc(string xml)
		{
			var xmldoc = new XmlDocument();
			xmldoc.LoadXml(xml);
			return xmldoc;
		}
		public static async Task<AnimeArr> GetAnimesAsync(string Search)
		{
			var result = await GetResultAsync(isAnime: true, Search: Search);
			var xmldoc = ConvertStringToXmlDoc(result);
			XObject xNode = XDocument.Parse(xmldoc.FirstChild.NextSibling.OuterXml);
			return JsonConvert.DeserializeObject<AnimeArr>(JObject.Parse(JsonConvert.SerializeXNode(xNode))["anime"].ToString());
		}
		public static async Task<MangaArr> GetMangasAsync(string Search)
		{
			var result = await GetResultAsync(isAnime: false, Search: Search);
			var xmldoc = ConvertStringToXmlDoc(result);
			XObject xNode = XDocument.Parse(xmldoc.FirstChild.NextSibling.OuterXml);
			return JsonConvert.DeserializeObject<MangaArr>(JObject.Parse(JsonConvert.SerializeXNode(xNode))["manga"].ToString());
		}
	}
}
