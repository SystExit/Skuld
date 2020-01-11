using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Core.Extensions.Discord
{
    public static class GuildExtensions
    {
        public static async Task<IReadOnlyList<IGuildUser>> GetAdministratorsAsync(this IGuild guild)
        {
            var users = new List<IGuildUser>();
            await guild.DownloadUsersAsync().ConfigureAwait(false);

            var usrs = await guild.GetUsersAsync();

            foreach (SocketGuildUser user in usrs)
            {
                if (user.Roles.Any(x => x.Permissions.Administrator))
                    users.Add(user);
            }

            return users;
        }

        public static async Task<IReadOnlyList<IGuildUser>> GetModeratorsAsync(this IGuild guild)
        {
            var users = new List<IGuildUser>();
            await guild.DownloadUsersAsync().ConfigureAwait(false);

            var usrs = await guild.GetUsersAsync().ConfigureAwait(false);

            foreach (SocketGuildUser user in usrs)
            {
                if (user.Roles.Any(x => x.Permissions.ManageMessages && x.Permissions.KickMembers && x.Permissions.ManageRoles))
                    users.Add(user);
            }

            return users;
        }
    }
}