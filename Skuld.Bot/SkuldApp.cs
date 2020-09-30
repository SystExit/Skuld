using Booru.Net;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using dotenv.net;
using IqdbApi;
using Microsoft.Extensions.DependencyInjection;
using Miki.API.Images;
using NodaTime;
using Octokit;
using Skuld.APIS;
using Skuld.Bot.Discord.Models;
using Skuld.Bot.Discord.TypeReaders;
using Skuld.Core;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Models;
using Skuld.Services.Bot.Discord;
using Skuld.Services.Globalization;
using Skuld.Services.Reminders;
using Skuld.Services.Twitch;
using Skuld.Services.Twitter;
using Skuld.Services.VoiceExperience;
using Skuld.Services.WebSocket;
using StatsdClient;
using SysEx.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Interfaces;
using DiscordNetCommands = Discord.Commands;
using Emoji = Discord.Emoji;

namespace Skuld.Bot
{
	public static class SkuldApp
	{
		public static DiscordShardedClient DiscordClient;
		public static CommandService CommandService;
		public static CommandServiceConfig CommandServiceConfig;
		public static MessageServiceConfig MessageServiceConfig;
		public static IServiceProvider Services;

		internal static SkuldConfig Configuration;

		public static WebSocketService WebSocket;

		private static void Main(string[] args)
			=> CreateAsync(args).GetAwaiter().GetResult();

		static void GeneratePixelQuery()
		{
			StringBuilder stb = new StringBuilder("INSERT INTO `placepixeldata` (XPos, YPos, R, G, B) VALUES ");

			for (int x = 1; x < SkuldAppContext.PLACEIMAGESIZE + 1; x++)
			{
				for (int y = 1; y < SkuldAppContext.PLACEIMAGESIZE + 1; y++)
				{
					stb.Append($"({x}, {y}, 0, 0, 0), ");
				}
			}

			string s = stb.ToString();

			string loc = Path.Combine(SkuldAppContext.BaseDirectory, "insert.sql");

			File.WriteAllText(loc, s[0..^2]);

			Console.WriteLine($"Done. Written to: {loc}");

			Console.ReadLine();

			Environment.Exit(0);

			return;
		}

		public static async Task ConfigureBotAsync(SkuldConfig inConfig,
			DiscordSocketConfig config,
			CommandServiceConfig cmdConfig,
			MessageServiceConfig msgConfig
		)
		{
			Configuration = inConfig;

			CommandServiceConfig = cmdConfig;

			MessageServiceConfig = msgConfig;

			DiscordClient = new DiscordShardedClient(config);

			InstallServices(Configuration);

			InitializeServices(Configuration);

			BotMessaging.Configure();

			var modules = await
				ConfigureCommandServiceAsync()
			.ConfigureAwait(false);

			modules
				.IsSuccess(x => Log.Info(
						"Framework",
						"Successfully built Command Tree"
					)
				)
				.IsError(x => Log.Error(
						"Framework",
						"Failure building Command Tree",
						null,
						x.Exception
					)
				);
		}

		public static async Task StartBotAsync()
		{
			BackgroundTasks();

			BotEvents.RegisterEvents();

			await DiscordClient.LoginAsync(TokenType.Bot, Configuration.DiscordToken).ConfigureAwait(false);

			await DiscordClient.StartAsync().ConfigureAwait(false);
		}

		public static async Task StopBotAsync(string source)
		{
			BotEvents.UnRegisterEvents();

			await DiscordClient.SetStatusAsync(UserStatus.Offline).ConfigureAwait(false);
			await DiscordClient.StopAsync().ConfigureAwait(false);
			await DiscordClient.LogoutAsync().ConfigureAwait(false);

			Log.Info(source, "Skuld is shutting down");
			DogStatsd.Event("FrameWork", $"Bot Stopped", alertType: "info", hostname: "Skuld");

			Log.FlushNewLine();

			await Console.Out.WriteLineAsync("Bot shutdown").ConfigureAwait(false);
			Console.ReadLine();
			Environment.Exit(0);
		}

		public static async Task CreateAsync(string[] args = null)
		{
			DotEnv.Config(filePath: Path.Combine(SkuldAppContext.BaseDirectory, ".env"));

			if (args.Contains("--pq"))
			{
				GeneratePixelQuery();
				return;
			}

			SkuldConfig Configuration = null;

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
				var database = new SkuldDbContextFactory().CreateDbContext();

				await database.ApplyPendingMigrations().ConfigureAwait(false);

				if (!database.Configurations.Any() ||
					args.Contains("--newconf") ||
					args.Contains("-nc"))
				{
					var conf = new SkuldConfig();
					database.Configurations.Add(conf);
					await database.SaveChangesAsync().ConfigureAwait(false);
					Log.Verbose("HostService", $"Created new configuration with Id: {conf.Id}", null);

					Log.Info("HostService", $"Please fill out the configuration information in the database matching the Id \"{database.Configurations.LastOrDefault().Id}\"");
					Console.ReadKey();
					Environment.Exit(0);
				}

				var configId = SkuldAppContext.GetEnvVar(SkuldAppContext.ConfigEnvVar);

				var c = database.Configurations.Find(configId);

				Configuration = c ?? database.Configurations.FirstOrDefault();

				SkuldAppContext.SetConfigurationId(Configuration.Id);
			}
			catch (Exception ex)
			{
				Log.Critical("HostService", ex.Message, null, ex);
			}

			await SkuldApp.ConfigureBotAsync(
				Configuration,
				new DiscordSocketConfig
				{
					MessageCacheSize = 100,
					DefaultRetryMode = RetryMode.AlwaysRetry,
					LogLevel = LogSeverity.Verbose,
					TotalShards = Configuration.Shards
				},
				new DiscordNetCommands.CommandServiceConfig
				{
					CaseSensitiveCommands = false,
					DefaultRunMode = DiscordNetCommands.RunMode.Async,
					LogLevel = LogSeverity.Verbose,
					IgnoreExtraArgs = true
				},
				new MessageServiceConfig
				{
					Prefix = Configuration.Prefix,
					AltPrefix = Configuration.AltPrefix
				}
			).ConfigureAwait(false);

			Log.Info("HostService", "Loaded Skuld v" + SkuldAppContext.Skuld.Key.Version);

			await SkuldApp.StartBotAsync().ConfigureAwait(false);

			await Task.Delay(-1).ConfigureAwait(false);

			SkuldApp.WebSocket.ShutdownServer();

			Environment.Exit(0);
		}


		#region Services

		internal static async Task<EventResult<IEnumerable<ModuleInfo>>> ConfigureCommandServiceAsync()
		{
			try
			{
				CommandService = new CommandService(CommandServiceConfig);

				CommandService.CommandExecuted += BotMessaging.CommandService_CommandExecuted;
				CommandService.Log += BotMessaging.CommandService_Log;

				CommandService.AddTypeReader<Uri>(new UriTypeReader());
				CommandService.AddTypeReader<Guid>(new GuidTypeReader());
				CommandService.AddTypeReader<Emoji>(new EmojiTypeReader());
				CommandService.AddTypeReader<Emote>(new EmoteTypeReader());
				CommandService.AddTypeReader<System.Drawing.Color>(new ColorTypeReader());
				CommandService.AddTypeReader<IPAddress>(new IPAddressTypeReader());
				CommandService.AddTypeReader<RoleConfig>(new RoleConfigTypeReader());
				CommandService.AddTypeReader<DateTimeZone>(new DateTimeZoneTypeReader());
				CommandService.AddTypeReader<GuildRoleConfig>(new GuildRoleConfigTypeReader());

				IEnumerable<ModuleInfo> modules = await
					CommandService.AddModulesAsync(
						Assembly.GetEntryAssembly(),
						Services
					)
				.ConfigureAwait(false);

				return EventResult<IEnumerable<ModuleInfo>>.FromSuccess(modules);
			}
			catch (Exception ex)
			{
				return EventResult<IEnumerable<ModuleInfo>>.FromFailureException(ex.Message, ex);
			}
		}

		private static void InstallServices(SkuldConfig Configuration)
		{
			try
			{
				var localServices = new ServiceCollection()
					.AddSingleton(Configuration)
					.AddSingleton<ISSClient>()
					.AddSingleton<SocialAPIS>()
					.AddSingleton<IqdbClient>()
					.AddSingleton<GiphyClient>()
					.AddSingleton<YNWTFClient>()
					.AddSingleton<SysExClient>()
					.AddSingleton<AnimalClient>()
					.AddSingleton<ImghoardClient>()
					.AddSingleton<NekosLifeClient>()
					.AddSingleton<WikipediaClient>()
					.AddSingleton<WebComicClients>()
					.AddSingleton<UrbanDictionaryClient>()
				#region Booru
					.AddSingleton<E621Client>()
					.AddSingleton<Rule34Client>()
					.AddSingleton<YandereClient>()
					.AddSingleton<KonaChanClient>()
					.AddSingleton<DanbooruClient>()
					.AddSingleton<GelbooruClient>()
					.AddSingleton<RealbooruClient>()
					.AddSingleton<SafebooruClient>()
				#endregion
					.AddSingleton(new Locale().InitialiseLocales());

				// Github
				{
					if (!string.IsNullOrEmpty(Configuration.GithubClientUsername) && !string.IsNullOrEmpty(Configuration.GithubClientPassword) && Configuration.GithubRepository != 0)
					{
						var github = new GitHubClient(new ProductHeaderValue("Skuld", SkuldAppContext.Skuld.Key.Version.ToString()));
						github.Connection.Credentials = new Credentials(Configuration.GithubClientUsername, Configuration.GithubClientPassword);

						localServices.AddSingleton(github);
					}
				}

				// NASA
				{
					if (!string.IsNullOrEmpty(Configuration.NASAApiKey))
					{
						localServices.AddSingleton(new NASAClient(Configuration.NASAApiKey));
					}
				}

				//Stands4Client
				{
					if (Configuration.STANDSUid != 0 && !string.IsNullOrEmpty(Configuration.STANDSToken))
					{
						localServices.AddSingleton(new Stands4Client(Configuration.STANDSUid, Configuration.STANDSToken));
					}
				}
				//Twitch
				{
					if (!string.IsNullOrEmpty(Configuration.TwitchClientID))
					{
						localServices.AddSingleton<ITwitchAPI>(new TwitchAPI(
							settings: new ApiSettings
							{
								ClientId = Configuration.TwitchClientID,
								AccessToken = Configuration.TwitchToken
							})
						);
					}
				}

				//Sentry
				{
					var sentryKey = Environment.GetEnvironmentVariable(SkuldAppContext.SentryIOEnvVar);

					Sentry.ISentryClient sentryClient = null;
					if (sentryKey != null)
					{
						Log.Info("HostService", "Sentry Key provided, enabling Sentry");
						sentryClient = new Sentry.SentryClient(new Sentry.SentryOptions
						{
							Dsn = new Sentry.Dsn(sentryKey)
						});
						localServices.AddSingleton(sentryClient);
					}
					else
					{
						Log.Info("HostService", "Sentry Key not provided, not using Sentry");
					}

					Log.Configure(sentryClient);
				}

				localServices.AddSingleton(new InteractiveService(DiscordClient, TimeSpan.FromSeconds(60)));

				Services = localServices.BuildServiceProvider();

				Log.Info("HostService", "Successfully built service provider");
			}
			catch (Exception ex)
			{
				Log.Critical("HostService", ex.Message, null, ex);
			}
		}

		private static void InitializeServices(SkuldConfig Configuration)
		{
			VoiceExpService.Configure(DiscordClient, Configuration);

			WebSocket = new WebSocketService(DiscordClient, Configuration);

			APIS.SearchClient.Configure(Configuration.GoogleAPI, Configuration.GoogleCx, Configuration.ImgurClientID, Configuration.ImgurClientSecret);

			ConfigureStatsCollector();
		}

		#endregion Services

		#region Statistics

		private static void ConfigureStatsCollector()
		{
			if (string.IsNullOrEmpty(Configuration.DataDogHost)) return;

			DogStatsd.Configure(new StatsdConfig
			{
				StatsdServerName = Configuration.DataDogHost,
				StatsdPort = Configuration.DataDogPort ?? 8125,
				Prefix = "skuld"
			});
		}

		private static Task SendDataToDataDog()
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			while (true)
			{
				DogStatsd.Gauge("shards.count", DiscordClient.Shards.Count);
				DogStatsd.Gauge("shards.connected", DiscordClient.Shards.Count(x => x.ConnectionState == ConnectionState.Connected));
				DogStatsd.Gauge("shards.disconnected", DiscordClient.Shards.Count(x => x.ConnectionState == ConnectionState.Disconnected));
				DogStatsd.Gauge("commands.count", CommandService.Commands.Count());
				if (DiscordClient.Shards.All(x => x.ConnectionState == ConnectionState.Connected))
				{
					DogStatsd.Gauge("guilds.total", DiscordClient.Guilds.Count);
				}
				Thread.Sleep(TimeSpan.FromSeconds(5));
			}
		}

		public static void BackgroundTasks()
		{
			new Thread(
				async () =>
				{
					Thread.CurrentThread.IsBackground = true;
					await SendDataToDataDog().ConfigureAwait(false);
				}
			).Start();

			new Thread(
				() =>
				{
					Thread.CurrentThread.IsBackground = true;
					ReminderService.Configure(DiscordClient);
					ReminderService.Run();
				}
			).Start();

			TwitterListener.Configure(DiscordClient);

			TwitterListener.Run();

			{
				var twitchAPI = Services.GetService<ITwitchAPI>();
				if (twitchAPI != null)
				{
					TwitchListener.Configure(DiscordClient);

					TwitchListener.Run(twitchAPI);
				}
			}
		}

		#endregion Statistics
	}
}