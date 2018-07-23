using Discord;
using Discord.Commands;
using Skuld.Commands;
using Skuld.Commands.Preconditions;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Core.Services;
using Skuld.Extensions;
using Skuld.Services;
using System;
using System.Threading.Tasks;

namespace Skuld.Modules
{
    [Group, RequireDatabase, RequireHuman]
    public class Profiles : SkuldBase<ShardedCommandContext>
    {
        public SkuldConfig Configuration { get; set; }
        public DatabaseService Database { get; set; }
        public GenericLogger Logger { get; set; }

        [Command("money"), Summary("Gets a user's money")]
        public async Task Money([Remainder]IGuildUser user = null)
        {
            if (user == null) user = (IGuildUser)Context.User;

            var skuser = await Database.GetUserAsync(user.Id);

            if (skuser != null)
            {
                if (user == Context.User)
                    await ReplyAsync(Context.Channel, $"You have: {Configuration.Preferences.MoneySymbol}{skuser.Money.ToString("N0")}");
                else
                    await ReplyAsync(Context.Channel, $"{user.Mention} has {Configuration.Preferences.MoneySymbol}{skuser.Money.ToString("N0")}");
            }
            else
            {
                await Database.InsertUserAsync(user);
                await Money(user);
            }
        }

        [Command("profile"), Summary("Get a users profile")]
        public async Task Profile([Remainder]IGuildUser user = null)
        {
            if (user == null) user = (IGuildUser)Context.User;

            var skuser = await Database.GetUserAsync(user.Id);
            if (skuser != null)
            {
                var embed = await skuser.GetProfileAsync(user, Configuration);

                await ReplyAsync(Context.Channel, embed);
            }
            else
            {
                await Database.InsertUserAsync(user);
                await Profile(user);
            }
        }

        [Command("profile-ext"), Summary("Get a users extended profile")]
        public async Task ExtProfile([Remainder]IGuildUser user = null)
        {
            if (user == null) user = (IGuildUser)Context.User;

            var skuser = await Database.GetUserAsync(user.Id);
            if (skuser != null)
            {
                var embed = await skuser.GetExtendedProfileAsync(user, Configuration);

                await ReplyAsync(Context.Channel, embed);
            }
            else
            {
                await Database.InsertUserAsync(user);
                await Profile(user);
            }
        }

        [Command("daily"), Summary("Daily Money")]
        public async Task Daily(IGuildUser user = null)
        {
            var context = await Database.GetUserAsync(Context.User.Id);
            if (user == null)
            {
                if (await context.DoDailyAsync(Database, Configuration))
                {
                    context = await Database.GetUserAsync(Context.User.Id);
                    await ReplyAsync(Context.Channel, $"You got your daily of: `{Configuration.Preferences.MoneySymbol + Configuration.Preferences.DailyAmount}`, you now have: {Configuration.Preferences.MoneySymbol}{(context.Money.ToString("N0"))}");
                }
                else
                {
                    var thing = context.Daily + 86400;
                    var remain = thing.FromEpoch().Subtract(DateTime.UtcNow);
                    string remaining = remain.Hours + " Hours " + remain.Minutes + " Minutes " + remain.Seconds + " Seconds";
                    await ReplyAsync(Context.Channel, $"You must wait `{remaining}`");
                }
            }
            else
            {
                var suser = await Database.GetUserAsync(user.Id);
                if (await suser.DoDailyAsync(Database, Configuration, context))
                {
                    suser = await Database.GetUserAsync(user.Id);
                    await ReplyAsync(Context.Channel, $"You just gave {user.Mention} your daily of: `{Configuration.Preferences.MoneySymbol + Configuration.Preferences.DailyAmount}`, they now have: {Configuration.Preferences.MoneySymbol}{(suser.Money.ToString("N0"))}");
                }
                else
                {
                    var thing = context.Daily + 86400;
                    var remain = thing.FromEpoch().Subtract(DateTime.UtcNow);
                    string remaining = remain.Hours + " Hours " + remain.Minutes + " Minutes " + remain.Seconds + " Seconds";
                    await ReplyAsync(Context.Channel, $"You must wait `{remaining}`");
                }
            }
        }

        [Command("give"), Summary("Give your money to people")]
        public async Task Give(IGuildUser user, ulong amount)
        {
            var skuser = await Database.GetUserAsync(Context.User.Id);
            if (skuser.Money < amount)
            {
                await ReplyWithMentionAsync(Context.Channel, Context.User, "You can't give more money than you have");
                return;
            }

            var skuser2 = await Database.GetUserAsync(user.Id);

            skuser.Money -= amount;
            skuser2.Money += amount;

            var res1 = await Database.UpdateUserAsync(skuser);
            var res2 = await Database.UpdateUserAsync(skuser2);

            if (res1.Successful && res2.Successful)
            {
                await ReplyWithMentionAsync(Context.Channel, Context.User, $"You just gave {user.Mention} {Configuration.Preferences.MoneySymbol}{amount.ToString("N0")}");
            }
            else
            {
                await ReplyFailedAsync(Context.Channel);
            }
        }

        [Command("heal"), Summary("Heal yourself or others here")]
        public async Task Heal(uint hp, [Remainder] IGuildUser user = null)
        {
            if (user == null) user = (IGuildUser)Context.User;

            var skuser = await Database.GetUserAsync(user.Id);
            if (user.Id == Context.User.Id)
            {
                if (skuser.HP == 10000)
                {
                    await ReplyWithMentionAsync(Context.Channel, Context.User, "You're already at max health");
                    return;
                }
                var amount = GetCostOfHP(hp);
                if (skuser.Money < amount)
                {
                    await ReplyWithMentionAsync(Context.Channel, Context.User, "You don't have enough money for this action");
                    return;
                }
                if (hp > (10000 - skuser.HP))
                {
                    await ReplyWithMentionAsync(Context.Channel, Context.User, "You only need to heal by: " + (10000 - skuser.HP));
                    return;
                }

                skuser.Money -= amount;
                skuser.HP += hp;

                if (skuser.HP > 10000)
                    skuser.HP = 10000;

                await Database.UpdateUserAsync(skuser);

                await ReplyAsync(Context.Channel, $"You have healed your HP by {hp} for {Configuration.Preferences.MoneySymbol}{amount.ToString("N0")}");
            }
            else
            {
                if (skuser.HP == 10000)
                {
                    await ReplyWithMentionAsync(Context.Channel, Context.User, "They're already at max health");
                    return;
                }
                var amount = GetCostOfHP(hp);
                if (skuser.Money < amount)
                {
                    await ReplyWithMentionAsync(Context.Channel, Context.User, "You don't have enough money for this action");
                    return;
                }
                if (hp > (10000 - skuser.HP))
                {
                    await ReplyWithMentionAsync(Context.Channel, Context.User, "You only need to heal them by: " + (10000 - skuser.HP));
                    return;
                }

                var skuser2 = await Database.GetUserAsync(Context.User.Id);

                skuser2.Money -= amount;
                skuser.HP += hp;

                if (skuser.HP > 10000)
                    skuser.HP = 10000;

                await Database.UpdateUserAsync(skuser);
                await Database.UpdateUserAsync(skuser2);

                await ReplyAsync(Context.Channel, $"You have healed {user.Mention}'s HP by {hp} for {Configuration.Preferences.MoneySymbol}{amount.ToString("N0")}");
            }
        }

        private ulong GetCostOfHP(uint hp)
        {
            return (ulong)(hp / 0.8);
        }
    }

    [Group("account"), Name("Accounts"), RequireDatabase]
    public class Account : SkuldBase<ShardedCommandContext>
    {
        public SkuldConfig Configuration { get; set; }
        public DatabaseService Database { get; set; }
        public GenericLogger Logger { get; set; }

        [Command("set-description"), Summary("Sets Description")]
        public async Task SetDescription([Remainder]string description)
        {
            var user = await Database.GetUserAsync(Context.User.Id);

            user.Description = description;

            var result = await Database.UpdateUserAsync(user);
            if (result.Successful)
            {
                await ReplyAsync(Context.Channel, $"Successfully set your description to **{description}**");
            }
            else
            {
                await ReplyFailedAsync(Context.Channel);

                await Logger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("Accounts", result.Error, LogSeverity.Error, result.Exception));
            }
        }

        [Command("clear-description"), Summary("Clears Description")]
        public async Task ClearDescription()
        {
            var user = await Database.GetUserAsync(Context.User.Id);

            user.Description = "";

            var result = await Database.UpdateUserAsync(user);
            if (result.Successful)
            {
                await ReplyAsync(Context.Channel, "Successfully cleared your description.");
            }
            else
            {
                await ReplyFailedAsync(Context.Channel);

                await Logger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("Accounts", result.Error, LogSeverity.Error, result.Exception));
            }
        }
    }
}