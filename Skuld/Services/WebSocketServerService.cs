using Discord.WebSocket;
using Fleck;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Skuld.Core.Services;
using Skuld.Core.Utilities.Stats;
using Skuld.Models;
using Skuld.Models.Database;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Services
{
    public class WebSocketServerService
    {
        public DiscordShardedClient Client { get; set; }
        public GenericLogger Logger { get; set; }
        private readonly WebSocketServer _server;

        public WebSocketServerService(DiscordShardedClient cli, GenericLogger log)
        {
            Client = cli;
            Logger = log;
            _server = new WebSocketServer("ws://127.0.0.1:37821");
            _server.Start(x =>
            {
                x.OnMessage = async (message) => await HandleMessageAsync(x, message);
            });
            Console.Clear();
            Logger.AddToLogsAsync(new Core.Models.LogMessage
            {
                Source = "WebSocketService - Ctr",
                Message = "New WebSocketServer started on: "+_server.Location,
                TimeStamp = DateTime.UtcNow,
                Severity = Discord.LogSeverity.Info
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

                    var res = new SqlResult
                    {
                        Successful = true,
                        Data = wsuser
                    };

                    var cnv = JsonConvert.SerializeObject(res);

                    await conn.Send(cnv);
                }
                else
                {
                    await conn.Send(JsonConvert.SerializeObject(new SqlResult
                    {
                        Successful = false,
                        Error = "User not found"
                    }));
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

                    var res = new SqlResult
                    {
                        Successful = true,
                        Data = wsgld
                    };

                    var cnv = JsonConvert.SerializeObject(res);

                    await conn.Send(cnv);
                }
                else
                {
                    await conn.Send(JsonConvert.SerializeObject(new SqlResult
                    {
                        Successful = false,
                        Error = "Guild not found"
                    }));
                }
            }
            if (message.ToLower() == "stats" || message.ToLower() == "status")
            {
                var sstats = HostService.Services.GetRequiredService<SoftwareStats>();
                var mem = "";

                if (HostService.HardwareStats.Memory.GetMBUsage > 1024)
                    mem = HostService.HardwareStats.Memory.GetGBUsage + "GB";
                else
                    mem = HostService.HardwareStats.Memory.GetMBUsage + "MB";

                var rawjson = $"{{\"Skuld\":\"{sstats.Skuld.Version.ToString()}\"," +
                    $"\"Uptime\":\"{string.Format("{0:dd}d {0:hh}:{0:mm}", DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime))}\"," +
                    $"\"Ping\":\"{Client.Latency}ms\"," +
                    $"\"Guilds\":{Client.Guilds.Count}," +
                    $"\"Users\":{HostService.BotService.Users}," +
                    $"\"Shards\":{Client.Shards.Count}," +
                    $"\"Commands\":{HostService.BotService.messageService.commandService.Commands.Count()}," +
                    $"\"Memory Used\":\"{mem}\"}}";

                await conn.Send(JsonConvert.SerializeObject(rawjson));
            }
        }
    }
}