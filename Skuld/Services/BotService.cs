using System;
using System.Threading.Tasks;
using System.Threading;
using Discord;
using Discord.WebSocket;
using System.IO;
using System.Net.Http;
using System.Text;
using Skuld.Tools;
using System.Linq;
using StatsdClient;
using System.Net;

namespace Skuld.Services
{
    public class BotService
    {
		DiscordShardedClient client;
		LoggingService logger;
		Config config;
		MessageService messageService;

		public BotService(DiscordShardedClient cli, LoggingService log, MessageService message)
		{
			client = cli;
			logger = log;
			messageService = message;
		}
		
		public void AddConfg(Config conf)
		{ config = conf; }

		public async Task StartAsync()
		{
			try
			{
				await client.LoginAsync(TokenType.Bot, config.Token);
				await client.StartAsync();

				Parallel.Invoke(() => SendDataToDataDog());

				//await UpdateStats();

				await Task.Delay(-1).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				await logger.AddToLogsAsync(new Models.LogMessage("Strt-Bot", "ERROR WITH THE BOT", LogSeverity.Error, ex));
				DogStatsd.Event("FrameWork", $"Bot Crashed on start: {ex}", alertType: "error", hostname: "Skuld");
				await StopBot("Init-Bt").ConfigureAwait(false);
			}
		}
		
		public async Task StopBot(string source)
		{
			logger.UnRegisterEvents();
			await client.SetStatusAsync(UserStatus.Offline);
			await logger.AddToLogsAsync(new Models.LogMessage(source, "Skuld is shutting down", LogSeverity.Info));
			DogStatsd.Event("FrameWork", $"Bot Stopped", alertType: "info", hostname: "Skuld");

			await logger.sw.WriteLineAsync("-------------------------------------------").ConfigureAwait(false);
			logger.sw.Close();

			await Console.Out.WriteLineAsync("Bot shutdown").ConfigureAwait(false);
			Console.ReadLine();
			Environment.Exit(0);
		}

		Task SendDataToDataDog()
		{
			while (true)
			{
				int users = 0;
				foreach (var guild in client.Guilds)
				{ users += guild.MemberCount; }
				DogStatsd.Gauge("shards.count", client.Shards.Count);
				DogStatsd.Gauge("shards.connected", client.Shards.Count(x => x.ConnectionState == ConnectionState.Connected));
				DogStatsd.Gauge("shards.disconnected", client.Shards.Count(x => x.ConnectionState == ConnectionState.Disconnected));
				DogStatsd.Gauge("commands.count", messageService.commandService.Commands.Count());
				DogStatsd.Gauge("guilds.total", client.Guilds.Count);
				DogStatsd.Gauge("users.total", users);
				Thread.Sleep(TimeSpan.FromSeconds(5));
			}
		}

		public async Task<string> UpdateStats()
		{
			System.Collections.Generic.List<Models.API.BotStats> botStats = new System.Collections.Generic.List<Models.API.BotStats>();
			for (var x = 0; x < client.Shards.Count; x++)
			{
				botStats.Add(new Models.API.BotStats
				{
					ServerCount = client.GetShard(x).Guilds.Count,
					ShardCount = client.Shards.Count,
					ShardID = x
				});
				await PublishStats(x);
			}

			var webclient = (HttpWebRequest)WebRequest.Create(new Uri($"https://skuld.systemexit.co.uk/tools/updateStats.php"));
			webclient.ContentType = "application/json";
			webclient.Method = "POST";
			webclient.Headers.Add(HttpRequestHeader.Authorization, config.SysExToken);
			using (var swriter = new StreamWriter(await webclient.GetRequestStreamAsync()))
			{
				swriter.Write(Newtonsoft.Json.JsonConvert.SerializeObject(botStats));
				await swriter.FlushAsync();
				swriter.Close();
			}
			var response = (HttpWebResponse)await webclient.GetResponseAsync();
			using (var streamReader = new StreamReader(response.GetResponseStream()))
			{
				var result = streamReader.ReadToEnd();
				return result;
			}
		}

		public async Task PublishStats(int shardid)
		{
			var bstats = new Models.API.BotStats()
			{
				ServerCount = client.GetShard(shardid).Guilds.Count,
				ShardCount = client.Shards.Count,
				ShardID = shardid
			};
			using (var webclient = new HttpClient())
			using (var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(bstats), Encoding.UTF8, "application/json"))
			{
				webclient.DefaultRequestHeaders.Add("Authorization", config.DBotsOrgKey);
				await webclient.PostAsync(new Uri($"https://discordbots.org/api/bots/{client.CurrentUser.Id}/stats"), content);
			}
			using (var webclient = new HttpClient())
			using (var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(bstats), Encoding.UTF8, "application/json"))
			{
				webclient.DefaultRequestHeaders.Add("Authorization", config.DiscordPWKey);
				await webclient.PostAsync(new Uri($"https://bots.discord.pw/api/bots/{client.CurrentUser.Id}/stats"), content);
			}
		}
	}
}
