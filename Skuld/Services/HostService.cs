using Booru.Net;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Customsearch.v1;
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
        private static string logfile;

        public HostService()
        {
            EnsureConfigExists();
            logfile = Path.Combine(AppContext.BaseDirectory, "skuld", "logs", DateTime.Now.ToString("dd-MM-yyyy") + ".log");
            Configuration = SkuldConfig.Load();
        }

        public async Task CreateAsync()
        {
            await InstallServicesAsync();
            await InitializeServicesAsync();

            var messServ = Services.GetRequiredService<MessageService>();

            BotService = new BotService(Client, messServ.logger, messServ, Services.GetRequiredService<TwitchService>());
            BotService.AddConfg(Configuration);
            BotService.AddHostService(this);

            await Services.GetRequiredService<GenericLogger>().AddToLogsAsync(new Core.Models.LogMessage("Framework", "Loaded Skuld v" + Services.GetRequiredService<SoftwareStats>().Skuld.Version, LogSeverity.Info));

            await BotService.StartAsync();
        }

        private static async Task InstallServicesAsync()
        {
            var logger = new GenericLogger(true, true, logfile);
            try
            {
                Client = new DiscordShardedClient(new DiscordSocketConfig
                {
                    MessageCacheSize = 1000,
                    DefaultRetryMode = RetryMode.AlwaysRetry,
                    LogLevel = LogSeverity.Verbose,
                    TotalShards = Configuration.Discord.Shards
                });

                var locale = new Locale(logger);

                var database = new DatabaseService(logger, locale, Client, Configuration);

                //var weebprovider = new WeebClient("Skuld", Assembly.GetEntryAssembly().GetName().Version.ToString());

                var twitch = new TwitchService(new TwitchLogger(logger));

                var mess = new MessageService(Client, database, new DiscordLogger(database, Client, logger, Configuration), new Models.MessageServiceConfig
                {
                    ArgPos = 0,
                    Prefix = Configuration.Discord.Prefix,
                    AltPrefix = Configuration.Discord.AltPrefix
                });


                Services = new ServiceCollection()
                    .AddSingleton(logger)
                    .AddSingleton(new BaseClient(logger))
                    .AddSingleton(Configuration)
                    .AddSingleton<HardwareStats>()
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
                    .AddSingleton(new NASAClient(logger, Configuration.APIS.NASAApiKey))
                    .AddSingleton<NekosLifeClient>()
                    .AddSingleton<SocialAPIS>()
                    .AddSingleton(new Stands4Client(Configuration.APIS.STANDSUid, Configuration.APIS.STANDSToken, logger))
                    .AddSingleton<StrawPollClient>()
                    .AddSingleton<UrbanDictionaryClient>()
                    .AddSingleton<WebComicClients>()
                    .AddSingleton<WikipediaClient>()
                    .AddSingleton<YNWTFClient>()
                    .AddSingleton(new BotListingClient(logger, Client))
                    .AddSingleton(twitch)
                    //.AddSingleton(weebprovider)
                    .AddSingleton(mess)
                    .AddSingleton(new ExperienceService(Client, database, logger))
                    .AddSingleton(new CustomCommandService(Client, database, logger, mess))
                    .AddSingleton(new WebSocketServerService(Client, logger))
                    .AddSingleton(new SearchService(logger, Configuration))
                    .BuildServiceProvider();

                await logger.AddToLogsAsync(new Core.Models.LogMessage("Framework", "Successfully built service provider", LogSeverity.Info));
            }
            catch (Exception ex)
            {
                await logger.AddToLogsAsync(new Core.Models.LogMessage("Framework", ex.Message, LogSeverity.Critical, ex));
            }
        }

        private async Task InitializeServicesAsync()
        {
            var logger = Services.GetRequiredService<MessageService>().logger;

            Services.GetRequiredService<HardwareStats>();

            logger.AddBotLister(Services.GetRequiredService<BotListingClient>());

            Services.GetRequiredService<TwitchService>().CreateClient(new NTwitch.Rest.TwitchRestConfig
            {
                ClientId = Configuration.APIS.TwitchClientID,
                LogLevel = NTwitch.LogSeverity.Verbose
            });

            var db = Services.GetRequiredService<DatabaseService>();
            await db.CheckConnectionAsync();

            await Services.GetRequiredService<Locale>().InitialiseLocalesAsync();

            await Services.GetRequiredService<MessageService>().ConfigureAsync(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Verbose,
                //IgnoreExtraArgs = true
            }, Services);

            Services.GetRequiredService<SearchService>().BuildGoogleClient();

            ConfigureStatsCollector();
        }

        private void EnsureConfigExists()
        {
            try
            {
                if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skuld", "storage")))
                    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skuld", "storage"));

                if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skuld", "logs")))
                    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skuld", "logs"));

                string loc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skuld", "storage", "configuration.json");
                logfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "skuld", "logs") + "/" + String.Format("{0:dd-MM-yyyy}", DateTime.Now.Date) + ".log";

                if (!File.Exists(loc))
                {
                    var config = new SkuldConfig();
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