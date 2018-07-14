using System;
using System.Threading.Tasks;
using System.Threading;
using Discord;
using Discord.WebSocket;
using System.Linq;
using StatsdClient;
using Skuld.Core.Models;
using Skuld.Core.Services;
using Skuld.Core.Utilities.Stats;
using Microsoft.Extensions.DependencyInjection;

namespace Skuld.Services
{
    public class BotService
    {
        HostService HostService;
        DiscordShardedClient client;
        DiscordLogger logger;
        SkuldConfig config;
        MessageService messageService;
        TwitchService twitch;

        public BotService(DiscordShardedClient shard,
            DiscordLogger log,
            MessageService msg,
            TwitchService twit)
        {
            client = shard;
            logger = log;
            twitch = twit;
            messageService = msg;
        }

		public void AddConfg(SkuldConfig conf)
		{
            config = conf;
        }

        public void AddHostService(HostService host)
        {
            HostService = host;
        }

		public async Task StartAsync()
		{
			try
			{
				await twitch.LoginAsync(config.APIS.TwitchToken);

                logger.RegisterEvents();

				await client.LoginAsync(TokenType.Bot, config.Discord.Token);
				await client.StartAsync();

                Parallel.Invoke(() => SendDataToDataDog());

                //await UpdateStatsAsync().ConfigureAwait(false);

                await Task.Delay(-1).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await logger.logger.AddToLogsAsync(new Core.Models.LogMessage("BotService", "ERROR WITH THE BOT", LogSeverity.Error, ex));
				DogStatsd.Event("FrameWork", $"Bot Crashed on start: {ex}", alertType: "error", hostname: "Skuld");
				await StopBotAsync("Init-Bt").ConfigureAwait(false);
			}
		}

        public async Task StopBotAsync(string source)
		{
			logger.UnRegisterEvents();
			await client.SetStatusAsync(UserStatus.Offline);
			await logger.logger.AddToLogsAsync(new Core.Models.LogMessage(source, "Skuld is shutting down", LogSeverity.Info));
			DogStatsd.Event("FrameWork", $"Bot Stopped", alertType: "info", hostname: "Skuld");

			await logger.logger.sw.WriteLineAsync("-------------------------------------------").ConfigureAwait(false);
			logger.logger.sw.Close();

			await Console.Out.WriteLineAsync("Bot shutdown").ConfigureAwait(false);
			Console.ReadLine();
			Environment.Exit(0);
        }

        Task SendDataToDataDog()
        {
            while (true)
            {
                DogStatsd.Gauge("shards.count", client.Shards.Count);
                DogStatsd.Gauge("shards.connected", client.Shards.Count(x => x.ConnectionState == ConnectionState.Connected));
                DogStatsd.Gauge("shards.disconnected", client.Shards.Count(x => x.ConnectionState == ConnectionState.Disconnected));
                DogStatsd.Gauge("commands.count", messageService.commandService.Commands.Count());
                DogStatsd.Gauge("guilds.total", client.Guilds.Count);
                HostService.Services.GetRequiredService<HardwareStats>().CPU.Feed();
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }
    }
}
