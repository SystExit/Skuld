using Discord;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Discord.Extensions
{
    public static class UserProfile
    {
        public static async Task<Embed> GetExtendedProfileAsync(this User user, IGuildUser guildUser, Guild guild, SkuldDatabaseContext dbContext)
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

            embed.AddField(guild.MoneyName, user.Money.ToString("N0") ?? "No Money", inline: true);
            embed.AddField("Daily", user.LastDaily != 0 ? user.LastDaily.FromEpoch().ToString("dd/MM/yyyy HH:mm:ss") : "Not used Daily", inline: true);
            embed.AddField("Pats", user.Pats > 0 ? user.Pats + " times" : "Not been patted", inline: true);
            embed.AddField("Patted", user.Patted > 0 ? user.Patted + " times" : "Not patted anyone", inline: true);
            embed.AddField("HP", user.HP > 0 ? user.HP + " HP" : "No HP", inline: true);

            var topcmd = dbContext.UserCommandUsage.OrderByDescending(x => x.Usage).FirstOrDefault(x => x.UserId == user.Id);

            embed.AddField("Favourite Command", topcmd != null ? $"`{topcmd.Command}` and it has been used {topcmd.Usage} times" : "No favourite Command", inline: true);

            //embed.AddField("Pasta Karma", $"{(await user.GetPastaKarmaAsync()).ToString("N0")} Karma");

            return embed.Build();
        }

        public static string FullName(this IUser usr)
            => $"{usr.Username}#{usr.Discriminator}";

        public static string FullNameWithNickname(this IGuildUser usr)
        {
            if (usr.Nickname == null)
                return usr.FullName();
            else
                return $"{usr.Username} ({usr.Nickname})#{usr.Discriminator}";
        }
    }
}