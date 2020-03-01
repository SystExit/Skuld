using Discord.Commands;
using Skuld.Bot.Extensions;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Formatting;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Services.Accounts.Banking.Models;
using Skuld.Services.Banking;
using Skuld.Services.Bot;
using Skuld.Services.Discord.Models;
using Skuld.Services.Discord.Preconditions;
using Skuld.Services.Messaging.Extensions;
using StatsdClient;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Bot.Modules
{
    [Group, Name("Donator")]
    public class DonatorModule : ModuleBase<ShardedCommandContext>
    {
        [Command("redeemkey")]
        [RequireContext(ContextType.DM)]
        public async Task RedeemKey(Guid key)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (Database.DonatorKeys.ToList().Any(x => x.KeyCode == key))
            {
                var dbKey = Database.DonatorKeys.ToList().FirstOrDefault(x => x.KeyCode == key);
                if (dbKey.Redeemed)
                {
                    await
                        EmbedExtensions
                        .FromError("Donator Module",
                                   "Key already redeemed",
                                   Context)
                        .QueueMessageAsync(Context)
                        .ConfigureAwait(false);
                }
                else
                {
                    dbKey.Redeemed = true;
                    dbKey.Redeemer = Context.User.Id;
                    dbKey.RedeemedWhen = DateTime.UtcNow.ToEpoch();

                    await Database.SaveChangesAsync().ConfigureAwait(false);

                    var usr = await Database.GetOrInsertUserAsync(Context.User).ConfigureAwait(false);

                    usr.Flags += DiscordUtilities.BotDonator;

                    await Database.SaveChangesAsync().ConfigureAwait(false);

                    await
                        EmbedExtensions
                        .FromSuccess("Donator Module",
                                   "You are now a donator",
                                   Context)
                        .QueueMessageAsync(Context)
                        .ConfigureAwait(false);
                    DogStatsd.Increment("donatorkeys.redeemed");
                }
            }
            else
            {
                await
                    EmbedExtensions
                    .FromError("Donator Module",
                               "Key doesn't exist",
                               Context)
                    .QueueMessageAsync(Context)
                    .ConfigureAwait(false);
            }
        }

        [Command("sellkey")]
        [RequireContext(ContextType.DM)]
        public async Task SellKey(Guid key)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (Database.DonatorKeys.ToList().Any(x => x.KeyCode == key))
            {
                var dbKey = Database.DonatorKeys.ToList().FirstOrDefault(x => x.KeyCode == key);
                if (dbKey.Redeemed)
                {
                    await
                        EmbedExtensions
                        .FromError("Donator Module",
                                   "Can't sell a redeemed key",
                                   Context)
                        .QueueMessageAsync(Context)
                        .ConfigureAwait(false);
                }
                else
                {
                    Database.DonatorKeys.Remove(dbKey);
                    await Database.SaveChangesAsync().ConfigureAwait(false);

                    var usr = await Database.GetOrInsertUserAsync(Context.User).ConfigureAwait(false);

                    TransactionService.DoTransaction(new TransactionStruct
                    {
                        Amount = 25000,
                        Receiver = usr
                    });

                    await Database.SaveChangesAsync().ConfigureAwait(false);

                    await
                        EmbedExtensions
                        .FromSuccess("Donator Module",
                                    $"You just sold your donator key for {BotService.MessageServiceConfig.MoneyIcon}{25000.ToFormattedString()}",
                                     Context)
                        .QueueMessageAsync(Context)
                        .ConfigureAwait(false);
                    DogStatsd.Increment("donatorkeys.sold");
                }
            }
            else
            {
                await
                    EmbedExtensions
                    .FromError("Donator Module",
                               "Key doesn't exist",
                               Context)
                    .QueueMessageAsync(Context)
                    .ConfigureAwait(false);
            }
        }

        [Command("donatorstatus")]
        [RequireBotFlag(BotAccessLevel.BotDonator)]
        public async Task CheckDonatorStatus()
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var keys = Database.DonatorKeys.ToList().Where(x => x.Redeemer == Context.User.Id);

            var keysordered = keys.OrderBy(x => x.RedeemedWhen);

            var amount = 365 * keys.Count();

            var time = keysordered.LastOrDefault().RedeemedWhen.FromEpoch();

            time = time.AddDays(amount);

            await $"{Context.User.Mention} You have the donator status until {time.ToDMYString()}".QueueMessageAsync(Context).ConfigureAwait(false);
        }
    }
}