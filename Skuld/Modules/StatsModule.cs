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
        PerformanceCounter CpuCounter;
        PerformanceCounter RamCounter;

        [Command("ping", RunMode = RunMode.Async), Summary("Print Ping")]
        public async Task Ping()
        {
            await MessageHandler.SendChannel(Context.Channel, "PONG: " + Bot.bot.GetShardFor(Context.Guild).Latency + "ms");
        }
        [Command("uptime", RunMode = RunMode.Async), Summary("Current Uptime")]
        public async Task Uptime()
        {
            await MessageHandler.SendChannel(Context.Channel, $"Uptime: {string.Format("{0:dd} Days {0:hh} Hours {0:mm} Minutes {0:ss} Seconds", DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime))}");
        }
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
            CpuCounter = new PerformanceCounter("Process", "% Processor Time", currProcess.ProcessName);
            RamCounter = new PerformanceCounter("Memory", "Available MBytes");

            embed.AddField("Commands", Bot.commands.Commands.Count().ToString(),inline:true);
            embed.AddField("CPU Load", GetCurrentCpuUsage(),inline:true);
            embed.AddField("Memory Used", (currProcess.WorkingSet64 / 1024) / 1024 + "MB",inline:true);
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
            var currentuser = await Context.Guild.GetCurrentUserAsync();
            var embed = new EmbedBuilder
            {
                Footer = new EmbedFooterBuilder { Text = "Generated" },
                ThumbnailUrl = currentuser.GetAvatarUrl(),
                Timestamp = DateTime.Now,
                Title = "System Load",
                Color = Tools.Tools.RandomColor(),
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = currentuser.GetAvatarUrl(),
                    Name = currentuser.Username
                }
            };

            CpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            RamCounter = new PerformanceCounter("Memory", "Available MBytes");
            
            embed.AddField("CPU Load", GetCurrentCpuUsage(),inline:true);
            embed.AddField("Total Free Ram", GetAvailableRAM(),inline:true); 
            embed.AddField("Uptime", $"{UpTime.Days} day(s) {UpTime.Hours}:{UpTime.Minutes}:{UpTime.Seconds}",inline:true);
            embed.AddField("OS", Environment.OSVersion+" "+RuntimeInformation.OSArchitecture,inline:true);
            await MessageHandler.SendChannel(Context.Channel, "", embed);
        }

        public string GetCurrentCpuUsage()
        {
            CpuCounter.NextValue();
            Thread.Sleep(500);
            return string.Format("{0:0.00}%", CpuCounter.NextValue());
        }

        public string GetAvailableRAM()
        {
            var value = RamCounter.NextValue();
            var totalinbytes = new ComputerInfo().TotalPhysicalMemory;
            double kbtotal = totalinbytes / 1024;
            var mbtotal = kbtotal / 1024;
            var gbtotal = kbtotal / (1024 * 1024);
            return string.Format("{0:0.00} GB / {1} GB\n{2} MB / {3} MB", (value / 1024), Math.Round(gbtotal,2), value, Math.Truncate(mbtotal));
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
