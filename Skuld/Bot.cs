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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using AIMLbot;
using Microsoft.Extensions.DependencyInjection;
using Discord.Addons.Interactive;
using System.Linq;
using System.Resources;

namespace Skuld
{
    public class Bot
    {
        /*START VARS*/
        public static ObservableCollection<Models.LogMessage> Logs = new ObservableCollection<Models.LogMessage>();
        public static DiscordShardedClient bot;
        public static CommandService commands;
        public static InteractiveService InteractiveCommands;
        public static IServiceProvider map;
        public static string logfile;
        public static StreamWriter sw;
        public static Random random = new Random();
        public static string Prefix = null;
        public static TwitchRestClient NTwitchClient;
        public static AIMLbot.Bot ChatService;
        public static User ChatUser;
        public static string PathToUserData;
		public static ResourceManager resourceManager;
		/*END VARS*/

        static void Main(string[] args) => CreateBot().GetAwaiter().GetResult();

        public static async Task CreateBot()
        {
            try
            {
                EnsureConfigExists();
				await InstallServices();
				Locale.InitialiseLocales();
				Logs.Add(new Models.LogMessage("FrameWk", $"Loaded: {Assembly.GetEntryAssembly().GetName().Name} v{Assembly.GetEntryAssembly().GetName().Version}", LogSeverity.Info));
				//await StartBot(Config.Load().Token);
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
               if(Config.Load().TwitchModule)
                    await APIS.Twitch.TwitchClient.CreateTwitchClient(Config.Load().TwitchToken, Config.Load().TwitchClientID);
                await bot.LoginAsync(TokenType.Bot, token);
                await bot.StartAsync();
                /*foreach (var shard in bot.Shards)
                    if(shard.ConnectionState == ConnectionState.Connected)
                        await PublishStats(shard.ShardId);*/
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
            Logs.Add(new Models.LogMessage(source, "Skuld is shutting down", LogSeverity.Info));
            await sw.WriteLineAsync("-------------------------------------------");
            sw.Close();
            await Console.Out.WriteLineAsync("Bot shutdown");
            Console.ReadLine();
            Environment.Exit(0);            
        }

        public static void InstallChatService()
        {
            try
            {
                string path = Path.Combine(AppContext.BaseDirectory, "config", "Settings.xml");
                if (File.Exists(path))
                {
                    ChatService = new AIMLbot.Bot()
                    {
                        AdminEmail = Config.Load().ChatServiceAdminEmail,
                        TrustAIML = true,
                        isAcceptingUserInput = false                        
                    };
                    ChatService.WrittenToLog += Events.SkuldEvents.ChatService_WrittenToLog;
                    ChatService.loadSettings(path);
                    ChatService.loadAIMLFromFiles();
                    ChatService.isAcceptingUserInput = true;
                    PathToUserData = Path.Combine(AppContext.BaseDirectory, "aimlusers");
                    Logs.Add(new Models.LogMessage("ChtSrvc","Loaded: Chat Service",LogSeverity.Info));
                }
                else { }
            }
            catch(Exception ex)
            {
                Logs.Add(new Models.LogMessage("ChtSrvc", "Error loading Chat Service", LogSeverity.Error,ex));
            }
        }

        public static async Task InstallServices()
        {
            sw = new StreamWriter(logfile, false, Encoding.UTF8);
            Prefix = Config.Load().Prefix;
            Logs.CollectionChanged += Events.SkuldEvents.Logs_CollectionChanged;

            bot = new DiscordShardedClient(new DiscordSocketConfig()
            {
                MessageCacheSize = 1000,
                LargeThreshold = 250,
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.RetryTimeouts,
                LogLevel = LogSeverity.Verbose,
                TotalShards = Config.Load().Shards
            });

            bot.Log += Events.SkuldEvents.Bot_Log;
            Events.DiscordEvents.RegisterEvents();

            commands = new CommandService(new CommandServiceConfig()
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Verbose,
                ThrowOnError = true
            });

            InteractiveCommands = new InteractiveService(bot,TimeSpan.FromSeconds(60));

            if (Config.Load().ChatServiceEnabled)
                InstallChatService();

            await commands.AddModulesAsync(Assembly.GetEntryAssembly());

            Logs.Add(new Models.LogMessage("CmdSrvc", $"Loaded {commands.Commands.Count()} Commands from {commands.Modules.Count()} Modules", LogSeverity.Info));

            map = new ServiceCollection()
                .AddSingleton(bot)
                .AddSingleton(commands)
                .AddSingleton(InteractiveCommands)
                .BuildServiceProvider();

        }

        public static async Task PublishStats(int shardid)
        {
            using (var webclient = new HttpClient())
            using (var content = new StringContent($"{{ \"server_count\": {bot.GetShard(shardid).Guilds.Count}, \"shard_id\": {shardid}, \"shard_count\": {bot.Shards.Count}}}", Encoding.UTF8, "application/json"))
            {
                webclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Config.Load().DBotsOrgKey);
                var response = await webclient.PostAsync(new Uri($"https://discordbots.org/api/bots/{Bot.bot.CurrentUser.Id}/stats"), content);
            }
            using (var webclient = new HttpClient())
            using (var content = new StringContent($"{{ \"server_count\": {bot.GetShard(shardid).Guilds.Count}, \"shard_id\": {shardid}, \"shard_count\": {bot.Shards.Count}}}", Encoding.UTF8, "application/json"))
            {
                webclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Config.Load().DiscordPWKey);
                var response = await webclient.PostAsync(new Uri($"https://bots.discord.pw/api/bots/{Bot.bot.CurrentUser.Id}/stats"), content);
            }
        }

        public static void EnsureConfigExists()
        {
            try
            {
                if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skuld", "storage")))
                    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skuld", "storage"));
                if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skuld", "logs")))
                    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skuld", "logs"));

                string loc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skuld", "storage", "configuration.json");
                logfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skuld", "logs") + "/" + String.Format("{0:MM-dd-yy}", DateTime.Now.Date) + ".log";

                if (!File.Exists(loc))
                {
                    var config = new Config();
                    config.Save();
                    Console.WriteLine("The configuration file has been created at '" + AppDomain.CurrentDomain.BaseDirectory + "\\skuld\\storage\\configuration.json'");
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
