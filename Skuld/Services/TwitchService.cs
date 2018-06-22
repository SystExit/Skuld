using System.Threading.Tasks;
using NTwitch.Rest;
using System.Linq;

namespace Skuld.Services
{
    public class TwitchService
    {
		public TwitchRestClient client { get; set; }
        readonly LoggingService logger;

        public TwitchService(LoggingService log)
        {
            logger = log;
        }

		public void CreateClient(TwitchRestConfig config)
		{
			client = new TwitchRestClient(config);
			client.Log += logger.TwitchLogger;
		}

		public async Task LoginAsync(string token)
		{
			await client.LoginAsync(token);
		}

        public async Task<RestChannel> GetUserAsync(string user)
        {
            var users = await client.GetUsersAsync(user);
            return await users.FirstOrDefault().GetChannelAsync();
        }
    }
}
