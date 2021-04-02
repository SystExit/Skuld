using Skuld.APIS.Extensions;
using SteamStorefrontAPI.Classes;
using SteamStoreQuery;
using System.Linq;

namespace Skuld.APIS.Utilities
{
	public static class SteamUtilities
	{
		private const string BaseSteamRunUrl = "steam://run/";
		private const string BaseSteamStoreUrl = "steam://store/";

		public static string GetSteamGameDescription(Listing game, SteamApp appdata)
		{
			var launchurl = BaseSteamRunUrl + appdata.SteamAppId;
			var storeurl = BaseSteamStoreUrl + appdata.SteamAppId;

			var fulldesc = appdata.AboutTheGame;
			string desc;

			if (fulldesc.Contains("<br"))
			{
				var clean = fulldesc.Replace("<br /> ", "").Replace("<br/> ", "").Replace("<br />", "").Replace("<br/>", "").Replace("<br> ", "\n").Replace("<br>", "\n");
				var split = clean.Split('\n');

				int count = 0;
				while (count < split.Length)
				{
					if (count == split.Length - 1)
					{
						count = -1; break;
					}
					if (string.IsNullOrEmpty(split[count]))
					{
						count++;
					}
					else
					{
						break;
					}
				}
				desc = split[count].StripHtml();
			}
			else
			{
				desc = string.Join(" ", fulldesc.Split(' ').Take(250));
			}

			string returnstring = "";
			//Description
			returnstring += desc;
			//seperator
			returnstring += "\n\n";
			//Cost
			if (!appdata.IsFree)
			{
				returnstring += "Price (USD): $" + game.PriceUSD;
			}
			else
			{
				returnstring += "Price (USD): Free";
			}
			//seperator
			returnstring += "\n\n";
			//LaunchURL
			returnstring += $"[Launch]({launchurl}) | [Store]({storeurl})";

			return returnstring;
		}
	}
}