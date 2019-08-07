using Akitaux.Twitch.Helix;
using Booru.Net;
using Discord;
using Discord.Addons.Interactive;
using DiscordNetCommands = Discord.Commands;
using Discord.WebSocket;
using Imgur.API.Authentication.Impl;
using IqdbApi;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Skuld.APIS;
using Skuld.Core;
using Skuld.Core.Globalization;
using Skuld.Core.Models;
using Skuld.Core.Utilities.Stats;
using Skuld.Database;
using Skuld.Discord;
using Skuld.Discord.Handlers;
using Skuld.Discord.Services;
using StatsdClient;
using SteamWebAPI2.Interfaces;
using SysEx.Net;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Voltaic;
using YoutubeExplode;

namespace Skuld.Bot.Services
{
    public class HostSerivce
    {
        public static SkuldConfig Configuration;
        public static WebSocketService WebSocket;
        public static IServiceProvider Services;
        private static string logfile;

        public async Task CreateAsync()
        {
            try
            {
                EnsureConfigExists();
                Configuration = SkuldConfig.Load();
                logfile = Path.Combine(AppContext.BaseDirectory, "logs", DateTime.Now.ToString("dd-MM-yyyy") + ".log");

                GenericLogger.Configure(Configuration, true, true, logfile);

                InitializeStaticClients();

                await InstallServicesAsync();

                await InitializeServicesAsync();

                BotService.AddBotLister(Services.GetRequiredService<BotListingClient>());

                BotService.AddServices(Services);

                await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("Framework", "Loaded Skuld v" + SoftwareStats.Skuld.Key.Version, LogSeverity.Info));

                var result = await MessageHandler.ConfigureCommandServiceAsync(new DiscordNetCommands.CommandServiceConfig
                {
                    CaseSensitiveCommands = false,
                    DefaultRunMode = DiscordNetCommands.RunMode.Async,
                    LogLevel = LogSeverity.Verbose
                }, new MessageServiceConfig
                {
                    Prefix = Configuration.Discord.Prefix,
                    AltPrefix = Configuration.Discord.AltPrefix,
                    ArgPos = 0
                }, Configuration, Assembly.GetExecutingAssembly(), Services);

                if(!result.Successful)
                {
                    await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("CmdServ-Configure", result.Error, LogSeverity.Critical, result.Exception));
                }
                else
                {
                    await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("CmdServ-Configure", "Configured Command Service", LogSeverity.Info));
                }

                await BotService.DiscordClient.LoginAsync(TokenType.Bot, Configuration.Discord.Token);

                await BotService.DiscordClient.StartAsync();

                WebSocket = new WebSocketService(Configuration);

                BotService.BackgroundTasks();

                MessageQueue.Run();

                await Task.Delay(-1);

                WebSocket.ShutdownServer();

                Environment.Exit(0);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
                Environment.Exit(13);
            }
        }

        private static void InitializeStaticClients()
        {
            BotService.ConfigureBot(new DiscordSocketConfig
            {
                MessageCacheSize = 1000,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                LogLevel = LogSeverity.Verbose,
                TotalShards = Configuration.Discord.Shards
            });

            if (Configuration.Modules.TwitchModule)
            {
                BotService.TwitchClient = new TwitchHelixClient
                {
                    ClientId = new Utf8String(Configuration.APIS.TwitchClientID)
                };
            }

            APIS.SearchClient.Configure(Configuration);

            DatabaseClient.Initialize(Configuration);
        }

        private static async Task InstallServicesAsync()
        {
            try
            {
                var locale = new Locale();
                await locale.InitialiseLocalesAsync();

                //var weebprovider = new WeebClient("Skuld", Assembly.GetEntryAssembly().GetName().Version.ToString());

                Services = new ServiceCollection()
                    .AddSingleton<BaseClient>()
                    .AddSingleton(Configuration)
                    .AddSingleton(locale)
                    .AddSingleton(new InteractiveService(BotService.DiscordClient, TimeSpan.FromSeconds(60)))
                    .AddSingleton<YoutubeClient>()
                    .AddSingleton(new ImgurClient(Configuration.APIS.ImgurClientID, Configuration.APIS.ImgurClientSecret))
                    .AddSingleton<Random>()
                    .AddSingleton<SysExClient>()
                    .AddSingleton<AnimalClient>()
                    .AddSingleton<BooruClient>()
                    .AddSingleton<SteamStore>()
                    .AddSingleton<AnimalClient>()
                    .AddSingleton<GiphyClient>()
                    .AddSingleton(new NASAClient(Configuration.APIS.NASAApiKey))
                    .AddSingleton<NekosLifeClient>()
                    .AddSingleton<SocialAPIS>()
                    .AddSingleton(new Stands4Client(Configuration.APIS.STANDSUid, Configuration.APIS.STANDSToken))
                    .AddSingleton<StrawPollClient>()
                    .AddSingleton<UrbanDictionaryClient>()
                    .AddSingleton<WebComicClients>()
                    .AddSingleton<WikipediaClient>()
                    .AddSingleton<YNWTFClient>()
                    .AddSingleton<ISSClient>()
                    .AddSingleton<IqdbClient>()
                    .AddSingleton(new BotListingClient(BotService.DiscordClient))
                    .AddSingleton(new GitHubClient(new ProductHeaderValue("Skuld")))
                    .BuildServiceProvider();

                await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("Framework", "Successfully built service provider", LogSeverity.Info));
            }
            catch (Exception ex)
            {
                await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("Framework", ex.Message, LogSeverity.Critical, ex));
            }
        }

        private static async Task InitializeServicesAsync()
        {
            ConfigureStatsCollector();

            await DatabaseClient.CheckConnectionAsync();
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

        private static void ConfigureStatsCollector()
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
