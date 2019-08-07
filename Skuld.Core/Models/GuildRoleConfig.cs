using Discord;
using Discord.Commands;
using System.Linq;

namespace Skuld.Core.Models
{
    public class GuildRoleConfig
    {
        public bool Optable;
        public int Cost;
        public bool LevelReward;
        public int RequireLevel;
        public IRole RequiredRole;

        public GuildRoleConfig()
        {
            Optable = false;
            Cost = 0;
            LevelReward = false;
            RequireLevel = 0;
            RequiredRole = null;
        }

        public static bool operator !=(GuildRoleConfig a, GuildRoleConfig b)
        {
            return a.Optable != b.Optable &&
                a.Cost != b.Cost &&
                a.LevelReward != b.LevelReward &&
                a.RequireLevel != b.RequireLevel &&
                a.RequiredRole.Id != b.RequiredRole.Id;
        }
        public static bool operator ==(GuildRoleConfig a, GuildRoleConfig b)
        {
            return a.Optable == b.Optable &&
                a.Cost == b.Cost &&
                a.LevelReward == b.LevelReward &&
                a.RequireLevel == b.RequireLevel &&
                a.RequiredRole.Id == b.RequiredRole.Id;
        }

        public static bool FromString(string input, ICommandContext context, out GuildRoleConfig roleConfig)
        {
            roleConfig = new GuildRoleConfig();
            string[] inputsplit = input.Split(' ');

            if(inputsplit.Where(x=>x.StartsWith("optable=")).Count() > 0)
            {
                if(bool.TryParse(inputsplit.FirstOrDefault(x => x.StartsWith("optable=")).Replace("optable=", ""), out bool result))
                {
                    roleConfig.Optable = result;
                }
                else
                {
                    return false;
                }
            }

            if (inputsplit.Where(x => x.StartsWith("cost=")).Count() > 0)
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

            if (inputsplit.Where(x => x.StartsWith("level-grant=")).Count() > 0)
            {
                if (bool.TryParse(inputsplit.FirstOrDefault(x => x.StartsWith("level-grant=")).Replace("level-grant=", ""), out bool result))
                {
                    roleConfig.LevelReward = result;
                }
                else
                {
                    return false;
                }
            }

            if (inputsplit.Where(x => x.StartsWith("require-level=")).Count() > 0)
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

            if (inputsplit.Where(x => x.StartsWith("require-role=")).Count() > 0)
            {
                var first = inputsplit.FirstOrDefault(x => x.StartsWith("require-role="));
                if (first["require-role=".Count()] == '"')
                {
                    var last = inputsplit.LastOrDefault(x => x.EndsWith("\""));

                    int firstIndex = 0;
                    int lastIndex = 0;
                    for(var x = 0; x < inputsplit.Count(); x++)
                    {
                        if(inputsplit[x] == first)
                        {
                            firstIndex = x;
                        }
                        if(inputsplit[x] == last)
                        {
                            lastIndex = x;
                        }
                    }

                    var skipped = inputsplit.Skip(firstIndex).Take(lastIndex - firstIndex);
                }
                var roleraw = inputsplit.FirstOrDefault(x => x.StartsWith("require-role=")).Replace("require-role=", "");
                IRole role = null;
                bool gottenRole = true;

                if(MentionUtils.TryParseRole(roleraw, out ulong roleID))
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

                if(role != null)
                {
                    roleConfig.RequiredRole = role;
                }
                else
                {
                    return false;
                }
            }

             return true;
        }
    }
}
