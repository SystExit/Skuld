using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Skuld.Utilities;
using Skuld.Extensions;
using Discord.WebSocket;
using Discord;
using StatsdClient;
using System.Threading;

namespace Skuld.Services
{
    public class LoggingService
    {
        public StreamWriter sw;
        List<Models.LogMessage> Logs;
        bool Console;
        bool File;
		static List<DiscordSocketClient> ShardsReady;
		DiscordShardedClient client;
		MessageService messageService;
		DatabaseService database;
		Random random;
		BotService botService;

		public void Config(BotService bot, DiscordShardedClient cli, MessageService message, DatabaseService db, Random ran)
		{
			botService = bot;
			client = cli;
			messageService = message;
			database = db;
			random = ran;
		}

		public LoggingService(string logfile)
        {
            Logs = new List<Models.LogMessage>();
            Console = false;
            File = false;
			sw = new StreamWriter(logfile, true, Encoding.Unicode)
			{
				AutoFlush = true
			};
		}

        public LoggingService(bool OutputToConsole, bool OutputToFile, string logfile)
        {
            Logs = new List<Models.LogMessage>();
            Console = OutputToConsole;
            File = OutputToFile;
			sw = new StreamWriter(logfile, true, Encoding.Unicode)
			{
				AutoFlush = true
			};
		}

		public async Task DiscordLogger(LogMessage arg)
		{
			await AddToLogsAsync(new Models.LogMessage(arg.Source, arg.Message, arg.Severity, arg.Exception));
		}

		public async Task TwitchLogger(NTwitch.LogMessage arg)
		{
			await AddToLogsAsync(new Models.LogMessage(arg.Source, arg.Message, (arg.Level).FromNTwitch(), arg.Exception));
		}

		public async Task AddToLogsAsync(Models.LogMessage message)
        {
			if (message.Severity == LogSeverity.Verbose) return;
            Logs.Add(message);
            string source = String.Join("", message.Source.Take(1));
            source += String.Join("", message.Source.Reverse().Take(3).Reverse());

            string tolog = null;
            string ConsoleMessage = null,
                FileMessage = null;
			
            if(message.Exception!=null)
            {
                var loglines = new List<string[]>
                {
                    new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp), "[" + message.Source + "]", "[" + message.Severity + "]", message.Message + Environment.NewLine + message.Exception }
                };
                if (Console && !File)
                {
                    ConsoleMessage = ConsoleUtils.PrettyLines(loglines, 2);
                }
                if(Console && File)
                {
                    ConsoleMessage = " CHECK LOGS FOR MORE INFO!";
                    FileMessage = ConsoleUtils.PrettyLines(loglines, 2);
                }
                if(!Console && File)
                {
                    FileMessage = ConsoleUtils.PrettyLines(loglines, 2);
                }
                if(!Console && !File)
                { }
            }

			switch(message.Severity)
			{
				case LogSeverity.Info:
					DogStatsd.Event(message.Source, $"{String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp)} [{message.Severity}] {message.Message}", "info");
					break;

				case LogSeverity.Warning:
					DogStatsd.Event(message.Source, $"{String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp)} [{message.Severity}] {message.Message}", "warning");
					break;

				case LogSeverity.Critical:
					DogStatsd.Event(message.Source, $"{String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp)} [{message.Severity}] {message.Message}\n{message.Exception}", "critical");
					break;

				case LogSeverity.Error:
					DogStatsd.Event(message.Source, $"{String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp)} [{message.Severity}] {message.Message}\n{message.Exception}", "error");
					break;
			}

			if (Console)
            {
                System.Console.ForegroundColor = Tools.Tools.ColorBasedOnSeverity(message.Severity);
                var consolelines = new List<string[]>();
                if(ConsoleMessage!=null)
                {
                    if (ConsoleMessage.StartsWith(" "))
                    {
                        consolelines.Add(new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp), "[" + source + "]", "[" + message.Severity.ToString()[0] + "]", message.Message + ConsoleMessage });
                    }
                    else
                    {
                        consolelines.Add(new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp), "[" + source + "]", "[" + message.Severity.ToString()[0] + "]", message.Message + Environment.NewLine + ConsoleMessage });
                    }
                }
                else
                {
                    consolelines.Add(new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp), "[" + source + "]", "[" + message.Severity.ToString()[0] + "]", message.Message });
                }
                string toconsole = ConsoleUtils.PrettyLines(consolelines, 2);
                await System.Console.Out.WriteLineAsync(toconsole);
                System.Console.ForegroundColor = ConsoleColor.White;
            }
            if(File)
            {
                if(FileMessage!=null)
                {
                    tolog = ConsoleUtils.PrettyLines(new List<string[]> { new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp), "[" + message.Source + "]", "[" + message.Severity.ToString()[0] + "]", message.Message + Environment.NewLine + FileMessage } }, 2);
                }
                else
                {
                    tolog = ConsoleUtils.PrettyLines(new List<string[]> { new string[] { String.Format("{0:dd/MM/yyyy HH:mm:ss}", message.TimeStamp), "[" + message.Source + "]", "[" + message.Severity.ToString()[0] + "]", message.Message } }, 2);
                }

                sw.WriteLine(tolog);
            }
        }
		
		//DiscordLoging
		public void RegisterEvents()
		{
			ShardsReady = new List<DiscordSocketClient>();
			/*All Events needed for running Skuld*/
			client.ShardReady += Bot_ShardReady;
			client.JoinedGuild += Bot_JoinedGuild;
			client.LeftGuild += Bot_LeftGuild;
			client.UserJoined += Bot_UserJoined;
			client.UserLeft += Bot_UserLeft;
			client.ReactionAdded += Bot_ReactionAdded;
			client.ShardConnected += Bot_ShardConnected;
			client.ShardDisconnected += Bot_ShardDisconnected;
		}

		public void UnRegisterEvents()
		{
			client.ShardReady -= Bot_ShardReady;
			client.MessageReceived -= messageService.OnMessageRecievedAsync;
			client.JoinedGuild -= Bot_JoinedGuild;
			client.LeftGuild -= Bot_LeftGuild;
			client.UserJoined -= Bot_UserJoined;
			client.UserLeft -= Bot_UserLeft;
			client.ReactionAdded -= Bot_ReactionAdded;
			client.ShardConnected -= Bot_ShardConnected;
			client.ShardDisconnected -= Bot_ShardDisconnected;
			ShardsReady = null;
		}

		async Task Bot_ShardReady(DiscordSocketClient arg)
		{
			if (!ShardsReady.Contains(arg))
			{
				ShardsReady.Add(arg);

				if (ShardsReady.Count == client.Shards.Count)
				{
					client.MessageReceived += messageService.OnMessageRecievedAsync;
					await client.SetGameAsync($"{Bot.Configuration.Discord.Prefix}help | {random.Next(0, client.Shards.Count) + 1}/{client.Shards.Count}", type: ActivityType.Listening);
				}
			}
		}

		async Task Bot_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
		{
			if (database.CanConnect)
			{
				var gld = client.Guilds.FirstOrDefault(x => x.TextChannels.FirstOrDefault(z => z.Id == arg2.Id) != null);
				var guild = await database.GetGuildAsync(gld.Id);
				if (guild != null && gld != null)
				{
					if (guild.GuildSettings.Features.Pinning)
					{
						var dldedmsg = await arg1.GetOrDownloadAsync();
						int pinboardThreshold = Bot.Configuration.Utils.PinboardThreshold;
						int pinboardReactions = 0;
						if (arg3.Emote.Name == "📌")
						{ pinboardReactions = dldedmsg.Reactions.FirstOrDefault(x => x.Key.Name == "📌").Value.ReactionCount; }
						if (pinboardReactions >= pinboardThreshold)
						{
							var now = dldedmsg.CreatedAt;
							var dt = DateTime.UtcNow.AddDays(-Bot.Configuration.Utils.PinboardDateLimit);
							if ((now - dt).TotalDays > 0)
							{
								if (!dldedmsg.IsPinned)
								{
									await dldedmsg.PinAsync();
									await AddToLogsAsync(new Models.LogMessage("PinBrd", $"Reached or Over Threshold, pinned a message in: {dldedmsg.Channel.Name} from: {gld.Name}", LogSeverity.Info));
								}
							}
						}
					}
				}
			}
		}

		async Task Bot_ShardConnected(DiscordSocketClient arg)
		{
			await arg.SetGameAsync($"{Bot.Configuration.Discord.Prefix}help | {random.Next(0, client.Shards.Count) + 1}/{client.Shards.Count}", type: ActivityType.Listening);
			DogStatsd.Event("shards.connected", $"Shard {arg.ShardId} Connected", alertType: "info");
		}
		Task Bot_ShardDisconnected(Exception arg1, DiscordSocketClient arg2)
		{
			DogStatsd.Event($"Shard.disconnected", $"Shard {arg2.ShardId} Disconnected, error: {arg1}", alertType: "error");
			return Task.CompletedTask;
		}

		//Start Users
		async Task Bot_UserJoined(SocketGuildUser arg)
		{
			await AddToLogsAsync(new Models.LogMessage("UsrJoin", $"User {arg.Username} joined {arg.Guild.Name}", LogSeverity.Info));
			if (database.CanConnect)
			{
				await database.InsertUserAsync(arg);

				var guild = await database.GetGuildAsync(arg.Guild.Id);

				if (guild!=null && guild.AutoJoinRole!=0)
				{
					var joinrole = arg.Guild.GetRole(guild.AutoJoinRole);
					await arg.AddRoleAsync(joinrole);
					await AddToLogsAsync(new Models.LogMessage("UsrJoin", $"Gave user {arg.Username}, the automatic role as per request of {arg.Guild.Name}.", LogSeverity.Info));
				}
				if (guild != null && guild.UserJoinChannel !=0 && !String.IsNullOrEmpty(guild.JoinMessage))
				{
					var channel = arg.Guild.GetTextChannel(guild.UserJoinChannel);
					var welcomemessage = guild.JoinMessage;
					welcomemessage = welcomemessage.Replace("-m", "**" + arg.Mention + "**");
					welcomemessage = welcomemessage.Replace("-s", "**" + arg.Guild.Name + "**");
					welcomemessage = welcomemessage.Replace("-uc", Convert.ToString(arg.Guild.MemberCount));
					welcomemessage = welcomemessage.Replace("-u", "**" + arg.Username + "**");
					await messageService.SendChannelAsync(channel, welcomemessage);
				}
				var discord = client.GetUser(arg.Id);
				var db = await database.GetUserAsync(arg.Id);
				if (discord == null && db != null)
				{
					await database.DropUserAsync(arg);
				}
				else if (discord != null && db == null)
				{
					await database.InsertUserAsync(discord);
				}
			}
		}
		async Task Bot_UserLeft(SocketGuildUser arg)
		{
			await AddToLogsAsync(new Models.LogMessage("UsrLeft", $"User {arg.Username} just left {arg.Guild.Name}", LogSeverity.Info));
			if (database.CanConnect)
			{
				var guild = await database.GetGuildAsync(arg.Guild.Id);
				string leavemessage = guild.LeaveMessage;
				if (!String.IsNullOrEmpty(guild.LeaveMessage))
				{
					var channel = arg.Guild.GetTextChannel(guild.UserLeaveChannel);
					leavemessage = leavemessage.Replace("-m", "**" + arg.Mention + "**");
					leavemessage = leavemessage.Replace("-s", "**" + guild.Name + "**");
					leavemessage = leavemessage.Replace("-uc", Convert.ToString(arg.Guild.MemberCount));
					leavemessage = leavemessage.Replace("-u", "**" + arg.Username + "**");
					await messageService.SendChannelAsync(channel, leavemessage);
				}
				var discord = client.GetUser(arg.Id);
				var db = await database.GetUserAsync(arg.Id);
				if (discord == null && db != null)
				{
					await database.DropUserAsync(arg);
				}
				else if (discord != null && db == null)
				{
					await database.InsertUserAsync(discord);
				}
			}
		}
		//End Users

		//Start Guilds
		async Task Bot_LeftGuild(SocketGuild arg)
		{
			DogStatsd.Increment("guilds.left");

			await database.DropGuildAsync(arg);

			Thread thd = new Thread(async () =>
			{
				await database.CheckGuildUsersAsync(arg);
			})
			{
				IsBackground = true 
			};
			thd.Start();
			await botService.UpdateStatsAsync();
		}
		async Task Bot_JoinedGuild(SocketGuild arg)
		{
			DogStatsd.Increment("guilds.joined");
			Thread thd = new Thread(async () =>
			{
				await database.InsertGuildAsync(arg);
			})
			{
				IsBackground = true
			};
			thd.Start();
			await botService.UpdateStatsAsync();
		}
		//End Guilds
	}
}