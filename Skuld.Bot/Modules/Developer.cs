using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using Skuld.APIS;
using Skuld.Bot.Models.Commands;
using Skuld.Bot.Services;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Generic.Models;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Discord.Extensions;
using Skuld.Discord.Preconditions;
using Skuld.Discord.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group, RequireBotAdmin]
    public class Developer : InteractiveBase<ShardedCommandContext>
    {
        public SkuldConfig Configuration { get => HostSerivce.Configuration; }
        public BotListingClient BotListing { get; set; }

        [Command("stop")]
        public async Task Stop()
        {
            await "Stopping!".QueueMessageAsync(Context).ConfigureAwait(false);
            await BotService.StopBotAsync("StopCmd").ConfigureAwait(false);
        }

        [Command("dropuser")]
        public async Task DropUser(ulong userId)
        {
            using var database = new SkuldDbContextFactory().CreateDbContext();

            await database.DropUserAsync(userId);
        }

        [Command("dropguild")]
        public async Task DropGuild(ulong guildId)
        {
            using var database = new SkuldDbContextFactory().CreateDbContext();

            await database.DropGuildAsync(guildId);
        }

        [Command("bean")]
        public async Task Bean(IGuildUser user, [Remainder]string reason)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var usr = Database.Users.FirstOrDefault(x => x.Id == user.Id);

            if (usr.Banned)
            {
                usr.Banned = false;
                usr.BanReason = null;
                await $"Un-beaned {user.Mention}".QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                usr.Banned = true;
                usr.BanReason = reason;
                await $"Beaned {user.Mention}".QueueMessageAsync(Context).ConfigureAwait(false);
            }

            await Database.SaveChangesAsync().ConfigureAwait(false);
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

                    foreach (var paras in cmd.Parameters)
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

            var filename = Path.Combine(SkuldAppContext.StorageDirectory, "commands.json");

            File.WriteAllText(filename, JsonConvert.SerializeObject(Modules));

            await $"Written commands to {filename}".QueueMessageAsync(Context).ConfigureAwait(false);
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
            await Context.Client.SetGameAsync(title).ConfigureAwait(false);
        }

        [Command("resetgame"), Summary("Reset Game")]
        public async Task ResetGame()
        {
            foreach (var shard in Context.Client.Shards)
            {
                await shard.SetGameAsync($"{Configuration.Discord.Prefix}help | {shard.ShardId + 1}/{Context.Client.Shards.Count}", type: ActivityType.Listening).ConfigureAwait(false);
            }
        }

        [Command("setstream"), Summary("Sets stream")]
        public async Task Stream(string streamer, [Remainder]string title)
        {
            await Context.Client.SetGameAsync(title, "https://twitch.tv/" + streamer, ActivityType.Streaming).ConfigureAwait(false);
        }

        [Command("setactivity"), Summary("Sets activity")]
        public async Task ActivityAsync(ActivityType activityType, [Remainder]string status)
        {
            await Context.Client.SetGameAsync(status, null, activityType).ConfigureAwait(false);
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

            await $"```\n{ConsoleUtils.PrettyLines(lines, 2)}```".QueueMessageAsync(Context).ConfigureAwait(false);
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
            await embed.Build().QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("shard"), Summary("Gets the shard the guild is on")]
        public async Task ShardGet() =>
            await $"{Context.User.Mention} the server: `{Context.Guild.Name}` is on `{Context.Client.GetShardIdFor(Context.Guild)}`".QueueMessageAsync(Context).ConfigureAwait(false);

        [Command("name"), Summary("Name")]
        public async Task Name([Remainder]string name)
            => await Context.Client.CurrentUser.ModifyAsync(x => x.Username = name).ConfigureAwait(false);

        [Command("status"), Summary("Status")]
        public async Task Status(string status)
        {
            if (status.ToLower() == "online")
            { await SetStatus(UserStatus.Online).ConfigureAwait(false); }
            if (status.ToLower() == "afk")
            { await SetStatus(UserStatus.AFK).ConfigureAwait(false); }
            if (status.ToLower() == "dnd" || status.ToLower() == "do not disturb" || status.ToLower() == "donotdisturb")
            { await SetStatus(UserStatus.DoNotDisturb).ConfigureAwait(false); }
            if (status.ToLower() == "idle")
            { await SetStatus(UserStatus.Idle).ConfigureAwait(false); }
            if (status.ToLower() == "offline")
            { await SetStatus(UserStatus.Offline).ConfigureAwait(false); }
            if (status.ToLower() == "invisible")
            { await SetStatus(UserStatus.Invisible).ConfigureAwait(false); }
        }

        public async Task SetStatus(UserStatus status) =>
            await Context.Client.SetStatusAsync(status).ConfigureAwait(false);

        [Command("moneyadd"), Summary("Gives money to people"), RequireDatabase]
        public async Task GiveMoney(IGuildUser user, ulong amount)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var usr = Database.Users.FirstOrDefault(x => x.Id == user.Id);
            usr.Money += amount;

            await Database.SaveChangesAsync().ConfigureAwait(false);

            await $"User {user.Username} now has: {(await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false)).MoneyIcon}{usr.Money.ToString("N0")}".QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("leave"), Summary("Leaves a server by id")]
        public async Task LeaveServer(ulong id)
        {
            var guild = Context.Client.GetGuild(id);
            await guild.LeaveAsync().ContinueWith(async x =>
            {
                if (Context.Client.GetGuild(id) == null)
                {
                    await $"Left guild **{guild.Name}**".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                }
                else
                {
                    await $"Hmm, I haven't left **{guild.Name}**".QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
        }

        [Command("pubstats"), Summary("no")]
        public async Task PubStats()
        {
            await "Ok, publishing stats to the Discord Bot lists.".QueueMessageAsync(Context).ConfigureAwait(false);
            string list = "";
            int shardcount = Context.Client.Shards.Count;
            await BotListing.SendDataAsync(Configuration.BotListing.DiscordGGKey, Configuration.BotListing.DBotsOrgKey, Configuration.BotListing.B4DToken).ConfigureAwait(false);
            foreach (var shard in Context.Client.Shards)
            {
                list += $"I sent ShardID: {shard.ShardId} Guilds: {shard.Guilds.Count} Shards: {shardcount}\n";
            }
            await list.QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("eval"), Summary("no")]
        public async Task EvalStuff([Remainder]string code)
        {
            try
            {
                if (code.ToLowerInvariant().Contains("token") || code.ToLowerInvariant().Contains("key"))
                {
                    await "Nope.".QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
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
                    .WithReferences(typeof(SkuldDatabaseContext).Assembly)
                    .WithReferences(typeof(ShardedCommandContext).Assembly, typeof(ShardedCommandContext).Assembly,
                    typeof(SocketGuildUser).Assembly, typeof(Task).Assembly, typeof(Queryable).Assembly,
                    typeof(BotService).Assembly)
                    .WithImports(typeof(SkuldDatabaseContext).FullName)
                    .WithImports(typeof(ShardedCommandContext).FullName, typeof(ShardedCommandContext).FullName,
                    typeof(SocketGuildUser).FullName, typeof(Task).FullName, typeof(Queryable).FullName,
                    typeof(BotService).FullName);

                var script = CSharpScript.Create(code, soptions, globalsType: typeof(ShardedCommandContext));
                script.Compile();

                var result = (await script.RunAsync(globals: globals).ConfigureAwait(false)).ReturnValue;

                embed.Author = new EmbedAuthorBuilder
                {
                    Name = result.GetType().ToString()
                };
                embed.Color = EmbedUtils.RandomColor();
                embed.Description = $"{result}";
                if (result != null)
                {
                    await embed.Build().QueueMessageAsync(Context).ConfigureAwait(false);
                }
                else
                {
                    await "Result is empty or null".QueueMessageAsync(Context).ConfigureAwait(false);
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
                Log.Error("EvalCMD", "Error with eval command " + ex.Message, ex);
                await embed.Build().QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
            }
        }

        public class Globals
        {
            public ShardedCommandContext Context { get; set; }
        }
    }
}