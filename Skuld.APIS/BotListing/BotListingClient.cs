using Discord.WebSocket;
using Newtonsoft.Json;
using Skuld.APIS.BotListing.Models;
using Skuld.APIS.Utilities;
using Skuld.Core.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.APIS
{
    public class BotListingClient : BaseClient
    {
        private readonly RateLimiter rateLimiter;
        private readonly DiscordShardedClient client;

        public BotListingClient(GenericLogger log, DiscordShardedClient cli) : base(log)
        {
            client = cli;
            rateLimiter = new RateLimiter();
        }

        public async Task SendDataAsync(string sysextoken, string discordpwtoken, string dbltoken)
        {
            List<BotStats> botStats = new List<BotStats>();

            for (var x = 0; x < client.Shards.Count; x++)
            {
                botStats.Add(new BotStats
                {
                    ServerCount = client.GetShard(x).Guilds.Count,
                    ShardCount = client.Shards.Count,
                    ShardID = x
                });
                //dbl
                {
                    using (var webclient = new HttpClient())
                    using (var content = new StringContent(JsonConvert.SerializeObject(botStats[x]), Encoding.UTF8, "application/json"))
                    {
                        webclient.DefaultRequestHeaders.Add("UserAgent", UAGENT);
                        webclient.DefaultRequestHeaders.Add("Authorization", dbltoken);
                        await webclient.PostAsync(new Uri($"https://discordbots.org/api/bots/{client.CurrentUser.Id}/stats"), content);
                    }
                }
                //dpw
                {
                    using (var webclient = new HttpClient())
                    using (var content = new StringContent(JsonConvert.SerializeObject(botStats[x]), Encoding.UTF8, "application/json"))
                    {
                        webclient.DefaultRequestHeaders.Add("UserAgent", UAGENT);
                        webclient.DefaultRequestHeaders.Add("Authorization", discordpwtoken);
                        await webclient.PostAsync(new Uri($"https://bots.discord.pw/api/bots/{client.CurrentUser.Id}/stats"), content);
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(5).Milliseconds).ConfigureAwait(false);
            }

            //sysex
            {
                using (var webclient = new HttpClient())
                using (var content = new StringContent(JsonConvert.SerializeObject(botStats), Encoding.UTF8, "application/json"))
                {
                    webclient.DefaultRequestHeaders.Add("UserAgent", UAGENT);
                    webclient.DefaultRequestHeaders.Add("Authorization", sysextoken);
                    await webclient.PostAsync(new Uri($"https://skuld.systemexit.co.uk/tools/updateStats.php"), content).ConfigureAwait(false);
                }
            }
        }
    }
}