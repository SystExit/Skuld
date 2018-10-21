﻿using Skuld.Core.Extensions;
using Skuld.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Database.Extensions
{
    public static class UserExtensions
    {
        public static async Task<long> GetPastaKarma(this SkuldUser user)
        {
            long returnkarma = 0;

            var pastas = await DatabaseClient.GetAllPastasAsync();
            if (!pastas.Successful) return 0;

            var pastalist = pastas.Data as IReadOnlyList<Pasta>;

            if (pastas != null && pastalist.Count > 0)
            {
                var ownedpastas = pastalist.Where(x => x.OwnerID == user.ID);

                if (ownedpastas != null)
                {
                    long upkarma = 0;
                    long downkarma = 0;
                    foreach (var pasta in ownedpastas)
                    {
                        upkarma += pasta.Upvotes;
                        downkarma += pasta.Downvotes;
                    }
                    returnkarma = upkarma - (downkarma / 5);
                }
            }

            return returnkarma;
        }

        public static async Task<UserExperience> GetUserExperienceAsync(this SkuldUser user)
        {
            var result = await DatabaseClient.GetUserExperienceAsync(user.ID).ConfigureAwait(false);
            if (result.Successful && result.Data is UserExperience)
                return result.Data as UserExperience;

            return null;
        }

        public static GuildExperience GetGuildExperience(this UserExperience xp, ulong GuildID)
            => xp.GuildExperiences.FirstOrDefault(x => x.GuildID == GuildID);

        public static async Task<bool> DoDailyAsync(this SkuldUser user, SkuldConfig config, SkuldUser sender = null)
        {
            if (sender == null)
            {
                if (user.Daily != 0)
                {
                    if (user.Daily < (86400 - DateTime.UtcNow.ToEpoch()))
                    {
                        user.Daily = DateTime.UtcNow.ToEpoch();
                        user.Money += config.Preferences.DailyAmount;
                        await DatabaseClient.UpdateUserAsync(user);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    user.Daily = DateTime.UtcNow.ToEpoch();
                    user.Money += config.Preferences.DailyAmount;
                    await DatabaseClient.UpdateUserAsync(user);
                    return true;
                }
            }
            else
            {
                if (sender.Daily != 0)
                {
                    if (sender.Daily < (86400 - DateTime.UtcNow.ToEpoch()))
                    {
                        sender.Daily = DateTime.UtcNow.ToEpoch();
                        user.Money += config.Preferences.DailyAmount;
                        await DatabaseClient.UpdateUserAsync(user);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    sender.Daily = DateTime.UtcNow.ToEpoch();
                    user.Money += config.Preferences.DailyAmount;
                    await DatabaseClient.UpdateUserAsync(user);
                    return true;
                }
            }
        }
    }
}