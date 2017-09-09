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
            command.Parameters.AddWithValue("@id", user.Id);
            var resp = await SqlTools.GetSingleAsync(command);
            ulong money = 0;
            if (!String.IsNullOrEmpty(resp))
                money = Convert.ToUInt64(resp);
            else
                await InsertUser(Context.User);
            if(Context.User == user)
                await MessageHandler.SendChannel(Context.Channel, $"You have: {Config.Load().MoneySymbol + money.ToString("N0")}");
            else
                await MessageHandler.SendChannel(Context.Channel, $"**{user.Nickname??user.Username}** has: {Config.Load().MoneySymbol + money.ToString("N0")}");
        }

        [Command("profile", RunMode= RunMode.Async), Summary("Get your profile")]
        public async Task Profile() =>
            await GetProfile(Context.User as IGuildUser);
        [Command("profile", RunMode = RunMode.Async), Summary("Get users profile")]
        public async Task GetProfile([Remainder]IGuildUser user)
        {
            try
            {
                var User = await SqlTools.GetUser(user.Id);
                if(User!=null)
                {
                    var embed = new EmbedBuilder()
                    {
                        Color = RandColor.RandomColor(),
                        Author = new EmbedAuthorBuilder()
                        {
                            Name = User.Username,
                            IconUrl = user.GetAvatarUrl() ?? "http://www.emoji.co.uk/files/mozilla-emojis/smileys-people-mozilla/11419-bust-in-silhouette.png"
                        }
                    };
                    embed.AddField("Description", User.Description ?? "No Description",true);
                    embed.AddField(Config.Load().MoneyName, User.Money.Value.ToString("N0") ?? "No Money",true);
                    embed.AddField("Luck Factor", User.LuckFactor.ToString("P2") ?? "No LuckFactor",true);
                    if (!String.IsNullOrEmpty(User.Daily))
                        embed.AddField("Daily", User.Daily,true);
                    else
                        embed.AddField("Daily", "Not used Daily",true);
                    if (User.FavCmd != null && User.FavCmdUsg != null)
                        embed.AddField("Favourite Command", $"`{User.FavCmd}` and it has been used {User.FavCmdUsg} times",true);
                    else
                        embed.AddField("Favourite Command", "No favourite Command",true);
                    await MessageHandler.SendChannel(Context.Channel, "", embed.Build());
                }
                else
                {
                    await MessageHandler.SendChannel(Context.Channel, "Error!! Fixing...", 5);
                    await InsertUser(user);
                }                
            }
            catch (Exception ex)
            {
                await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Author = new EmbedAuthorBuilder() { Name = "Error with the command" }, Description = ex.Message, Color = new Color(255, 0, 0) }.Build());
                Console.WriteLine(ex);
            }            
        }

        [Command("profile-ext", RunMode = RunMode.Async), Summary("Gets extended information about you")]
        public async Task ProfileExt() =>
            await GetProfileExt(Context.User as IGuildUser);
        [Command("profile-ext", RunMode = RunMode.Async), Summary("Gets extended information about a user")]
        public async Task GetProfileExt([Remainder]IGuildUser user)
        {
            try
            {
                var User = await SqlTools.GetUser(user.Id);
                if(User!=null)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder() { Name = User.Username, IconUrl = user.GetAvatarUrl() ?? "http://www.emoji.co.uk/files/mozilla-emojis/smileys-people-mozilla/11419-bust-in-silhouette.png" },
                        Color = RandColor.RandomColor()
                    };
                    embed.AddField("Description", User.Description ?? "No Description",true);
                    embed.AddField(Config.Load().MoneyName, User.Money.Value.ToString("N0") ?? "No Money",true);
                    embed.AddField("Luck Factor", User.LuckFactor.ToString("P2") ?? "No LuckFactor",true);
                    if (!String.IsNullOrEmpty(User.Daily))
                        embed.AddField("Daily", User.Daily,true);
                    else
                        embed.AddField("Daily", "Not used Daily",true);
                    if (User.Glares > 0)
                        embed.AddField("Glares", User.Glares + " times",true);
                    else
                        embed.AddField("Glares", "Not glared at anyone",true);
                    if (User.GlaredAt > 0)
                        embed.AddField("Glared At", User.GlaredAt + " times",true);
                    else
                        embed.AddField("Glared At", "Not been glared at",true);
                    if (User.Pets > 0)
                        embed.AddField("Pets", User.Pets + " times",true);
                    else
                        embed.AddField("Pets", "Not been petted",true);
                    if (User.Petted > 0)
                        embed.AddField("Petted", User.Petted + " times",true);
                    else
                        embed.AddField("Petted", "Not petted anyone",true);
                    if (User.HP > 0)
                        embed.AddField("HP", User.HP,true);
                    else
                        embed.AddField("HP", "No HP",true);
                    if (User.FavCmd != null && User.FavCmdUsg != null)
                        embed.AddField("Favourite Command", $"`{User.FavCmd}` and it has been used {User.FavCmdUsg} times",true);
                    else
                        embed.AddField("Favourite Command", "No favourite Command",true);
                    await MessageHandler.SendChannel(Context.Channel, "", embed.Build());
                }
                else
                {
                    await MessageHandler.SendChannel(Context.Channel, "Error!! Fixing...", 5);
                    await InsertUser(user);
                }
            }
            catch(Exception ex)
            {
                await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Author = new EmbedAuthorBuilder() { Name = "Error with the command" }, Description = ex.Message, Color = new Color(255, 0, 0) }.Build());
                var appinfo = await Bot.bot.GetApplicationInfoAsync();
                await appinfo.Owner.SendMessageAsync(ex.ToString());
            }            
        }

        [Command("description", RunMode = RunMode.Async), Summary("Sets description, this is archived and will be stored")]
        public async Task SetDescription([Remainder]string description)
        {
            var command = new MySqlCommand("UPDATE accounts SET description = @description WHERE ID = @userid");
            command.Parameters.AddWithValue("@userid", Context.User.Id);
            command.Parameters.AddWithValue("@description", description);
            await SqlTools.InsertAsync(command).ContinueWith(async x =>
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
            await MessageHandler.SendChannel(Context.Channel, $"Successfully set your description to **{await SqlTools.GetSingleAsync(command)}**");
        }

        [Command("daily", RunMode = RunMode.Async), Summary("Daily Money")]
        public async Task Daily()
        {
            DateTime olddate = new DateTime();
            var command = new MySqlCommand("select daily from accounts where ID = @userid");
            command.Parameters.AddWithValue("@userid", Context.User.Id);
            var resp = await SqlTools.GetSingleAsync(command);
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
            var command = new MySqlCommand("SELECT `money` FROM `accounts` WHERE ID = @userid");
            command.Parameters.AddWithValue("@userid", Context.User.Id);
            var tresp = await SqlTools.GetSingleAsync(command);
            if (!String.IsNullOrEmpty(tresp))
                oldmoney = Convert.ToUInt64(tresp);
            ulong newamount = oldmoney + Config.Load().DailyAmount;

            command = new MySqlCommand("UPDATE accounts SET money = @money, daily = @daily WHERE ID = @userid");
            command.Parameters.AddWithValue("@userid", Context.User.Id);
            command.Parameters.AddWithValue("@money", newamount);
            command.Parameters.AddWithValue("@daily", DateTime.UtcNow);
            await SqlTools.InsertAsync(command).ContinueWith(async x =>
            {
                if (x.IsCompleted)
                {
                    await MessageHandler.SendChannel(Context.Channel, $"You now have: {Config.Load().MoneySymbol + newamount.ToString("N0")}");
                }
            });
        }
        [Command("daily", RunMode = RunMode.Async), Summary("Daily Money")]
        public async Task GiveDaily([Remainder]IGuildUser usertogive)
        {
            if (usertogive.IsBot) { await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Author = new EmbedAuthorBuilder() { Name = "Error with the command" }, Description = "Bot Accounts cannot be given any money", Color = new Color(255,0,0) }.Build()); }
            else
            {
                DateTime olddate = new DateTime();
                var command = new MySqlCommand("SELECT `daily` FROM `accounts` WHERE ID = @userid");
                command.Parameters.AddWithValue("@userid", Context.User.Id);
                var resp = await SqlTools.GetSingleAsync(command);
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
                        command = new MySqlCommand("SELECT `money` FROM `accounts` WHERE ID = @userid");
                        command.Parameters.AddWithValue("@userid", usertogive.Id);
                        var tresp = await SqlTools.GetSingleAsync(command);
                        if (!String.IsNullOrEmpty(tresp))
                            oldmoney = Convert.ToUInt64(tresp);
                        ulong newamount = oldmoney + Config.Load().DailyAmount;

                        command = new MySqlCommand("UPDATE ACCOUNTS SET Daily = @daily WHERE ID = @userid");
                        command.Parameters.AddWithValue("@userid", Context.User.Id);
                        command.Parameters.AddWithValue("@daily", DateTime.UtcNow);
                        await SqlTools.InsertAsync(command).ContinueWith(async x =>
                        {
                            if (x.IsCompleted)
                            {
                                command = new MySqlCommand("UPDATE ACCOUNTS SET money = @money WHERE ID = @userid");
                                command.Parameters.AddWithValue("@userid", usertogive.Id);
                                command.Parameters.AddWithValue("@money", newamount);
                                await SqlTools.InsertAsync(command).ContinueWith(async z =>
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
                    await NewDailyGive(usertogive);
            }
        }
        private async Task NewDailyGive(IGuildUser user)
        {
            ulong oldmoney = 0;
            var command = new MySqlCommand("SELECT `money` FROM `accounts` WHERE ID = @userid");
            command.Parameters.AddWithValue("@userid", Context.User.Id);
            var tresp = await SqlTools.GetSingleAsync(command);
            if (!String.IsNullOrEmpty(tresp))
                oldmoney = Convert.ToUInt64(tresp);
            ulong newamount = oldmoney + Config.Load().DailyAmount;

            command = new MySqlCommand("UPDATE accounts SET money = @money, daily = @daily WHERE ID = @userid");
            command.Parameters.AddWithValue("@userid", Context.User.Id);
            command.Parameters.AddWithValue("@money", newamount);
            command.Parameters.AddWithValue("@daily", DateTime.UtcNow);
            await SqlTools.InsertAsync(command).ContinueWith(async x =>
            {
                if (x.IsCompleted)
                {
                    await MessageHandler.SendChannel(Context.Channel, $"You just gave {user.Mention} {Config.Load().MoneySymbol + Config.Load().DailyAmount}");
                }
            });
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
                var reader = await SqlTools.GetSingleAsync(command);
                if (!String.IsNullOrEmpty(reader))
                    oldrecipmoney = Convert.ToUInt64(reader);
                else
                    await InsertUser(Context.User);
                command = new MySqlCommand("SELECT money FROM accounts WHERE ID = @userid");
                command.Parameters.AddWithValue("@userid", sender.Id);
                var reader2 = await SqlTools.GetSingleAsync(command);
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
                    await SqlTools.InsertAsync(command).ContinueWith(async x =>
                    {
                        command = new MySqlCommand("UPDATE accounts SET money = @money WHERE ID = @userid");
                        command.Parameters.AddWithValue("@userid", sender.Id);
                        command.Parameters.AddWithValue("@money", newsendmoney);
                        await SqlTools.InsertAsync(command);
                        ulong temprecp = 0, tempsend = 0;
                        command = new MySqlCommand("SELECT money FROM accounts WHERE ID = @userid");
                        command.Parameters.AddWithValue("@userid", user.Id);
                        var reader3 = await SqlTools.GetSingleAsync(command);
                        if (!String.IsNullOrEmpty(reader3))
                            temprecp = Convert.ToUInt64(reader);
                        else
                            await InsertUser(Context.User);
                        command = new MySqlCommand("SELECT money FROM accounts WHERE ID = @userid");
                        command.Parameters.AddWithValue("@userid", sender.Id);
                        var reader4 = await SqlTools.GetSingleAsync(command);
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
            MySqlCommand command = new MySqlCommand("SELECT username FROM accounts WHERE ID = @userid");
            command.Parameters.AddWithValue("@userid", Context.User.Id);
            var username = await SqlTools.GetSingleAsync(command);
            if(Context.User.Username+"#"+Context.User.DiscriminatorValue != username)
            {
                MySqlCommand cmd = new MySqlCommand("UPDATE accounts SET username = @username WHERE ID = @userid");
                cmd.Parameters.AddWithValue("@username", $"{Context.User.Username.Replace("\"", "\\").Replace("\'", "\\'")}#{Context.User.DiscriminatorValue}");
                cmd.Parameters.AddWithValue("@userid", Context.User.Id);
                await SqlTools.InsertAsync(cmd).ContinueWith(async x=>
                {
                if (x.IsCompleted)
                    await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Author = new EmbedAuthorBuilder() { Name = "Success" }, Description = "Synced your username :D", Color = RandColor.RandomColor() }.Build());
                });
            }
            else
                await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Author = new EmbedAuthorBuilder() { Name = "Error" }, Description = "Your username is already the same as the one I have on record.", Color = new Color(255, 0, 0) }.Build());
        }

        private async Task InsertUser(IUser user)
        {
            MySqlCommand command = new MySqlCommand("INSERT IGNORE INTO `accounts` (`ID`, `username`, `description`) VALUES (@userid , @username, \"I have no description\");");
            command.Parameters.AddWithValue("@username", $"{user.Username.Replace("\"", "\\").Replace("\'", "\\'")}#{user.DiscriminatorValue}");
            command.Parameters.AddWithValue("@userid", user.Id);
            await SqlTools.InsertAsync(command);
        }
    }
}
