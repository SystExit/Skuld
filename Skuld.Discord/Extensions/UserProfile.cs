using Discord;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Database.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Discord.Extensions
{
    public static class UserProfile
    {
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
            embed.AddField("Daily", user.Daily != 0 ? user.Daily.FromEpoch().ToString("dd/MM/yyyy HH:mm:ss") : "Not used Daily", inline: true);
            embed.AddField("Pats", user.Pats > 0 ? user.Pats + " times" : "Not been patted", inline: true);
            embed.AddField("Patted", user.Patted > 0 ? user.Patted + " times" : "Not patted anyone", inline: true);
            embed.AddField("HP", user.HP > 0 ? user.HP + " HP" : "No HP", inline: true);

            var topcmd = user.GetFavouriteCommand();
            embed.AddField("Favourite Command", topcmd != null ? $"`{topcmd.Command}` and it has been used {topcmd.Usage} times" : "No favourite Command", inline: true);

            embed.AddField("Pasta Karma", $"{(await user.GetPastaKarmaAsync()).ToString("N0")} Karma");

            return embed.Build();
        }

        public static string FullName (this IUser usr)
            => $"{usr.Username}#{usr.Discriminator}";
    }
}
