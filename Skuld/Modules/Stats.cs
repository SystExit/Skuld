using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Skuld.Services;
using Skuld.Tools.Stats;
using Skuld.Extensions;
using Skuld.Utilities;

namespace Skuld.Modules
{
    [Group]
    public class Stats : ModuleBase<ShardedCommandContext>
    {
        public HardwareStats HStats { get; set; }
        public SoftwareStats SStats { get; set; }
        public MessageService MessageService { get; set; }

        [Command("ping"), Summary("Print Ping")]
        public async Task Ping() =>
            await Context.Channel.ReplyAsync("PONG: " + Context.Client.GetShardFor(Context.Guild).Latency + "ms");

        [Command("uptime"), Summary("Current Uptime")]
        public async Task Uptime() =>
            await Context.Channel.ReplyAsync($"Uptime: {string.Format("{0:dd} Days {0:hh} Hours {0:mm} Minutes {0:ss} Seconds", DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime))}");

        [Command("stats"), Summary("All stats")]
        public async Task StatsAll()
        {
            var currentuser = Context.Client.CurrentUser;

            var embed = new EmbedBuilder
            {
                Footer = new EmbedFooterBuilder { Text = "Generated" },
                Author = new EmbedAuthorBuilder { IconUrl = currentuser.GetAvatarUrl(), Name = currentuser.Username },
                ThumbnailUrl = currentuser.GetAvatarUrl(),
                Timestamp = DateTime.Now,
                Title = "Stats",
                Color = EmbedUtils.RandomColor()
            };

            string apiversions =
                "SysEx: " + SStats.SysEx.Version.ToString() + "\n" +
                "Booru: " + SStats.Booru.Version.ToString() + "\n" +
                "Weebsh: " + SStats.Weebsh.Version.ToString();

            string botstats =
                "Skuld: " + SStats.Skuld.Version.ToString() + "\n" +
                "Uptime: " + string.Format("{0:dd}d {0:hh}:{0:mm}", DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime)) + "\n" +
                "Ping: " + Context.Client.GetShardFor(Context.Guild).Latency + "ms\n" +
                "Guilds: " + Context.Client.Guilds.Count + "\n" +
                "Shards: " + Context.Client.Shards.Count + "\n" +
                "Commands: " + MessageService.commandService.Commands.Count();

            string systemstats =
                "CPU Used: " + HStats.CPU.GetCPUUsage + "%\n" +
                "Memory Used: " + HStats.Memory.GetMBUsage + "MB\n" +
                "Operating System: " + SStats.WindowsVersion;

            embed.AddField("Bot", botstats);
            embed.AddField("APIs", apiversions);
            embed.AddField("System", systemstats);

            await Context.Channel.ReplyAsync(embed.Build());
        }

        [Command("netfw"), Summary(".Net Info")]
        public async Task Netinfo() =>
            await Context.Channel.ReplyAsync($"{RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}");

        [Command("discord"), Summary("Discord Info")]
        public async Task Discnet() =>
            await Context.Channel.ReplyAsync($"Discord.Net Library Version: {DiscordConfig.Version}");

    }
}
