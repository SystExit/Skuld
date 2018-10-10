using Discord;
using Discord.Commands;
using Skuld.Commands;
using Skuld.Core.Utilities.Stats;
using Skuld.Services;
using Skuld.Utilities.Discord;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Skuld.Modules
{
    [Group]
    public class Stats : SkuldBase<SkuldCommandContext>
    {
        public HardwareStats HStats { get => HostService.HardwareStats; }
        public SoftwareStats SStats { get; set; }

        [Command("ping"), Summary("Print Ping")]
        public async Task Ping()
            => await ReplyAsync(Context.Channel, $"PONG: {Context.Client.GetShardFor(Context.Guild).Latency}ms");

        [Command("uptime"), Summary("Current Uptime")]
        public async Task Uptime()
            => await ReplyAsync(Context.Channel, $"Uptime: {string.Format("{0:dd} Days {0:hh} Hours {0:mm} Minutes {0:ss} Seconds", DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime))}");

        [Command("netfw"), Summary(".Net Info")]
        public async Task Netinfo()
            => await ReplyAsync(Context.Channel, $"{RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}");

        [Command("discord"), Summary("Discord Info")]
        public async Task Discnet()
            => await ReplyAsync(Context.Channel, $"Discord.Net Library Version: {DiscordConfig.Version}");

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
                "Users: " + HostService.BotService.Users + "\n" +
                "Shards: " + Context.Client.Shards.Count + "\n" +
                "Commands: " + HostService.BotService.messageService.commandService.Commands.Count();

            string systemstats =
                "Memory Used: " + HStats.Memory.GetMBUsage + "MB\n" +
                "Operating System: " + SStats.WindowsVersion;

            embed.AddField("Bot", botstats);
            embed.AddField("APIs", apiversions);
            embed.AddField("System", systemstats);

            await ReplyAsync(Context.Channel, embed.Build());
        }
    }
}