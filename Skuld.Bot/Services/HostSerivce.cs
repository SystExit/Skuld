﻿using Akitaux.Twitch.Helix;
using Booru.Net;
using Discord;
using Discord.Addons.Interactive;
using Discord.WebSocket;
using Imgur.API.Authentication.Impl;
using IqdbApi;
using Microsoft.Extensions.DependencyInjection;
using Miki.API.Images;
using Octokit;
using Skuld.APIS;
using Skuld.Bot.Globalization;
using Skuld.Core;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Discord;
using Skuld.Discord.Handlers;
using Skuld.Discord.Services;
using StatsdClient;
using SteamWebAPI2.Interfaces;
using SysEx.Net;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Voltaic;
using YoutubeExplode;
using DiscordNetCommands = Discord.Commands;

namespace Skuld.Bot.Services
{
    public static class HostSerivce
    {
        public static SkuldConfig Configuration;
        public static WebSocketService WebSocket;
        public static IServiceProvider Services;

        private static VoiceExpService voiceService;

        public static async Task CreateAsync(string[] args = null)
        {
            if (!Directory.Exists(SkuldAppContext.LogDirectory))
            {
                Directory.CreateDirectory(SkuldAppContext.LogDirectory);
            }
            if (!Directory.Exists(SkuldAppContext.StorageDirectory))
            {
                Directory.CreateDirectory(SkuldAppContext.StorageDirectory);
            }
            if (!Directory.Exists(SkuldAppContext.FontDirectory))
            {
                Directory.CreateDirectory(SkuldAppContext.FontDirectory);
            }
            try
            {
                {
                    var database = new SkuldDbContextFactory().CreateDbContext();

                    if (!database.Configurations.Any() || args.Contains("--newconf") || args.Contains("-nc"))
                    {
                        var conf = new SkuldConfig();
                        database.Configurations.Add(conf);
                        await database.SaveChangesAsync().ConfigureAwait(false);
                        Log.Verbose("HostService", $"Created new configuration with Id: {conf.Id}");
                    }

                    var configId = SkuldAppContext.GetEnvVar(SkuldAppContext.ConfigEnvVar);

                    var c = database.Configurations.FirstOrDefault(x => x.Id == configId);

                    Configuration = c ?? database.Configurations.FirstOrDefault();

                    SkuldAppContext.SetConfigurationId(Configuration.Id);

                    DiscordLogger.FeedConfiguration(Configuration);
                }

                BotService.ConfigureBot(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Debug,
                    TotalShards = Configuration.Shards,
                    LargeThreshold = 250,
                    AlwaysDownloadUsers = false,
                    ExclusiveBulkDelete = true,
                    MessageCacheSize = 1000
                });

                await InstallServicesAsync().ConfigureAwait(false);

                InitializeStaticClients();

                InitializeServices();

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
                        Prefix = Configuration.Prefix,
                        AltPrefix = Configuration.AltPrefix,
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

                await BotService.DiscordClient.LoginAsync(TokenType.Bot, Configuration.DiscordToken).ConfigureAwait(false);

                await BotService.DiscordClient.StartAsync().ConfigureAwait(false);

                WebSocket = new WebSocketService(Configuration);

                BotService.BackgroundTasks();

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
                TotalShards = Configuration.Shards
            });

            APIS.SearchClient.Configure(Configuration.GoogleAPI, Configuration.GoogleCx, Configuration.ImgurClientID, Configuration.ImgurClientSecret);
        }

        private static async Task InstallServicesAsync()
        {
            try
            {
                var random = new Random();
                var locale = new Locale();
                await locale.InitialiseLocalesAsync().ConfigureAwait(false);

                var github = new GitHubClient(new ProductHeaderValue("Skuld", SkuldAppContext.Skuld.Key.Version.ToString()));
                github.Connection.Credentials = new Credentials(Configuration.GithubClientUsername, Configuration.GithubClientPassword);

                Services = new ServiceCollection()
                    .AddSingleton(locale)
                    .AddSingleton(github)
                    .AddSingleton<Random>()
                    .AddSingleton<ISSClient>()
                    .AddSingleton<SocialAPIS>()
                    .AddSingleton<SteamStore>()
                    .AddSingleton<IqdbClient>()
                    .AddSingleton<BooruClient>()
                    .AddSingleton<GiphyClient>()
                    .AddSingleton<YNWTFClient>()
                    .AddSingleton<SysExClient>()
                    .AddSingleton<AnimalClient>()
                    .AddSingleton<YoutubeClient>()
                    .AddSingleton<ImghoardClient>()
                    .AddSingleton<NekosLifeClient>()
                    .AddSingleton<WikipediaClient>()
                    .AddSingleton<WebComicClients>()
                    .AddSingleton<UrbanDictionaryClient>()
                    .AddSingleton(new NASAClient(Configuration.NASAApiKey))
                    .AddSingleton(new Stands4Client(Configuration.STANDSUid, Configuration.STANDSToken))
                    .AddSingleton(new InteractiveService(BotService.DiscordClient, TimeSpan.FromSeconds(60)))
                    .AddSingleton(new ImgurClient(Configuration.ImgurClientID, Configuration.ImgurClientSecret))
                    .AddSingleton(new TwitchHelixClient
                    {
                        ClientId = new Utf8String(Configuration.TwitchClientID)
                    })
                    .BuildServiceProvider();

                Log.Info("Framework", "Successfully built service provider");
            }
            catch (Exception ex)
            {
                Log.Critical("Framework", ex.Message, ex);
            }
        }

        private static void InitializeServices()
        {
            voiceService = new VoiceExpService(BotService.DiscordClient);

            ConfigureStatsCollector();
        }

        private static void ConfigureStatsCollector()
        {
            DogStatsd.Configure(new StatsdConfig
            {
                StatsdServerName = Configuration.DataDogHost,
                StatsdPort = Configuration.DataDogPort ?? 8125,
                Prefix = "skuld"
            });
        }
    }
}