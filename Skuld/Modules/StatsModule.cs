using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Skuld.Services;
using System.Reflection;

namespace Skuld.Modules
{
    [Group, Name("Stats")]
    public class Stats : ModuleBase<ShardedCommandContext>
	{
		public MessageService MessageService { get; set; }		
		Process Process = Process.GetCurrentProcess();

        [Command("ping"), Summary("Print Ping")]
        public async Task Ping() =>
            await MessageService.SendChannelAsync(Context.Channel, "PONG: " + Context.Client.GetShardFor(Context.Guild).Latency + "ms");

        [Command("uptime"), Summary("Current Uptime")]
        public async Task Uptime()=>
            await MessageService.SendChannelAsync(Context.Channel, $"Uptime: {string.Format("{0:dd} Days {0:hh} Hours {0:mm} Minutes {0:ss} Seconds", DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime))}");

        [Command("stats"), Summary("All stats")]
        public async Task StatsAll()
		{
			var currentuser = Context.Client.CurrentUser;

            var embed = new EmbedBuilder
            {
                Footer = new EmbedFooterBuilder{Text = "Generated"},
                Author = new EmbedAuthorBuilder{IconUrl = currentuser.GetAvatarUrl(),Name = currentuser.Username},
                ThumbnailUrl = currentuser.GetAvatarUrl(),
                Timestamp = DateTime.Now,
                Title = "Stats",
                Color = Tools.Tools.RandomColor()
            };         
			
            embed.AddField("Version",Assembly.GetEntryAssembly().GetName().Version.ToString(),inline:true);
            embed.AddField("Uptime",string.Format("{0:dd}d {0:hh}:{0:mm}", DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime)),inline:true);
            embed.AddField("Pong",Context.Client.GetShardFor(Context.Guild).Latency + "ms",inline:true);
            embed.AddField("Guilds", Context.Client.Guilds.Count().ToString(),inline:true);
            embed.AddField("Shards", Context.Client.Shards.Count().ToString(),inline:true);
            embed.AddField("Commands", MessageService.commandService.Commands.Count().ToString(),inline:true);
            embed.AddField("Memory Used", (Process.WorkingSet64 / 1024) / 1024 + "MB",inline:true);
			embed.AddField("Operating System", Environment.OSVersion);

            await MessageService.SendChannelAsync(Context.Channel, "", embed.Build());
        }

        [Command("netfw"), Summary(".Net Info")]
        public async Task Netinfo() =>
            await MessageService.SendChannelAsync(Context.Channel, $"{RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}");

        [Command("discord"), Summary("Discord Info")]
        public async Task Discnet() => 
            await MessageService.SendChannelAsync(Context.Channel, $"Discord.Net Library Version: {DiscordConfig.Version}");
    }
}
