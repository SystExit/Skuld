using Discord;
using Discord.Commands;
using Skuld.Core.Utilities;
using Skuld.Core.Utilities.Stats;
using Skuld.Discord;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group]
    public class Stats : SkuldBase<SkuldCommandContext>
    {
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
            try
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
                    "SysEx: " + SoftwareStats.SysEx.Version.ToString() + "\n" +
                    "Booru: " + SoftwareStats.Booru.Version.ToString() + "\n" +
                    "Weebsh: " + SoftwareStats.Weebsh.Version.ToString();

                string botstats = "";
                    botstats += "Skuld: " + SoftwareStats.Skuld.Version.ToString() + "\n";
                    botstats += "Uptime: " + string.Format("{0:dd}d {0:hh}:{0:mm}", DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime)) + "\n";
                    botstats += "Ping: " + Context.Client.GetShardFor(Context.Guild).Latency + "ms\n";
                    botstats += "Guilds: " + Context.Client.Guilds.Count + "\n";
                    botstats += "Users: " + BotService.Users + "\n";
                    botstats += "Shards: " + Context.Client.Shards.Count + "\n";
                    botstats += "Commands: " + BotService.CommandService.Commands.Count();

                string systemstats =
                    "Memory Used: " + HardwareStats.Memory.GetMBUsage + "MB\n" +
                    "Operating System: " + SoftwareStats.WindowsVersion;

                embed.AddField("Bot", botstats);
                embed.AddField("APIs", apiversions);
                embed.AddField("System", systemstats);

                await ReplyAsync(Context.Channel, embed.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}