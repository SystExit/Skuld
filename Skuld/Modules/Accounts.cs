using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Skuld.Services;
using Skuld.Tools;
using System.Globalization;
using Skuld.Extensions;
using Skuld.Utilities;

namespace Skuld.Modules
{
    [Group]
    public class Accounts : ModuleBase<ShardedCommandContext>
    {
        public DatabaseService Database { get; set; }
        public LoggingService Logger { get; set; }

        [Command("money", RunMode = RunMode.Async), Summary("Gets a user's money"), RequireDatabase]
        public async Task GetMoney([Remainder]IUser user = null)
        {
            if (user == null)
            {
                user = Context.User;
            }

            var usr = await Database.GetUserAsync(user.Id);

            if (Context.User == user)
            {
                await Context.Channel.ReplyAsync($"You have: {Bot.Configuration.Utils.MoneySymbol + usr.Money.ToString("N0")}");
            }
            else
            {
                await Context.Channel.ReplyAsync($"**{user.Username}** has: {Bot.Configuration.Utils.MoneySymbol + usr.Money.ToString("N0")}");
            }
        }

        [Command("profile"), Summary("Get a user's profile"), RequireDatabase]
        public async Task GetProfile([Remainder]IUser user = null)
        {
            if (user == null)
                user = Context.User;
            try
            {
                var userLocal = await Database.GetUserAsync(user.Id);
                if (userLocal != null)
                {
                    var embed = new EmbedBuilder
                    {
                        Color = EmbedUtils.RandomColor(),
                        Author = new EmbedAuthorBuilder
                        {
                            Name = Context.Client.GetUser(userLocal.ID).Username,
                            IconUrl = user.GetAvatarUrl() ?? "http://www.emoji.co.uk/files/mozilla-emojis/smileys-people-mozilla/11419-bust-in-silhouette.png"
                        }
                    };
                    embed.AddField(Bot.Configuration.Utils.MoneyName, userLocal.Money.ToString("N0") ?? "No Money", inline: true);
                    embed.AddField("Luck Factor", userLocal.LuckFactor.ToString("P2") ?? "No LuckFactor", inline: true);
                    if (userLocal.Daily != null)
                    { embed.AddField("Daily", userLocal.Daily, inline: true); }
                    else
                    { embed.AddField("Daily", "Not used Daily", inline: true); }
                    if (userLocal.FavCmd != null && userLocal.FavCmdUsg != 0)
                    { embed.AddField("Favourite Command", $"`{userLocal.FavCmd}` and it has been used {userLocal.FavCmdUsg} times", inline: true); }
                    else
                    { embed.AddField("Favourite Command", "No favourite Command", inline: true); }
                    embed.AddField("Pasta Karma", $"{(await userLocal.GetPastaKarma()).ToString("N0")} Karma");
                    embed.AddField("Description", userLocal.Description ?? "No Description", inline: false);
                    await Context.Channel.ReplyAsync(embed.Build());
                }
                else
                {
                    await Logger.AddToLogsAsync(new Models.LogMessage("CMD-Prof", "User doesn't exist", LogSeverity.Error));
                    var msg = await Context.Channel.ReplyAsync("Error!! Fixing...");
                    StatsdClient.DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                    await InsertUser(user).ConfigureAwait(false);
                    await msg.ModifyAsync(x => x.Content = "Try again now. ~~You may delete this message~~");
                }
            }
            catch (Exception ex)
            {
                await Logger.AddToLogsAsync(new Models.LogMessage("CMD-Prof", "Error in Profile", LogSeverity.Error, ex));
                var msg = await Context.Channel.ReplyAsync("Error!! Fixing...");
                StatsdClient.DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                await InsertUser(user).ConfigureAwait(false);
                await msg.ModifyAsync(x => x.Content = "Try again now. ~~You may delete this message~~");
            }
        }

        [Command("profile-ext"), Summary("Gets extended information about a user"), RequireDatabase]
        public async Task GetProfileExt([Remainder]IUser user = null)
        {
            if (user == null)
            {
                user = Context.User;
            }
            try
            {
                var userLocal = await Database.GetUserAsync(user.Id);
                if (userLocal != null)
                {
                    var embed = new EmbedBuilder
                    {
                        Author = new EmbedAuthorBuilder { Name = Context.Client.GetUser(userLocal.ID).Username, IconUrl = user.GetAvatarUrl() ?? "http://www.emoji.co.uk/files/mozilla-emojis/smileys-people-mozilla/11419-bust-in-silhouette.png" },
                        Color = EmbedUtils.RandomColor()
                    };

                    embed.AddField(Bot.Configuration.Utils.MoneyName, userLocal.Money.ToString("N0") ?? "No Money", inline: true);
                    embed.AddField("Luck Factor", userLocal.LuckFactor.ToString("P2") ?? "No LuckFactor", inline: true);

                    if (userLocal.Daily != null)
                    { embed.AddField("Daily", userLocal.Daily, inline: true); }
                    else
                    { embed.AddField("Daily", "Not used Daily", inline: true); }

                    if (userLocal.Glares > 0)
                    { embed.AddField("Glares", userLocal.Glares + " times", inline: true); }
                    else
                    { embed.AddField("Glares", "Not glared at anyone", inline: true); }

                    if (userLocal.GlaredAt > 0)
                    { embed.AddField("Glared At", userLocal.GlaredAt + " times", inline: true); }
                    else
                    { embed.AddField("Glared At", "Not been glared at", inline: true); }

                    if (userLocal.Pets > 0)
                    { embed.AddField("Pets", userLocal.Pets + " times", inline: true); }
                    else
                    { embed.AddField("Pets", "Not been petted", inline: true); }

                    if (userLocal.Petted > 0)
                    { embed.AddField("Petted", userLocal.Petted + " times", inline: true); }
                    else
                    { embed.AddField("Petted", "Not petted anyone", inline: true); }

                    if (userLocal.HP > 0)
                    { embed.AddField("HP", userLocal.HP + " HP", inline: true); }
                    else
                    { embed.AddField("HP", "No HP", inline: true); }

                    if (userLocal.FavCmd != null && userLocal.FavCmdUsg != 0)
                    { embed.AddField("Favourite Command", $"`{userLocal.FavCmd}` and it has been used {userLocal.FavCmdUsg} times", inline: true); }
                    else
                    { embed.AddField("Favourite Command", "No favourite Command", inline: true); }

                    embed.AddField("Description", userLocal.Description ?? "No Description", inline: false);
                    await Context.Channel.ReplyAsync(embed.Build());
                }
                else
                {
                    await Logger.AddToLogsAsync(new Models.LogMessage("CMD-ProfExt", "User doesn't exist", LogSeverity.Error));
                    var msg = await Context.Channel.ReplyAsync("Error!! Fixing...");
                    StatsdClient.DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                    await InsertUser(user).ConfigureAwait(false);
                    await msg.ModifyAsync(x => x.Content = "Try again now. ~~You may delete this message~~");
                }
            }
            catch (Exception ex)
            {
                await Logger.AddToLogsAsync(new Models.LogMessage("CMD-ProfExt", "Error in Profile-Ext", LogSeverity.Error, ex));
                var msg = await Context.Channel.ReplyAsync("Error!! Fixing...");
                StatsdClient.DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                await InsertUser(user).ConfigureAwait(false);
                await msg.ModifyAsync(x => x.Content = "Try again now. ~~You may delete this message~~");
            }
        }

        [Command("description"), Summary("Sets description, if no argument is passed, cleans the description"), RequireDatabase]
        public async Task SetDescription([Remainder]string description = null)
        {
            if (description != null)
            {
                var user = await Database.GetUserAsync(Context.User.Id);
                user.Description = description;
                var result = await Database.UpdateUserAsync(user);
                if (result.Successful)
                {
                    await Context.Channel.ReplyAsync($"Successfully set your description to **{description}**");
                }
                else
                {
                    await Context.Channel.ReplyAsync($"Something happened <:blobsick:350673776071147521>");
                }
            }
            else
            {
                var user = await Database.GetUserAsync(Context.User.Id);
                user.Description = "";
                var result = await Database.UpdateUserAsync(user);
                if (result.Successful)
                {
                    await Context.Channel.ReplyAsync($"Successfully cleared your description.");
                }
                else
                {
                    await Context.Channel.ReplyAsync("Something happened <:blobsick:350673776071147521>");
                }
            }
        }

        private async Task NewDaily(IUser user)
        {
            var suser = await Database.GetUserAsync(user.Id);

            if (suser != null)
            {
                suser.Daily = DateTime.UtcNow;
                suser.Money += Bot.Configuration.Utils.DailyAmount;
                await Database.UpdateUserAsync(suser);
                await Context.Channel.ReplyAsync($"You got your daily of: `{Bot.Configuration.Utils.MoneySymbol + Bot.Configuration.Utils.DailyAmount}`, you now have: {Bot.Configuration.Utils.MoneySymbol}{(suser.Money.ToString("N0"))}");
            }
            else
            {
                await InsertUser(user).ConfigureAwait(false);
            }
        }
        [Command("daily"), Summary("Daily Money"), RequireDatabase]
        public async Task Daily([Remainder]IUser user = null)
        {
            if (user == null)
            {
                var suser = await Database.GetUserAsync(Context.User.Id);
                if (suser.Daily != null)
                {
                    var olddate = suser.Daily.Value;
                    var newdate = olddate.AddHours(23).AddMinutes(59).AddSeconds(59);
                    if (DateTime.Compare(newdate, DateTime.UtcNow) == 1)
                    {
                        var remain = olddate.AddDays(1).Subtract(DateTime.UtcNow);
                        string remaining = remain.Hours + " Hours " + remain.Minutes + " Minutes " + remain.Seconds + " Seconds";
                        await Context.Channel.ReplyAsync($"You must wait `{remaining}`");
                    }
                    else
                    { await NewDaily(Context.User).ConfigureAwait(false); }
                }
                else
                { await NewDaily(Context.User).ConfigureAwait(false); }
            }
            else
            {
                var csuser = await Database.GetUserAsync(Context.User.Id);
                if (csuser.Daily != null)
                {
                    var olddate = csuser.Daily.Value;
                    var newdate = olddate.AddHours(23).AddMinutes(59).AddSeconds(59);
                    if (DateTime.Compare(newdate, DateTime.UtcNow) == 1)
                    {
                        var remain = olddate.AddDays(1).Subtract(DateTime.UtcNow);
                        string remaining = remain.Hours + " Hours " + remain.Minutes + " Minutes " + remain.Seconds + " Seconds";
                        await Context.Channel.ReplyAsync($"You must wait `{remaining}`");
                    }
                    else
                    {
                        if (user.IsBot)
                        {
                            await Context.Channel.ReplyAsync(new EmbedBuilder
                            {
                                Author = new EmbedAuthorBuilder
                                {
                                    Name = "Error with the command"
                                },
                                Description = "Bot Accounts cannot be given any money",
                                Color = Color.Red
                            }.Build());
                            StatsdClient.DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                        }
                        else
                        {
                            if (user != Context.User)
                            {
                                var suser = await Database.GetUserAsync(user.Id);
                                if (suser != null && !user.IsBot)
                                {
                                    suser.Money += Bot.Configuration.Utils.DailyAmount;
                                    await Database.UpdateUserAsync(suser);

                                    csuser.Daily = DateTime.UtcNow;
                                    await Database.UpdateUserAsync(csuser);

                                    await Context.Channel.ReplyAsync($"Yo, you just gave {user.Username} {Bot.Configuration.Utils.MoneySymbol}{Bot.Configuration.Utils.DailyAmount}! They now have {Bot.Configuration.Utils.MoneySymbol}{suser.Money.ToString("N0")}");
                                }
                                else
                                {
                                    await InsertUser(user).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                await Daily();
                            }
                        }
                    }
                }
            }
        }

        [Command("give"), Summary("Give away ur money"), RequireDatabase]
        public async Task Give(IUser user, ulong amount)
        {
            if (amount == 0)
            {
                await Context.Channel.ReplyAsync("Why would you want to give zero money to someone? :thinking:");
                return;
            }
            if (user != Context.User)
            {
                var oldusergive = await Database.GetUserAsync(user.Id);
                if (oldusergive != null && !user.IsBot)
                {
                    var oldusersend = await Database.GetUserAsync(Context.User.Id);

                    if (oldusersend.Money >= amount)
                    {
                        oldusergive.Money += amount;
                        oldusersend.Money -= amount;

                        var resp = await Database.UpdateUserAsync(oldusergive);
                        var resp2 = await Database.UpdateUserAsync(oldusersend);

                        if (resp.Successful && resp2.Successful)
                        {
                            await Context.Channel.ReplyAsync($"Successfully gave **{user.Username}** {Bot.Configuration.Utils.MoneySymbol + amount}");
                        }
                        else
                        {
                            await Logger.AddToLogsAsync(new Models.LogMessage("DailyGive", $"{resp.Error}\t{resp2.Error}", LogSeverity.Error));
                            await Context.Channel.ReplyAsync($"Oops, something happened. :(");
                            StatsdClient.DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                        }
                    }
                }
                else if (user.IsBot)
                {
                    await Context.Channel.ReplyAsync($"Hey, uhhh... Robots aren't supported. :(");
                }
                else
                {
                    await InsertUser(user);
                    await Give(user, amount).ConfigureAwait(false);
                }
            }
            else
            {
                await Context.Channel.ReplyAsync("<:gibeOops:350681606362759173> Can't give money to yourself... Oh, I guess you can... But why would you want to?");
            }
        }

        [Command("heal"), Summary("Did you run out of health? Here's the healing station"), RequireDatabase]
        public async Task Heal(uint hp, [Remainder]IUser User = null)
        {
            if (User == null)
                User = Context.User;
            if (User == Context.User)
            {
                var user = await Database.GetUserAsync(User.Id);
                var offset = 10000 - user.HP;
                if (hp > offset)
                {
                    await Context.Channel.ReplyAsync("You sure you wanna do that? You only need to heal by: `" + offset + "` HP");
                }
                var cost = GetCostOfHP(hp);
                if (user.Money >= cost)
                {
                    if (user.HP == 10000)
                    {
                        await Context.Channel.ReplyAsync("You're already at max health");
                        return;
                    }
                    user.Money -= cost;
                    user.HP += hp;

                    if (user.HP > 10000)
                        user.HP = 10000;

                    await Database.UpdateUserAsync(user);

                    await Context.Channel.ReplyAsync($"You have healed your hp by {hp} for {Bot.Configuration.Utils.MoneySymbol}{cost.ToString("N0")}");
                }
                else
                {
                    await Context.Channel.ReplyAsync("You don't have enough money for this action.");
                }
            }
            else
            {
                var user = await Database.GetUserAsync(User.Id);
                var you = await Database.GetUserAsync(Context.User.Id);
                var offset = 10000 - user.HP;
                if (hp > offset)
                {
                    await Context.Channel.ReplyAsync("You sure you wanna do that? They only need to heal by: `" + offset + "` HP");
                }
                var cost = GetCostOfHP(hp);
                if (you.Money >= cost)
                {
                    if (user.HP == 10000)
                    {
                        await Context.Channel.ReplyAsync("They're already at max health");
                        return;
                    }
                    user.HP += hp;
                    you.Money -= cost;

                    if (user.HP > 10000)
                        user.HP = 10000;

                    await Database.UpdateUserAsync(user);
                    await Database.UpdateUserAsync(you);

                    await Context.Channel.ReplyAsync($"You have healed {User.Username}'s health by {hp} for {Bot.Configuration.Utils.MoneySymbol}{cost.ToString("N0")}");
                }
                else
                {
                    await Context.Channel.ReplyAsync("You don't have enough money for this action.");
                }
            }
        }

        ulong GetCostOfHP(uint hp)
        {
            return (ulong)(hp / 0.8);
        }

        private async Task InsertUser(IUser user)
        {
            var result = await Database.InsertUserAsync((user as Discord.WebSocket.SocketUser));
            if (!result.Successful)
            {
                await Logger.AddToLogsAsync(new Models.LogMessage("AccountModule", result.Error, LogSeverity.Error));
                await Context.Channel.ReplyAsync($"I'm sorry there was an issue; `{result.Error}`");
                StatsdClient.DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
            }
        }
    }
}
