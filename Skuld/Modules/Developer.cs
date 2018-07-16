using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Skuld.APIS;
using Skuld.Commands;
using Skuld.Commands.Preconditions;
using Skuld.Core.Models;
using Skuld.Core.Services;
using Skuld.Core.Utilities;
using Skuld.Services;
using Skuld.Utilities.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Skuld.Modules
{
    [Group, RequireDeveloper]
    public class Developer : SkuldBase<ShardedCommandContext>
    {
        public MessageService MessageService { get; set; }
        public Random Random { get; set; }
        public DatabaseService Database { get; set; }
        public GenericLogger Logger { get; set; }
        public SkuldConfig Configuration { get; set; }
        public BotListingClient BotListing { get; set; }

        [Command("stop")]
        public async Task Stop()
        {
            await ReplyAsync(Context.Channel, "Stopping!");
            await HostService.BotService.StopBotAsync("StopCmd").ConfigureAwait(false);
        }

        [Command("bean")]
        public async Task Bean([Remainder]IGuildUser user)
        {
            var usr = await Database.GetUserAsync(user.Id);
            if(usr.Banned)
            {
                usr.Banned = false;
                await ReplyAsync(Context.Channel, $"Un-beaned {user.Mention}");
            }
            else
            {
                usr.Banned = true;
                await ReplyAsync(Context.Channel, $"Beaned {user.Mention}");
            }
            await Database.UpdateUserAsync(usr);
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
                await ReplyAsync(Context.Channel, $":nauseated_face: Something went wrong. Try again.");
            }
        }

        [Command("resetgame"), Summary("Reset Game")]
        public async Task ResetGame()
        {
            try
            {
                await Context.Client.SetGameAsync($"{MessageService.config.Prefix}help | {Random.Next(0, Context.Client.Shards.Count) + 1}/{Context.Client.Shards.Count}");
                await Context.Message.DeleteAsync();
            }
            catch
            {
                await ReplyAsync(Context.Channel, $":nauseated_face: Something went wrong. Try again.");
            }
        }

        [Command("setstream"), Summary("Sets stream")]
        public async Task Stream(string streamer, [Remainder]string title)
        {
            await Context.Client.SetGameAsync(title, "https://twitch.tv/" + streamer, ActivityType.Streaming);
        }

        [Command("setactivity"), Summary("Sets activity")]
        public async Task ActivityAsync(ActivityType activityType, [Remainder]string status)
        {
            await Context.Client.SetGameAsync(status, null, activityType);
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

            await ReplyAsync(Context.Channel, "```" + ConsoleUtils.PrettyLines(lines, 2) + "```");
        }

        [Command("getshard"), Summary("Gets all information about specific shard")]
        public async Task ShardGet(int shardid = -1)
        {
            if (shardid > -1)
                await ShardInfo(shardid).ConfigureAwait(false);
            else
                await ShardInfo(Context.Client.GetShardIdFor(Context.Guild)).ConfigureAwait(false);
        }

        public async Task ShardInfo(int shardid)
        {
            var shard = Context.Client.GetShard(shardid);
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = $"Shard ID: {shardid}",
                    IconUrl = shard.CurrentUser.GetAvatarUrl()
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = "Generated at"
                },
                Timestamp = DateTime.Now,
                Color = EmbedUtils.RandomColor()
            };
            embed.AddField("Guilds", shard.Guilds.Count.ToString(), true);
            embed.AddField("Status", shard.ConnectionState, true);
            embed.AddField("Latencty", shard.Latency + "ms", true);
            await ReplyAsync(Context.Channel, embed.Build());
        }

        [Command("shard"), Summary("Gets the shard the guild is on")]
        public async Task ShardGet() =>
            await ReplyAsync(Context.Channel, $"{Context.User.Mention} the server: `{Context.Guild.Name}` is on `{Context.Client.GetShardIdFor(Context.Guild)}`");

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
            var suser = await Database.GetUserAsync(user.Id);
            if (suser != null)
            {
                suser.Money += amount;

                await Database.UpdateUserAsync(suser);

                await ReplyAsync(Context.Channel, $"User {user.Username} now has: {Configuration.Preferences.MoneySymbol + suser.Money}");
            }
            else
            {
                await Database.InsertUserAsync(user as SocketUser);
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
                    await ReplyAsync(Context.Channel, $"Left guild **{guild.Name}**");
                }
                else
                {
                    await ReplyAsync(Context.Channel, $"Hmm, I haven't left **{guild.Name}**");
                }
            });
        }

        [Command("pubstats"), Summary("no")]
        public async Task PubStats()
        {
            await ReplyAsync(Context.Channel, "Ok, publishing stats to the Discord Bot lists.");
            string list = "";
            int shardcount = Context.Client.Shards.Count;
            await BotListing.SendDataAsync(Configuration.BotListing.SysExToken, Configuration.BotListing.DiscordPWKey, Configuration.BotListing.DBotsOrgKey);
            foreach (var shard in Context.Client.Shards)
            {
                list += $"I sent ShardID: {shard.ShardId} Guilds: {shard.Guilds.Count} Shards: {shardcount}\n";
            }
            await ReplyAsync(Context.Channel, list);
        }

        [Command("sql")]
        public async Task Sql(string query)
        {
            var command = new MySql.Data.MySqlClient.MySqlCommand(query);
            var result = await Database.SingleQueryAsync(command);

            if (result.Successful)
            {
                await ReplyAsync(Context.Channel, $"Success!\n\nResponce: {result.Data ?? "No Responce Available"}");
            }
            else
            {
                await ReplyAsync(Context.Channel, $"{result.Error}\nStack Trace:```{result.Exception}```");
            }
        }

        [Command("rebuildguilds")]
        public async Task RebuildGuilds()
        {
            string message = "";
            int count = 0;
            Thread thd = new Thread(async () =>
            {
                foreach (var guild in Context.Client.Guilds)
                {
                    count++;
                    await Database.InsertGuildAsync(guild);
                    Thread.Sleep(2000);
                    message += count + ". Inserted\n";
                }
            })
            {
                IsBackground = true
            };
            thd.Start();
            if (message != "")
            { await ReplyAsync(Context.Channel, message); }
        }

        [Command("rebuildusers")]
        public async Task RebuildUsers()
        {
            string message = "";
            int count = 0;
            Thread thd = new Thread(async () =>
            {
                foreach (var guild in Context.Client.Guilds)
                {
                    await guild.DownloadUsersAsync();
                    foreach (var user in guild.Users)
                    {
                        count++;
                        var usr = await Database.GetUserAsync(user.Id);
                        if (usr == null) await Database.InsertUserAsync(user);
                        else
                        {
                            await Database.SingleQueryAsync(new MySql.Data.MySqlClient.MySqlCommand($"UPDATE `users` SET `AvatarUrl` = \"{user.GetAvatarUrl()}\" WHERE `UserID` = {user.Id};"));
                        }
                        Thread.Sleep(2000);
                        message += count + ". Inserted\n";
                    }
                }
            })
            {
                IsBackground = true
            };
            thd.Start();
            if (message != "")
            { await ReplyAsync(Context.Channel, message); }
        }

        [Command("eval"), Summary("no")]
        public async Task EvalStuff([Remainder]string code)
        {
            try
            {
                if (code.ToLowerInvariant().Contains("token") || code.ToLowerInvariant().Contains("config"))
                {
                    await ReplyAsync(Context.Channel, "Nope.");
                    return;
                }
                if (code.StartsWith("```cs", StringComparison.Ordinal))
                {
                    code = code.Replace("`", "");
                    code = code.Remove(0, 2);
                }
                else if (code.StartsWith("```", StringComparison.Ordinal))
                {
                    code = code.Replace("`", "");
                }

                var embed = new EmbedBuilder();
                var globals = new Globals().Context = Context as ShardedCommandContext;
                var soptions = ScriptOptions
                    .Default
                    .WithReferences(typeof(ShardedCommandContext).Assembly, typeof(Program).Assembly, typeof(SocketGuildUser).Assembly, typeof(Task).Assembly, typeof(Queryable).Assembly)
                    .WithImports(typeof(ShardedCommandContext).FullName, typeof(Program).FullName, typeof(SocketGuildUser).FullName, typeof(Task).FullName, typeof(Queryable).FullName);

                var script = CSharpScript.Create(code, soptions, globalsType: typeof(ShardedCommandContext));
                script.Compile();

                var result = (await script.RunAsync(globals: globals)).ReturnValue;

                embed.Author = new EmbedAuthorBuilder
                {
                    Name = result.GetType().ToString()
                };
                embed.Color = EmbedUtils.RandomColor();
                embed.Description = $"{result}";
                if (result != null)
                {
                    await ReplyAsync(Context.Channel, embed.Build());
                }
                else
                {
                    await ReplyAsync(Context.Channel, "Result is empty or null");
                }
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (NullReferenceException ex) { /*Do nothing here*/ }
#pragma warning restore CS0168 // Variable is declared but never used
            catch (Exception ex)
            {
                var embed = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = "ERROR WITH EVAL"
                    },
                    Color = Color.Red,
                    Description = $"{ex.Message}"
                };
                await Logger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("EvalCMD", "Error with eval command " + ex.Message, LogSeverity.Error, ex));
                await ReplyAsync(Context.Channel, embed.Build());
            }
        }

        public class Globals
        {
            public ShardedCommandContext Context { get; set; }
        }
    }
}