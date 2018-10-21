using NTwitch.Rest;
using Skuld.Core;
using Skuld.Core.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.APIS
{
    public static class TwitchClient
    {
        public static TwitchRestClient Client;

        public static async void ConfigureAndStartAsync(TwitchRestConfig config, string token)
        {
            Client = new TwitchRestClient(config);
            Client.Log += async (arg) =>
            {
                await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("TwitchClient", arg.Message, arg.Level.ToDiscord(), arg.Exception));
            };
            await Client.LoginAsync(token).ConfigureAwait(false);
        }

        public static async Task<RestChannel> GetUserAsync(string user)
        {
            var users = await Client.GetUsersAsync(user);
            return await users.FirstOrDefault().GetChannelAsync();
        }
    }
}
