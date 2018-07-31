using System;
using System.Collections.Generic;
using System.Text;
using Skuld.Core.Services;
using System.Net;
using System.Net.Sockets;
using Discord.WebSocket;
using Skuld.Core.Models;
using Fleck;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Skuld.Models;

namespace Skuld.Services
{
    public class WebSocketServerService
    {
        public DiscordShardedClient Client { get; set; }
        public GenericLogger Logger { get; set; }
        private WebSocketServer Server;

        public WebSocketServerService(DiscordShardedClient cli, GenericLogger log)
        {
            Client = cli;
            Logger = log;
            Server = new WebSocketServer("ws://127.0.0.1:37821");
            Server.Start(x =>
            {
                x.OnMessage = async (message) => await HandleMessageAsync(x, message);
            });
        }

        public async Task HandleMessageAsync(IWebSocketConnection conn, string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (message.StartsWith("user:"))
            {
                ulong userid = 0;
                ulong.TryParse(message.Replace("user:", ""), out userid);

                var usr = Client.GetUser(userid);

                var wsuser = new WebSocketUser
                {
                    Username = usr.Username,
                    Id = usr.Id,
                    Discriminator = usr.Discriminator,
                    UserIconUrl = usr.GetAvatarUrl() ?? usr.GetDefaultAvatarUrl()
                };

                var cnv = JsonConvert.SerializeObject(wsuser);

                await conn.Send(cnv);
            }
            if (message.StartsWith("guild:"))
            {
                ulong guildid = 0;
                ulong.TryParse(message.Replace("guild:", ""), out guildid);

                var gld = Client.GetGuild(guildid);

                var wsgld = new WebSocketGuild
                {
                    Name = gld.Name,
                    GuildIconUrl = gld.IconUrl
                };

                var cnv = JsonConvert.SerializeObject(wsgld);

                await conn.Send(cnv);
            }
        }
    }
}
