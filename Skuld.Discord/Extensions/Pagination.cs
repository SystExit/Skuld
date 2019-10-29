using Skuld.Core.Models;
using Skuld.Core.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Skuld.Discord.Extensions
{
    public static class Pagination
    {
        public static IReadOnlyList<string> PaginateLeaderboard(this IEnumerable<User> list, Guild guild)
        {
            var pages = new List<string>();
            string pagetext = "";

            int x = 0;
            foreach (var usr in list)
            {
                pagetext += $"{x + 1}. {usr.Username} {guild.MoneyIcon}{usr.Money.ToString("N0")}\n";

                if ((x + 1) % 10 == 0 || (x + 1) == list.Count())
                {
                    pages.Add(pagetext);
                    pagetext = "";
                }

                x++;
            }

            return pages;
        }

        public static IReadOnlyList<string> PaginateLeaderboard(this IEnumerable<UserExperience> list)
        {
            using var database = new SkuldDbContextFactory().CreateDbContext();

            var pages = new List<string>();
            string pagetext = "";

            int x = 0;
            foreach (var usr in list)
            {
                pagetext += $"{x + 1}/{list.Count()}. {database.Users.FirstOrDefault(x => x.Id == usr.UserId).Username} - TotalXP: {usr.TotalXP} | Level: {usr.Level} | XP: {usr.XP}/{DiscordTools.GetXPLevelRequirement(usr.Level + 1, DiscordTools.PHI)}\n";

                if ((x + 1) % 10 == 0 || (x + 1) == list.Count())
                {
                    pages.Add(pagetext);
                    pagetext = "";
                }

                x++;
            }

            return pages;
        }
    }
}