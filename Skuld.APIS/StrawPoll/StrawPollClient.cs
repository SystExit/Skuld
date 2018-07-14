using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Skuld.APIS.Utilities;
using Skuld.APIS.StrawPoll.Models;
using Skuld.Core.Services;

namespace Skuld.APIS
{
    public class StrawPollClient : BaseClient
    {
        private readonly GenericLogger logger;
        private readonly RateLimiter rateLimiter;

        public StrawPollClient(GenericLogger log) : base(log)
        {
            logger = log;
            rateLimiter = new RateLimiter();
        }

        public async Task<RecievePoll> SendPoll(string title, string[] polloptions)
        {
            try
            {
                if (rateLimiter.IsRatelimited()) return null;

                var sendpoll = new SendPoll
                    {
                        Title = title,
                        Options = polloptions,
                        Multi = false
                    };
                    var sendpollserialized = JsonConvert.SerializeObject(sendpoll);
                    var post = await PostStringAsync(new Uri("https://strawpoll.me/api/v2/polls"), new StringContent(sendpollserialized, Encoding.UTF8, "application/json"));
                    return JsonConvert.DeserializeObject<RecievePoll>(post);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Core.Models.LogMessage("StrwPoll", "Error: " + ex.Message, Discord.LogSeverity.Error, ex));
            }
            return null;
        }

        public async Task<RecievePoll> GetPoll(int pollid)
            => await GetPoll("https://strawpoll.me/api/v2/polls/" + pollid);

        public async Task<RecievePoll> GetPoll(string url)
        {
            try
            {
                if (rateLimiter.IsRatelimited()) return null;

                int pollid = 0;
                var urlarr = url.Split('/');
                for (int x = 0; x < urlarr.Length; x++)
                {
                    if (x > 0)
                    {
                        if (urlarr[x - 1] == "strawpoll.me" || urlarr[x - 1] == "www.strawpoll.me")
                        {
                            pollid = Convert.ToInt32(urlarr[x]);
                            break;
                        }
                    }
                }
                var result = await ReturnStringAsync(new Uri("https://strawpoll.me/api/v2/polls/" + pollid));
                return JsonConvert.DeserializeObject<RecievePoll>(result);
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Core.Models.LogMessage("StrwPoll", "Error: " + ex.Message, Discord.LogSeverity.Error, ex));
            }
            return null;
        }
    }
}