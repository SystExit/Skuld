using Discord;
using Discord.WebSocket;
using Fleck;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Skuld.Core;
using Skuld.Core.Models;
using Skuld.Core.Utilities.Stats;
using Skuld.Bot.Models.WebSocket;
using Skuld.Database;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Skuld.Discord;

namespace Skuld.Bot.Services
{
    public class WebSocketService
    {
        public DiscordShardedClient Client { get => BotService.DiscordClient; }
        private readonly WebSocketServer _server;

        public WebSocketService()
        {
            _server = new WebSocketServer("ws://127.0.0.1:37821");
            _server.Start(x =>
            {
                x.OnMessage = async (message) => await HandleMessageAsync(x, message);
            });
            Console.Clear();
            GenericLogger.AddToLogsAsync(new Core.Models.LogMessage
            {
                Source = "WebSocketService - Ctr",
                Message = "New WebSocketServer started on: "+_server.Location,
                TimeStamp = DateTime.UtcNow,
                Severity = LogSeverity.Info
            }).GetAwaiter().GetResult();
        }

        public void ShutdownServer()
            => _server.Dispose();

        public async Task HandleMessageAsync(IWebSocketConnection conn, string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (message.StartsWith("user:"))
            {
                ulong.TryParse(message.Replace("user:", ""), out var userid);

                var usr = Client.GetUser(userid);
                if (usr != null)
                {
                    var wsuser = new WebSocketUser
                    {
                        Username = usr.Username,
                        Id = usr.Id,
                        Discriminator = usr.Discriminator,
                        UserIconUrl = usr.GetAvatarUrl() ?? usr.GetDefaultAvatarUrl(),
                        Status = usr.Status.ToString()
                    };

                    var res = EventResult.FromSuccess(wsuser);

                    var cnv = JsonConvert.SerializeObject(res);

                    await conn.Send(cnv);
                }
                else
                {
                    await conn.Send(JsonConvert.SerializeObject(EventResult.FromFailure("User not found")));
                }
            }
            if (message.StartsWith("guild:"))
            {
                ulong.TryParse(message.Replace("guild:", ""), out var guildid);

                var gld = Client.GetGuild(guildid);
                if (gld != null)
                {
                    var wsgld = new WebSocketGuild
                    {
                        Name = gld.Name,
                        GuildIconUrl = gld.IconUrl,
                        Id = gld.Id
                    };

                    var res = EventResult.FromSuccess(wsgld);

                    var cnv = JsonConvert.SerializeObject(res);

                    await conn.Send(cnv);
                }
                else
                {
                    await conn.Send(JsonConvert.SerializeObject(EventResult.FromFailure("Guild not found")));
                }
            }
            if (message.ToLower() == "stats" || message.ToLower() == "status")
            {
                var mem = "";

                if (HardwareStats.Memory.GetMBUsage > 1024)
                    mem = HardwareStats.Memory.GetGBUsage + "GB";
                else
                    mem = HardwareStats.Memory.GetMBUsage + "MB";

                var rawjson = $"{{\"Skuld\":\"{SoftwareStats.Skuld.Version.ToString()}\"," +
                    $"\"Uptime\":\"{string.Format("{0:dd}d {0:hh}:{0:mm}", DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime))}\"," +
                    $"\"Ping\":\"{Client.Latency}ms\"," +
                    $"\"Guilds\":{Client.Guilds.Count}," +
                    $"\"Users\":{BotService.Users}," +
                    $"\"Shards\":{Client.Shards.Count}," +
                    $"\"Commands\":{BotService.CommandService.Commands.Count()}," +
                    $"\"Memory Used\":\"{mem}\"}}";

                await conn.Send(JsonConvert.SerializeObject(rawjson));
            }
        }
    }
}