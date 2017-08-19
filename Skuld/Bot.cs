using System;
using System.Reflection;
using System.Threading.Tasks;
using Skuld.Tools;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;
using System.Collections.ObjectModel;
using NTwitch.Rest;

namespace Skuld
{
    public class Bot
    {
        /*START VARS*/
        public static ObservableCollection<Models.LogMessage> Logs = new ObservableCollection<Models.LogMessage>();
        public static DiscordShardedClient bot;
        public static CommandService commands;
        public static IServiceProvider map;
        public static string logfile;
        public static StreamWriter sw;
        public static Random random = new Random();
        public static string Prefix = null;
        public static TwitchRestClient NTwitchClient;
        public static FileSystemWatcher fsw;
        /*END VARS*/

        static void Main(string[] args) => CreateBot().GetAwaiter().GetResult();

        public static async Task CreateBot()
        {
            try
            {
                EnsureConfigExists();
                fsw = new FileSystemWatcher(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modules"))
                {
                    EnableRaisingEvents = true
                };
                sw = new StreamWriter(logfile, false, System.Text.Encoding.UTF8);
                Prefix = Config.Load().Prefix;
                fsw.Changed += Events.SkuldEvents.Fsw_Changed;
                Logs.CollectionChanged += Events.SkuldEvents.Logs_CollectionChanged;
                Logs.Add(new Models.LogMessage("FrameWk", $"Loaded {Assembly.GetEntryAssembly().GetName().Name} v{Assembly.GetEntryAssembly().GetName().Version}", LogSeverity.Info));
                bot = new DiscordShardedClient(new DiscordSocketConfig()
                {
                    MessageCacheSize = 1000,
                    LargeThreshold = 250,
                    AlwaysDownloadUsers = true,
                    DefaultRetryMode = RetryMode.RetryTimeouts,
                    LogLevel = LogSeverity.Info,
                    TotalShards = Config.Load().Shards
                });
                bot.Log += Events.SkuldEvents.Bot_Log;
                Events.DiscordEvents.RegisterEvents();
                commands = new CommandService(new CommandServiceConfig()
                {
                    CaseSensitiveCommands = false,
                    DefaultRunMode = RunMode.Async,
                    LogLevel = LogSeverity.Info
                });
                await Modules.ModuleHandler.LoadAll();
                await Modules.ModuleHandler.CountModulesCommands();
                await StartBot(Config.Load().Token);
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
                await APIS.Twitch.TwitchClient.CreateTwitchClient(Config.Load().TwitchToken, Config.Load().TwitchClientID);
                await bot.LoginAsync(TokenType.Bot, token);
                await bot.StartAsync();
                await Task.Delay(-1);
            }
            catch(Exception ex)
            {
                Logs.Add(new Models.LogMessage("Strt-Bot", "ERROR WITH THE BOT", LogSeverity.Error, ex));
                await StopBot("Init-Bt");
            }
        }

        public static async Task StopBot(string source)
        {
            Events.DiscordEvents.UnRegisterEvents();
            await bot.SetStatusAsync(UserStatus.Offline);
            Logs.Add(new Models.LogMessage(source, "Skuld is shutting down", Discord.LogSeverity.Info));
            await sw.WriteLineAsync("-------------------------------------------");
            sw.Close();
            await Console.Out.WriteLineAsync("Bot shutdown");
            Console.ReadLine();
            Environment.Exit(0);
        }

        public static void EnsureConfigExists()
        {
            try
            {
                if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage")))
                    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage"));
                if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modules")))
                    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modules"));
                if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs")))
                    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"));

                string loc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage/configuration.json");
                logfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs") + "/" + String.Format("{0:MM-dd-yy}", DateTime.Now.Date) + ".log";

                if (!File.Exists(loc))
                {
                    var config = new Config();
                    config.Save();
                    Console.WriteLine("The configuration file has been created at '" + AppDomain.CurrentDomain.BaseDirectory + "storage/configuration.json'");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
                if (!File.Exists(logfile))
                    File.Create(logfile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }
        }
    }
}
