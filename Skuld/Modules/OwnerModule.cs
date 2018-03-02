using System.Threading.Tasks;
using Discord.Commands;
using Skuld.Tools;
using Discord;
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Discord.WebSocket;
using System.Diagnostics;
using System.IO;

namespace Skuld.Commands
{
    [RequireOwner, Group, Name("Owner")]
    public class Owner : ModuleBase
    {
        [Command("stop", RunMode = RunMode.Async)]
        public async Task Stop() { await MessageHandler.SendChannelAsync(Context.Channel, "Stopping!");  await Bot.StopBot("StopCmd").ConfigureAwait(false); }

        [Command("populate", RunMode = RunMode.Async)]
        public async Task Populate() { await MessageHandler.SendChannelAsync(Context.Channel, "Starting to populate guilds and users o7!"); await Events.DiscordEvents.PopulateGuilds().ConfigureAwait(false); }

        [Command("shardrestart", RunMode = RunMode.Async), Summary("Restarts shard")]
        public async Task ReShard(int shard)
        {

            await Bot.bot.GetShard(shard).StopAsync().ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            await Bot.bot.GetShard(shard).StartAsync().ConfigureAwait(false);
        }

        [Command("setgame", RunMode = RunMode.Async), Summary("Set Game")]
        public async Task Game([Remainder]string title)
        {
            try
            {
                await Bot.bot.SetGameAsync(title);
                await Context.Message.DeleteAsync();
            }
            catch
            {
                await MessageHandler.SendChannelAsync(Context.Channel, $":nauseated_face: Something went wrong. Try again.");
            }
        }
        [Command("resetgame", RunMode = RunMode.Async), Summary("Reset Game")]
        public async Task ResetGame()
        {
            try
            {
                await Bot.bot.SetGameAsync($"{Bot.Prefix}help | {Bot.random.Next(0, Bot.bot.Shards.Count) + 1}/{Bot.bot.Shards.Count}");
                await Context.Message.DeleteAsync();
            }
            catch
            {
                await MessageHandler.SendChannelAsync(Context.Channel, $":nauseated_face: Something went wrong. Try again.");
            }
        }

        [Command("dumpshards", RunMode = RunMode.Async), Summary("Shard Info"), Alias("dumpshard")]
        public async Task Shard()
        {
            var lines = new List<string[]>
            {
                new string[] { "Shard", "State", "Latency", "Guilds" }
            };
            foreach (var item in Bot.bot.Shards)
            {
                lines.Add(new string[] { item.ShardId.ToString(), item.ConnectionState.ToString(), item.Latency.ToString(), item.Guilds.Count.ToString() });
            }

            await MessageHandler.SendChannelAsync(Context.Channel, "```"+ ConsoleUtils.PrettyLines(lines, 2) + "```");
        }

        [Command("getshard", RunMode = RunMode.Async), Summary("Gets all information about specific shard")]
        public async Task ShardGet(int shardid)
        {
            await ShardInfo(shardid).ConfigureAwait(false);
        }

        [Command("getshard", RunMode = RunMode.Async), Summary("Gets all information about current shard")]
        public async Task CurrShard()
        {
            await ShardInfo(Bot.bot.GetShardIdFor(Context.Guild)).ConfigureAwait(false);
        }
        public async Task ShardInfo(int shardid)
        {
            var shard = Bot.bot.GetShard(shardid);
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

            embed.AddField("Guilds", shard.Guilds.Count.ToString(), inline: true);
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Status";
                if (!String.IsNullOrEmpty(Convert.ToString(shard.ConnectionState)))
                {
                    x.Value = Convert.ToString(shard.ConnectionState);
                }
                else
                {
                    x.Value = Convert.ToString(shard.ConnectionState);
                }
            });
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Latency";
                if (!String.IsNullOrEmpty(Convert.ToString(shard.Latency)))
                {
                    x.Value = Convert.ToString(shard.Latency) + "ms";
                }
                else
                {
                    x.Value = "Connected?";
                }
            });
            await MessageHandler.SendChannelAsync(Context.Channel, "", embed.Build());
        }

        [Command("shard", RunMode = RunMode.Async), Summary("Gets the shard the guild is on")]
        public async Task ShardGet() => await MessageHandler.SendChannelAsync(Context.Channel, $"{Context.User.Mention} the server: `{Context.Guild.Name}` is on `{Bot.bot.GetShardIdFor(Context.Guild)}`");        

        [Command("name", RunMode = RunMode.Async), Summary("Name")]
        public async Task Name([Remainder]string name) => await Bot.bot.CurrentUser.ModifyAsync(x => x.Username = name);

        [Command("status", RunMode = RunMode.Async), Summary("Status")]
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
        public async Task SetStatus(UserStatus status) { await Bot.bot.SetStatusAsync(status); }

        [Command("moneyadd", RunMode = RunMode.Async), Summary("Gives money to people")]
        public async Task GiveMoney(IGuildUser user, ulong amount)
        {
            if(!String.IsNullOrEmpty(Bot.Configuration.SqlDBHost))
            {
                var suser = await Bot.Database.GetUserAsync(user.Id);
                if (suser!=null)
                {
                    ulong newmoney = suser.Money + amount;

                    await Bot.Database.ModifyUserAsync(user as SocketUser, "money", Convert.ToString(newmoney));

                    await MessageHandler.SendChannelAsync(Context.Channel, $"User {user.Username} now has: {Bot.Configuration.MoneySymbol + newmoney}");
                }
                else
                {
                    await Bot.Database.InsertUserAsync(user as SocketUser);
                    await GiveMoney(user, amount).ConfigureAwait(false);
                }
            }
        }

        [Command("leave", RunMode = RunMode.Async), Summary("Leaves a server by id")]
        public async Task LeaveServer(ulong id)
        {
            var client = Bot.bot;
            var guild = client.GetGuild(id);
            await guild.LeaveAsync().ContinueWith(async x =>
            {
                if (client.GetGuild(id) == null)
                {
                    await MessageHandler.SendChannelAsync(Context.Channel,$"Left guild **{guild.Name}**");
                }
                else
                {
                    await MessageHandler.SendChannelAsync(Context.Channel, $"Hmm, I haven't left **{guild.Name}**");
                }
            });
        }

        [Command("syncguilds", RunMode = RunMode.Async), Summary("Syncs Guilds")]
        public async Task SyncGuilds()
        {
            foreach(var guild in Bot.bot.Guilds)
            {
                var cmd = new MySqlCommand($"UPDATE guild SET Name = \"{guild.Name.Replace("\"","\\").Replace("\'","\\")}\" WHERE ID = {guild.Id}");
                await Bot.Database.NonQueryAsync(cmd);
            }
            await MessageHandler.SendChannelAsync(Context.Channel, $"Synced the guilds");
        }

        [Command("eval", RunMode = RunMode.Async), Summary("no")]
        public async Task EvalStuff([Remainder]string code)
        {
            try
            {
                if (code.ToLowerInvariant().Contains("token") || code.ToLowerInvariant().Contains("config"))
                    await MessageHandler.SendChannelAsync(Context.Channel, "Nope.");

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
                    await MessageHandler.SendChannelAsync(Context.Channel, "", embed.Build());
                else
                {
                    await MessageHandler.SendChannelAsync(Context.Channel, "Result is empty or null");
                }                
            }
            catch (NullReferenceException ex) { /*Do nothing here*/ }
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
                await Bot.Logger.AddToLogs(new Models.LogMessage("EvalCMD", "Error with eval command " + ex.Message, LogSeverity.Error, ex));
                await MessageHandler.SendChannelAsync(Context.Channel, "", embed.Build());
            }
        }

        [Command("pubstats", RunMode = RunMode.Async), Summary("no")]
        public async Task PubStats()
        {
            await MessageHandler.SendChannelAsync(Context.Channel, "Ok, publishing stats to the Discord Bot lists.");
            string list = "";
            int shardcount = Bot.bot.Shards.Count;
            foreach(var shard in Bot.bot.Shards)
            {
                await Bot.PublishStats(shard.ShardId);
                list += $"I sent ShardID: {shard.ShardId} Guilds: {shard.Guilds.Count} Shards: {shardcount}\n";
            }
            await MessageHandler.SendChannelAsync(Context.Channel, list);
        }

        [Command("rebuildguilds", RunMode = RunMode.Async)]
        public async Task RebuildGuilds()
        {
            foreach(var guild in  Bot.bot.Guilds)
            {
                var gcmd = new MySqlCommand("INSERT IGNORE INTO `guild` (`ID`,`name`,`prefix`) VALUES ");
                gcmd.CommandText += $"( {guild.Id} , \"{guild.Name.Replace("\"", "\\").Replace("\'", "\\'")}\" ,\"{Bot.Configuration.Prefix}\" )";
                await Bot.Database.NonQueryAsync(gcmd);

                //Configures Modules
                gcmd = new MySqlCommand("INSERT IGNORE INTO `guildcommandmodules` " +
                    "(`ID`,`Accounts`,`Actions`,`Admin`,`Fun`,`Help`,`Information`,`Search`,`Stats`) " +
                    $"VALUES ( {guild.Id} , 1, 1, 1, 1, 1, 1, 1, 1 )");
                await Bot.Database.NonQueryAsync(gcmd);

                //Configures Settings
                gcmd = new MySqlCommand("INSERT IGNORE INTO `guildfeaturemodules` " +                            "(`ID`,`Starboard`,`Pinning`,`Experience`,`UserJoinLeave`,`UserModification`,`UserBanEvents`,`GuildModification`,`GuildChannelModification`,`GuildRoleModification`) " +
                    $"VALUES ( {guild.Id} , 0, 0, 0, 0, 0, 0, 0, 0, 0 )");
                await Bot.Database.NonQueryAsync(gcmd);

                await Bot.Logger.AddToLogs(new Models.LogMessage("IsrtGld", $"Inserted {guild.Name}!", LogSeverity.Info));
                await Events.DiscordEvents.PopulateEntireGuildUsers(guild);
            }
        }        
    }

    public class Globals
    {
        public ShardedCommandContext Context { get; set; }
    }
}
