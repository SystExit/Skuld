using Booru.Net;
using CachNet;
using CachNet.Entities;
using CachNet.Net;
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
using Skuld.API;
using Skuld.APIS;
using Skuld.Bot.Discord.Models;
using Skuld.Bot.Discord.TypeReaders;
using Skuld.Core;
using Skuld.Core.Extensions.Verification;
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
using Emoji = Discord.Emoji;

namespace Skuld.Bot
{
	public static class SkuldApp
	{
		const string Key = "SYSTEM";
		public static DiscordShardedClient DiscordClient;
		public static CommandService CommandService;
		public static CommandServiceConfig CommandServiceConfig;
		public static MessageServiceConfig MessageServiceConfig;
		public static IServiceProvider Services;
		public static WebSocketService WebSocket;
		internal static SkuldConfig Configuration;
		static ICachetClient cachetClient;

		public static async Task Main(string[] args = null)
		{
			string defaultEnv = Path.Combine(SkuldAppContext.BaseDirectory, ".env");
			if (!File.Exists(defaultEnv))
			{
				Console.WriteLine("Copy .env.default into .env and enter details");
				return;
			}

			DotEnv.Load(new DotEnvOptions(envFilePaths: new[] { defaultEnv }));

			if (args.Contains("--pq"))
			{
				GeneratePixelQuery();
				return;
			}

			SkuldConfig Configuration = null;

			Log.Configure();

			if (!Directory.Exists(SkuldAppContext.StorageDirectory))
			{
				Directory.CreateDirectory(SkuldAppContext.StorageDirectory);
				Log.Verbose(Key, "Created Storage Directory", null);
			}
			if (!Directory.Exists(SkuldAppContext.FontDirectory))
			{
				Directory.CreateDirectory(SkuldAppContext.FontDirectory);
				Log.Verbose(Key, "Created Font Directory", null);
			}

			try
			{
				var database = new SkuldDbContextFactory().CreateDbContext();

				if (!database.Configurations.Any() ||
					args.Contains("--newconf") ||
					args.Contains("-nc"))
				{
					var conf = new SkuldConfig();
					database.Configurations.Add(conf);
					await database.SaveChangesAsync().ConfigureAwait(false);
					Log.Verbose(Key, $"Created new configuration with Id: {conf.Id}", null);

					Log.Info(Key, $"Please fill out the configuration information in the database matching the Id \"{database.Configurations.LastOrDefault().Id}\"");
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
				Log.Critical(Key, ex.Message, null, ex);
			}

			if (Configuration.DiscordToken.IsNullOrWhiteSpace())
			{
				Log.Critical(Key, "You haven't provided a discord token, exiting", null);
				return;
			}

			await ConfigureBotAsync(
				Configuration,
				new DiscordSocketConfig
				{
					MessageCacheSize = 100,
					DefaultRetryMode = RetryMode.AlwaysRetry,
					LogLevel = LogSeverity.Verbose,
					GatewayIntents = GatewayIntents.Guilds |
						GatewayIntents.GuildMembers |
						GatewayIntents.GuildBans |
						GatewayIntents.GuildEmojis |
						GatewayIntents.GuildIntegrations |
						GatewayIntents.GuildWebhooks |
						GatewayIntents.GuildInvites |
						GatewayIntents.GuildVoiceStates |
						GatewayIntents.GuildMessages |
						GatewayIntents.GuildMessageReactions |
						GatewayIntents.DirectMessages |
						GatewayIntents.DirectMessageReactions
				},
				new CommandServiceConfig
				{
					CaseSensitiveCommands = false,
					DefaultRunMode = RunMode.Async,
					LogLevel = LogSeverity.Verbose,
					IgnoreExtraArgs = true
				},
				new MessageServiceConfig
				{
					Prefix = Configuration.Prefix,
					AltPrefix = Configuration.AltPrefix
				}
			).ConfigureAwait(false);

			Log.Info(Key, "Loaded Skuld v" + SkuldAppContext.Skuld.Key.Version);

			await StartBotAsync().ConfigureAwait(false);

			await Task.Delay(-1).ConfigureAwait(false);

			await StopBotAsync(Key);

			WebSocket.ShutdownServer();

			Environment.Exit(0);
		}

		static void GeneratePixelQuery()
		{
			StringBuilder stb = new("INSERT INTO `placepixeldata` (XPos, YPos, R, G, B) VALUES ");

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

			DiscordClient = new DiscordShardedClient();

			await DiscordClient.LoginAsync(TokenType.Bot, Configuration.DiscordToken);

			var recommendedShards = await DiscordClient.GetRecommendedShardCountAsync();
			Log.Verbose(Key, $"Discord recommends using: {recommendedShards}, using that", null);

			config.TotalShards = recommendedShards;

			await DiscordClient.LogoutAsync();

			DiscordClient = new DiscordShardedClient(config);

			InstallServices();

			InitializeServices();

			BotMessaging.Configure();

			var modules = await
				ConfigureCommandServiceAsync()
			.ConfigureAwait(false);

			modules
				.IsSuccess(x => Log.Info(
						Key,
						"Successfully built Command Tree"
					)
				)
				.IsError(x => Log.Error(
						Key,
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

		#region Services

		internal static async Task<EventResult<IEnumerable<ModuleInfo>>> ConfigureCommandServiceAsync()
		{
			IEnumerable<ModuleInfo> modules = null;
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

				modules = await
					CommandService.AddModulesAsync(
						Assembly.GetEntryAssembly(),
						Services
					)
				.ConfigureAwait(false);

				return EventResult<IEnumerable<ModuleInfo>>.FromSuccess(modules);
			}
			catch (Exception ex)
			{
				return EventResult<IEnumerable<ModuleInfo>>.FromFailureException(modules, ex.Message, ex);
			}
		}

		private static void InstallServices()
		{
			try
			{
				var localServices = new ServiceCollection()
					.AddSingleton(Configuration)
					.AddSingleton<ISSClient>()
					.AddSingleton<SocialAPIS>()
					.AddSingleton<IqdbClient>()
					.AddSingleton(new GiphyClient(Configuration.IsDevelopmentBuild ? "dc6zaTOxFJmzC" : null))
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
					if (sentryKey is not null)
					{
						Log.Info(Key, "Sentry Key provided, enabling Sentry");
						sentryClient = new Sentry.SentryClient(new Sentry.SentryOptions
						{
							Dsn = sentryKey
						});
						localServices.AddSingleton(sentryClient);
					}
					else
					{
						Log.Info(Key, "Sentry Key not provided, not using Sentry");
					}

					Log.ConfigureSentry(sentryClient);
				}

				//SkuldApi
				if (!Configuration.SkuldAPIBase.IsNullOrWhiteSpace() && !Configuration.SkuldAPIToken.IsNullOrWhiteSpace())
				{
					localServices.AddSingleton<ISkuldAPIClient>(new SkuldAPI(Configuration.SkuldAPIBase, Configuration.SkuldAPIToken));
				}
				else
				{
					localServices.AddSingleton<ISkuldAPIClient>(new SkuldAPI());
				}

				localServices.AddSingleton(new InteractiveService(DiscordClient, TimeSpan.FromSeconds(60)));

				Services = localServices.BuildServiceProvider();

				Log.Info(Key, "Successfully built service provider");
			}
			catch (Exception ex)
			{
				Log.Critical(Key, ex.Message, null, ex);
			}
		}

		private static void InitializeServices()
		{
			if (!Configuration.CachetBase.IsNullOrWhiteSpace() &&
				!Configuration.CachetToken.IsNullOrWhiteSpace() &&
				Configuration.CachetShardGroup != -1)
			{
				cachetClient = new CachetClient(Configuration.CachetBase, Configuration.CachetToken);
			}

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

		private static async Task HandleDataDog()
		{
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
				Log.Debug(Key, "Pushed to DogstatsD", null);
				Thread.Sleep(TimeSpan.FromSeconds(5));
			}
		}

		static bool didSyncComponents = false;
		private static async Task HandleCachet()
		{
			//wait for first connection
			while (true)
			{
				if (DiscordClient.Shards.All(shard => shard.ConnectionState == ConnectionState.Connected))
				{
					break;
				}

				Log.Verbose(Key, "Waiting for first connection, Cachet", null);
				Thread.Sleep(TimeSpan.FromSeconds(10));
			}

			while (true)
			{
				try
				{
					var components = await cachetClient.GetAllComponentsAsync();

					var shardComponents = components.Data.Where(c => c.Name.StartsWith("Shard #"));

					if (!didSyncComponents)
					{
						bool didModify = false;
						if (shardComponents.Count() > DiscordClient.Shards.Count)
						{
							for (int s = DiscordClient.Shards.Count; s < shardComponents.Count(); s++)
							{
								int trueShard = s + 1;

								var sComponent = shardComponents.FirstOrDefault(c => c.Name == $"Shard #{trueShard}");

								await cachetClient.DeleteComponentAsync(sComponent.Id);
							}
							didModify = true;
						}
						else if (shardComponents.Count() < DiscordClient.Shards.Count)
						{
							for (int s = shardComponents.Count(); s < DiscordClient.Shards.Count; s++)
							{
								int trueShard = s + 1;

								await cachetClient.AddComponentAsync(new()
								{
									Name = $"Shard #{trueShard}"
								});
							}

							didModify = true;
						}

						if (didModify)
						{
							components = await cachetClient.GetAllComponentsAsync();
						}

						didSyncComponents = true;
					}

					foreach (var component in components.Data)
					{
						if (component.Name.StartsWith("Shard #"))
						{
							foreach (var shard in DiscordClient.Shards)
							{
								if (component.GroupId == Configuration.CachetShardGroup &&
									component.Name.Equals($"Shard #{shard.ShardId + 1}"))
								{
									ComponentStatus status = ComponentStatus.Operational;

									switch (shard.ConnectionState)
									{
										case ConnectionState.Connected:
											status = ComponentStatus.Operational;
											break;
										case ConnectionState.Connecting:
											status = ComponentStatus.PerformanceIssues;
											break;
										case ConnectionState.Disconnecting:
											status = ComponentStatus.PartialOutage;
											break;
										case ConnectionState.Disconnected:
											status = ComponentStatus.MajorOutage;
											break;
									}

									await cachetClient.UpdateComponentAsync(component.Id, new()
									{
										Status = status
									});
								}
							}
						}
					}

					Log.Debug(Key, "Pushed to Cachet", null);
				}
				catch (Exception ex)
				{
					Log.Error(Key, ex.Message, null, ex);
				}
				Thread.Sleep(TimeSpan.FromMinutes(1));
			}
		}

		public static void BackgroundTasks()
		{
			new Thread(
				async () =>
				{
					Thread.CurrentThread.IsBackground = true;
					await HandleDataDog().ConfigureAwait(false);
					Log.Verbose(Key, "Started DogstatsD", null);
				}
			).Start();

			if (cachetClient is not null)
			{
				new Thread(
					async () =>
					{
						Thread.CurrentThread.IsBackground = true;
						await HandleCachet().ConfigureAwait(false);
						Log.Verbose(Key, "Started Cachet", null);
					}
				).Start();
			}

			new Thread(
				() =>
				{
					Thread.CurrentThread.IsBackground = true;
					ReminderService.Configure(DiscordClient);
					ReminderService.Run();
					Log.Verbose(Key, "Started Reminders", null);
				}
			).Start();

			TwitterListener.Configure(DiscordClient);

			TwitterListener.Run();
			Log.Verbose(Key, "Started Twitter", null);

			{
				var twitchAPI = Services.GetService<ITwitchAPI>();
				if (twitchAPI is not null)
				{
					TwitchListener.Configure(DiscordClient);

					TwitchListener.Run(twitchAPI);
					Log.Verbose(Key, "Started Twitch", null);
				}
			}
		}

		#endregion Statistics
	}
}