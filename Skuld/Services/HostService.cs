using Booru.Net;
using Discord;
using Discord.Addons.Interactive;
using Discord.WebSocket;
using Imgur.API.Authentication.Impl;
using Microsoft.Extensions.DependencyInjection;
using PokeSharp;
using Skuld.APIS;
using Skuld.Core.Globalization;
using Skuld.Core.Models;
using Skuld.Core.Services;
using Skuld.Core.Utilities.Stats;
using StatsdClient;
using SteamWebAPI2.Interfaces;
using SysEx.Net;
using System;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode;

namespace Skuld.Services
{
    public class HostService
    {
        public static DiscordShardedClient Client;
        public static SkuldConfig Configuration;
        public static IServiceProvider Services;
        public static BotService BotService;
        public static HardwareStats HardwareStats;
        public static GenericLogger Logger;
        private static string logfile;

        public HostService()
        {
            EnsureConfigExists();
            logfile = Path.Combine(AppContext.BaseDirectory, "logs", DateTime.Now.ToString("dd-MM-yyyy") + ".log");
            Configuration = SkuldConfig.Load();
            HardwareStats = new HardwareStats();
            Logger = new GenericLogger(Configuration, true, true, logfile);
        }

        public async Task CreateAsync()
        {
            await InstallServicesAsync();
            await InitializeServicesAsync();

            BotService = new BotService(Client, Services.GetRequiredService<DatabaseService>(), /*Services.GetRequiredService<TwitchService>()*/ null);

            await Logger.AddToLogsAsync(new Core.Models.LogMessage("Framework", "Loaded Skuld v" + Services.GetRequiredService<SoftwareStats>().Skuld.Version, LogSeverity.Info));

            await BotService.StartAsync();
        }

        private static async Task InstallServicesAsync()
        {
            try
            {
                Client = new DiscordShardedClient(new DiscordSocketConfig
                {
                    MessageCacheSize = 1000,
                    DefaultRetryMode = RetryMode.AlwaysRetry,
                    LogLevel = LogSeverity.Verbose,
                    TotalShards = Configuration.Discord.Shards
                });

                var locale = new Locale(Logger);

                var database = new DatabaseService(Logger, locale, Client, Configuration);

                //var weebprovider = new WeebClient("Skuld", Assembly.GetEntryAssembly().GetName().Version.ToString());

                var twitch = new TwitchService(new TwitchLogger(Logger));

                Services = new ServiceCollection()
                    .AddSingleton(Logger)
                    .AddSingleton<BaseClient>()
                    .AddSingleton(Configuration)
                    .AddSingleton<SoftwareStats>()
                    .AddSingleton(locale)
                    .AddSingleton(new InteractiveService(Client, TimeSpan.FromSeconds(60)))
                    .AddSingleton(database)
                    .AddSingleton<YoutubeClient>()
                    .AddSingleton(new ImgurClient(Configuration.APIS.ImgurClientID, Configuration.APIS.ImgurClientSecret))
                    .AddSingleton<Random>()
                    .AddSingleton<SysExClient>()
                    .AddSingleton<AnimalClient>()
                    .AddSingleton<BooruClient>()
                    .AddSingleton<PokeSharpClient>()
                    .AddSingleton<SteamStore>()
                    .AddSingleton<AnimalClient>()
                    .AddSingleton<GiphyClient>()
                    .AddSingleton(new NASAClient(Logger, Configuration.APIS.NASAApiKey))
                    .AddSingleton<NekosLifeClient>()
                    .AddSingleton<SocialAPIS>()
                    .AddSingleton(new Stands4Client(Configuration.APIS.STANDSUid, Configuration.APIS.STANDSToken, Logger))
                    .AddSingleton<StrawPollClient>()
                    .AddSingleton<UrbanDictionaryClient>()
                    .AddSingleton<WebComicClients>()
                    .AddSingleton<WikipediaClient>()
                    .AddSingleton<YNWTFClient>()
                    .AddSingleton(new BotListingClient(Logger, Client))
                    .AddSingleton(twitch)
                    //.AddSingleton(weebprovider)
                    .AddSingleton(new WebSocketServerService(Client, Logger))
                    .AddSingleton(new SearchService(Logger, Configuration))
                    .BuildServiceProvider();

                await Logger.AddToLogsAsync(new Core.Models.LogMessage("Framework", "Successfully built service provider", LogSeverity.Info));
            }
            catch (Exception ex)
            {
                await Logger.AddToLogsAsync(new Core.Models.LogMessage("Framework", ex.Message, LogSeverity.Critical, ex));
            }
        }

        private async Task InitializeServicesAsync()
        {
            Services.GetRequiredService<TwitchService>().CreateClient(new NTwitch.Rest.TwitchRestConfig
            {
                ClientId = Configuration.APIS.TwitchClientID,
                LogLevel = NTwitch.LogSeverity.Verbose
            });

            var db = Services.GetRequiredService<DatabaseService>();
            await db.CheckConnectionAsync();

            await Services.GetRequiredService<Locale>().InitialiseLocalesAsync();

            Services.GetRequiredService<SearchService>().BuildGoogleClient();

            ConfigureStatsCollector();
        }

        private void EnsureConfigExists()
        {
            try
            {
                if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage")))
                    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage"));

                if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs")))
                    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"));

                string loc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configuration.json");
                logfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs") + "/" + String.Format("{0:dd-MM-yyyy}", DateTime.Now.Date) + ".log";

                if (!File.Exists(loc))
                {
                    var config = new SkuldConfig();
                    config.Save();
                    Console.WriteLine("The Configuration file has been created at '" + loc + "'");
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

        private void ConfigureStatsCollector()
        {
            DogStatsd.Configure(new StatsdConfig
            {
                StatsdServerName = Configuration.APIS.DataDogHost,
                StatsdPort = Configuration.APIS.DataDogPort ?? 8125,
                Prefix = "skuld"
            });
        }
    }
}