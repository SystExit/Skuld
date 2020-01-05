using Discord;
using Discord.Commands;
using System.Linq;
using System.Text;

namespace Skuld.Core.Generic.Models
{
    public class GuildRoleConfig
    {
        public int Cost;
        public int RequireLevel;
        public bool Giveable;
        public IRole RequiredRole;

        public GuildRoleConfig()
        {
            Cost = 0;
            RequireLevel = 0;
            RequiredRole = null;
        }

        public static bool FromString(string input, ICommandContext context, out GuildRoleConfig roleConfig)
        {
            roleConfig = new GuildRoleConfig();
            string[] inputsplit = input.Split(' ');

            if (inputsplit.Where(x => x.StartsWith("cost=")).Any())
            {
                if (int.TryParse(inputsplit.FirstOrDefault(x => x.StartsWith("cost=")).Replace("cost=", ""), out int result))
                {
                    roleConfig.Cost = result;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                roleConfig.Cost = 0;
            }

            if (inputsplit.Where(x => x.StartsWith("require-level=")).Any())
            {
                if (int.TryParse(inputsplit.FirstOrDefault(x => x.StartsWith("require-level=")).Replace("require-level=", ""), out int result))
                {
                    roleConfig.RequireLevel = result;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                roleConfig.RequireLevel = 0;
            }

            if (inputsplit.Where(x => x.StartsWith("require-role=")).Any())
            {
                var first = inputsplit.FirstOrDefault(x => x.StartsWith("require-role="));
                if (first["require-role=".Count()] == '"')
                {
                    var last = inputsplit.LastOrDefault(x => x.EndsWith("\""));

                    int firstIndex = 0;
                    int lastIndex = 0;
                    for (var x = 0; x < inputsplit.Count(); x++)
                    {
                        if (inputsplit[x] == first)
                        {
                            firstIndex = x;
                        }
                        if (inputsplit[x] == last)
                        {
                            lastIndex = x;
                        }
                    }

                    var skipped = inputsplit.Skip(firstIndex).Take(lastIndex - firstIndex);
                }
                var roleraw = inputsplit.FirstOrDefault(x => x.StartsWith("require-role=")).Replace("require-role=", "");
                IRole role = null;
                bool gottenRole = true;

                if (MentionUtils.TryParseRole(roleraw, out ulong roleID))
                {
                    role = context.Guild.GetRole(roleID);
                }
                else
                {
                    gottenRole = false;
                }

                if (ulong.TryParse(roleraw, out roleID))
                {
                    role = context.Guild.GetRole(roleID);
                }
                else
                {
                    gottenRole = false;
                }

                if (!gottenRole)
                {
                    role = context.Guild.Roles.FirstOrDefault(x => x.Name.ToLowerInvariant() == roleraw.ToLowerInvariant());
                }

                if (role != null)
                {
                    roleConfig.RequiredRole = role;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                roleConfig.RequiredRole = null;
            }

            return true;
        }

        public override string ToString()
        {
            StringBuilder message = new StringBuilder();

            if (Cost != 0)
                message.Append($"cost={Cost} ");
            if (RequireLevel != 0)
                message.Append($"require-level={RequireLevel} ");
            if (RequiredRole != null)
                message.Append($"require-role={RequiredRole.Id} ");

            return message.ToString()[0..^1];
        }
    }
}