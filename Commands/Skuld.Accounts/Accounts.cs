using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Skuld.Models;
using Skuld.Tools;
using MySql.Data.MySqlClient;

namespace Skuld.Commands
{
    [Group, Name("Accounts")]

    public class Accounts : ModuleBase
    {
        [Command("money", RunMode = RunMode.Async), Summary("Gets the amount of money you have")]
        public async Task GetYourMoney() =>
            await GetMoney(Context.User as IGuildUser);
        [Command("money",RunMode = RunMode.Async), Summary("Gets a users amount of money")]
        public async Task GetMoney(IGuildUser user)
        {
            var command = new MySqlCommand("select money from accounts where ID = @id");
            command.Parameters.AddWithValue("@id", Context.User.Id);
            var resp = await Sql.GetSingleAsync(command);
            ulong money = 0;
            if (!String.IsNullOrEmpty(resp))
                money = Convert.ToUInt64(resp);
            else
                await InsertUser(Context.User);
            await MessageHandler.SendChannel(Context.Channel, $"You have: {Config.Load().MoneySymbol + money.ToString("N0")}");
        }

        [Command("profile", RunMode= RunMode.Async), Summary("Get your profile")]
        public async Task Profile() =>
            await GetProfile(Context.User as IGuildUser);
        [Command("profile", RunMode = RunMode.Async), Summary("Get users profile")]
        public async Task GetProfile([Remainder]IGuildUser user)
        {
            SkuldUser User = new SkuldUser();
            EmbedBuilder embed = new EmbedBuilder();
            embed.Color = RandColor.RandomColor();
            var command = new MySqlCommand("SELECT * FROM accounts WHERE ID = @userid");
            command.Parameters.AddWithValue("@userid", user.Id);
            using (var reader = await Sql.GetAsync(command))
            {
                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        User.Username = Convert.ToString(reader["username"]);
                        User.Description = Convert.ToString(reader["description"]);
                        User.Money = Convert.ToUInt64(reader["money"]);
                        User.LuckFactor = Convert.ToDouble(reader["luckfactor"]);
                        User.Daily = reader["daily"].ToString();
                    }
                    command = new MySqlCommand("SELECT * FROM commandusage WHERE UserID = @userid ORDER BY UserUsage DESC LIMIT 1");
                    command.Parameters.AddWithValue("@userid", user.Id);
                    var resp = await Sql.GetAsync(command);
                    if (resp.HasRows)
                    {
                        while (await resp.ReadAsync())
                        {
                            var com = Convert.ToString(resp["command"]);
                            var comusg = Convert.ToUInt64(resp["UserUsage"]);
                            User.FavCmd = com;
                            User.FavCmdUsg = comusg;
                        }
                    }
                    embed.Author = new EmbedAuthorBuilder()
                    {
                        Name = User.Username,
                        IconUrl = user.GetAvatarUrl() ?? "http://www.emoji.co.uk/files/mozilla-emojis/smileys-people-mozilla/11419-bust-in-silhouette.png"
                    };
                    embed.AddInlineField("Description", User.Description ?? "No Description");
                    embed.AddInlineField(Config.Load().MoneyName, User.Money.Value.ToString("N0") ?? "No Money");
                    embed.AddInlineField("Luck Factor", User.LuckFactor.ToString("P2") ?? "No LuckFactor");
                    embed.AddInlineField("Daily", User.Daily ?? "Not used Daily");
                    embed.AddInlineField("Favourite Command", $"`{User.FavCmd}` and it has been used {User.FavCmdUsg} times" ?? "No favourite Command");
                    await MessageHandler.SendChannel(Context.Channel, "", embed);
                }
                else
                {
                    await MessageHandler.SendChannel(Context.Channel, "Error!! Fixing...", null, 5);
                    await InsertUser(user);
                }
            }
        }

        [Command("profile-ext", RunMode = RunMode.Async), Summary("Gets extended information about you")]
        public async Task ProfileExt() =>
            await GetProfileExt(Context.User as IGuildUser);
        [Command("profile-ext", RunMode = RunMode.Async), Summary("Gets extended information about a user")]
        public async Task GetProfileExt([Remainder]IGuildUser user)
        {
            SkuldUser User = new SkuldUser();
            EmbedBuilder embed = new EmbedBuilder();
            EmbedAuthorBuilder auth = new EmbedAuthorBuilder();
            embed.Color = RandColor.RandomColor();
            var command = new MySqlCommand("SELECT * FROM accounts WHERE ID = @userid");
            command.Parameters.AddWithValue("@userid", user.Id);
            using (var reader = await Sql.GetAsync(command))
            {
                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        User.Username = User.Username = Convert.ToString(reader["username"]);
                        User.Description = Convert.ToString(reader["description"]);
                        User.Money = Convert.ToUInt64(reader["money"]);
                        User.LuckFactor = Convert.ToDouble(reader["luckfactor"]);
                        User.Daily = reader["daily"].ToString();
                        User.Glares = Convert.ToUInt32(reader["glares"]);
                        User.GlaredAt = Convert.ToUInt32(reader["glaredat"]);
                        User.Pets = Convert.ToUInt32(reader["pets"]);
                        User.Petted = Convert.ToUInt32(reader["petted"]);
                        User.HP = Convert.ToUInt32(reader["hp"]);
                    }
                    command = new MySqlCommand("SELECT * FROM commandusage WHERE UserID = @userid ORDER BY UserUsage DESC LIMIT 1");
                    command.Parameters.AddWithValue("@userid", user.Id);
                    var resp = await Sql.GetAsync(command);
                    if (resp.HasRows)
                    {
                        while (await resp.ReadAsync())
                        {
                            var com = Convert.ToString(resp["command"]);
                            var comusg = Convert.ToUInt64(resp["UserUsage"]);
                            User.FavCmd = com;
                            User.FavCmdUsg = comusg;
                        }
                    }
                    auth.Name = User.Username;
                    auth.IconUrl = user.GetAvatarUrl();
                    embed.Author = new EmbedAuthorBuilder(){Name = User.Username,IconUrl = user.GetAvatarUrl() ?? "http://www.emoji.co.uk/files/mozilla-emojis/smileys-people-mozilla/11419-bust-in-silhouette.png"};
                    embed.AddInlineField("Description", User.Description ?? "No Description");
                    embed.AddInlineField(Config.Load().MoneyName, User.Money.Value.ToString("N0") ?? "No Money");
                    embed.AddInlineField("Luck Factor", User.LuckFactor.ToString("P2") ?? "No LuckFactor");
                    embed.AddInlineField("Daily", User.Daily ?? "Not used Daily");
                    embed.AddInlineField("Glares", User.Glares + " times" ?? "Not glared at anyone");
                    embed.AddInlineField("Glared At", User.GlaredAt + " times" ?? "Not been glared at");
                    if(User.Pets>0)
                        embed.AddInlineField("Pets",User.Pets + " times");
                    else
                        embed.AddInlineField("Pets", "Not been petted");
                    if (User.Petted>0)
                        embed.AddInlineField("Petted", User.Petted + " times");
                    else
                        embed.AddInlineField("Petted", "Not petted anyone");
                    if(User.HP>0)
                        embed.AddInlineField("HP",User.HP);
                    else
                        embed.AddInlineField("HP", "No HP");
                    embed.AddInlineField("Favourite Command", $"`{User.FavCmd}` and it has been used {User.FavCmdUsg} times" ?? "No favourite Command");
                    await MessageHandler.SendChannel(Context.Channel, "", embed);
                }
                else
                {
                    await MessageHandler.SendChannel(Context.Channel, "Error!! Fixing...", null, 5);
                    await InsertUser(user);
                }
            }
        }

        [Command("description", RunMode = RunMode.Async), Summary("Sets description, this is archived and will be stored")]
        public async Task SetDescription([Remainder]string description)
        {
            var command = new MySqlCommand("UPDATE accounts SET description = @description WHERE ID = @userid");
            command.Parameters.AddWithValue("@userid", Context.User.Id);
            command.Parameters.AddWithValue("@description", description);
            await Sql.InsertAsync(command).ContinueWith(async x =>
            {
                if (x.IsCompleted)
                    await MessageHandler.SendChannel(Context.Channel,$"Successfully set your description to **{description}**");
            });
        }
        [Command("description", RunMode = RunMode.Async), Summary("Gets description")]
        public async Task GetDescription()
        {
            var command = new MySqlCommand("SELECT Description FROM accounts WHERE ID = @userid");
            command.Parameters.AddWithValue("@userid", Context.User.Id);
            await MessageHandler.SendChannel(Context.Channel, $"Successfully set your description to **{await Sql.GetSingleAsync(command)}**");
        }

        [Command("me", RunMode = RunMode.Async), Summary("Alias of sk!whois {tag yourself}")]
        public async Task Whois() =>
            await GetProile(Context.User as IGuildUser);
        [Command("whois", RunMode = RunMode.Async), Summary("Get's information about a user"),Alias("user")]
        public async Task GetProile([Remainder]IGuildUser whois)
        {
            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = RandColor.RandomColor(),
                Author = new EmbedAuthorBuilder() { IconUrl = whois.GetAvatarUrl(ImageFormat.Auto)??"", Name = whois.Username??"Unknown" },
                ImageUrl = whois.GetAvatarUrl(ImageFormat.Auto)??""
            };
            int seencount = 0;
            foreach (var item in Bot.bot.Guilds)
                if (item.GetUser(whois.Id) != null)
                    seencount++;
            string game = null;
            if (whois.Game.HasValue)
            {
                if (whois.Game.Value.StreamType != StreamType.NotStreaming)
                    game = $"[{whois.Game.Value.Name}]({whois.Game.Value.StreamUrl})";
                else
                    game = whois.Game.Value.Name;
            }                    
            else
                game = "Nothing";
            embed.AddInlineField(":id: ID", whois.Id.ToString()??"Unknown");
            embed.AddInlineField(":video_game: Currently Playing",game);
            embed.AddInlineField(":information_source: Status", whois.Status.ToString()??"Unknown");
            embed.AddInlineField(":robot: Bot?", whois.IsBot.ToString()??"Unknown");
            embed.AddInlineField(":eyes: Mutual Servers", $"{seencount} servers");
            embed.AddInlineField(":eyes: Last Seen", "Shard: "+(Bot.bot.GetShardIdFor(Context.Guild).ToString()??"Unknown"));
            embed.AddInlineField(":shield: Roles", $"Do `{Config.Load().Prefix}roles` to see your roles");
            embed.AddField(":inbox_tray: Server Join", whois.JoinedAt.Value.ToString("dd'/'MM'/'yyyy hh:mm:ss tt")+"\t`DD/MM/YYYY`");
            embed.AddField(":globe_with_meridians: Discord Join", whois.CreatedAt.ToString("dd'/'MM'/'yyyy hh:mm:ss tt") + "\t`DD/MM/YYYY`");
            await MessageHandler.SendChannel(Context.Channel, "", embed);
        }
        [Command("roles", RunMode = RunMode.Async), Summary("Gets your current roles")]
        public async Task GetRole() =>
            await GetRole(Context.User as IGuildUser);
        [Command("roles", RunMode = RunMode.Async), Summary("Gets a users current roles")]
        public async Task GetRole(IGuildUser user)
        {
            var guild = Context.Guild;
            var userroles = user.RoleIds;
            var roles = userroles.Select(query => guild.GetRole(query).Name).Aggregate((current, next) => current.TrimStart('@') + ", " + next);
            string username=null;
            if (!String.IsNullOrEmpty(user.Nickname))
                username = user.Nickname + "#" + user.DiscriminatorValue;
            else
                username = user.Username + "#" + user.DiscriminatorValue;
            await MessageHandler.SendChannel(Context.Channel, "Roles of __**" + username + "**__\n\n`" + (roles??"No roles")+"`");
        }

        [Command("daily", RunMode = RunMode.Async), Summary("Daily Money")]
        public async Task Daily()
        {
            DateTime olddate = new DateTime();
            var command = new MySqlCommand("select daily from accounts where ID = @userid");
            command.Parameters.AddWithValue("@userid", Context.User.Id);
            var resp = await Sql.GetSingleAsync(command);
            if (!String.IsNullOrEmpty(resp))
            {
                olddate = Convert.ToDateTime(resp);
                DateTime newdate = olddate.AddHours(23).AddMinutes(59).AddSeconds(59);
                var temp = DateTime.Compare(newdate, DateTime.UtcNow);
                if (temp == 1)
                {
                    DateTime calcdate = olddate;
                    DateTime extraday = calcdate.AddDays(1);
                    TimeSpan remain = extraday.Subtract(DateTime.UtcNow);
                    string remaining = remain.Hours + " Hours " + remain.Minutes + " Minutes " + remain.Seconds + " Seconds";
                    await MessageHandler.SendChannel(Context.Channel, $"You must wait `{remaining}`");
                }
                else
                    await NewDaily(Context.User);
            }
            else
                await NewDaily(Context.User);
        }
        private async Task NewDaily(IUser user)
        {
            ulong oldmoney = 0;
            var command = new MySqlCommand("select money from accounts where ID = @userid");
            command.Parameters.AddWithValue("@userid", Context.User.Id);
            var tresp = await Sql.GetSingleAsync(command);
            if (!String.IsNullOrEmpty(tresp))
                oldmoney = Convert.ToUInt64(tresp);
            ulong newamount = oldmoney + Config.Load().DailyAmount;

            command = new MySqlCommand("UPDATE accounts SET money = @money, daily = @daily WHERE ID = @userid");
            command.Parameters.AddWithValue("@userid", Context.User.Id);
            command.Parameters.AddWithValue("@money", newamount);
            command.Parameters.AddWithValue("@daily", DateTime.UtcNow);
            await Sql.InsertAsync(command).ContinueWith(async x =>
            {
                if (x.IsCompleted)
                {
                    await MessageHandler.SendChannel(Context.Channel, $"You now have: {Config.Load().MoneySymbol + newamount.ToString("N0")}");
                }
            });
        }
        [Command("daily", RunMode = RunMode.Async), Summary("Daily Money")]
        public async Task GiveDaily(IUser usertogive)
        {
            if (usertogive.IsBot) { await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Author = new EmbedAuthorBuilder() { Name = "Error with the command" }, Description = "Bot Accounts cannot be given any money", Color = new Color(255,0,0) }); }
            else
            {
                DateTime olddate = new DateTime();
                var command = new MySqlCommand("select daily from accounts where ID = @userid");
                command.Parameters.AddWithValue("@userid", Context.User.Id);
                var resp = await Sql.GetSingleAsync(command);
                if (!String.IsNullOrEmpty(resp))
                {
                    olddate = Convert.ToDateTime(resp);
                    DateTime newdate = olddate.AddHours(23).AddMinutes(59).AddSeconds(59);
                    var temp = DateTime.Compare(newdate, DateTime.UtcNow);
                    if (temp == 1)
                    {
                        DateTime calcdate = olddate;
                        DateTime extraday = calcdate.AddDays(1);
                        TimeSpan remain = extraday.Subtract(DateTime.UtcNow);
                        string remaining = remain.Hours + " Hours " + remain.Minutes + " Minutes " + remain.Seconds + " Seconds";
                        await MessageHandler.SendChannel(Context.Channel, $"You must wait `{remaining}`");
                    }
                    else
                    {
                        ulong oldmoney = 0;
                        command = new MySqlCommand("select money from accounts where ID = @userid");
                        command.Parameters.AddWithValue("@userid", usertogive.Id);
                        var tresp = await Sql.GetSingleAsync(command);
                        if (!String.IsNullOrEmpty(tresp))
                            oldmoney = Convert.ToUInt64(tresp);
                        ulong newamount = oldmoney + Config.Load().DailyAmount;

                        command = new MySqlCommand("UPDATE ACCOUNTS SET Daily = @daily from accounts where ID = @userid");
                        command.Parameters.AddWithValue("@userid", Context.User.Id);
                        command.Parameters.AddWithValue("@daily", DateTime.UtcNow);
                        await Sql.InsertAsync(command).ContinueWith(async x =>
                        {
                            if (x.IsCompleted)
                            {
                                command = new MySqlCommand("UPDATE ACCOUNTS SET money = @money from accounts where ID = @userid");
                                command.Parameters.AddWithValue("@userid", usertogive.Id);
                                command.Parameters.AddWithValue("@money", newamount);
                                await Sql.InsertAsync(command).ContinueWith(async z =>
                                {
                                    if (z.IsCompleted)
                                    {
                                        await MessageHandler.SendChannel(Context.Channel, $"You just gave {usertogive.Mention} {Config.Load().MoneySymbol + Config.Load().DailyAmount}");
                                    }
                                });
                            }
                        });
                    }
                }
                else
                    await NewDaily(usertogive);
            }            
        }

        [Command("give", RunMode = RunMode.Async), Summary("Give away ur money")]
        public async Task Give(IGuildUser user, ulong amount)
        {
            if (amount < 0) { await MessageHandler.SendChannel(Context.Channel, "HEY! Stop trying to reduce their money. >:("); }
            if (amount == 0) { await MessageHandler.SendChannel(Context.Channel,"Why would you want to give zero money to someone? :thinking:"); }
            else
            {
                var sender = Context.User;
                ulong oldrecipmoney = 0;
                ulong oldsendmoney = 0;
                var command = new MySqlCommand("SELECT money FROM accounts WHERE ID = @userid");
                command.Parameters.AddWithValue("@userid", user.Id);
                var reader = await Sql.GetSingleAsync(command);
                if (!String.IsNullOrEmpty(reader))
                    oldrecipmoney = Convert.ToUInt64(reader);
                else
                    await InsertUser(Context.User);
                command = new MySqlCommand("SELECT money FROM accounts WHERE ID = @userid");
                command.Parameters.AddWithValue("@userid", sender.Id);
                var reader2 = await Sql.GetSingleAsync(command);
                if (!String.IsNullOrEmpty(reader2))
                    oldsendmoney = Convert.ToUInt64(reader2);
                else
                    await InsertUser(Context.User);
                if (oldsendmoney < amount) { await MessageHandler.SendChannel(Context.Channel, "ERROR: You are trying to give more than you currently have. Please select an amount under `" + oldsendmoney + "`");  }
                else
                {
                    var newrecipmoney = oldrecipmoney + amount;
                    var newsendmoney = oldsendmoney - amount;

                    command = new MySqlCommand("UPDATE accounts SET money = @money WHERE ID = @userid");
                    command.Parameters.AddWithValue("@userid", user.Id);
                    command.Parameters.AddWithValue("@money", newrecipmoney);
                    await Sql.InsertAsync(command).ContinueWith(async x =>
                    {
                        command = new MySqlCommand("UPDATE accounts SET money = @money WHERE ID = @userid");
                        command.Parameters.AddWithValue("@userid", sender.Id);
                        command.Parameters.AddWithValue("@money", newsendmoney);
                        await Sql.InsertAsync(command);
                        ulong temprecp = 0, tempsend = 0;
                        command = new MySqlCommand("SELECT money FROM accounts WHERE ID = @userid");
                        command.Parameters.AddWithValue("@userid", user.Id);
                        var reader3 = await Sql.GetSingleAsync(command);
                        if (!String.IsNullOrEmpty(reader3))
                            temprecp = Convert.ToUInt64(reader);
                        else
                            await InsertUser(Context.User);
                        command = new MySqlCommand("SELECT money FROM accounts WHERE ID = @userid");
                        command.Parameters.AddWithValue("@userid", sender.Id);
                        var reader4 = await Sql.GetSingleAsync(command);
                        if (!String.IsNullOrEmpty(reader4))
                            tempsend = Convert.ToUInt64(reader);
                        else
                            await InsertUser(Context.User);
                        if (x.IsCompleted)
                            await MessageHandler.SendChannel(Context.Channel,$"Successfully gave **{user.Username}** {Config.Load().MoneySymbol + amount}");
                    });
                }
            }
        }
        [Command("syncname", RunMode = RunMode.Async), Summary("Syncs your username in the database")]
        public async Task SyncName()
        {
            MySqlCommand command = new MySqlCommand()
            {
                CommandText = "SELECT username FROM accounts WHERE ID = @userid"
            };
            command.Parameters.AddWithValue("@userid", Context.User.Id);
            var username = await Sql.GetSingleAsync(command);
            if(Context.User.Username+"#"+Context.User.DiscriminatorValue != username)
            {
                MySqlCommand cmd = new MySqlCommand()
                {
                    CommandText = "UPDATE accounts SET username = @username WHERE ID = @userid"
                };
                cmd.Parameters.AddWithValue("@username", $"{Context.User.Username.Replace("\"", "\\").Replace("\'", "\\'")}#{Context.User.DiscriminatorValue}");
                cmd.Parameters.AddWithValue("@userid", Context.User.Id);
                await Sql.InsertAsync(cmd).ContinueWith(async x=>
                {
                if (x.IsCompleted)
                    await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Author = new EmbedAuthorBuilder() { Name = "Success" }, Description = "Synced your username :D", Color = RandColor.RandomColor() });
                });

            }
            else
            {
                await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Author = new EmbedAuthorBuilder() { Name = "Error" }, Description = "Your username is already the same as the one I have on record.", Color = new Color(255, 0, 0) });
            }
        }
        private async Task InsertUser(IUser user)
        {
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "INSERT IGNORE INTO `accounts` (`ID`, `username`, `description`) VALUES (@userid , @username, \"I have no description\");";
            command.Parameters.AddWithValue("@username", $"{user.Username.Replace("\"", "\\").Replace("\'", "\\'")}#{user.DiscriminatorValue}");
            command.Parameters.AddWithValue("@userid", user.Id);
            await Sql.InsertAsync(command);
        }
    }
}
