using SysEx.Net.Models;
using System.Collections.Generic;

namespace Skuld.Bot.Extensions
{
    public static class Fun
    {
        public static IList<string> PaginateList(this IReadOnlyList<MemeEndpoints> list)
        {
            var pages = new List<string>();
            string pagetext = "";

            for (int x = 0; x < list.Count; x++)
            {
                var obj = list[x];

                pagetext += $"Template: {obj.Name} | Required Sources: {obj.RequiredSources}\n";

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
