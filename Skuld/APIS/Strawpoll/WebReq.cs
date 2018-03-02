using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using Skuld.Models.API.Strawpoll;

namespace Skuld.APIS
{
    public partial class APIWebReq
    {
        public static async Task<RecievePoll> SendPoll(string title, string[] polloptions)
        {
            try
            {
                var sendpoll = new SendPoll(title, polloptions);
                var sendpollserialized = JsonConvert.SerializeObject(sendpoll);
                var post = await PostString(new Uri("https://strawpoll.me/api/v2/polls"), new StringContent(sendpollserialized, Encoding.UTF8, "application/json"));
                return JsonConvert.DeserializeObject<RecievePoll>(post);
            }
            catch (Exception ex)
            {
                await Bot.Logger.AddToLogs(new Models.LogMessage("StrwPoll", "Error: " + ex.Message, Discord.LogSeverity.Error, ex));
            }
            return null;
        }
        public static async Task<RecievePoll> GetPoll(int pollid)
        {
            try
            {
                var result = await ReturnString(new Uri("https://strawpoll.me/api/v2/polls/" + pollid));
                return JsonConvert.DeserializeObject<RecievePoll>(result);
            }
            catch (Exception ex)
            {
                await Bot.Logger.AddToLogs(new Models.LogMessage("StrwPoll", "Error: " + ex.Message, Discord.LogSeverity.Error, ex));
            }
            return null;
        }
        public static async Task<RecievePoll> GetPoll(string url)
        {
            try
            {
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
                var result = await ReturnString(new Uri("https://strawpoll.me/api/v2/polls/" + pollid));
                return JsonConvert.DeserializeObject<RecievePoll>(result);
            }
            catch (Exception ex)
            {
                await Bot.Logger.AddToLogs(new Models.LogMessage("StrwPoll", "Error: " + ex.Message, Discord.LogSeverity.Error, ex));
            }
            return null;
        }
    }
}
