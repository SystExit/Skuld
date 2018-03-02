using System;
using System.Reflection;
using System.Threading.Tasks;
using Skuld.Tools;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;
using NTwitch.Rest;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Discord.Addons.Interactive;
using System.Linq;
using YoutubeExplode;
using StatsdClient;
using Imgur.API.Authentication.Impl;

namespace Skuld
{
    public class Bot
    {
        /*START VARS*/
        public static DiscordShardedClient bot;
        public static CommandService commands;
        public static InteractiveService InteractiveCommands;
        public static DatabaseService Database;
        public static LoggingService Logger;
        public static YoutubeClient YouTubeClient;
        public static IServiceProvider services;
		public static ImgurClient imgurClient;
        public static string logfile;
        public static Random random = new Random();
        public static string Prefix;
        public static TwitchRestClient NTwitchClient;
        public static string PathToUserData;
        public static Config Configuration;
        /*END VARS*/

        static void Main() => CreateBot().GetAwaiter().GetResult();

        public static async Task CreateBot()
        {
            try
            {
                EnsureConfigExists();
                Configuration = Config.Load();
                await InstallServices().ConfigureAwait(false);
                ConfigureStatsCollector();
                Locale.InitialiseLocales();
				await Logger.AddToLogs(new Models.LogMessage("FrameWk", $"Loaded: {Assembly.GetEntryAssembly().GetName().Name} v{Assembly.GetEntryAssembly().GetName().Version}", LogSeverity.Info));
                DogStatsd.Event("FrameWork", $"Configured and Loaded: {Assembly.GetEntryAssembly().GetName().Name} v{Assembly.GetEntryAssembly().GetName().Version}", "info", hostname: "Skuld");
                await StartBot(Configuration.Token).ConfigureAwait(false);
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }
        }
        
        public static async Task StartBot(string token)
        {
            try
            {
                if (Configuration.TwitchModule)
                    await APIS.Twitch.TwitchClient.CreateTwitchClient(Configuration.TwitchToken, Configuration.TwitchClientID);
                await bot.LoginAsync(TokenType.Bot, token);
                await bot.StartAsync();
                //Parallel.Invoke(() => SendDataToDataDog());
                //foreach (var shard in bot.Shards)
                //{
                //    if (shard.ConnectionState == ConnectionState.Connected)
                //    { await PublishStats(shard.ShardId).ConfigureAwait(false); }
                //}
                await Task.Delay(-1).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                await Logger.AddToLogs(new Models.LogMessage("Strt-Bot", "ERROR WITH THE BOT", LogSeverity.Error, ex));
                DogStatsd.Event("FrameWork", $"Bot Crashed on start: {ex}", alertType: "error", hostname: "Skuld");
                await StopBot("Init-Bt").ConfigureAwait(false);
            }
        }

        public static async Task StopBot(string source)
        {
            Events.DiscordEvents.UnRegisterEvents();
            await bot.SetStatusAsync(UserStatus.Offline);
            await Logger.AddToLogs(new Models.LogMessage(source, "Skuld is shutting down", LogSeverity.Info));
            DogStatsd.Event("FrameWork", $"Bot Stopped", alertType: "info", hostname: "Skuld");
            await Logger.sw.WriteLineAsync("-------------------------------------------").ConfigureAwait(false);
            Logger.sw.Close();
            await Console.Out.WriteLineAsync("Bot shutdown").ConfigureAwait(false);
            Console.ReadLine();
            Environment.Exit(0);
        }
        
        static async Task InstallServices()
        {
            try
            {
                Logger = new LoggingService(true, true, logfile);
                Prefix = Configuration.Prefix;
                bot = new DiscordShardedClient(new DiscordSocketConfig
                {
                    MessageCacheSize = 1000,
                    DefaultRetryMode = RetryMode.RetryTimeouts,
                    LogLevel = LogSeverity.Verbose,
                    TotalShards = Configuration.Shards
                });
                bot.Log += Events.SkuldEvents.Bot_Log;
                Events.DiscordEvents.RegisterEvents();
                commands = new CommandService(new CommandServiceConfig
                {
                    CaseSensitiveCommands = false,
                    DefaultRunMode = RunMode.Async,
                    LogLevel = LogSeverity.Info
                });
                InteractiveCommands = new InteractiveService(bot, TimeSpan.FromSeconds(60));
                Database = new DatabaseService(bot);
                YouTubeClient = new YoutubeClient();
				imgurClient = new ImgurClient(Configuration.ImgurClientID, Configuration.ImgurClientSecret);
				services = new ServiceCollection()
					.AddSingleton(bot)
					.AddSingleton(commands)
					.AddSingleton(InteractiveCommands)
					.AddSingleton(Database)
					.AddSingleton(Logger)
					.AddSingleton(YouTubeClient)
					.AddSingleton(imgurClient)
                    .BuildServiceProvider();
                await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);                
                await Logger.AddToLogs(new Models.LogMessage("CmdSrvc", $"Loaded {commands.Commands.Count()} Commands from {commands.Modules.Count()} Modules", LogSeverity.Info));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static async Task PublishStats(int shardid)
        {
            using (var webclient = new HttpClient())
            using (var content = new StringContent($"{{ \"server_count\": {bot.GetShard(shardid).Guilds.Count}, \"shard_id\": {shardid}, \"shard_count\": {bot.Shards.Count}}}", Encoding.UTF8, "application/json"))
            {
                webclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Configuration.DBotsOrgKey);
                await webclient.PostAsync(new Uri($"https://discordbots.org/api/bots/{bot.CurrentUser.Id}/stats"), content);
            }
            using (var webclient = new HttpClient())
            using (var content = new StringContent($"{{ \"server_count\": {bot.GetShard(shardid).Guilds.Count}, \"shard_id\": {shardid}, \"shard_count\": {bot.Shards.Count}}}", Encoding.UTF8, "application/json"))
            {
                webclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Configuration.DiscordPWKey);
                await webclient.PostAsync(new Uri($"https://bots.discord.pw/api/bots/{bot.CurrentUser.Id}/stats"), content);
            }
        }

        public static void EnsureConfigExists()
        {
            try
            {
                if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skuld", "storage")))
                { Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skuld", "storage")); }
                if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skuld", "logs")))
                { Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skuld", "logs")); }

                string loc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skuld", "storage", "configuration.json");
                logfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skuld", "logs") + "/" + String.Format("{0:dd-MM-yyyy}", DateTime.Now.Date) + ".log";

                if (!File.Exists(loc))
                {
                    var config = new Config();
                    config.Save();
                    Console.WriteLine("The Configuration file has been created at '" + AppDomain.CurrentDomain.BaseDirectory + "/skuld/storage/configuration.json'");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }
        }

        static void ConfigureStatsCollector()
        {
            var dogstatsconfig = new StatsdConfig
            {
                StatsdTruncateIfTooLong = true,
                StatsdServerName = Configuration.DataDogHost,
                StatsdPort = 8125,
                Prefix = "skuld"
            };
            DogStatsd.Configure(dogstatsconfig);
        }

        Task SendDataToDataDog()
        {
            while (true)
            {
                int users = 0;
                foreach (var guild in bot.Guilds)
                { users += guild.MemberCount; }
                DogStatsd.Gauge("shards.count", bot.Shards.Count);
                DogStatsd.Gauge("shards.connected", bot.Shards.Count(x => x.ConnectionState == ConnectionState.Connected));
                DogStatsd.Gauge("shards.disconnected", bot.Shards.Count(x => x.ConnectionState == ConnectionState.Disconnected));
                DogStatsd.Gauge("commands.count", commands.Commands.Count());
                DogStatsd.Gauge("guilds.total", bot.Guilds.Count);
                DogStatsd.Gauge("users.total", users);
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }
    }
}
