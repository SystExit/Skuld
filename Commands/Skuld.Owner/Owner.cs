using System.Threading.Tasks;
using Discord.Commands;
using Skuld.Modules;
using Skuld.Tools;
using Discord;
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Skuld.Owner
{
    [RequireOwner, Group, Name("Owner")]
    public class Owner : ModuleBase
    {
        [Command("unload", RunMode = RunMode.Async)]
        public async Task UnLoadModule(string module)
        {
            if (await ModuleHandler.UnloadSpecificModule(module)) await MessageHandler.SendChannel(Context.Channel, $"Ok, unloaded module `{module}`, check the logs for confirmation!");
            else await MessageHandler.SendChannel(Context.Channel, $"I think it's either already unloaded or I couldn't find the module. :thinking:");
        }
        [Command("load", RunMode = RunMode.Async)]
        public async Task LoadModule(string module)
        {
            if (await ModuleHandler.LoadSpecificModule(module)) await MessageHandler.SendChannel(Context.Channel, $"Ok, loaded module `{module}`, check the logs for confirmation!");
            else await MessageHandler.SendChannel(Context.Channel, $"I think it's either already loaded or I couldn't find the module. :thinking:");
        }
        [Command("reload", RunMode = RunMode.Async)]
        public async Task ReloadModule(string module)
        {
            await ModuleHandler.ReloadSpecific(module);
            await MessageHandler.SendChannel(Context.Channel, $"I reloaded the `{module}` module :blush:");
        }
        [Command("reload", RunMode = RunMode.Async)]
        public async Task ReloadModule()
        {
            await ModuleHandler.ReloadAll();
            await MessageHandler.SendChannel(Context.Channel, "I reloaded all modules. :blush:");
        }
        [Command("stop", RunMode = RunMode.Async)]
        public async Task Stop() { await MessageHandler.SendChannel(Context.Channel, "Stopping!");  await Bot.StopBot("StopCmd"); }
        [Command("populate", RunMode = RunMode.Async)]
        public async Task Populate() { await MessageHandler.SendChannel(Context.Channel, "Starting to populate guilds and users o7!"); await Events.DiscordEvents.PopulateGuilds(); }
        [Command("shardrestart", RunMode = RunMode.Async), Summary("Restarts shard")]
        public async Task ReShard(int shard)
        {

            await Bot.bot.GetShard(shard).StopAsync();
            await Task.Delay(TimeSpan.FromSeconds(30));
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
                await Bot.bot.SetGameAsync(title, "https://twitch.tv/domarije", StreamType.Twitch);
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
            Random rnd = new Random();
            try
            {
                await Bot.bot.SetGameAsync($"{Bot.Prefix}help | {rnd.Next(0, Bot.bot.Shards.Count) + 1}/{Bot.bot.Shards.Count}");
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
            var lines = new List<string[]>();
            lines.Add(new string[] { "Shard", "State", "Latency", "Guilds" });

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
            EmbedBuilder _embed = new EmbedBuilder();
            EmbedAuthorBuilder _auth = new EmbedAuthorBuilder();
            EmbedFooterBuilder _foot = new EmbedFooterBuilder();
            _embed.Color = RandColor.RandomColor();

            _auth.Name = $"Shard ID: {shardid}";
            _auth.IconUrl = shard.CurrentUser.GetAvatarUrl();

            _foot.Text = "Generated at";
            _embed.Footer = _foot;
            _embed.Author = _auth;
            _embed.Timestamp = DateTime.Now;
            _embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Guilds";
                x.Value = shard.Guilds.Count;
            });
            _embed.AddField(x =>
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
            _embed.AddField(x =>
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
            await MessageHandler.SendChannel(Context.Channel, "", _embed);
        }
        [Command("shard", RunMode = RunMode.Async), Summary("Gets the shard the guild is on")]
        public async Task ShardGet() { await MessageHandler.SendChannel(Context.Channel, $"{Context.User.Mention} the server: `{Context.Guild.Name}` is on `{Bot.bot.GetShardIdFor(Context.Guild)}`"); }
        [Command("name", RunMode = RunMode.Async), Summary("Name")]
        public async Task Name([Remainder]string name) { await Bot.bot.CurrentUser.ModifyAsync(x => x.Username = name); }
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
            string message = "Resut:```cs\n";
            var reader = await Sql.GetAsync(new MySqlCommand(query));
            if(reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    for(int i = 0;i<reader.FieldCount;i++)
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
            await Sql.getconn.CloseAsync();
            await MessageHandler.SendChannel(Context.Channel, message);
        }
        [Command("moneyadd", RunMode = RunMode.Async), Summary("Gives money to people")]
        public async Task GiveMoney(IGuildUser user, int amount)
        {
            var command = new MySqlCommand("select id from accounts where ID = @userid");
            command.Parameters.AddWithValue("@userid", user.Id);
            if(!string.IsNullOrEmpty(await Sql.GetSingleAsync(command)))
            {
                int oldmoney = Convert.ToInt32(await Sql.GetSingleAsync(command));
                int newmoney = oldmoney + amount;

                command = new MySqlCommand("UPDATE accounts SET money = @newmoney WHERE ID = @userid");
                command.Parameters.AddWithValue("@userid", user.Id);
                command.Parameters.AddWithValue("@newmoney", newmoney);
                await Sql.InsertAsync(command);
                command = new MySqlCommand("select money from accounts where ID = @userid");
                command.Parameters.AddWithValue("@userid", user.Id);
                int newnewmoney = Convert.ToInt32(await Sql.GetSingleAsync(command));
                await MessageHandler.SendChannel(Context.Channel, $"User {user.Username} now has: {Config.Load().MoneySymbol + newnewmoney}");
            }
            else
            {
                command = new MySqlCommand();
                command.CommandText = "INSERT IGNORE INTO `accounts` (`ID`, `username`, `description`) VALUES (@userid , @username , \"I have no description\");";
                command.Parameters.AddWithValue("@userid", user.Id);
                command.Parameters.AddWithValue("@username", $"{user.Username.Replace("\"", "\\\"").Replace("\'", "\\'")}#{user.DiscriminatorValue}");
                await Sql.InsertAsync(command);
                await GiveMoney(user, amount);
            }
        }

        [Command("leave", RunMode = RunMode.Async), Summary("Leaves a server by id")]
        public async Task LeaveServer(ulong id)
        {
            var client = Bot.bot;
            var guild = client.GetGuild(id);
            await guild.LeaveAsync().ContinueWith(async x =>
            {
                var appinfo = await client.GetApplicationInfoAsync();
                var owner = appinfo.Owner;
                var dmchannel = await owner.GetOrCreateDMChannelAsync();
                if (client.GetGuild(id) == null)
                {
                    await dmchannel.SendMessageAsync($"Left guild **{guild.Name}**");
                }
                else
                {
                    await dmchannel.SendMessageAsync($"Hmm, I haven't left **{guild.Name}**");
                }
            });
        }
    }
}
