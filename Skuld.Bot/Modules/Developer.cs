using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Skuld.APIS;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Database;
using Skuld.Discord.Commands;
using Skuld.Discord.Extensions;
using Skuld.Discord.Preconditions;
using Skuld.Discord.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Skuld.Bot.Models;
using Newtonsoft.Json;

namespace Skuld.Bot.Commands
{
    [Group, RequireBotAdmin]
    public class Developer : InteractiveBase<SkuldCommandContext>
    {
        public Random Random { get; set; }
        public SkuldConfig Configuration { get; set; }
        public BotListingClient BotListing { get; set; }

        [Command("stop")]
        public async Task Stop()
        {
            await "Stopping!".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            await BotService.StopBotAsync("StopCmd").ConfigureAwait(false);
        }

        [Command("bean")]
        public async Task Bean([Remainder]IGuildUser user)
        {
            var usrresp = await DatabaseClient.GetUserAsync(user.Id);
            if(usrresp.Successful)
            {
                var usr = usrresp.Data as SkuldUser;
                if (usr.Banned)
                {
                    usr.Banned = false;
                    var resp = await DatabaseClient.UpdateUserAsync(usr);
                    if (resp.Successful)
                        await $"Un-beaned {user.Mention}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                }
                else
                {
                    usr.Banned = true;
                    var resp = await DatabaseClient.UpdateUserAsync(usr);
                    if (resp.Successful)
                        await $"Beaned {user.Mention}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                }
            }
        }

        [Command("jsoncommands")]
        public async Task Commands()
        {
            var Modules = new List<ModuleSkuld>();

            foreach (var module in BotService.CommandService.Modules)
            {

                ModuleSkuld mod = new ModuleSkuld
                {
                    Name = module.Name,
                    Commands = new List<CommandSkuld>()
                };

                List<CommandSkuld> comm = new List<CommandSkuld>();

                foreach (var cmd in module.Commands)
                {
                    var parameters = new List<ParameterSkuld>();

                    foreach(var paras in cmd.Parameters)
                    {
                        parameters.Add(new ParameterSkuld
                        {
                            Name = paras.Name,
                            Optional = paras.IsOptional
                        });
                    }

                    mod.Commands.Add(new CommandSkuld
                    {
                        Name = cmd.Name,
                        Description = cmd.Summary,
                        Aliases = cmd.Aliases.ToArray(),
                        Parameters = parameters.ToArray()
                    });
                }
                Modules.Add(mod);
            }

            File.WriteAllText(AppContext.BaseDirectory + "commands.txt", JsonConvert.SerializeObject(Modules));

            await $"Written commands to {AppContext.BaseDirectory}commands.txt".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
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
                await ":nauseated_face: Something went wrong. Try again.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            }
        }

        [Command("resetgame"), Summary("Reset Game")]
        public async Task ResetGame()
        {
            try
            {
                foreach(var shard in Context.Client.Shards)
                {
                    await shard.SetGameAsync($"{Configuration.Discord.Prefix}help | {shard.ShardId + 1}/{Context.Client.Shards.Count}", type: ActivityType.Listening);
                }
                await Context.Message.DeleteAsync();
            }
            catch
            {
                await ":nauseated_face: Something went wrong. Try again.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
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

            await $"```\n{ConsoleUtils.PrettyLines(lines, 2)}```".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
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
            embed.AddInlineField("Guilds", shard.Guilds.Count.ToString());
            embed.AddInlineField("Status", shard.ConnectionState);
            embed.AddInlineField("Latency", shard.Latency + "ms");
            embed.AddField("Game", shard.Activity.Name);
            await embed.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("shard"), Summary("Gets the shard the guild is on")]
        public async Task ShardGet() =>
            await $"{Context.User.Mention} the server: `{Context.Guild.Name}` is on `{Context.Client.GetShardIdFor(Context.Guild)}`".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);

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
            var suserresp = await DatabaseClient.GetUserAsync(user.Id);
            if(suserresp.Successful)
            {
                if (suserresp.Data is SkuldUser suser)
                {
                    suser.Money += amount;

                    await DatabaseClient.UpdateUserAsync(suser);

                    await $"User {user.Username} now has: {Configuration.Preferences.MoneySymbol}{suser.Money}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel); ;
                }
                else
                {
                    await DatabaseClient.InsertUserAsync(user as SocketUser);
                    await GiveMoney(user, amount).ConfigureAwait(false);
                }
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
                    await $"Left guild **{guild.Name}**".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                }
                else
                {
                    await $"Hmm, I haven't left **{guild.Name}**".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                }
            });
        }

        [Command("pubstats"), Summary("no")]
        public async Task PubStats()
        {
            await "Ok, publishing stats to the Discord Bot lists.".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            string list = "";
            int shardcount = Context.Client.Shards.Count;
            await BotListing.SendDataAsync(Configuration.BotListing.SysExToken, Configuration.BotListing.DiscordGGKey, Configuration.BotListing.DBotsOrgKey, Configuration.BotListing.B4DToken);
            foreach (var shard in Context.Client.Shards)
            {
                list += $"I sent ShardID: {shard.ShardId} Guilds: {shard.Guilds.Count} Shards: {shardcount}\n";
            }
            await list.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("sql")]
        public async Task Sql(string query)
        {
            if (query.ToUpper().Contains("DROP"))
            {
                var nxt = await NextMessageAsync();
                if (nxt.Content.ToLowerInvariant().StartsWith("y"))
                {
                    await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("SQLCMD", $"{Context.User.Username}#{Context.User.Discriminator} - {Context.User.Id} just used a DROP statement, full query: \"{query}\"", LogSeverity.Critical));
                }
                else { return; }
            }
            var command = new MySql.Data.MySqlClient.MySqlCommand(query);
            var result = await DatabaseClient.SingleQueryAsync(command);

            if (result.Successful)
            {
                await $"Response: {result.Data ?? "No Response Available"}".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
            }
            else
            {
                await $"{result.Error}\nStack Trace:```{result.Exception}```".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel, null, result.Exception);
            }
        }

        [Command("rebuildusers")]
        public async Task RebuildUsers()
        {
            Thread thd = new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;
                string message = "";
                int count = 0;
                foreach (var guild in Context.Client.Guilds)
                {
                    await guild.DownloadUsersAsync();
                    foreach (var user in guild.Users)
                    {
                        count++;
                        await DatabaseClient.InsertUserAsync(user, "en-GB");
                        Thread.Sleep(2000);
                        message += count + ". Inserted\n";
                    }
                }
                await message.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            })
            {
                IsBackground = true
            };
            thd.Start();
        }

        [Command("rebuildguilds")]
        public async Task RebuildGuilds()
        {
            Thread thd = new Thread(async () =>
            {
                string message = "";
                int count = 0;
                foreach (var guild in Context.Client.Guilds)
                {
                    count++;
                    await DatabaseClient.InsertGuildAsync(guild.Id, Configuration.Discord.Prefix);
                    Thread.Sleep(2000);
                    message += count + ". Inserted\n";
                }
                await message.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            })
            {
                IsBackground = true
            };
            thd.Start();
        }

        [Command("eval"), Summary("no")]
        public async Task EvalStuff([Remainder]string code)
        {
            try
            {
                if (code.ToLowerInvariant().Contains("token") || code.ToLowerInvariant().Contains("key"))
                {
                    await "Nope.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
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
                var globals = new Globals().Context = Context as SkuldCommandContext;
                var soptions = ScriptOptions
                    .Default
                    .WithReferences(typeof(SkuldCommandContext).Assembly, typeof(ShardedCommandContext).Assembly,
                    typeof(SocketGuildUser).Assembly, typeof(Task).Assembly, typeof(Queryable).Assembly,
                    typeof(BotService).Assembly)
                    .WithImports(typeof(SkuldCommandContext).FullName, typeof(ShardedCommandContext).FullName,
                    typeof(SocketGuildUser).FullName, typeof(Task).FullName, typeof(Queryable).FullName,
                    typeof(BotService).FullName);

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
                    await embed.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                }
                else
                {
                    await "Result is empty or null".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
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
                await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("EvalCMD", "Error with eval command " + ex.Message, LogSeverity.Error, ex));
                await embed.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        public class Globals
        {
            public ShardedCommandContext Context { get; set; }
        }
    }
}