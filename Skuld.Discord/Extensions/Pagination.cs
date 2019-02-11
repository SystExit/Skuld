using Skuld.Core.Models;
using Skuld.Discord.Services;
using Skuld.Discord.Utilities;
using System.Collections.Generic;

namespace Skuld.Discord.Extensions
{
    public static class Pagination
    {
        public static IReadOnlyList<string> PaginateLeaderboard(this IList<MoneyLeaderboardEntry> list, SkuldConfig config)
        {
            var pages = new List<string>();
            string pagetext = "";

            for (int x = 0; x < list.Count; x++)
            {
                var usr = list[x];

                pagetext += $"{x + 1}. {BotService.DiscordClient.GetUser(usr.ID).Username} {config.Preferences.MoneySymbol}{usr.Money.ToString("N0")}\n";

                if ((x + 1) % 10 == 0 || (x + 1) == list.Count)
                {
                    pages.Add(pagetext);
                    pagetext = "";
                }
            }

            return pages;
        }
        public static IReadOnlyList<string> PaginateLeaderboard(this IList<ExperienceLeaderboardEntry> list)
        {
            var pages = new List<string>();
            string pagetext = "";

            for (int x = 0; x < list.Count; x++)
            {
                var usr = list[x];

                pagetext += $"{x + 1}/{list.Count}. {BotService.DiscordClient.GetUser(usr.ID).Username} - TotalXP: {usr.TotalXP} | Level: {usr.Level} | XP: {usr.XP}/{DiscordUtilities.GetXPLevelRequirement(usr.Level + 1, DiscordUtilities.PHI)}";

                if ((x + 1) % 10 == 0 || (x + 1) == list.Count)
                {
                    pages.Add(pagetext);
                    pagetext = "";
                }
            }

            return pages;
        }
    }
}
