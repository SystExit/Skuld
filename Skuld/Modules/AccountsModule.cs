using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Skuld.Models;
using Skuld.Tools;
using MySql.Data.MySqlClient;
#pragma warning disable GCop126 
#pragma warning disable GCop646

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
            var usr = await SqlTools.GetUserAsync(user.Id);

            if(Context.User == user)
                await MessageHandler.SendChannel(Context.Channel, $"You have: {Config.Load().MoneySymbol + usr.Money.Value.ToString("N0")}");
            else
                await MessageHandler.SendChannel(Context.Channel, message: $"**{user.Nickname??user.Username}** has: {Config.Load().MoneySymbol + usr.Money.Value.ToString("N0")}");
        }

        [Command("profile", RunMode= RunMode.Async), Summary("Get your profile")]
        public async Task Profile() =>
            await GetProfile(Context.User as IGuildUser);
        [Command("profile", RunMode = RunMode.Async), Summary("Get users profile")]
        public async Task GetProfile([Remainder]IGuildUser user)
        {
            try
            {
                var userLocal = await SqlTools.GetUserAsync(user.Id);
                if(userLocal != null)
                {
                    var embed = new EmbedBuilder()
                    {
                        Color = Tools.Tools.RandomColor(),
                        Author = new EmbedAuthorBuilder()
                        {
                            Name = user.Username,
                            IconUrl = user.GetAvatarUrl() ?? "http://www.emoji.co.uk/files/mozilla-emojis/smileys-people-mozilla/11419-bust-in-silhouette.png"
                        }
                    };
                    embed.AddField("Description", userLocal.Description ?? "No Description", inline: true);
                    embed.AddField(Config.Load().MoneyName, userLocal.Money.Value.ToString("N0") ?? "No Money", inline: true);
                    embed.AddField("Luck Factor", userLocal.LuckFactor.ToString("P2") ?? "No LuckFactor",inline: true);
                    if (!string.IsNullOrEmpty(userLocal.Daily))
                        embed.AddField("Daily", userLocal.Daily, inline: true);
                    else
                        embed.AddField("Daily", "Not used Daily", inline: true);
                    if (userLocal.FavCmd != null && userLocal.FavCmdUsg.HasValue)
                        embed.AddField("Favourite Command", $"`{userLocal.FavCmd}` and it has been used {userLocal.FavCmdUsg} times", inline: true);
                    else
                        embed.AddField("Favourite Command", "No favourite Command", inline: true);
                    await MessageHandler.SendChannel(Context.Channel, "", embed);
                }
                else
                {
                    await MessageHandler.SendChannel(Context.Channel, "Error!! Fixing...", 5);
                    await InsertUser(user);
                }                
            }
            catch (Exception ex)
            {
                await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Author = new EmbedAuthorBuilder() { Name = "Error with the command" }, Description = ex.Message, Color = new Color(255, 0, 0) });
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
                var userLocal = await SqlTools.GetUserAsync(user.Id);
                if(userLocal != null)
                {
                    var embed = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder() { Name = userLocal.Username, IconUrl = user.GetAvatarUrl() ?? "http://www.emoji.co.uk/files/mozilla-emojis/smileys-people-mozilla/11419-bust-in-silhouette.png" },
                        Color = Tools.Tools.RandomColor()
                    };
                    embed.AddField("Description", userLocal.Description ?? "No Description",inline: true);
                    embed.AddField(Config.Load().MoneyName, userLocal.Money.Value.ToString("N0") ?? "No Money", inline: true);
                    embed.AddField("Luck Factor", userLocal.LuckFactor.ToString("P2") ?? "No LuckFactor", inline: true);
                    if (!String.IsNullOrEmpty(userLocal.Daily))
                        embed.AddField("Daily", userLocal.Daily, inline: true);
                    else
                        embed.AddField("Daily", "Not used Daily", inline: true);
                    if (userLocal.Glares > 0)
                        embed.AddField("Glares", userLocal.Glares + " times", inline: true);
                    else
                        embed.AddField("Glares", "Not glared at anyone", inline: true);
                    if (userLocal.GlaredAt > 0)
                        embed.AddField("Glared At", userLocal.GlaredAt + " times", inline: true);
                    else
                        embed.AddField("Glared At", "Not been glared at", inline: true);
                    if (userLocal.Pets > 0)
                        embed.AddField("Pets", userLocal.Pets + " times", inline: true);
                    else
                        embed.AddField("Pets", "Not been petted", inline: true);
                    if (userLocal.Petted > 0)
                        embed.AddField("Petted", userLocal.Petted + " times", inline: true);
                    else
                        embed.AddField("Petted", "Not petted anyone", inline: true);
                    if (userLocal.HP > 0)
                        embed.AddField("HP", userLocal.HP + " HP", inline: true);
                    else
                        embed.AddField("HP", "No HP", inline: true);
                    if (userLocal.FavCmd != null && userLocal.FavCmdUsg.HasValue)
                        embed.AddField("Favourite Command", $"`{userLocal.FavCmd}` and it has been used {userLocal.FavCmdUsg} times", inline: true);
                    else
                        embed.AddField("Favourite Command", "No favourite Command", inline: true);
                    await MessageHandler.SendChannel(Context.Channel, "", embed);
                }
                else
                {
                    await MessageHandler.SendChannel(Context.Channel, "Error!! Fixing...", 5);
                    await InsertUser(user);
                }
            }
            catch(Exception ex)
            {
                await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Author = new EmbedAuthorBuilder() { Name = "Error with the command" }, Description = ex.Message, Color = new Color(255, 0, 0) });
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
        [Command("description", RunMode = RunMode.Async), Summary("Cleans description")]
        public async Task SetDescription()
        {
            var command = new MySqlCommand("UPDATE `accounts` SET Description = NULL WHERE ID = @userid");
            command.Parameters.AddWithValue("@userid", Context.User.Id);
            await SqlTools.GetSingleAsync(command);
            await MessageHandler.SendChannel(Context.Channel, $"I cleared your description.");
        }

        [Command("daily", RunMode = RunMode.Async), Summary("Daily Money")]
        public async Task Daily()
        {
            var suser = await SqlTools.GetUserAsync(Context.User.Id);
            if (!String.IsNullOrEmpty(suser.Daily))
            {
                var olddate = Convert.ToDateTime(suser.Daily);
                var newdate = olddate.AddHours(23).AddMinutes(59).AddSeconds(59);
                if (DateTime.Compare(newdate, DateTime.UtcNow) == 1)
                {
                    var remain = olddate.AddDays(1).Subtract(DateTime.UtcNow);
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
            var suser = await SqlTools.GetUserAsync(user.Id);

            if(suser!=null)
            {
                await SqlTools.ModifyUserAsync((user as Discord.WebSocket.SocketUser), "money", Convert.ToString(suser.Money + Config.Load().DailyAmount));
                await SqlTools.ModifyUserAsync((user as Discord.WebSocket.SocketUser), "daily", Convert.ToString(DateTime.UtcNow));
            }
            else
            {
                await InsertUser(user);
            }
        }
        [Command("daily", RunMode = RunMode.Async), Summary("Daily Money")]
        public async Task GiveDaily([Remainder]IGuildUser user)
        {
            if (user.IsBot)
            {
                await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() {
                    Author = new EmbedAuthorBuilder() {
                        Name = "Error with the command" },
                    Description = "Bot Accounts cannot be given any money", Color = new Color(255,0,0)
                });
            }
            else
            {
                var suser = await SqlTools.GetUserAsync(user.Id);
                if (suser != null && !user.IsBot)
                {
                    await SqlTools.ModifyUserAsync((user as Discord.WebSocket.SocketUser), "money", Convert.ToString(suser.Money + Config.Load().DailyAmount));
                    await SqlTools.ModifyUserAsync((Context.User as Discord.WebSocket.SocketUser), "daily", Convert.ToString(DateTime.UtcNow));
                    await MessageHandler.SendChannel(Context.Channel, $"Yo, you just gave {user.Nickname ?? user.Username} {Config.Load().MoneySymbol}{Config.Load().DailyAmount}! They now have {Config.Load().MoneySymbol}{suser.Money + Config.Load().DailyAmount}");
                }
            }
        }
        private async Task NewDailyGive(IGuildUser user)
        {
            await InsertUser(user);
            var suser = await SqlTools.GetUserAsync(user.Id);
            if (suser != null && !user.IsBot)
            {
                await SqlTools.ModifyUserAsync((user as Discord.WebSocket.SocketUser), "money", Convert.ToString(suser.Money + Config.Load().DailyAmount));
                await SqlTools.ModifyUserAsync((Context.User as Discord.WebSocket.SocketUser), "daily", Convert.ToString(DateTime.UtcNow));
                await MessageHandler.SendChannel(Context.Channel, $"Yo, you just gave {user.Nickname ?? user.Username} {Config.Load().MoneySymbol}{Config.Load().DailyAmount}! They now have {Config.Load().MoneySymbol}{suser.Money + Config.Load().DailyAmount}");
            }
        }

        [Command("give", RunMode = RunMode.Async), Summary("Give away ur money")]
        public async Task Give(IGuildUser user, ulong amount)
        {
            if (amount < 0)
            {
                await MessageHandler.SendChannel(Context.Channel, "HEY! Stop trying to reduce their money. >:(");
            }
            if (amount == 0)
            {
                await MessageHandler.SendChannel(Context.Channel,"Why would you want to give zero money to someone? :thinking:");
            }
            else
            {
                var oldusergive = await SqlTools.GetUserAsync(user.Id);
                if (oldusergive != null&&!user.IsBot)
                {
                    var oldusersend = await SqlTools.GetUserAsync(Context.User.Id);
                    var res1 = await SqlTools.ModifyUserAsync((user as Discord.WebSocket.SocketUser), "money", Convert.ToString(oldusergive.Money + amount));
                    var res2 = await SqlTools.ModifyUserAsync((Context.User as Discord.WebSocket.SocketUser), "money", Convert.ToString(oldusersend.Money - amount));
                    if (res1.Successful && res2.Successful)
                        await MessageHandler.SendChannel(Context.Channel, $"Successfully gave **{user.Username}** {Config.Load().MoneySymbol + amount}");
                    else
                    {
                        string message = $"Res1 error: {res1.Error ?? "None"}\nRes2 error: {res2.Error ?? "None"}";
                        await MessageHandler.SendChannel(Context.Channel, $"Oops, something bad happened. :(\n```\n{message}```");
                    }
                }
                else if(user.IsBot)
                    await MessageHandler.SendChannel(Context.Channel, $"Hey, uhhh... Robots aren't supported. :(");
                else
                {
                    await InsertUser(user);
                    await Give(user, amount);
                }
            }
        }
        [Command("syncname", RunMode = RunMode.Async), Summary("Syncs your username in the database")]
        public async Task SyncName()
        {
            var command = new MySqlCommand("SELECT username FROM accounts WHERE ID = @userid");
            command.Parameters.AddWithValue("@userid", Context.User.Id);
            var username = await SqlTools.GetSingleAsync(command);
            if(Context.User.Username+"#"+Context.User.DiscriminatorValue != username)
            {
                var cmd = new MySqlCommand("UPDATE accounts SET username = @username WHERE ID = @userid");
                cmd.Parameters.AddWithValue("@username", $"{Context.User.Username.Replace("\"", "\\").Replace("\'", "\\'")}#{Context.User.DiscriminatorValue}");
                cmd.Parameters.AddWithValue("@userid", Context.User.Id);
                await SqlTools.InsertAsync(cmd).ContinueWith(async x=>
                {
                if (x.IsCompleted)
                    await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Author = new EmbedAuthorBuilder() { Name = "Success" }, Description = "Synced your username :D", Color = Tools.Tools.RandomColor() });
                });
            }
            else
                await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Author = new EmbedAuthorBuilder() { Name = "Error" }, Description = "Your username is already the same as the one I have on record.", Color = new Color(255, 0, 0) });
        }

        private async Task InsertUser(IUser user)
        {
            var result = await SqlTools.InsertUserAsync((user as Discord.WebSocket.SocketUser));
            if (result.Successful)
                return;
            else
            {
                Bot.Logs.Add(new Models.LogMessage("AccountModule", result.Error, LogSeverity.Error));
                await MessageHandler.SendChannel(Context.Channel, $"I'm sorry there was an issue; `{result.Error}`");
            }
        }
    }
}
