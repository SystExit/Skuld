using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Skuld.Core.Extensions;
using Skuld.Core.Models;
using Skuld.Core;
using Skuld.Core.Utilities;
using Skuld.Database;
using Skuld.Database.Extensions;
using Skuld.Discord.Commands;
using Skuld.Discord.Extensions;
using Skuld.Discord.Preconditions;
using System;
using System.Threading.Tasks;
using Skuld.Discord.Utilities;

namespace Skuld.Bot.Commands
{
    [Group, RequireDatabase, RequireEnabledModule]
    public class Profiles : InteractiveBase<SkuldCommandContext>
    {
        public SkuldConfig Configuration { get; set; }

        [Command("money"), Summary("Gets a user's money")]
        public async Task Money([Remainder]IGuildUser user = null)
        {
            try
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
                    await $"You have: {Configuration.Preferences.MoneySymbol}{skuser.Money.ToString("N0")}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                else
                    await $"{user.Mention} has {Configuration.Preferences.MoneySymbol}{skuser.Money.ToString("N0")}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            catch (Exception ex)
            {
                await ex.Message.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel, null, ex);
                await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CMD-MONEY", ex.Message, LogSeverity.Error, ex));
            }
        }

        [Command("profile"), Summary("Get a users profile")]
        public async Task Profile([Remainder]IGuildUser user = null)
        {
            try
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
                        await Profile(user);
                    }
                }

                var embed = await skuser.GetProfileAsync(user ?? (IGuildUser)Context.User, Configuration);

                await embed.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            catch (Exception ex)
            {
                await ex.Message.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel, null, ex);
                await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CMD-PROFILE", ex.Message, LogSeverity.Error, ex));
            }
        }

        [Command("profile-ext"), Summary("Get a users extended profile")]
        public async Task ExtProfile([Remainder]IGuildUser user = null)
        {
            try
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

                await embed.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            catch (Exception ex)
            {
                await ex.Message.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel, null, ex);
                await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CMD-PROFILEEXT", ex.Message, LogSeverity.Error, ex));
            }
        }

        [Command("daily"), Summary("Daily Money")]
        public async Task Daily(IGuildUser user = null)
        {
            try
            {
                var context = await DatabaseClient.GetUserAsync(Context.User.Id);
                if (user == null)
                {
                    if (context.Data is SkuldUser)
                    {
                        await ((SkuldUser)context.Data).DoDailyAsync(Configuration);
                        context = await DatabaseClient.GetUserAsync(Context.User.Id);
                        await $"You got your daily of: `{Configuration.Preferences.MoneySymbol + Configuration.Preferences.DailyAmount}`, you now have: {Configuration.Preferences.MoneySymbol}{((SkuldUser)context.Data).Money.ToString("N0")}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                    else
                    {
                        var thing = ((SkuldUser)context.Data).Daily + 86400;
                        var remain = thing.FromEpoch().Subtract(DateTime.UtcNow);
                        string remaining = remain.Hours + " Hours " + remain.Minutes + " Minutes " + remain.Seconds + " Seconds";
                        await $"You must wait `{remaining}`".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                }
                else
                {
                    var suser = await DatabaseClient.GetUserAsync(user.Id);
                    if (suser.Data is SkuldUser)
                    {
                        await ((SkuldUser)suser.Data).DoDailyAsync(Configuration, (SkuldUser)context.Data);
                        suser = await DatabaseClient.GetUserAsync(user.Id);
                        await $"You just gave {user.Mention} your daily of: `{Configuration.Preferences.MoneySymbol + Configuration.Preferences.DailyAmount}`, they now have: {Configuration.Preferences.MoneySymbol}{((SkuldUser)suser.Data).Money.ToString("N0")}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                    else
                    {
                        var thing = ((SkuldUser)context.Data).Daily + 86400;
                        var remain = thing.FromEpoch().Subtract(DateTime.UtcNow);
                        string remaining = remain.Hours + " Hours " + remain.Minutes + " Minutes " + remain.Seconds + " Seconds";
                        await $"You must wait `{remaining}`".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    }
                }
            }
            catch (Exception ex)
            {
                await ex.Message.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel, null, ex);
                await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CMD-DAILY", ex.Message, LogSeverity.Error, ex));
            }
        }

        [Command("give"), Summary("Give your money to people")]
        public async Task Give(IGuildUser user, ulong amount)
        {
            try
            {
                var skuserResp = await DatabaseClient.GetUserAsync(Context.User.Id).ConfigureAwait(false);
                SkuldUser skuser = null;
                if(skuserResp.Data is SkuldUser)
                {
                    skuser = skuserResp.Data as SkuldUser;
                    if (skuser.Money < amount)
                    {
                        await "You can't give more money than you have".QueueMessage(Discord.Models.MessageType.Mention, Context.User, Context.Channel);
                        return;
                    }
                }
                else
                {
                    await "Error parsing user information".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
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
                        await $"You just gave {user.Mention} {Configuration.Preferences.MoneySymbol}{amount.ToString("N0")}".QueueMessage(Discord.Models.MessageType.Mention, Context.User, Context.Channel);
                    }
                    else
                    {
                        await "Updating Unsuccessful".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                    }
                }
                else
                {
                    await "Error parsing user information".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                }
            }
            catch (Exception ex)
            {
                await ex.Message.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel, null, ex);
                await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CMD-GIVE", ex.Message, LogSeverity.Error, ex));
            }
        }

        [Command("rank"), Summary("Gets your or someone's current level"), RequireDatabase]
        public async Task Level(IGuildUser user = null)
        {
            try
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

                await embed.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            catch (Exception ex)
            {
                await ex.Message.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel, null, ex);
                await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CMD-RANK", ex.Message, LogSeverity.Error, ex));
            }
        }

        [Command("heal"), Summary("Shows you how much you can heal by")]
        public async Task HealAmount()
        {
            try
            {
                if(Context.DBUser != null)
                {
                    var amnt = Context.DBUser.HP / 0.8;
                    await $"You can heal for: `{Math.Floor(amnt)}`HP".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                }
                else
                {
                    await DatabaseClient.InsertUserAsync(Context.User);
                    await HealAmount();
                }
            }
            catch (Exception ex)
            {
                await ex.Message.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel, null, ex);
                await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CMD-HEAL", ex.Message, LogSeverity.Error, ex));
            }
        }

        [Command("heal"), Summary("Heal yourself or others here")]
        public async Task Heal(uint hp, [Remainder] IGuildUser user = null)
        {
            var contextDB = Context.DBUser;
            var d = await DatabaseClient.GetUserAsync(user.Id).ConfigureAwait(false);
            var userDB = user == null ? null : ((d.Successful) ? (SkuldUser)d.Data : null);

            if (user == null)
            {
                if (contextDB.HP == 10000)
                {
                    await "You're already at max health".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    return;
                }
                var amount = GetCostOfHP(hp);
                if (contextDB.Money < amount)
                {
                    await "You don't have enough money for this action".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    return;
                }
                if (hp > (10000 - contextDB.HP))
                {
                    await ("You only need to heal by: " + (10000 - contextDB.HP)).QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    return;
                }

                contextDB.Money -= amount;
                contextDB.HP += hp;

                if (contextDB.HP > 10000)
                    contextDB.HP = 10000;

                await DatabaseClient.UpdateUserAsync(contextDB);

                await $"You have healed your HP by {hp} for {Configuration.Preferences.MoneySymbol}{amount.ToString("N0")}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            else
            {
                if (contextDB.HP == 10000)
                {
                    await "They're already at max health".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    return;
                }
                var amount = GetCostOfHP(hp);
                if (contextDB.Money < amount)
                {
                    await "You don't have enough money for this action".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    return;
                }
                if (hp > (10000 - userDB.HP))
                {
                    await ("You only need to heal them by: " + (10000 - userDB.HP)).QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    return;
                }

                contextDB.Money -= amount;
                userDB.HP += hp;

                if (userDB.HP > 10000)
                    userDB.HP = 10000;

                await DatabaseClient.UpdateUserAsync(contextDB);
                await DatabaseClient.UpdateUserAsync(userDB);

                await $"You have healed {user.Mention}'s HP by {hp} for {Configuration.Preferences.MoneySymbol}{amount.ToString("N0")}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        private ulong GetCostOfHP(uint hp)
            => (ulong)Math.Round(Math.Ceiling(hp / 0.8));
    }

    [Group, Name("Accounts"), RequireDatabase]
    public class Account : InteractiveBase<SkuldCommandContext>
    {
        public SkuldConfig Configuration { get; set; }

        [Command("set-description"), Summary("Sets Description")]
        public async Task SetDescription([Remainder]string description)
        {
            try
            {
                var userResp = await DatabaseClient.GetUserAsync(Context.User.Id).ConfigureAwait(false);
                var user = userResp.Data as SkuldUser;

                user.Description = description;

                var result = await DatabaseClient.UpdateUserAsync(user);
                if (result.Successful)
                {
                    await $"Successfully set your description to **{description}**".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                }
                else
                {
                    await "Couldn't Parse User Information".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);

                    await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("Accounts", result.Error, LogSeverity.Error, result.Exception));
                }
            }
            catch (Exception ex)
            {
                await ex.Message.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel, null, ex);
                await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CMD-SDESC", ex.Message, LogSeverity.Error, ex));
            }
        }

        [Command("clear-description"), Summary("Clears Description")]
        public async Task ClearDescription()
        {
            try
            {
                var userResp = await DatabaseClient.GetUserAsync(Context.User.Id);
                var user = userResp.Data as SkuldUser;

                user.Description = "I have no description";

                var result = await DatabaseClient.UpdateUserAsync(user);
                if (result.Successful)
                {
                    await "Successfully cleared your description.".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                }
                else
                {
                    await "Couldn't Parse User Information".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);

                    await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("Accounts", result.Error, LogSeverity.Error, result.Exception));
                }
            }
            catch (Exception ex)
            {
                await ex.Message.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel, null, ex);
                await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CMD-CDESC", ex.Message, LogSeverity.Error, ex));
            }
        }
    }
}