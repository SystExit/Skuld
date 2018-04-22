using System.Threading.Tasks;
using Discord.Commands;
using Skuld.Utilities;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Discord.WebSocket;
using Skuld.Services;
using Skuld.Tools;

namespace Skuld.Modules
{
    [RequireOwner, Group, Name("Owner")]
    public class Owner : ModuleBase<ShardedCommandContext>
    {
		readonly MessageService messageService;
		readonly Random random;
		readonly DatabaseService database;
		readonly LoggingService logger;
		readonly BotService botService;

		public Owner(MessageService msg,
			BotService bot,
			DatabaseService db,
			LoggingService log,
			Random ran) //depinj
		{
			messageService = msg;
			botService = bot;
			database = db;
			logger = log;
			random = ran;
		}
        
			
		[Command("stop")]
        public async Task Stop()
		{
			await messageService.SendChannelAsync(Context.Channel, "Stopping!");
			await botService.StopBot("StopCmd").ConfigureAwait(false);
		}

        [Command("populate"), RequireDatabase]
        public async Task Populate()
		{
			await messageService.SendChannelAsync(Context.Channel, "Starting to populate guilds and users o7!");
			var resp = await database.PopulateGuildsAsync();
			if(resp.Where(x=>x.Error.ToLowerInvariant().Contains("robot")==false).All(x=>x.Successful==true))
			{
				await messageService.SendChannelAsync(Context.Channel, "All populated ok!");
			}
			else
			{
				var badeggs = resp.Where(x => x.Successful != true);
				string msg = "";
				string bigmsg = "";
				foreach(var badegg in badeggs)
				{
					if(!badegg.Error.ToLowerInvariant().Contains("robot"))
					{
						msg += $"{badegg.Error}\n\n";
						bigmsg += $"===={badegg.Error}====\n{badegg.Exception}\n";
					}
				}
				if(msg.Length>0)
				{
					if (msg.Length > 2000)
					{
						await logger.AddToLogsAsync(new Models.LogMessage("popcmd", "Error populating guild.", LogSeverity.Error, new Exception(bigmsg)));
					}
					else
					{
						await messageService.SendDMsAsync(Context.Channel, await Context.User.GetOrCreateDMChannelAsync(), msg);
					}
				}
			}
		}

        [Command("shardrestart"), Summary("Restarts shard")]
        public async Task ReShard(int shard)
        {

            await Context.Client.GetShard(shard).StopAsync().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            await Context.Client.GetShard(shard).StartAsync().ConfigureAwait(false);
        }

        [Command("setgame"), Summary("Set Game")]
        public async Task Game([Remainder]string title)
        {
            try
            {
                await Context.Client.SetGameAsync(title);
                await Context.Message.DeleteAsync();
            }
            catch
            {
                await messageService.SendChannelAsync(Context.Channel, $":nauseated_face: Something went wrong. Try again.");
            }
        }
        [Command("resetgame"), Summary("Reset Game")]
        public async Task ResetGame()
        {
            try
            {
                await Context.Client.SetGameAsync($"{messageService.config.Prefix}help | {random.Next(0, Context.Client.Shards.Count) + 1}/{Context.Client.Shards.Count}");
                await Context.Message.DeleteAsync();
            }
            catch
            {
                await messageService.SendChannelAsync(Context.Channel, $":nauseated_face: Something went wrong. Try again.");
            }
        }

        [Command("dumpshards"), Summary("Shard Info"), Alias("dumpshard")]
        public async Task Shard()
        {
            var lines = new List<string[]>
            {
                new string[] { "Shard", "State", "Latency", "Guilds" }
            };
            foreach (var item in Context.Client.Shards)
            {
                lines.Add(new string[] { item.ShardId.ToString(), item.ConnectionState.ToString(), item.Latency.ToString(), item.Guilds.Count.ToString() });
            }

            await messageService.SendChannelAsync(Context.Channel, "```"+ ConsoleUtils.PrettyLines(lines, 2) + "```");
        }
        [Command("getshard"), Summary("Gets all information about specific shard")]
        public async Task ShardGet(int shardid = -1)
        {
			if(shardid>-1)
				await ShardInfo(shardid).ConfigureAwait(false);
			else
				await ShardInfo(Context.Client.GetShardIdFor(Context.Guild)).ConfigureAwait(false);
        }
        public async Task ShardInfo(int shardid)
        {
            var shard = Context.Client.GetShard(shardid);
            var embed = new EmbedBuilder
            {
                Color = Tools.Tools.RandomColor(),
                Author = new EmbedAuthorBuilder
                {
                    Name = $"Shard ID: {shardid}",
                    IconUrl = shard.CurrentUser.GetAvatarUrl()
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = "Generated at"
                },
                Timestamp = DateTime.Now
            };

            embed.AddField("Guilds", shard.Guilds.Count.ToString(), true);
            embed.AddField("Status", shard.ConnectionState, true);
            embed.AddField("Latencty", shard.Latency+"ms", true);
            await messageService.SendChannelAsync(Context.Channel, "", embed.Build());
        }
        [Command("shard"), Summary("Gets the shard the guild is on")]
        public async Task ShardGet() => 
			await messageService.SendChannelAsync(Context.Channel, $"{Context.User.Mention} the server: `{Context.Guild.Name}` is on `{Context.Client.GetShardIdFor(Context.Guild)}`");        

        [Command("name"), Summary("Name")]
        public async Task Name([Remainder]string name) => await Context.Client.CurrentUser.ModifyAsync(x => x.Username = name);

        [Command("status"), Summary("Status")]
        public async Task Status(string status)
        {
            if (status.ToLower() == "online")
            { await SetStatus(UserStatus.Online); }
            if (status.ToLower() == "afk")
            { await SetStatus(UserStatus.AFK); }
            if (status.ToLower() == "dnd" || status.ToLower() == "do not disturb" || status.ToLower() == "donotdisturb")
            { await SetStatus(UserStatus.DoNotDisturb); }
            if (status.ToLower() == "idle")
            { await SetStatus(UserStatus.Idle); }
            if (status.ToLower() == "offline")
            { await SetStatus(UserStatus.Offline); }
            if (status.ToLower() == "invisible")
            { await SetStatus(UserStatus.Invisible); }
        }
        public async Task SetStatus(UserStatus status) =>
			await Context.Client.SetStatusAsync(status);

        [Command("moneyadd"), Summary("Gives money to people"), RequireDatabase]
        public async Task GiveMoney(IGuildUser user, ulong amount)
        {
            var suser = await database.GetUserAsync(user.Id);
            if (suser!=null)
            {
                suser.Money += amount;
					
                await database.UpdateUserAsync(suser);

                await messageService.SendChannelAsync(Context.Channel, $"User {user.Username} now has: {Bot.Configuration.Utils.MoneySymbol + suser.Money}");
            }
            else
            {
                await database.InsertUserAsync(user as SocketUser);
                await GiveMoney(user, amount).ConfigureAwait(false);
            }            
        }

        [Command("leave"), Summary("Leaves a server by id")]
        public async Task LeaveServer(ulong id)
        {
            var guild = Context.Client.GetGuild(id);
            await guild.LeaveAsync().ContinueWith(async x =>
            {
                if (Context.Client.GetGuild(id) == null)
                {
                    await messageService.SendChannelAsync(Context.Channel,$"Left guild **{guild.Name}**");
                }
                else
                {
                    await messageService.SendChannelAsync(Context.Channel, $"Hmm, I haven't left **{guild.Name}**");
                }
            });
        }

        [Command("syncguilds"), Summary("Syncs Guilds")]
        public async Task SyncGuilds()
        {
            foreach(var guild in Context.Client.Guilds)
            {
				var gld = await database.GetGuildAsync(guild.Id);
				gld.Name = guild.Name.Replace("\"","\\").Replace("\'","\\");
                await database.UpdateGuildAsync(gld);
            }
            await messageService.SendChannelAsync(Context.Channel, $"Synced the guilds");
		}
		[Command("rebuildguilds")]
		public async Task RebuildGuilds()
		{
			foreach (var guild in Context.Client.Guilds)
			{
				var resp = await database.RebuildGuildAsync(guild);
				if(resp.All(x=>x.Successful==true))
				{
					await logger.AddToLogsAsync(new Models.LogMessage("IsrtGld", $"Rebuilt {guild.Name}!", LogSeverity.Info));
				}
				else
				{
					string msg = "";
					foreach(var res in resp)
					{
						msg += $"===={res.Error}====\n{res.Exception}\n";
					}
					var exep = new Exception(msg);
					await logger.AddToLogsAsync(new Models.LogMessage("IsrtGld", $"Failed Rebuilding {guild.Name}!", LogSeverity.Error,exep));
				}
			}
		}

		[Command("eval"), Summary("no")]
        public async Task EvalStuff([Remainder]string code)
        {
            try
            {
				if (code.ToLowerInvariant().Contains("token") || code.ToLowerInvariant().Contains("config"))
				{ await messageService.SendChannelAsync(Context.Channel, "Nope."); return; }

                if (code.StartsWith("```cs", StringComparison.Ordinal)&&code.EndsWith("```", StringComparison.Ordinal))
                {
                    code = code.Replace("`", "");
                    code = code.Remove(0, 2);
                }
                else if (code.StartsWith("```cs", StringComparison.Ordinal) && !code.EndsWith("```", StringComparison.Ordinal))
                {
                    code = code.Replace("`", "");
                    code = code.Remove(0, 2);
                }

                var embed = new EmbedBuilder();
                var globals = new Globals().Context = Context as ShardedCommandContext;
                var soptions = ScriptOptions
                    .Default
                    .WithReferences(typeof(ShardedCommandContext).Assembly, typeof(Bot).Assembly, typeof(SocketGuildUser).Assembly, typeof(Task).Assembly, typeof(Queryable).Assembly)
                    .WithImports(typeof(ShardedCommandContext).FullName, typeof(Bot).FullName, typeof(SocketGuildUser).FullName, typeof(Task).FullName, typeof(Queryable).FullName);

                var script = CSharpScript.Create(code, soptions, globalsType: typeof(ShardedCommandContext));
                script.Compile();

                var result = (await script.RunAsync(globals: globals)).ReturnValue;

                embed.Author = new EmbedAuthorBuilder()
                {
                    Name = result.GetType().ToString()
                };

                embed.Color = Tools.Tools.RandomColor();
                embed.Description = $"{result}";
                if (result != null)
                    await messageService.SendChannelAsync(Context.Channel, "", embed.Build());
                else
                {
                    await messageService.SendChannelAsync(Context.Channel, "Result is empty or null");
                }                
            }
#pragma warning disable CS0168 // Variable is declared but never used
			catch (NullReferenceException ex) { /*Do nothing here*/ }
#pragma warning restore CS0168 // Variable is declared but never used
			catch (Exception ex)
            {
                var embed = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = "ERROR WITH EVAL"
                    },
                    Color = new Color(255, 0, 0),
                    Description = $"{ex.Message}"
                };
                await logger.AddToLogsAsync(new Models.LogMessage("EvalCMD", "Error with eval command " + ex.Message, LogSeverity.Error, ex));
                await messageService.SendChannelAsync(Context.Channel, "", embed.Build());
            }
        }

        [Command("pubstats"), Summary("no")]
        public async Task PubStats()
        {
            await messageService.SendChannelAsync(Context.Channel, "Ok, publishing stats to the Discord Bot lists.");
            string list = "";
            int shardcount = Context.Client.Shards.Count;
			await botService.UpdateStats();
            foreach(var shard in Context.Client.Shards)
            {                
                list += $"I sent ShardID: {shard.ShardId} Guilds: {shard.Guilds.Count} Shards: {shardcount}\n";
            }
            await messageService.SendChannelAsync(Context.Channel, list);
        }
    }

    public class Globals
    {
        public ShardedCommandContext Context { get; set; }
    }
}
