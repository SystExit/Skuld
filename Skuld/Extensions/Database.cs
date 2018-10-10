using Discord;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Models.Database;
using Skuld.Services;
using Skuld.Utilities.Discord;
using System;
using System.Threading.Tasks;

namespace Skuld.Extensions
{
    public static class Database
    {
        public static async Task<Embed> GetProfileAsync(this SkuldUser user, IGuildUser guildUser, SkuldConfig Configuration)
        {
            var embed = new EmbedBuilder
            {
                Color = EmbedUtils.RandomColor(),
                Author = new EmbedAuthorBuilder
                {
                    Name = guildUser.Username,
                    IconUrl = guildUser.GetAvatarUrl() ?? "http://www.emoji.co.uk/files/mozilla-emojis/smileys-people-mozilla/11419-bust-in-silhouette.png"
                }
            };
            embed.AddField(Configuration.Preferences.MoneyName, user.Money.ToString("N0") ?? "No Money", inline: true);
            if (user.Daily != 0)
            {
                embed.AddField("Daily", user.Daily.FromEpoch().ToString("dd/MM/yyyy HH:mm:ss"), inline: true);
            }
            else
            {
                embed.AddField("Daily", "Not used Daily", inline: true);
            }
            if (user.FavCmd != null && user.FavCmdUsg != 0)
            {
                embed.AddField("Favourite Command", $"`{user.FavCmd}` and it has been used {user.FavCmdUsg} times", inline: true);
            }
            else
            {
                embed.AddField("Favourite Command", "No favourite Command", inline: true);
            }
            embed.AddField("Pasta Karma", $"{(await user.GetPastaKarma()).ToString("N0")} Karma");
            embed.AddField("Description", user.Description ?? "No Description", inline: false);

            return embed.Build();
        }

        public static async Task<Embed> GetExtendedProfileAsync(this SkuldUser user, IGuildUser guildUser, SkuldConfig Configuration)
        {
            var embed = new EmbedBuilder
            {
                Color = EmbedUtils.RandomColor(),
                Author = new EmbedAuthorBuilder
                {
                    Name = guildUser.Username,
                    IconUrl = guildUser.GetAvatarUrl() ?? "http://www.emoji.co.uk/files/mozilla-emojis/smileys-people-mozilla/11419-bust-in-silhouette.png"
                }
            };

            embed.AddField(Configuration.Preferences.MoneyName, user.Money.ToString("N0") ?? "No Money", inline: true);

            if (user.Daily != 0)
            {
                embed.AddField("Daily", user.Daily.FromEpoch().ToString("dd/MM/yyyy HH:mm:ss"), inline: true);
            }
            else
            {
                embed.AddField("Daily", "Not used Daily", inline: true);
            }

            if (user.Glares > 0)
            {
                embed.AddField("Glares", user.Glares + " times", inline: true);
            }
            else
            {
                embed.AddField("Glares", "Not glared at anyone", inline: true);
            }

            if (user.GlaredAt > 0)
            {
                embed.AddField("Glared At", user.GlaredAt + " times", inline: true);
            }
            else
            {
                embed.AddField("Glared At", "Not been glared at", inline: true);
            }

            if (user.Pats > 0)
            {
                embed.AddField("Pats", user.Pats + " times", inline: true);
            }
            else
            {
                embed.AddField("Pats", "Not been patted", inline: true);
            }

            if (user.Patted > 0)
            {
                embed.AddField("Patted", user.Patted + " times", inline: true);
            }
            else
            {
                embed.AddField("Patted", "Not patted anyone", inline: true);
            }

            if (user.HP > 0)
            {
                embed.AddField("HP", user.HP + " HP", inline: true);
            }
            else
            {
                embed.AddField("HP", "No HP", inline: true);
            }

            if (user.FavCmd != null && user.FavCmdUsg != 0)
            {
                embed.AddField("Favourite Command", $"`{user.FavCmd}` and it has been used {user.FavCmdUsg} times", inline: true);
            }
            else
            {
                embed.AddField("Favourite Command", "No favourite Command", inline: true);
            }

            embed.AddField("Pasta Karma", $"{(await user.GetPastaKarma()).ToString("N0")} Karma");
            embed.AddField("Description", user.Description ?? "No Description", inline: false);

            return embed.Build();
        }

        public static async Task<bool> DoDailyAsync(this SkuldUser user, DatabaseService db, SkuldConfig config, SkuldUser sender = null)
        {
            if (sender != null)
            {
                if (sender.Daily != 0)
                {
                    if (sender.Daily < (DateTime.UtcNow.ToEpoch() - 86400))
                    {
                        sender.Daily = DateTime.UtcNow.ToEpoch();
                        await db.UpdateUserAsync(sender);
                        user.Money += config.Preferences.DailyAmount;
                        await db.UpdateUserAsync(user);
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
                    await db.UpdateUserAsync(sender);
                    user.Money += config.Preferences.DailyAmount;
                    await db.UpdateUserAsync(user);
                    return true;
                }
            }
            else
            {
                if (user.Daily != 0)
                {
                    if (user.Daily < (DateTime.UtcNow.ToEpoch() - 86400))
                    {
                        user.Daily = DateTime.UtcNow.ToEpoch();
                        user.Money += config.Preferences.DailyAmount;
                        await db.UpdateUserAsync(user);
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
                    await db.UpdateUserAsync(user);
                    return true;
                }
            }
        }
    }
}