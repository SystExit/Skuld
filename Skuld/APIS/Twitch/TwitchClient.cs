using System.Threading.Tasks;
using NTwitch;
using NTwitch.Rest;

namespace Skuld.APIS.Twitch
{
    public class TwitchClient : Bot
    {
        public static async Task CreateTwitchClient(string token, string clientid)
        {
            NTwitchClient = new TwitchRestClient(new TwitchRestConfig()
            {
                ClientId = clientid,
                LogLevel = LogSeverity.Verbose
            });
            NTwitchClient.Log += Events.SkuldEvents.NTwitchClient_Log;
            await NTwitchClient.LoginAsync(token);
        }
    }
}
