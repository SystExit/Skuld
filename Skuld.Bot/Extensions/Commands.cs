using Discord;
using Skuld.Core.Models.Skuld;
using SysEx.Net.Models;
using System.Collections.Generic;
using System.Text;

namespace Skuld.Bot.Extensions
{
    public static class Commands
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

        public static IList<string> Paginate(this IReadOnlyList<IAmRole> roles, SkuldGuild sguild, IGuild guild)
        {
            var pages = new List<string>();

            var s = new StringBuilder();

            for (int x = 0; x < roles.Count; x++)
            {
                var rol = roles[x];
                var role = guild.GetRole(rol.RoleId);

                var sl = new StringBuilder($"{x+1}. {role.Name} | ");

                if (rol.Price != 0)
                {
                    sl.Append($"Cost = {sguild.MoneyIcon}{rol.Price} | ");
                }
                if (rol.LevelRequired != 0)
                {
                    sl.Append($"Level = {rol.LevelRequired} | ");
                }
                if (rol.RequiredRoleId != 0)
                {
                    sl.Append($"Requires = {guild.GetRole(rol.RequiredRoleId).Name} | ");
                }

                s.Append(sl.ToString().Substring(0, sl.Length - 3));

                if ((x + 1) % 10 == 0 || (x + 1) == roles.Count)
                {
                    pages.Add(s.ToString());
                    s.Clear();
                }
                else
                {
                    s.AppendLine();
                }
            }

            return pages;
        }
    }
}
