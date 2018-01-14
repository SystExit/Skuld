using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Skuld.Tools;
using System.Reflection;
using System.Threading;

namespace Skuld.Commands
{
    [Group("Stats"),Name("Stats")]
    public class Stats : ModuleBase
    {
        [Command("ping", RunMode = RunMode.Async), Summary("Print Ping")]
        public async Task Ping() =>
            await MessageHandler.SendChannel(Context.Channel, "PONG: " + Bot.bot.GetShardFor(Context.Guild).Latency + "ms");

        [Command("uptime", RunMode = RunMode.Async), Summary("Current Uptime")]
        public async Task Uptime()=>
            await MessageHandler.SendChannel(Context.Channel, $"Uptime: {string.Format("{0:dd} Days {0:hh} Hours {0:mm} Minutes {0:ss} Seconds", DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime))}");

        [Command("", RunMode = RunMode.Async), Summary("All stats")]
        public async Task StatsAll()
        {
            var currentuser = await Context.Guild.GetCurrentUserAsync();
            var embed = new EmbedBuilder
            {
                Footer = new EmbedFooterBuilder
                {
                    Text = "Generated"
                },
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = currentuser.GetAvatarUrl(),
                    Name = currentuser.Username
                },

                ThumbnailUrl = currentuser.GetAvatarUrl(),
                Timestamp = DateTime.Now,
                Title = "Stats",
                Color = Tools.Tools.RandomColor()
            };
            
            embed.AddField("Version",Assembly.GetEntryAssembly().GetName().Version.ToString(),inline:true);
            embed.AddField("Uptime",string.Format("{0:dd}d {0:hh}:{0:mm}", DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime)),inline:true);
            embed.AddField("Pong",Bot.bot.GetShardFor(Context.Guild).Latency + "ms",inline:true);
            embed.AddField("Guilds", Bot.bot.Guilds.Count().ToString(),inline:true);
            embed.AddField("Shards", Bot.bot.Shards.Count().ToString(),inline:true);

            var currProcess = Process.GetCurrentProcess();
            embed.AddField("Commands", Bot.commands.Commands.Count().ToString(),inline:true);
            embed.AddField("Memory Used", (currProcess.WorkingSet64 / 1024) / 1024 + "MB",inline:true);
            await MessageHandler.SendChannel(Context.Channel, "", embed.Build());
        }

        [Command("netfw", RunMode = RunMode.Async), Summary(".Net Info")]
        public async Task Netinfo() =>
            await MessageHandler.SendChannel(Context.Channel, $"{RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}");

        [Command("discord", RunMode = RunMode.Async), Summary("Discord Info")]
        public async Task Discnet() => 
            await MessageHandler.SendChannel(Context.Channel, $"Discord.Net Library Version: {DiscordConfig.Version}");
    }
}
