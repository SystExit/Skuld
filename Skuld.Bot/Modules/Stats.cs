using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Octokit;
using Skuld.Core;
using Skuld.Core.Utilities;
using Skuld.Core.Utilities.Stats;
using Skuld.Discord.Commands;
using Skuld.Discord.Extensions;
using Skuld.Discord.Preconditions;
using Skuld.Discord.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group, RequireEnabledModule]
    public class Stats : InteractiveBase<SkuldCommandContext>
    {
        public GitHubClient GitClient { get; set; }

        [Command("ping"), Summary("Print Ping")]
        public async Task Ping()
            => await $"PONG: {Context.Client.GetShardFor(Context.Guild).Latency}ms".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);

        [Command("uptime"), Summary("Current Uptime")]
        public async Task Uptime()
            => await $"Uptime: {string.Format("{0:dd} Days {0:hh} Hours {0:mm} Minutes {0:ss} Seconds", DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime))}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);

        [Command("netfw"), Summary(".Net Info")]
        public async Task Netinfo()
            => await $"{RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);

        [Command("discord"), Summary("Discord Info")]
        public async Task Discnet()
            => await $"Discord.Net Library Version: {DiscordConfig.Version}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);

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
                    $"[Booru: {SoftwareStats.Booru.Key.Version.ToString()}](https://github.com/{SoftwareStats.Booru.Value.Owner}/{SoftwareStats.Booru.Value.Repo})\n" +   
                    $"[SysEx: {SoftwareStats.SysEx.Key.Version.ToString()}](https://github.com/{SoftwareStats.SysEx.Value.Owner}/{SoftwareStats.SysEx.Value.Repo})\n" +
                    $"[Twitch: {SoftwareStats.Twitch.Key.Version.ToString()}](https://github.com/{SoftwareStats.Twitch.Value.Owner}/{SoftwareStats.Twitch.Value.Repo})\n" +
                    $"[Weebsh: {SoftwareStats.Weebsh.Key.Version.ToString()}](https://github.com/{SoftwareStats.Weebsh.Value.Owner}/{SoftwareStats.Weebsh.Value.Repo})";

                var commits = await GitClient.Repository.Commit.GetAll(SoftwareStats.Skuld.Value.Owner, SoftwareStats.Skuld.Value.Repo);

                string botstats = "";
                    botstats += $"[Skuld: {SoftwareStats.Skuld.Key.Version.ToString()}](https://github.com/{SoftwareStats.Skuld.Value.Owner}/{SoftwareStats.Skuld.Value.Repo})\n";
                    botstats += $"Uptime: {string.Format("{0:dd}d {0:hh}:{0:mm}", DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime))}\n";
                    botstats += $"Ping: {Context.Client.GetShardFor(Context.Guild).Latency}ms\n";
                    botstats += $"Guilds: {Context.Client.Guilds.Count}\n";
                    botstats += $"Users: {BotService.Users}\n";
                    botstats += $"Shards: {Context.Client.Shards.Count}\n";
                    botstats += $"Commands: {BotService.CommandService.Commands.Count()}\n";
                    botstats += $"Commits: {commits.Count}\n";
                    botstats += $"Most Recent Commit: [`{commits.First().Sha.Substring(0, 7)}`]({commits.First().HtmlUrl}) {commits.First().Commit.Message}";

                string systemstats =
                    "Memory Used: " + HardwareStats.Memory.GetMBUsage + "MB\n" +
                    "Operating System: " + SoftwareStats.WindowsVersion;

                embed.AddField("Bot", botstats);
                embed.AddField("APIs", apiversions);
                embed.AddField("System", systemstats);

                await embed.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            catch (Exception ex)
            {
                await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("Stats-Cmd", ex.Message, LogSeverity.Error, ex));
                await ex.Message.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            }
        }
    }
}