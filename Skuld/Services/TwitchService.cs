using System.Threading.Tasks;
using NTwitch;
using NTwitch.Rest;

namespace Skuld.Services
{
    public class TwitchService
    {
		TwitchRestClient client;
		LoggingService logger;

		public TwitchRestClient Client { get { return client; } }

		public TwitchService(LoggingService log) //depinj
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
    }
}
