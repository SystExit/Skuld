using System;
using System.Reflection;
using System.Threading.Tasks;
using Skuld.Tools;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;
using Skuld.APIS;
using Microsoft.Extensions.DependencyInjection;
using Discord.Addons.Interactive;
using YoutubeExplode;
using StatsdClient;
using Imgur.API.Authentication.Impl;
using Skuld.Services;

namespace Skuld
{
    public class Bot
    {
		/*START VARS*/
        public static IServiceProvider services;
        static string logfile;
        static string Prefix;
        public static Config Configuration;
        /*END VARS*/
		static void Main() => Create().GetAwaiter().GetResult();

        public static async Task Create()
        {
			try
            {
                EnsureConfigExists();
                Configuration = Config.Load();
				ConfigureStatsCollector();
				await InstallServices().ConfigureAwait(false);

				await services.GetRequiredService<LoggingService>().AddToLogsAsync(new Models.LogMessage("FrameWk", $"Loaded: {Assembly.GetEntryAssembly().GetName().Name} v{Assembly.GetEntryAssembly().GetName().Version}", LogSeverity.Info));
                DogStatsd.Event("FrameWork", $"Configured and Loaded: {Assembly.GetEntryAssembly().GetName().Name} v{Assembly.GetEntryAssembly().GetName().Version}", "info", hostname: "Skuld");

				await services.GetRequiredService<BotService>().StartAsync();
			}
            catch (Exception ex)
			{
				await services.GetRequiredService<BotService>().StopBot("MainBot");
				Console.WriteLine(ex);
                Console.ReadLine();
            }
        }
        
        static async Task InstallServices()
		{
			Prefix = Configuration.Discord.Prefix;

			var cli = new DiscordShardedClient(new DiscordSocketConfig
			{
				MessageCacheSize = 1000,
				DefaultRetryMode = RetryMode.RetryTimeouts,
				LogLevel = LogSeverity.Verbose,
				TotalShards = Configuration.Discord.Shards
			});
			
			services = new ServiceCollection()
				.AddSingleton(cli)
				.AddSingleton<BotService>()
				.AddSingleton(new InteractiveService(cli, TimeSpan.FromSeconds(60)))
				.AddSingleton(new LoggingService(true, true, logfile))
				.AddSingleton<DatabaseService>()
				.AddSingleton<YoutubeClient>()
				.AddSingleton(new ImgurClient(Configuration.APIS.ImgurClientID, Configuration.APIS.ImgurClientSecret))
				.AddSingleton<Random>()
				.AddSingleton<PokeSharpClient>()
				.AddSingleton<SysExClient>()
				.AddSingleton<NASAClient>()
				.AddSingleton<AnimalAPIS>()
				.AddSingleton<MALAPI>()
				.AddSingleton<YNWTF>()
				.AddSingleton<SocialAPIS>()
				.AddSingleton<TwitchService>()
				.AddSingleton<Strawpoll>()
				.AddSingleton<WebComicClients>()
				.AddSingleton<Locale>()
				.AddSingleton<MessageService>()
				.AddSingleton(new Utilities.MessageServiceConfig
				{
					ArgPos = 0,
					Prefix = Configuration.Discord.Prefix,
					AltPrefix = Configuration.Discord.AltPrefix
				})
				.BuildServiceProvider();

			await InitializeServicesAsync();
			
			services.GetRequiredService<BotService>().AddConfg(Configuration);
		}

		static async Task InitializeServicesAsync()
		{
			services.GetRequiredService<Random>();
			services.GetRequiredService<PokeSharpClient>();
			services.GetRequiredService<SysExClient>();
			services.GetRequiredService<NASAClient>();
			services.GetRequiredService<AnimalAPIS>();
			services.GetRequiredService<MALAPI>();
			services.GetRequiredService<YNWTF>();
			services.GetRequiredService<SocialAPIS>();
			services.GetRequiredService<Strawpoll>();
			await services.GetRequiredService<WebComicClients>().GetXKCDLastPageAsync();
			services.GetRequiredService<Locale>();
			
			var db = services.GetRequiredService<DatabaseService>();
			await db.CheckConnectionAsync();

			await services.GetRequiredService<Locale>().InitialiseLocalesAsync();

			var logger = services.GetRequiredService<LoggingService>();

			logger.Config(services.GetRequiredService<BotService>(),
				services.GetRequiredService<DiscordShardedClient>(),
				services.GetRequiredService<MessageService>(),
				services.GetRequiredService<DatabaseService>(),
				services.GetRequiredService<Random>());

			logger.RegisterEvents();

			await services.GetRequiredService<MessageService>().ConfigureAsync(new CommandServiceConfig
			{
				CaseSensitiveCommands = false,
				DefaultRunMode = RunMode.Async,
				LogLevel = LogSeverity.Verbose,
				IgnoreExtraArgs = true
			}, services);

			services.GetRequiredService<DiscordShardedClient>().Log += logger.DiscordLogger;
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
			DogStatsd.Configure(new StatsdConfig
			{
				StatsdServerName = Configuration.APIS.DataDogHost,
				StatsdPort = 8125,
				Prefix = "skuld"
			});
		}
	}
}
