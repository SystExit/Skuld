using Discord.WebSocket;
using Newtonsoft.Json;
using Skuld.APIS.BotListing.Models;
using Skuld.APIS.Utilities;
using Skuld.Core;
using Skuld.Core.Generic.Models;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.APIS
{
    public static class BotListingClient
    {
        private static RateLimiter rateLimiter;

        private static void LogData(string key, string message, bool error)
        {
            if (error)
                Log.Error(key, message);
            else
                Log.Info(key, message);
        }

        public static async Task SendDataAsync(this DiscordShardedClient client, string discordggtoken, string dbltoken, string b4dtoken)
        {
            {
                using var Database = new SkuldDbContextFactory().CreateDbContext();

                var config = Database.Configurations.FirstOrDefault(x => x.Id == SkuldAppContext.ConfigurationId);

                if (config.IsDevelopmentBuild)
                    return;
            }

            if(rateLimiter == null)
            {
                rateLimiter = new RateLimiter();
            }

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
                        string key = "DBL";

                        webclient.DefaultRequestHeaders.Add("UserAgent", BaseClient.UAGENT);
                        webclient.DefaultRequestHeaders.Add("Authorization", dbltoken);
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        var _ = await webclient.PostAsync(new Uri($"https://discordbots.org/api/bots/" + client.CurrentUser.Id + "/stats"), content).ConfigureAwait(false);

                        LogData("BLC", _.IsSuccessStatusCode ? $"Successfully sent data to {key}" : $"Error sending data to {key} | " + _.ReasonPhrase, _.IsSuccessStatusCode);
                    }
                }
                //dbgg
                {
                    using (var webclient = new HttpClient())
                    using (var content = new StringContent(JsonConvert.SerializeObject((DBGGStats)botStats[x]), Encoding.UTF8, "application/json"))
                    {
                        string key = "D.B.GG";

                        webclient.DefaultRequestHeaders.Add("UserAgent", BaseClient.UAGENT);
                        webclient.DefaultRequestHeaders.Add("Authorization", discordggtoken);
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        var _ = await webclient.PostAsync(new Uri("https://discord.bots.gg/api/v1/bots/" + client.CurrentUser.Id + "/stats"), content).ConfigureAwait(false);

                        LogData("BLC", _.IsSuccessStatusCode ? $"Successfully sent data to {key}" : $"Error sending data to {key} | " + _.ReasonPhrase, _.IsSuccessStatusCode);
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(5).Milliseconds).ConfigureAwait(false);
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

                using (var webclient = new HttpClient())
                using (var content = new StringContent(JsonConvert.SerializeObject(stat), Encoding.UTF8, "application/json"))
                {
                    string key = "B4D";

                    webclient.DefaultRequestHeaders.Add("UserAgent", BaseClient.UAGENT);
                    webclient.DefaultRequestHeaders.Add("Authorization", b4dtoken);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var _ = await webclient.PostAsync(new Uri("https://botsfordiscord.com/api/bot/" + client.CurrentUser.Id), content).ConfigureAwait(false);

                    LogData("BLC", _.IsSuccessStatusCode ? $"Successfully sent data to {key}" : $"Error sending data to {key} | " + _.ReasonPhrase, _.IsSuccessStatusCode);
                }
            }
        }
    }
}