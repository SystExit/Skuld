using Discord;
using Discord.Commands;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Core;
using Skuld.Core.Utilities;
using Skuld.Database;
using Skuld.Database.Extensions;
using Skuld.Discord;
using Skuld.Discord.Extensions;
using Skuld.Discord.Preconditions;
using System;
using System.Threading.Tasks;
using Skuld.Discord.Utilities;

namespace Skuld.Bot.Commands
{
    [Group, RequireDatabase, RequireHuman]
    public class Profiles : SkuldBase<SkuldCommandContext>
    {
        public SkuldConfig Configuration { get; set; }

        [Command("money"), Summary("Gets a user's money")]
        public async Task Money([Remainder]IGuildUser user = null)
        {
            var skuser = Context.DBUser;

            if (user != null)
            {
                var resp = await DatabaseClient.GetUserAsync(user.Id).ConfigureAwait(false);
                if (resp.Successful)
                    skuser = resp.Data as SkuldUser;
                else
                {
                    await DatabaseClient.InsertUserAsync(user);
                    await Money(user);
                }
            }

            if (user == null)
                await ReplyAsync(Context.Channel, $"You have: {Configuration.Preferences.MoneySymbol}{skuser.Money.ToString("N0")}");
            else
                await ReplyAsync(Context.Channel, $"{user.Mention} has {Configuration.Preferences.MoneySymbol}{skuser.Money.ToString("N0")}");
        }

        [Command("profile"), Summary("Get a users profile")]
        public async Task Profile([Remainder]IGuildUser user = null)
        {
            var skuser = Context.DBUser;

            if(user != null)
            {
                var resp = await DatabaseClient.GetUserAsync(user.Id).ConfigureAwait(false);
                if (resp.Successful)
                    skuser = resp.Data as SkuldUser;
                else
                {
                    await DatabaseClient.InsertUserAsync(user);
                    await Profile(user);
                }
            }

            var embed = await skuser.GetProfileAsync(user??(IGuildUser)Context.User, Configuration);

            await ReplyAsync(Context.Channel, embed);
        }

        [Command("profile-ext"), Summary("Get a users extended profile")]
        public async Task ExtProfile([Remainder]IGuildUser user = null)
        {
            var skuser = Context.DBUser;

            if (user != null)
            {
                var resp = await DatabaseClient.GetUserAsync(user.Id).ConfigureAwait(false);
                if (resp.Successful)
                    skuser = resp.Data as SkuldUser;
                else
                {
                    await DatabaseClient.InsertUserAsync(user);
                    await ExtProfile(user);
                }
            }

            var embed = await skuser.GetExtendedProfileAsync(user ?? (IGuildUser)Context.User, Configuration);

            await ReplyAsync(Context.Channel, embed);
        }

        [Command("daily"), Summary("Daily Money")]
        public async Task Daily(IGuildUser user = null)
        {
            var context = await DatabaseClient.GetUserAsync(Context.User.Id);
            if (user == null)
            {
                if (context.Data is SkuldUser)
                {
                    await ((SkuldUser)context.Data).DoDailyAsync(Configuration);
                    context = await DatabaseClient.GetUserAsync(Context.User.Id);
                    await ReplyAsync(Context.Channel, $"You got your daily of: `{Configuration.Preferences.MoneySymbol + Configuration.Preferences.DailyAmount}`, you now have: {Configuration.Preferences.MoneySymbol}{((SkuldUser)context.Data).Money.ToString("N0")}");
                }
                else
                {
                    var thing = ((SkuldUser)context.Data).Daily + 86400;
                    var remain = thing.FromEpoch().Subtract(DateTime.UtcNow);
                    string remaining = remain.Hours + " Hours " + remain.Minutes + " Minutes " + remain.Seconds + " Seconds";
                    await ReplyAsync(Context.Channel, $"You must wait `{remaining}`");
                }
            }
            else
            {
                var suser = await DatabaseClient.GetUserAsync(user.Id);
                if (suser.Data is SkuldUser)
                {
                    await ((SkuldUser)suser.Data).DoDailyAsync(Configuration, (SkuldUser)context.Data);
                    suser = await DatabaseClient.GetUserAsync(user.Id);
                    await ReplyAsync(Context.Channel, $"You just gave {user.Mention} your daily of: `{Configuration.Preferences.MoneySymbol + Configuration.Preferences.DailyAmount}`, they now have: {Configuration.Preferences.MoneySymbol}{((SkuldUser)suser.Data).Money.ToString("N0")}");
                }
                else
                {
                    var thing = ((SkuldUser)context.Data).Daily + 86400;
                    var remain = thing.FromEpoch().Subtract(DateTime.UtcNow);
                    string remaining = remain.Hours + " Hours " + remain.Minutes + " Minutes " + remain.Seconds + " Seconds";
                    await ReplyAsync(Context.Channel, $"You must wait `{remaining}`");
                }
            }
        }

        [Command("give"), Summary("Give your money to people")]
        public async Task Give(IGuildUser user, ulong amount)
        {
            var skuserResp = await DatabaseClient.GetUserAsync(Context.User.Id).ConfigureAwait(false);
            SkuldUser skuser = null;
            if(skuserResp.Data is SkuldUser)
            {
                skuser = skuserResp.Data as SkuldUser;
                if (skuser.Money < amount)
                {
                    await ReplyWithMentionAsync(Context.Channel, Context.User, "You can't give more money than you have");
                    return;
                }
            }
            else
            {
                await ReplyFailedAsync(Context.Channel, "Error parsing user information");
            }

            var skuser2resp = await DatabaseClient.GetUserAsync(user.Id);
            if(skuser2resp.Data is SkuldUser)
            {
                var skuser2 = skuser2resp.Data as SkuldUser;
                skuser.Money -= amount;
                skuser2.Money += amount;

                var res1 = await DatabaseClient.UpdateUserAsync(skuser);
                var res2 = await DatabaseClient.UpdateUserAsync(skuser2);

                if (res1.Successful && res2.Successful)
                {
                    await ReplyWithMentionAsync(Context.Channel, Context.User, $"You just gave {user.Mention} {Configuration.Preferences.MoneySymbol}{amount.ToString("N0")}");
                }
                else
                {
                    await ReplyFailedAsync(Context.Channel);
                }
            }
            else
            {
                await ReplyFailedAsync(Context.Channel, "Error parsing user information");
            }
        }

        /*[Command("rank"), Summary("Gets your or someone's current level"), RequireDatabase]
        public async Task Level(IGuildUser user = null)
        {
            var guildCountResp = await DatabaseClient.GetGuildExperienceCountAsync(Context.Guild.Id).ConfigureAwait(false);
            int guildCount = -1;
            if (guildCountResp.Successful)
                guildCount = ConversionTools.ParseInt32OrDefault(Convert.ToString(guildCountResp.Data));

            UserExperience experience = null;
            long userpos = -1;

            if (user == null)
            {
                user = Context.User as IGuildUser;
                experience = await Context.DBUser.GetUserExperienceAsync().ConfigureAwait(false);
            }
            else
            {
                var usrr = await DatabaseClient.GetUserAsync(user.Id).ConfigureAwait(false);
                if (usrr.Successful)
                    experience = await (usrr.Data as SkuldUser).GetUserExperienceAsync().ConfigureAwait(false);
            }

            var guildexperience = experience.GetGuildExperience(Context.Guild.Id);
            var userposresp = await DatabaseClient.GetUserGuildPositionAsync(user.Id, Context.Guild.Id);

            if (userposresp.Successful)
            {
                userpos = Convert.ToInt64(userposresp.Data) + 1;
            }

            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = $"Experience of: {user.FullName()}",
                    IconUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()
                },
                Color = EmbedUtils.RandomColor()
            };
            embed.AddField("Rank", $"{userpos}/{guildCount}");
            embed.AddField("Level", guildexperience.Level);
            embed.AddField("XP", $"{guildexperience.XP}/{DiscordUtilities.GetXPLevelRequirement(guildexperience.Level+1, DiscordUtilities.PHI)}");

            await ReplyAsync(Context.Channel, embed.Build());
        }*/

        [Command("heal"), Summary("Shows you how much you can heal by")]
        public async Task HealAmount()
        {
            if(Context.DBUser != null)
            {
                var amnt = Context.DBUser.HP / 0.8;
                await ReplyAsync(Context.Channel, $"You can heal for: `{Math.Floor(amnt)}`HP");
            }
            else
            {
                await DatabaseClient.InsertUserAsync(Context.User);
                await HealAmount();
            }
        }

        [Command("heal"), Summary("Heal yourself or others here")]
        public async Task Heal(uint hp, [Remainder] IGuildUser user = null)
        {
            var skuser = Context.DBUser;

            if (user != null)
            {
                var resp = await DatabaseClient.GetUserAsync(user.Id).ConfigureAwait(false);
                if (resp.Successful)
                    skuser = resp.Data as SkuldUser;
                else
                {
                    await DatabaseClient.InsertUserAsync(user);
                    await Heal(hp, user);
                    return;
                }
            }

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

                await DatabaseClient.UpdateUserAsync(skuser);

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

                var skuser2resp = await DatabaseClient.GetUserAsync(user.Id);
                if (skuser2resp.Data is SkuldUser)
                {
                    var skuser2 = skuser2resp.Data as SkuldUser;
                    skuser.Money -= amount;
                    skuser2.HP += hp;

                    if (skuser2.HP > 10000)
                        skuser2.HP = 10000;

                    await DatabaseClient.UpdateUserAsync(skuser);
                    await DatabaseClient.UpdateUserAsync(skuser2);

                    await ReplyAsync(Context.Channel, $"You have healed {user.Mention}'s HP by {hp} for {Configuration.Preferences.MoneySymbol}{amount.ToString("N0")}");
                }
                else
                {
                    await ReplyFailedAsync(Context.Channel, "Error parsing user information");
                }
            }
        }

        private ulong GetCostOfHP(uint hp)
            => (ulong)(hp / 0.8);
    }

    [Group, Name("Accounts"), RequireDatabase]
    public class Account : SkuldBase<SkuldCommandContext>
    {
        public SkuldConfig Configuration { get; set; }

        [Command("set-description"), Summary("Sets Description")]
        public async Task SetDescription([Remainder]string description)
        {
            var userResp = await DatabaseClient.GetUserAsync(Context.User.Id).ConfigureAwait(false);
            var user = userResp.Data as SkuldUser;

            user.Description = description;

            var result = await DatabaseClient.UpdateUserAsync(user);
            if (result.Successful)
            {
                await ReplyAsync(Context.Channel, $"Successfully set your description to **{description}**");
            }
            else
            {
                await ReplyFailedAsync(Context.Channel);

                await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("Accounts", result.Error, LogSeverity.Error, result.Exception));
            }
        }

        [Command("clear-description"), Summary("Clears Description")]
        public async Task ClearDescription()
        {
            var userResp = await DatabaseClient.GetUserAsync(Context.User.Id);
            var user = userResp.Data as SkuldUser;

            user.Description = "I have no description";

            var result = await DatabaseClient.UpdateUserAsync(user);
            if (result.Successful)
            {
                await ReplyAsync(Context.Channel, "Successfully cleared your description.");
            }
            else
            {
                await ReplyFailedAsync(Context.Channel);

                await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("Accounts", result.Error, LogSeverity.Error, result.Exception));
            }
        }
    }
}