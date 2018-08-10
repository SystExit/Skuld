using Skuld.Core.Services;
using Discord.WebSocket;
using Fleck;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Skuld.Models;
using Skuld.Models.Database;

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
                if(usr != null)
                {
                    var wsuser = new WebSocketUser
                    {
                        Username = usr.Username,
                        Id = usr.Id,
                        Discriminator = usr.Discriminator,
                        UserIconUrl = usr.GetAvatarUrl() ?? usr.GetDefaultAvatarUrl()
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
                if(gld != null)
                {
                    var wsgld = new WebSocketGuild
                    {
                        Name = gld.Name,
                        GuildIconUrl = gld.IconUrl
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
        }
    }
}
