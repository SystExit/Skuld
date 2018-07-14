using System.Threading.Tasks;
using Skuld.Core.Extensions;
using Skuld.Core.Services;
using Skuld.Core.Models;

namespace Skuld.Services
{
    public class TwitchLogger
    {
        private GenericLogger logger;

        public TwitchLogger(GenericLogger log)
        {
            logger = log;
        }

        public async Task TwitchLog(NTwitch.LogMessage arg)
            => await logger.AddToLogsAsync(new LogMessage("TwitchClient - " + arg.Source, arg.Message, arg.Level.ToDiscord(), arg.Exception));
    }
}
