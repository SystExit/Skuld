using Discord.WebSocket;
using Newtonsoft.Json;
using Skuld.APIS.BotListing.Models;
using Skuld.APIS.Utilities;
using Skuld.Core;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.APIS
{
    public class BotListingClient : BaseClient
    {
        private readonly RateLimiter rateLimiter;
        private readonly DiscordShardedClient client;

        public BotListingClient(DiscordShardedClient cli) : base()
        {
            client = cli;
            rateLimiter = new RateLimiter();
        }

        public async Task SendDataAsync(string sysextoken, string discordggtoken, string dbltoken, string b4dtoken)
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
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        var _ = await webclient.PostAsync(new Uri($"https://discordbots.org/api/bots/"+client.CurrentUser.Id+"/stats"), content);
                        if (_.IsSuccessStatusCode)
                            await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("BLC", "Successfully sent data to DBL", Discord.LogSeverity.Info));
                        else
                            await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("BLC", "Error sending data to DBL | "+_.ReasonPhrase, Discord.LogSeverity.Error));
                    }
                }
                //dbgg
                {
                    using (var webclient = new HttpClient())
                    using (var content = new StringContent(JsonConvert.SerializeObject((DBGGStats)botStats[x]), Encoding.UTF8, "application/json"))
                    {
                        webclient.DefaultRequestHeaders.Add("UserAgent", UAGENT);
                        webclient.DefaultRequestHeaders.Add("Authorization", discordggtoken);
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        var _ = await webclient.PostAsync(new Uri("https://discord.bots.gg/api/v1/bots/" + client.CurrentUser.Id+"/stats"), content);
                        if (_.IsSuccessStatusCode)
                            await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("BLC", "Successfully sent data to D.B.GG", Discord.LogSeverity.Info));
                        else
                            await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("BLC", "Error sending data to D.B.GG | " + _.ReasonPhrase, Discord.LogSeverity.Error));
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
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var _ = await webclient.PostAsync(new Uri($"https://skuld.systemexit.co.uk/tools/updateStats.php"), content).ConfigureAwait(false);
                    if (_.IsSuccessStatusCode)
                        await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("BLC", "Successfully sent data to SysEx", Discord.LogSeverity.Info));
                    else
                        await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("BLC", "Error sending data to SysEx | " + _.ReasonPhrase, Discord.LogSeverity.Error));
                }
            }

            //b4d
            {
                int servercount = 0;
                foreach (var shard in botStats)
                {
                    servercount += shard.ServerCount;
                }

                var stat = new BotStats
                {
                    ServerCount = servercount
                };

                var serializedpayload = JsonConvert.SerializeObject(stat);

                using (var webclient = new HttpClient())
                using (var content = new StringContent(JsonConvert.SerializeObject(stat), Encoding.UTF8, "application/json"))
                {
                    webclient.DefaultRequestHeaders.Add("UserAgent", UAGENT);
                    webclient.DefaultRequestHeaders.Add("Authorization", b4dtoken);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var _ = await webclient.PostAsync(new Uri("https://botsfordiscord.com/api/bot/" + client.CurrentUser.Id), content).ConfigureAwait(false);
                    if (_.IsSuccessStatusCode)
                        await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("BLC", "Successfully sent data to B4D", Discord.LogSeverity.Info));
                    else
                        await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("BLC", "Error sending data to B4D | " + _.ReasonPhrase, Discord.LogSeverity.Error));
                }
            }
        }
    }
}