using Skuld.Core.Extensions;
using SteamStoreQuery;

namespace Skuld.APIS.Utilities
{
    public static class SteamUtilities
    {
        private static string BaseSteamRunUrl = "https://skuld.systemexit.co.uk/tools/steam.php?action=run&appid=";
        private static string BaseSteamStoreUrl = "https://skuld.systemexit.co.uk/tools/steam.php?action=store&appid=";

        public static string GetSteamGameDescription(Listing game, Steam.Models.SteamStore.StoreAppDetailsDataModel appdata)
        {
            var launchurl = BaseSteamRunUrl += appdata.SteamAppId;
            var storeurl = BaseSteamStoreUrl += appdata.SteamAppId;

            var fulldesc = appdata.AboutTheGame;
            var clean = fulldesc.Replace("<br /> ", "").Replace("<br/> ", "").Replace("<br />", "").Replace("<br/>", "").Replace("<br> ", "\n").Replace("<br>", "\n");
            var split = clean.Split("\n");

            int count = 0;
            while (count < split.Length)
            {
                if (count == split.Length - 1)
                {
                    count = -1; break;
                }
                if (split[count] == "")
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            var desc = split[count].StripHtml();

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