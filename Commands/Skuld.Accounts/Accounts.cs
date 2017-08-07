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
        [Command("money", RunMode = RunMode.Async), Summary("gets the anount of money you have")]
        public async Task GetMoney()
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
        public async Task Profile()
        {
            await GetProfile(Context.User);
        }
        [Command("profile", RunMode = RunMode.Async), Summary("Get users profile")]
        public async Task Profile(IGuildUser user)
        {
            await GetProfile(user as IUser);
        }
        public async Task GetProfile(IUser user)
        {
            SkuldUser User = new SkuldUser();
            EmbedBuilder embed = new EmbedBuilder();
            EmbedAuthorBuilder auth = new EmbedAuthorBuilder();
            embed.Color = RandColor.RandomColor();
            var command = new MySqlCommand("SELECT * FROM accounts WHERE ID = @userid");
            command.Parameters.AddWithValue("@userid",user.Id);
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
                        User.PrevCmd = Convert.ToString(reader["prevcmd"]);
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
                    embed.Author = auth;
                    embed.AddField(x => {
                        x.IsInline = false;
                        x.Name = "Description";
                        if (string.IsNullOrWhiteSpace(User.Description))
                            x.Value = "No Description";
                        else
                            x.Value = User.Description;
                    });
                    embed.AddField(x => {
                        x.IsInline = true;
                        x.Name = Config.Load().MoneyName;
                        if (User.Money <= 0)
                            x.Value = "No Money";
                        else
                            x.Value = User.Money.ToString("N0");
                    });
                    embed.AddField(x => {
                        x.IsInline = true;
                        x.Name = "Luck Factor";
                        if (User.LuckFactor <= 0)
                            x.Value = "No LuckFactor";
                        else
                            x.Value = User.LuckFactor;
                    });
                    embed.AddField(x => {
                        x.IsInline = false;
                        x.Name = "Daily";
                        if (string.IsNullOrWhiteSpace(User.Daily))
                            x.Value = "No Description";
                        else
                            x.Value = User.Daily;
                    });
                    embed.AddField(x => {
                        x.IsInline = false;
                        x.Name = "Previous Command";
                        if (string.IsNullOrWhiteSpace(User.PrevCmd))
                            x.Value = "Unknown";
                        else
                            x.Value = User.PrevCmd;
                    });
                    embed.AddField(x => {
                        x.IsInline = false;
                        x.Name = "Favourite Command";
                        if (string.IsNullOrWhiteSpace(User.FavCmd))
                            x.Value = "No favourite command";
                        else
                            x.Value = $"`{User.FavCmd}` and it has been used {User.FavCmdUsg} times";
                    });
                    await MessageHandler.SendChannel(Context.Channel, "", embed);
                }
                else
                {
                    await MessageHandler.SendChannel(Context.Channel, "Error!! Fixing...",null,5);
                    await InsertUser(user);
                }
            }
        }

        [Command("profile-ext", RunMode = RunMode.Async), Summary("Gets extended information about you")]
        public async Task ProfilxExt()
        {
            await GetProfileExt(Context.User);
        }
        [Command("profile-ext", RunMode = RunMode.Async), Summary("Gets extended information about a user")]
        public async Task ProfileExt(IGuildUser user)
        {
            await GetProfileExt(user as IUser);
        }
        public async Task GetProfileExt(IUser user)
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
                        User.Username = Convert.ToString(reader["username"]);
                        User.Description = Convert.ToString(reader["description"]);
                        User.Money = Convert.ToUInt64(reader["money"]);
                        User.LuckFactor = Convert.ToDouble(reader["luckfactor"]);
                        User.Daily = reader["daily"].ToString();
                        User.Glares = Convert.ToUInt32(reader["glares"]);
                        User.GlaredAt = Convert.ToUInt32(reader["glaredat"]);
                        User.Pets = Convert.ToUInt32(reader["pets"]);
                        User.Petted = Convert.ToUInt32(reader["petted"]);
                        User.HP = Convert.ToUInt32(reader["hp"]);
                        User.PrevCmd = Convert.ToString(reader["prevcmd"]);
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
                    embed.Author = auth;
                    embed.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = "Description";
                        if (string.IsNullOrWhiteSpace(User.Description))
                            x.Value = "No Description";
                        else
                            x.Value = User.Description;
                    });
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = Config.Load().MoneyName;
                        if (User.Money <= 0)
                            x.Value = "No Money";
                        else
                            x.Value = User.Money.ToString("N0");
                    });
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "Luck Factor";
                        if (User.LuckFactor <= 0)
                            x.Value = "No LuckFactor";
                        else
                            x.Value = User.LuckFactor;
                    });
                    embed.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = "Daily";
                        if (string.IsNullOrWhiteSpace(User.Daily))
                            x.Value = "No Description";
                        else
                            x.Value = User.Daily;
                    });
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "Glares";
                        if (User.Glares <= 0)
                            x.Value = "No Glares";
                        else
                            x.Value = User.Glares + " times";
                    });
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "Glared At";
                        if (User.GlaredAt <= 0)
                            x.Value = "Not been glared at";
                        else
                            x.Value = User.GlaredAt + " times";
                    });
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "Pets";
                        if (User.Pets <= 0)
                            x.Value = "No Pets";
                        else
                            x.Value = User.Pets + " times";
                    });
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "Petted";
                        if (User.Petted <= 0)
                            x.Value = "Not been petted";
                        else
                            x.Value = User.Petted + " times";
                    });
                    embed.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = "HP";
                        if (User.HP <= 0)
                            x.Value = "No HP";
                        else
                            x.Value = User.HP;
                    });
                    embed.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = "Previous Command";
                        if (string.IsNullOrWhiteSpace(User.PrevCmd))
                            x.Value = "Unknown";
                        else
                            x.Value = User.PrevCmd;
                    });
                    embed.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = "Favourite Command";
                        if (string.IsNullOrWhiteSpace(User.FavCmd))
                            x.Value = "No favourite command";
                        else
                            x.Value = "`"+User.FavCmd + "` and it has been used " + User.FavCmdUsg + " times";
                    });
                    await MessageHandler.SendChannel(Context.Channel, "", embed);
                }
                else
                {
                    await MessageHandler.SendChannel(Context.Channel, "Error!! Fixing...", null, 5);
                    await InsertUser(user);
                }
            }
        }

        [Command("description", RunMode = RunMode.Async), Summary("Sets description")]
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

        [Command("whois", RunMode = RunMode.Async), Summary("Get's information about a user")]
        [Alias("user")]
        public async Task Whois(IGuildUser whois) { await GetProile(whois); }
        [Command("info", RunMode = RunMode.Async), Summary("Alias of sk!whois {tag yourself}")]
        [Alias("me")]
        public async Task Me() { await GetProile(Context.User); }
        public async Task GetProile(IUser user)
        {
            var whois = user as IGuildUser;
            var temp = await Bot.bot.GetApplicationInfoAsync();
            var owner = temp.Owner;
            string userimg = whois.GetAvatarUrl();
            if (!String.IsNullOrEmpty(userimg))
                if (userimg.Contains("a_"))
                    userimg = userimg.Replace(".jpg", ".gif");
            EmbedBuilder embed = new EmbedBuilder();
            EmbedAuthorBuilder auth = new EmbedAuthorBuilder();
            embed.Color = RandColor.RandomColor();
            var guild = Context.Guild;
            var roles = guild.Roles;
            var userroles = whois.RoleIds;
            var embedcolorrole = userroles.Select(query => guild.GetRole(query));
            string rolesjoined = userroles.Select(query => guild.GetRole(query).Name).Aggregate((current, next) => current.TrimStart('@') + ", " + next);
            auth.Name = whois.Username;
            embed.Author = auth;
            embed.ImageUrl = userimg;
            if (whois.Id == owner.Id)
                embed.Description = "**BOT OWNER**";
            embed.AddInlineField(":bust_in_silhouette: Name", whois.Username + "#" + whois.DiscriminatorValue);
            embed.AddInlineField(":id: ID", whois.Id.ToString());
            embed.AddInlineField(":shield: Roles", rolesjoined);
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = ":video_game: Currently Playing";
                if (whois.Game.HasValue)
                    x.Value = whois.Game.Value.Name.ToString();
                else
                    x.Value = "Nothing";
            });
            embed.AddInlineField(":inbox_tray: Server Join", whois.JoinedAt.ToString());
            embed.AddInlineField(":globe_with_meridians: Discord Join", whois.CreatedAt.ToString());
            embed.AddInlineField(":information_source: Status", whois.Status.ToString());
            embed.AddInlineField(":robot: Bot?", whois.IsBot.ToString());
            embed.AddField(x =>
            {
                int seencount = 0;
                x.IsInline = true;
                x.Name = ":eyes: Mutual Servers";
                foreach (var item in Bot.bot.Guilds)
                    if (item.GetUser(whois.Id) != null)
                        seencount++;
                x.Value = $"{seencount} servers";
            });
            var currentshard = Bot.bot.GetShardFor(guild);
            embed.AddInlineField("Last Seen", $"Shard: {currentshard.ShardId}");
            await MessageHandler.SendChannel(Context.Channel, "", embed);
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

        [Command("syncname", RunMode = RunMode.Async), Summary("Sync your username")]
        public async Task SyncName()
        {
            var command = new MySqlCommand("select username from accounts where ID = @userid");
            command.Parameters.AddWithValue("@userid", Context.User.Id);
            var oldname = await Sql.GetSingleAsync(command);
            if (!String.IsNullOrEmpty(oldname))
            {
                string newname = Context.User.Username.Replace("\"", "\\\"").Replace("\'", "\\'") + "#" + Context.User.Discriminator;
                if (oldname != newname)
                {
                    command = new MySqlCommand("UPDATE accounts SET username = @username WHERE ID = @userid");
                    command.Parameters.AddWithValue("@userid", Context.User.Id);
                    command.Parameters.AddWithValue("@username", newname);
                    await Sql.InsertAsync(command).ContinueWith(async x =>
                    {
                        command = new MySqlCommand("select username from accounts where ID = @userid");
                        command.Parameters.AddWithValue("@userid", Context.User.Id);
                        string newnewname = await Sql.GetSingleAsync(command);
                        if (!String.IsNullOrEmpty(newnewname))
                        {
                            if (x.IsCompleted && newname == newnewname)
                            {
                                await MessageHandler.SendChannel(Context.Channel,$"Successfully synced username from **{oldname}** to **{newname}**");
                            }
                        }
                        else
                            await InsertUser(Context.User);
                    });
                }
                else
                {
                    await MessageHandler.SendChannel(Context.Channel, $"{Context.User.Username} your name is already synced");
                }
            }
            else
                await InsertUser(Context.User);
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
        private async Task InsertUser(IUser user)
        {
            MySqlCommand command = new MySqlCommand();
            command.CommandText = "INSERT IGNORE INTO `accounts` (`ID`, `username`, `description`) VALUES (@userid , @username , \"I have no description\");";
            command.Parameters.AddWithValue("@userid", user.Id);
            command.Parameters.AddWithValue("@username", $"{user.Username.Replace("\"", "\\\"").Replace("\'", "\\'")}#{user.DiscriminatorValue}");
            await Sql.InsertAsync(command);
        }
    }
}
