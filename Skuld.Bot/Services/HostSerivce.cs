using Akitaux.Twitch.Helix;
using Booru.Net;
using Discord;
using Discord.Addons.Interactive;
using Discord.WebSocket;
using Imgur.API.Authentication.Impl;
using IqdbApi;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Skuld.APIS;
using Skuld.Core;
using Skuld.Core.Generic.Models;
using Skuld.Core.Globalization;
using Skuld.Core.Utilities;
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
using DiscordNetCommands = Discord.Commands;

namespace Skuld.Bot.Services
{
    public class HostSerivce
    {
        public static SkuldConfig Configuration;
        public static WebSocketService WebSocket;
        public static IServiceProvider Services;

        public async Task CreateAsync()
        {
            try
            {
                EnsureConfigExists();
                Configuration = SkuldConfig.Load();

                BotService.ConfigureBot(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Debug,
                    GuildSubscriptions = false,
                    RateLimitPrecision = RateLimitPrecision.Millisecond,
                    UseSystemClock = true,
                    TotalShards = Configuration.Discord.Shards,
                    LargeThreshold = 250,
                    AlwaysDownloadUsers = false,
                    ExclusiveBulkDelete = true,
                    MessageCacheSize = 1000
                });

                await InstallServicesAsync().ConfigureAwait(false);

                InitializeStaticClients();

                await InitializeServicesAsync().ConfigureAwait(false);

                BotService.AddServices(Services);

                Log.Info("Framework", "Loaded Skuld v" + SkuldAppContext.Skuld.Key.Version);

                var result = await MessageHandler.ConfigureCommandServiceAsync(
                    new DiscordNetCommands.CommandServiceConfig
                    {
                        CaseSensitiveCommands = false,
                        DefaultRunMode = DiscordNetCommands.RunMode.Async,
                        LogLevel = LogSeverity.Verbose
                    }, 
                    new MessageServiceConfig
                    {
                        Prefix = Configuration.Discord.Prefix,
                        AltPrefix = Configuration.Discord.AltPrefix,
                        ArgPos = 0
                    },
                    Configuration,
                    Assembly.GetExecutingAssembly(),
                    Services
                ).ConfigureAwait(false);

                if (!result.Successful)
                {
                    Log.Critical("CmdServ-Configure", result.Error, result.Exception);
                }
                else
                {
                    Log.Info("CmdServ-Configure", "Configured Command Service");
                }

                await BotService.DiscordClient.LoginAsync(TokenType.Bot, Configuration.Discord.Token).ConfigureAwait(false);

                await BotService.DiscordClient.StartAsync().ConfigureAwait(false);

                WebSocket = new WebSocketService(Configuration);

                BotService.BackgroundTasks();

                MessageQueue.Run();

                await Task.Delay(-1).ConfigureAwait(false);

                WebSocket.ShutdownServer();

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
                Environment.Exit(0);
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
        }

        private static async Task InstallServicesAsync()
        {
            try
            {
                var random = new Random();
                var locale = new Locale();
                await locale.InitialiseLocalesAsync().ConfigureAwait(false);

                Services = new ServiceCollection()
                    .AddSingleton(locale)
                    .AddSingleton<Random>()
                    .AddSingleton<ISSClient>()
                    .AddSingleton<SocialAPIS>()
                    .AddSingleton<SteamStore>()
                    .AddSingleton<BaseClient>()
                    .AddSingleton<IqdbClient>()
                    .AddSingleton<BooruClient>()
                    .AddSingleton<GiphyClient>()
                    .AddSingleton<YNWTFClient>()
                    .AddSingleton<SysExClient>()
                    .AddSingleton<AnimalClient>()
                    .AddSingleton<YoutubeClient>()
                    .AddSingleton<NekosLifeClient>()
                    .AddSingleton<WikipediaClient>()
                    .AddSingleton<WebComicClients>()
                    .AddSingleton<UrbanDictionaryClient>()
                    .AddSingleton(new NASAClient(Configuration.APIS.NASAApiKey))
                    .AddSingleton(new BotListingClient(BotService.DiscordClient))
                    .AddSingleton(new GitHubClient(new ProductHeaderValue("Skuld")))
                    .AddSingleton(new InteractiveService(BotService.DiscordClient, TimeSpan.FromSeconds(60)))
                    .AddSingleton(new Stands4Client(Configuration.APIS.STANDSUid, Configuration.APIS.STANDSToken))
                    .AddSingleton(new ImgurClient(Configuration.APIS.ImgurClientID, Configuration.APIS.ImgurClientSecret))
                    .BuildServiceProvider();

                Log.Info("Framework", "Successfully built service provider");
            }
            catch (Exception ex)
            {
                Log.Critical("Framework", ex.Message, ex);
            }
        }

        private static async Task InitializeServicesAsync()
        {
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