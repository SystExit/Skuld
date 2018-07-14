using NTwitch.Rest;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Services
{
    public class TwitchService
    {
        public TwitchRestClient Client { get; set; }
        private readonly TwitchLogger logger;

        public TwitchService(TwitchLogger log)
        {
            logger = log;
        }

        public void CreateClient(TwitchRestConfig config)
        {
            Client = new TwitchRestClient(config);
            Client.Log += logger.TwitchLog;
        }

        public async Task LoginAsync(string token)
        {
            await Client.LoginAsync(token);
        }

        public async Task<RestChannel> GetUserAsync(string user)
        {
            var users = await Client.GetUsersAsync(user);
            return await users.FirstOrDefault().GetChannelAsync();
        }
    }
}