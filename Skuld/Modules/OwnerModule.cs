﻿using System.Threading.Tasks;
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
        public async Task Stop() { await MessageHandler.SendChannel(Context.Channel, "Stopping!");  await Bot.StopBot("StopCmd"); }
        [Command("populate", RunMode = RunMode.Async)]
        public async Task Populate() { await MessageHandler.SendChannel(Context.Channel, "Starting to populate guilds and users o7!"); await Events.DiscordEvents.PopulateGuilds(); }
        [Command("shardrestart", RunMode = RunMode.Async), Summary("Restarts shard")]
        public async Task ReShard(int shard)
        {

            await Bot.bot.GetShard(shard).StopAsync();
            await Task.Delay(TimeSpan.FromSeconds(5));
            await Bot.bot.GetShard(shard).StartAsync();
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
                await MessageHandler.SendChannel(Context.Channel, $":nauseated_face: Something went wrong. Try again.");
            }
        }
        [Command("setstream", RunMode = RunMode.Async), Summary("Set Stream")]
        public async Task Stream([Remainder]string title)
        {
            try
            {
                await Bot.bot.SetGameAsync(title, "https://twitch.tv/", StreamType.Twitch);
                if ((await Context.Guild.GetCurrentUserAsync()).GuildPermissions.ManageMessages)
                    await Context.Message.DeleteAsync();
                else { await MessageHandler.SendChannel(Context.Channel, "Cannot delete messages. :thinking:"); }
            }
            catch
            {
                await MessageHandler.SendChannel(Context.Channel, $":nauseated_face: Something went wrong. Try again.");
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
                await MessageHandler.SendChannel(Context.Channel, $":nauseated_face: Something went wrong. Try again.");
            }
        }
        [Command("dumpshards", RunMode = RunMode.Async), Summary("Shard Info"), Alias("dumpshard")]
        public async Task Shard()
        {
            var lines = new List<string[]>(){
                new string[] { "Shard", "State", "Latency", "Guilds" }
            };
            foreach (var item in Bot.bot.Shards)
                lines.Add(new string[] { item.ShardId.ToString(), item.ConnectionState.ToString(), item.Latency.ToString(), item.Guilds.Count.ToString() });

            await MessageHandler.SendChannel(Context.Channel, "```"+ ConsoleUtils.PrettyLines(lines, 2) + "```");
        }
        [Command("getshard", RunMode = RunMode.Async), Summary("Gets all information about specific shard")]
        public async Task ShardGet(int shardid)
        {
            await ShardInfo(shardid);
        }
        [Command("getshard", RunMode = RunMode.Async), Summary("Gets all information about current shard")]
        public async Task CurrShard()
        {
            await ShardInfo(Bot.bot.GetShardIdFor(Context.Guild));
        }
        public async Task ShardInfo(int shardid)
        {
            var shard = Bot.bot.GetShard(shardid);
            var embed = new EmbedBuilder()
            {
                Color = Tools.Tools.RandomColor(),
                Author = new EmbedAuthorBuilder()
                {
                    Name = $"Shard ID: {shardid}",
                    IconUrl = shard.CurrentUser.GetAvatarUrl()
                },
                Footer = new EmbedFooterBuilder()
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
                    x.Value = Convert.ToString(shard.ConnectionState);
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
                    x.Value = Convert.ToString(shard.Latency) + "ms";
                else
                {
                    x.Value = "Connected?";
                }
            });
            await MessageHandler.SendChannel(Context.Channel, "", embed);
        }
        [Command("shard", RunMode = RunMode.Async), Summary("Gets the shard the guild is on")]
        public async Task ShardGet() => await MessageHandler.SendChannel(Context.Channel, $"{Context.User.Mention} the server: `{Context.Guild.Name}` is on `{Bot.bot.GetShardIdFor(Context.Guild)}`");        
        [Command("name", RunMode = RunMode.Async), Summary("Name")]
        public async Task Name([Remainder]string name) => await Bot.bot.CurrentUser.ModifyAsync(x => x.Username = name);
        [Command("status", RunMode = RunMode.Async), Summary("Status")]
        public async Task Status(string status)
        {
            if (status.ToLower() == "online")
                await SetStatus(UserStatus.Online);
            if (status.ToLower() == "afk")
                await SetStatus(UserStatus.AFK);
            if (status.ToLower() == "dnd" || status.ToLower() == "do not disturb" || status.ToLower() == "donotdisturb")
                await SetStatus(UserStatus.DoNotDisturb);
            if (status.ToLower() == "idle")
                await SetStatus(UserStatus.Idle);
            if (status.ToLower() == "offline")
                await SetStatus(UserStatus.Offline);
            if (status.ToLower() == "invisible")
                await SetStatus(UserStatus.Invisible);
        }
        public async Task SetStatus(UserStatus status) { await Bot.bot.SetStatusAsync(status); }
        [Command("sql", RunMode = RunMode.Async), Summary("Sql stuff")]
        public async Task SQL([Remainder]string query)
        {
            if(!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                string message = "Result:```cs\n";
                var reader = await SqlTools.GetAsync(new MySqlCommand(query));
                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            message += reader.GetValue(i) + "\n";
                        }
                    }
                    message += "```";
                }
                else
                {
                    message = $"Executed command, {reader.RecordsAffected} records affected";
                }
                reader.Close();
                await SqlTools.getconn.CloseAsync();
                await MessageHandler.SendChannel(Context.Channel, message);
            }
        }
        [Command("moneyadd", RunMode = RunMode.Async), Summary("Gives money to people")]
        public async Task GiveMoney(IGuildUser user, int amount)
        {
            if(!String.IsNullOrEmpty(Config.Load().SqlDBHost))
            {
                var command = new MySqlCommand("select money from accounts where ID = @userid");
                command.Parameters.AddWithValue("@userid", user.Id);
                if (!string.IsNullOrEmpty(await SqlTools.GetSingleAsync(command)))
                {
                    int oldmoney = Convert.ToInt32(await SqlTools.GetSingleAsync(command));
                    int newmoney = oldmoney + amount;

                    command = new MySqlCommand("UPDATE accounts SET money = @newmoney WHERE ID = @userid");
                    command.Parameters.AddWithValue("@userid", user.Id);
                    command.Parameters.AddWithValue("@newmoney", newmoney);
                    await SqlTools.InsertAsync(command);
                    command = new MySqlCommand("select money from accounts where ID = @userid");
                    command.Parameters.AddWithValue("@userid", user.Id);
                    int newnewmoney = Convert.ToInt32(await SqlTools.GetSingleAsync(command));
                    await MessageHandler.SendChannel(Context.Channel, $"User {user.Username} now has: {Config.Load().MoneySymbol + newnewmoney}");
                }
                else
                {
                    command = new MySqlCommand("INSERT IGNORE INTO `accounts` (`ID`, `username`, `description`) VALUES (@userid, @username, \"I have no description\");");
                    command.Parameters.AddWithValue("@username", $"{user.Username.Replace("\"", "\\").Replace("\'", "\\'")}#{user.DiscriminatorValue}");
                    command.Parameters.AddWithValue("@userid", user.Id);
                    await SqlTools.InsertAsync(command);
                    await GiveMoney(user, amount);
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
                    await MessageHandler.SendChannel(Context.Channel,$"Left guild **{guild.Name}**");
                }
                else
                {
                    await MessageHandler.SendChannel(Context.Channel, $"Hmm, I haven't left **{guild.Name}**");
                }
            });
        }
        [Command("syncguilds", RunMode = RunMode.Async), Summary("Syncs Guilds")]
        public async Task SyncGuilds()
        {
            foreach(var guild in Bot.bot.Guilds)
            {
                var cmd = new MySqlCommand($"UPDATE guild SET Name = \"{guild.Name.Replace("\"","\\").Replace("\'","\\")}\" WHERE ID = {guild.Id}");
                await SqlTools.InsertAsync(cmd);
            }
            await MessageHandler.SendChannel(Context.Channel, $"Synced the guilds");
        }
        [Command("eval", RunMode = RunMode.Async), Summary("no")]
        public async Task EvalStuff([Remainder]string code)
        {
            if (code.ToLowerInvariant().Contains("token")||code.ToLowerInvariant().Contains("config"))
                await MessageHandler.SendChannel(Context.Channel, "Nope.");
            else
            {
                var embed = new EmbedBuilder();
                try
                {
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
                        await MessageHandler.SendChannel(Context.Channel, "", embed);
                    else
                    {
                        await MessageHandler.SendChannel(Context.Channel, "Result is empty or null");
                    }
                }
                catch (Exception ex)
                {
                    embed.Author = new EmbedAuthorBuilder()
                    {
                        Name = "ERROR WITH EVAL"
                    };
                    embed.Color = new Color(255, 0, 0);
                    embed.Description = $"{ex.Message}";
                    Bot.Logs.Add(new Models.LogMessage("EvalCMD", "Error with eval command " + ex.Message, LogSeverity.Error, ex));
                    await MessageHandler.SendChannel(Context.Channel, "", embed);
                }
            }            
        }
        [Command("pubstats", RunMode = RunMode.Async), Summary("no")]
        public async Task PubStats()
        {
            await MessageHandler.SendChannel(Context.Channel, "Ok, publishing stats to the Discord Bot lists.");
            string list = "";
            int shardcount = Bot.bot.Shards.Count;
            foreach(var shard in Bot.bot.Shards)
            {
                await Bot.PublishStats(shard.ShardId);
                list += $"I sent ShardID: {shard.ShardId} Guilds: {shard.Guilds.Count} Shards: {shardcount}\n";
            }
            await MessageHandler.SendChannel(Context.Channel, list);
        }
        [Command("exec",RunMode = RunMode.Async), Summary("no")]
        public async Task ExecPS([Remainder]string code)
        {
            var ps = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    FileName = "powershell.exe"
                }
            };
            ps.Start();
            await ps.StandardInput.WriteLineAsync(code);
            await ps.StandardInput.FlushAsync();
            await ps.StandardInput.WriteLineAsync("exit");
            await ps.StandardInput.FlushAsync();
            ps.StandardInput.Close();
            ps.WaitForExit();
            string retrn = await ps.StandardOutput.ReadToEndAsync();
            retrn = retrn.Remove(0, (90+AppContext.BaseDirectory.Length+code.Length));
            retrn = retrn.Remove((retrn.Length - AppContext.BaseDirectory.Length - 10));
            if (retrn.Length >= 2000)
            {
                File.WriteAllText(AppContext.BaseDirectory + "/exec.txt", retrn);
                await Context.Channel.SendFileAsync(AppContext.BaseDirectory + "/exec.txt", "Over 2k characters, here's the result.");
            }
            else
            {
                await MessageHandler.SendChannel(Context.Channel, "```" + retrn + "```");
            }
        }
        [Command("rebuildguilds", RunMode = RunMode.Async)]
        public async Task RebuildGuilds()
        {
            foreach(var guild in  Bot.bot.Guilds)
            {
                var gcmd = new MySqlCommand("INSERT IGNORE INTO `guild` (`ID`,`name`,`prefix`) VALUES ");
                gcmd.CommandText += $"( {guild.Id} , \"{guild.Name.Replace("\"", "\\").Replace("\'", "\\'")}\" ,\"{Config.Load().Prefix}\" )";
                await SqlTools.InsertAsync(gcmd);

                //Configures Modules
                gcmd = new MySqlCommand("INSERT IGNORE INTO `guildcommandmodules` " +
                    "(`ID`,`Accounts`,`Actions`,`Admin`,`Fun`,`Help`,`Information`,`Search`,`Stats`) " +
                    $"VALUES ( {guild.Id} , 1, 1, 1, 1, 1, 1, 1, 1 )");
                await SqlTools.InsertAsync(gcmd);

                //Configures Settings
                gcmd = new MySqlCommand("INSERT IGNORE INTO `guildfeaturemodules` " +                            "(`ID`,`Starboard`,`Pinning`,`Experience`,`UserJoinLeave`,`UserModification`,`UserBanEvents`,`GuildModification`,`GuildChannelModification`,`GuildRoleModification`) " +
                    $"VALUES ( {guild.Id} , 0, 0, 0, 0, 0, 0, 0, 0, 0 )");
                await SqlTools.InsertAsync(gcmd);

                Bot.Logs.Add(new Models.LogMessage("IsrtGld", $"Inserted {guild.Name}!", LogSeverity.Info));
                await Events.DiscordEvents.PopulateEntireGuildUsers(guild);
            }
        }
    }
    public class Globals
    {
        public ShardedCommandContext Context { get; set; }
    }
}
