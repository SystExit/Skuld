using Discord;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Database.Extensions;
using System.Threading.Tasks;

namespace Skuld.Discord.Extensions
{
    public static class UserProfile
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
            /*if (user.FavCmd != null && user.FavCmdUsg != 0)
            {
                embed.AddField("Favourite Command", $"`{user.FavCmd}` and it has been used {user.FavCmdUsg} times", inline: true);
            }
            else
            {
                embed.AddField("Favourite Command", "No favourite Command", inline: true);
            }*/
            embed.AddField("Pasta Karma", $"{(await user.GetPastaKarma()).ToString("N0")} Karma");
            embed.AddField("Description", user.Description, inline: false);

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

            /*if (user.FavCmd != null && user.FavCmdUsg != 0)
            {
                embed.AddField("Favourite Command", $"`{user.FavCmd}` and it has been used {user.FavCmdUsg} times", inline: true);
            }
            else
            {
                embed.AddField("Favourite Command", "No favourite Command", inline: true);
            }*/

            embed.AddField("Pasta Karma", $"{(await user.GetPastaKarma()).ToString("N0")} Karma");
            embed.AddField("Description", user.Description, inline: false);

            return embed.Build();
        }

        public static string FullName (this IUser usr)
            => $"{usr.Username}#{usr.Discriminator}";
    }
}
