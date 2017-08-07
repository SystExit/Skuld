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
using Microsoft.VisualBasic.Devices;

namespace Skuld.Commands
{
    [Group("Stats"),Name("Stats")]
    public class Stats : ModuleBase
    {
        PerformanceCounter cpuCounter;
        PerformanceCounter ramCounter;

        [Command("ping", RunMode = RunMode.Async), Summary("Print Ping")]
        public async Task Ping()
        {
            await MessageHandler.SendChannel(Context.Channel, "PONG: " + Bot.bot.GetShardFor(Context.Guild).Latency.ToString() + "ms");
        }
        [Command("uptime", RunMode = RunMode.Async), Summary("Current Uptime")]
        public async Task Uptime()
        {
            await MessageHandler.SendChannel(Context.Channel, $"Uptime: {string.Format("{0:dd} Days {0:hh} Hours {0:mm} Minutes {0:ss} Seconds", DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime))}");
        }
        [Command("", RunMode = RunMode.Async), Summary("All stats")]
        public async Task AllStats()
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.Footer = new EmbedFooterBuilder()
            {
                Text = "Generated"
            };
            embed.Author = new EmbedAuthorBuilder()
            {
                IconUrl = (await Context.Guild.GetCurrentUserAsync()).GetAvatarUrl(),
                Name = (await Context.Guild.GetCurrentUserAsync()).Username
            };

            embed.ThumbnailUrl = (await Context.Guild.GetCurrentUserAsync()).GetAvatarUrl();
            embed.Timestamp = DateTime.Now;
            embed.Title = "Stats";
            embed.Color = RandColor.RandomColor();
            
            embed.AddInlineField("Version",Assembly.GetEntryAssembly().GetName().Version);
            embed.AddInlineField("Uptime",string.Format("{0:dd}d {0:hh}:{0:mm}", DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime)));
            embed.AddInlineField( "Pong",Bot.bot.GetShardFor(Context.Guild).Latency + "ms");
            embed.AddInlineField("Guilds", Bot.bot.Guilds.Count().ToString());
            embed.AddInlineField("Shards", Bot.bot.Shards.Count().ToString());

            var currProcess = Process.GetCurrentProcess();
            cpuCounter = new PerformanceCounter("Process", "% Processor Time", currProcess.ProcessName);
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");

            embed.AddInlineField("Commands", Bot.commands.Commands.Count());
            embed.AddInlineField("CPU Load", getCurrentCpuUsage());
            embed.AddInlineField("Memory Used", (currProcess.WorkingSet64 / 1024) / 1024 + "MB");

            await MessageHandler.SendChannel(Context.Channel, "", embed);
        }
        [Command("netfw", RunMode = RunMode.Async), Summary(".Net Info")]
        public async Task Netinfo()
        {
            await MessageHandler.SendChannel(Context.Channel, $"{RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}");
        }
        [Command("discord", RunMode = RunMode.Async), Summary("Discord Info")]
        public async Task Discnet()
        {
            await MessageHandler.SendChannel(Context.Channel, $"Discord.Net Library Version: {DiscordConfig.Version}");
        }
        [Command("system", RunMode = RunMode.Async), Summary("System load")]
        public async Task System()
        {
            EmbedFooterBuilder footer = new EmbedFooterBuilder();
            footer.Text = "Generated";

            EmbedBuilder embed = new EmbedBuilder();
            embed.Footer = footer;
            embed.ThumbnailUrl = (await Context.Guild.GetCurrentUserAsync()).GetAvatarUrl();
            embed.Timestamp = DateTime.Now;
            embed.Title = "System Load";
            embed.Color = RandColor.RandomColor();

            EmbedAuthorBuilder auth = new EmbedAuthorBuilder();
            auth.IconUrl = (await Context.Guild.GetCurrentUserAsync()).GetAvatarUrl();
            auth.Name = (await Context.Guild.GetCurrentUserAsync()).Username;
            embed.Author = auth;

            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            
            embed.AddInlineField("CPU Load", getCurrentCpuUsage());
            embed.AddInlineField("Total Free Ram", getAvailableRAM()); 
            embed.AddInlineField("Uptime", UpTime);
            await MessageHandler.SendChannel(Context.Channel, "", embed);
        }

        public string getCurrentCpuUsage()
        {
            cpuCounter.NextValue();
            Thread.Sleep(500);
            return String.Format("{0:0.00}%", cpuCounter.NextValue());
        }

        public string getAvailableRAM()
        {
            float value = ramCounter.NextValue();
            var totalinbytes = new ComputerInfo().TotalPhysicalMemory;
            double kbtotal = totalinbytes / 1024;
            double mbtotal = kbtotal / 1024;
            double gbtotal = kbtotal / (1024 * 1024);
            return String.Format("{0:0.00} GB / {1} GB\n{2} MB / {3} MB", (value / 1024), Math.Round(gbtotal,2), value, Math.Truncate(mbtotal));
        }
        public TimeSpan UpTime
        {
            get
            {
                using (var uptime = new PerformanceCounter("System", "System Up Time"))
                {
                    uptime.NextValue();
                    return TimeSpan.FromSeconds(uptime.NextValue());
                }
            }
        }
    }
}
