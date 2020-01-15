using Discord;
using Discord.Commands;
using Octokit;
using Skuld.Core;
using Skuld.Core.Utilities;
using Skuld.Discord.Extensions;
using Skuld.Discord.Preconditions;
using Skuld.Discord.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group, RequireEnabledModule]
    public class Stats : ModuleBase<ShardedCommandContext>
    {
        public GitHubClient GitClient { get; set; }

        [Command("ping"), Summary("Print Ping")]
        public async Task Ping()
            => await $"PONG: {Context.Client.GetShardFor(Context.Guild).Latency}ms".QueueMessageAsync(Context).ConfigureAwait(false);

        [Command("uptime"), Summary("Current Uptime")]
        public async Task Uptime()
            => await $"Uptime: {string.Format("{0:dd} Days {0:hh} Hours {0:mm} Minutes {0:ss} Seconds", DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime))}".QueueMessageAsync(Context).ConfigureAwait(false);

        [Command("netfw"), Summary(".Net Info")]
        public async Task Netinfo()
            => await $"{RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}".QueueMessageAsync(Context).ConfigureAwait(false);

        [Command("discord"), Summary("Discord Info")]
        public async Task Discnet()
            => await $"Discord.Net Library Version: {DiscordConfig.Version}".QueueMessageAsync(Context).ConfigureAwait(false);

        [Command("stats"), Summary("All stats")]
        public async Task StatsAll()
        {
            try
            {
                var currentuser = Context.Client.CurrentUser;
                var avatar = currentuser.GetAvatarUrl() ?? currentuser.GetDefaultAvatarUrl();
                var color = Color.Teal;

                if (!Context.IsPrivate)
                    color = Context.Guild.GetUser(currentuser.Id).GetHighestRoleColor(Context.Guild);

                var embed = new EmbedBuilder()
                    .WithFooter("Generated")
                    .WithAuthor(currentuser.Username, "", SkuldAppContext.Website)
                    .WithThumbnailUrl(avatar)
                    .WithCurrentTimestamp()
                    .WithColor(color);

                string apiversions =
                    $"[Booru.net: {SkuldAppContext.Booru.Key.Version.ToString()}]({SkuldAppContext.Booru.Value.ToString()})\n" +
                    $"[Imghoard: {SkuldAppContext.Imghoard.Key.Version.ToString()}]({SkuldAppContext.Imghoard.Value.ToString()})\n" +
                    $"[SysEx.net: {SkuldAppContext.SysEx.Key.Version.ToString()}]({SkuldAppContext.SysEx.Value.ToString()})\n" +
                    $"[Twitch: {SkuldAppContext.Twitch.Key.Version.ToString()}]({SkuldAppContext.Twitch.Value.ToString()})\n" +
                    $"[Weebsh: {SkuldAppContext.Weebsh.Key.Version.ToString()}]({SkuldAppContext.Weebsh.Value.ToString()})";

                var commits = await GitClient.Repository.Commit.GetAll(SkuldAppContext.Skuld.Value.Owner, SkuldAppContext.Skuld.Value.Repo).ConfigureAwait(false);

                string botstats = "";
                botstats += $"[Skuld: {SkuldAppContext.Skuld.Key.Version.ToString()}]({SkuldAppContext.Skuld.Value.ToString()})\n";
                botstats += $"Uptime: {string.Format("{0:dd}d {0:hh}:{0:mm}", DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime))}\n";
                botstats += $"Ping: {Context.Client.GetShardFor(Context.Guild).Latency}ms\n";
                botstats += $"Guilds: {Context.Client.Guilds.Count}\n";
                botstats += $"Users: {BotService.Users}\n";
                botstats += $"Shards: {Context.Client.Shards.Count}\n";
                botstats += $"Commands: {BotService.CommandService.Commands.Count()}\n";
                botstats += $"Commits: {commits.Count}\n";
                botstats += $"Most Recent Commit: [`{commits.First().Sha.Substring(0, 7)}`]({commits.First().HtmlUrl}) {commits.First().Commit.Message}";

                string systemstats =
                    "Memory Used: " + SkuldAppContext.Memory.GetMBUsage + "MB\n" +
                    "Operating System: " + SkuldAppContext.WindowsVersion;

                embed.AddField("Bot", botstats);
                embed.AddField("APIs", apiversions);
                embed.AddField("System", systemstats);

                await embed.Build().QueueMessageAsync(Context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error("Stats-Cmd", ex.Message, ex);
                await EmbedExtensions.FromError(ex.Message, Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }
    }
}