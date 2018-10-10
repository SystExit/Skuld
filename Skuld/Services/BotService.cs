using Discord;
using Discord.Commands;
using Discord.WebSocket;
using StatsdClient;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Skuld.Services
{
    public class BotService
    {
        private DiscordShardedClient client;
        private TwitchService twitch;
        private DiscordLogger logger;
        public MessageService messageService;
        private CustomCommandService customCommandService;
        private ExperienceService experienceService;
        public int Users { get; private set; }

        public BotService(DiscordShardedClient shardclient, DatabaseService db, TwitchService twit)
        {
            client = shardclient;
            twitch = twit;
            Users = 0;
            logger = new DiscordLogger(db, client);
            messageService = new MessageService(client, db, new Models.MessageServiceConfig
            {
                ArgPos = 0,
                Prefix = HostService.Configuration.Discord.Prefix,
                AltPrefix = HostService.Configuration.Discord.AltPrefix
            });
            customCommandService = new CustomCommandService(client, messageService.config, db);
            experienceService = new ExperienceService(client, db);
        }

        public async Task StartAsync()
        {
            try
            {
                await messageService.ConfigureAsync(new CommandServiceConfig
                {
                    CaseSensitiveCommands = false,
                    DefaultRunMode = RunMode.Async,
                    LogLevel = LogSeverity.Verbose,
                    IgnoreExtraArgs = true
                });

                if (HostService.Configuration.Modules.TwitchModule)
                {
                    await twitch.LoginAsync(HostService.Configuration.APIS.TwitchToken).ConfigureAwait(false);
                }

                //logger.AddBotLister(HostService.Services.GetRequiredService<BotListingClient>());
                logger.AddBotService(this);
                logger.RegisterEvents();

                await client.LoginAsync(TokenType.Bot, HostService.Configuration.Discord.Token);
                await client.StartAsync();

                BackgroundTasks();

                await Task.Delay(-1).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await HostService.Logger.AddToLogsAsync(new Core.Models.LogMessage("BotService", "ERROR WITH THE BOT", LogSeverity.Error, ex));
                DogStatsd.Event("FrameWork", $"Bot Crashed on start: {ex}", alertType: "error", hostname: "Skuld");
                await StopBotAsync("Init-Bot").ConfigureAwait(false);
            }
        }

        public async Task StopBotAsync(string source)
        {
            logger.UnRegisterEvents();

            await client.SetStatusAsync(UserStatus.Offline);
            await HostService.Logger.AddToLogsAsync(new Core.Models.LogMessage(source, "Skuld is shutting down", LogSeverity.Info));
            DogStatsd.Event("FrameWork", $"Bot Stopped", alertType: "info", hostname: "Skuld");

            await HostService.Logger.sw.WriteLineAsync("-------------------------------------------").ConfigureAwait(false);
            HostService.Logger.sw.Close();

            await Console.Out.WriteLineAsync("Bot shutdown").ConfigureAwait(false);
            Console.ReadLine();
            Environment.Exit(0);
        }

        private void BackgroundTasks()
        {
            new Thread(
                async () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    await SendDataToDataDog().ConfigureAwait(false);
                }
            ).Start();
            new Thread(
                async () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    await FeedUsersAsync().ConfigureAwait(false);
                }
            ).Start();
        }

        private Task SendDataToDataDog()
        {
            while (true)
            {
                DogStatsd.Gauge("shards.count", client.Shards.Count);
                DogStatsd.Gauge("shards.connected", client.Shards.Count(x => x.ConnectionState == ConnectionState.Connected));
                DogStatsd.Gauge("shards.disconnected", client.Shards.Count(x => x.ConnectionState == ConnectionState.Disconnected));
                DogStatsd.Gauge("commands.count", messageService.commandService.Commands.Count());
                DogStatsd.Gauge("guilds.total", client.Guilds.Count);
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }

        private async Task FeedUsersAsync()
        {
            while (true)
            {
                if (client.Shards.All(x => x.ConnectionState == ConnectionState.Connected))
                {
                    Users = 0;
                    await client.DownloadUsersAsync(client.Guilds);
                    foreach (var gld in client.Guilds)
                    {
                        Users += gld.Users.Count;
                    }
                    await Task.Delay(TimeSpan.FromHours(1));
                }
            }
        }
    }
}