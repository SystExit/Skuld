using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Skuld.Bot.Extensions;
using Skuld.Bot.Models;
using Skuld.Bot.Models.GamblingModule;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Utilities;
using Skuld.Models;
using Skuld.Services.Accounts.Banking.Models;
using Skuld.Services.Banking;
using Skuld.Services.Bot;
using Skuld.Bot.Discord.Attributes;
using Skuld.Bot.Discord.Preconditions;
using Skuld.Services.Exceptions;
using Skuld.Services.Gambling;
using Skuld.Services.Globalization;
using Skuld.Services.Messaging.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group, Name("Gambling"), RequireEnabledModule]
    public class GamblingModule : InteractiveBase<ShardedCommandContext>
    {
        public Locale Locale { get; set; }

        #region Variables

        private readonly Dictionary<string, string> coinflip = new Dictionary<string, string>
        {
            { "SKULD_COINFLIP_HEADS", "https://static.skuldbot.uk/img/flip/heads.png" },
            { "SKULD_COINFLIP_TAILS", "https://static.skuldbot.uk/img/flip/tails.png" }
        };

        private readonly Dictionary<RPSThrow, string> rps = new Dictionary<RPSThrow, string>()
        {
            { RPSThrow.Rock, "SKULD_RPS_ROCK" },
            { RPSThrow.Paper, "SKULD_RPS_PAPER" },
            { RPSThrow.Scissors, "SKULD_RPS_SCISSORS" }
        };

        private readonly Weightable<RPSThrow>[] rpsWeights = new[]
        {
            new Weightable<RPSThrow>
            {
                Weight = 33,
                Value = RPSThrow.Rock
            },
            new Weightable<RPSThrow>
            {
                Weight = 33,
                Value = RPSThrow.Paper
            },
            new Weightable<RPSThrow>
            {
                Weight = 33,
                Value = RPSThrow.Scissors
            }
        };

        private static readonly Weightable<SlotIcon>[] slotsWeights = new[]
        {
            new Weightable<SlotIcon>
            {
                Weight = 25,
                Value = SlotIcon.Cherry
            },
            new Weightable<SlotIcon>
            {
                Weight = 20,
                Value = SlotIcon.Lemon
            },
            new Weightable<SlotIcon>
            {
                Weight = 20,
                Value = SlotIcon.Melon
            },
            new Weightable<SlotIcon>
            {
                Weight = 15,
                Value = SlotIcon.Bell
            },
            new Weightable<SlotIcon>
            {
                Weight = 10,
                Value = SlotIcon.Crown
            },
            new Weightable<SlotIcon>
            {
                Weight = 5,
                Value = SlotIcon.Diamond
            },
            new Weightable<SlotIcon>
            {
                Weight = 5,
                Value = SlotIcon.Star
            }
        };

        private static readonly Dictionary<SlotIcon, string> slotIcons = new Dictionary<SlotIcon, string>
        {
            { SlotIcon.Bell, "🔔" },
            { SlotIcon.Cherry, "🍒" },
            { SlotIcon.Crown, "👑" },
            { SlotIcon.Diamond, "💎" },
            { SlotIcon.Lemon, "🍋" },
            { SlotIcon.Melon, "🍉" },
            { SlotIcon.Star, "⭐" }
        };

        const int MinimumBet = 100;

        #endregion Variables

        private static bool IsValidBet(ulong betAmount)
            => betAmount >= MinimumBet;

        [Command("flip")]
        [Disabled(false, true, "Incomplete")]
        [Usage("heads", "heads 250")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task HeadsOrTails(string guess, ulong bet)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();
            var user = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

            string MoneyPrefix = SkuldApp.MessageServiceConfig.MoneyIcon;
            string Prefix = SkuldApp.MessageServiceConfig.Prefix;

            if (!Context.IsPrivate)
            {
                var guild = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);
                MoneyPrefix = guild.MoneyIcon;
                Prefix = guild.Prefix;
            }

            if (!IsValidBet(bet))
            {
                await EmbedExtensions.FromError("Heads Or Tails", $"You have not specified a valid bet, minimum is {MoneyPrefix}{MinimumBet.ToFormattedString()}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            if (user.Money < bet)
            {
                await EmbedExtensions.FromError("Heads Or Tails", $"You don't have enough money available to make that bet, you have {MoneyPrefix}{user.Money.ToFormattedString()} available", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            TransactionService.DoTransaction(new TransactionStruct
            {
                Amount = bet,
                Sender = user
            });

            await Database.SaveChangesAsync().ConfigureAwait(false);

            var result = SkuldRandom.Next(0, coinflip.Count);

            var loweredGuess = guess.ToLowerInvariant();
            switch (loweredGuess)
            {
                case "heads":
                case "head":
                case "h":
                case "tails":
                case "tail":
                case "t":
                    {
                        bool playerguess = loweredGuess == "heads" || loweredGuess == "head";

                        var res = (coinflip.Keys.ElementAt(result), coinflip.Values.ElementAt(result));

                        bool didWin = false;

                        if (result == 0 && playerguess)
                        {
                            didWin = true;
                        }
                        else if (result == 1 && !playerguess)
                        {
                            didWin = true;
                        }

                        string suffix;

                        if (didWin)
                        {
                            TransactionService.DoTransaction(new TransactionStruct
                            {
                                Amount = bet * 2,
                                Receiver = user
                            });
                        }

                        if (didWin)
                        {
                            suffix = $"You Won! <:blobsquish:350681075296501760> Your money is now {MoneyPrefix}`{user.Money.ToFormattedString()}`";
                        }
                        else
                        {
                            suffix = $"You Lost! <:blobcrying:662304318531305492> Your money is now {MoneyPrefix}`{user.Money.ToFormattedString()}`";
                        }

                        await Database.SaveChangesAsync().ConfigureAwait(false);

                        await 
                            EmbedExtensions
                            .FromImage(res.Item2, didWin ? Color.Green : Color.Red, Context)
                            .WithTitle("Heads Or Tails")
                            .WithDescription($"Result are: {Locale.GetLocale(user.Language).GetString(res.Item1)} {suffix}")
                            .QueueMessageAsync(Context)
                        .ConfigureAwait(false);
                    }
                    break;

                default:
                    await EmbedExtensions.FromError("Heads Or Tails", $"Incorrect guess value. Try; `{Prefix}flip heads`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    return;
            }
        }

        #region Rock Paper Scissors

        [Command("rps")]
        [Usage("rock", "rock 250")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task RPS(string shoot, ulong bet)
        {
            if (bet <= 0)
            {
                await EmbedExtensions.FromError("Rock Paper Scissors", $"Can't bet 0", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            using var Database = new SkuldDbContextFactory().CreateDbContext();
            var user = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

            var skuldThrow = rpsWeights.GetRandomWeightedValue().Value;

            var playerThrow = RockPaperScissorsHelper.FromString(shoot);

            if (playerThrow != RPSThrow.Invalid)
            {
                var result = (WinResult)((playerThrow - skuldThrow + 2) % 3);

                var throwName = Locale.GetLocale(user.Language).GetString(rps.FirstOrDefault(x => x.Key == skuldThrow).Value);

                string MoneyPrefix = SkuldApp.MessageServiceConfig.MoneyIcon;

                if (!Context.IsPrivate)
                {
                    var guild = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);
                    MoneyPrefix = guild.MoneyIcon;
                }

                {
                    if (!IsValidBet(bet))
                    {
                        await EmbedExtensions.FromError("Rock Paper Scissors", $"You have not specified a valid bet, minimum is {MoneyPrefix}{MinimumBet.ToFormattedString()}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        return;
                    }

                    if (user.Money < bet)
                    {
                        await EmbedExtensions.FromError("Rock Paper Scissors", $"You don't have enough money available to make that bet, you have {MoneyPrefix}{user.Money.ToFormattedString()} available", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        return;
                    }

                    TransactionService.DoTransaction(new TransactionStruct
                    {
                        Amount = bet,
                        Sender = user
                    });

                    await Database.SaveChangesAsync().ConfigureAwait(false);
                }

                switch (result)
                {
                    case WinResult.BotWin:
                        {
                            await EmbedExtensions.FromError("Rock Paper Scissors", $"I draw {throwName} and... You lost, you now have {MoneyPrefix}`{user.Money}`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        break;

                    case WinResult.PlayerWin:
                        {
                            TransactionService.DoTransaction(new TransactionStruct
                            {
                                Amount = bet * 2,
                                Receiver = user
                            });

                            await Database.SaveChangesAsync().ConfigureAwait(false);

                            await EmbedExtensions.FromSuccess("Rock Paper Scissors", $"I draw {throwName} and... You won, you now have {MoneyPrefix}`{user.Money}`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        break;

                    case WinResult.Draw:
                        {
                            TransactionService.DoTransaction(new TransactionStruct
                            {
                                Amount = bet,
                                Receiver = user
                            });

                            await Database.SaveChangesAsync().ConfigureAwait(false);

                            await EmbedExtensions.FromInfo("Rock Paper Scissors", $"I draw {throwName} and... It's a draw, your money has not been affected", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        break;
                }
            }
            else
            {
                await
                    EmbedExtensions.FromError("Rock Paper Scissors", $"`{shoot}` is not a valid option", Context)
                    .QueueMessageAsync(Context)
                .ConfigureAwait(false);
            }
        }

        #endregion Rock Paper Scissors

        #region Slots

        [Command("slots")]
        [Usage("slots <bet>")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task Slots(ulong bet)
        {
            if (bet <= 0)
            {
                await EmbedExtensions.FromError("Slots", $"Can't bet 0", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            using var Database = new SkuldDbContextFactory().CreateDbContext();
            var user = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

            string MoneyPrefix = SkuldApp.MessageServiceConfig.MoneyIcon;

            if (!Context.IsPrivate)
            {
                var guild = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);
                MoneyPrefix = guild.MoneyIcon;
            }

            {
                if (!IsValidBet(bet))
                {
                    await EmbedExtensions.FromError("Slots", $"You have not specified a valid bet, minimum is {MoneyPrefix}{MinimumBet.ToFormattedString()}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    return;
                }

                if (user.Money < bet)
                {
                    await EmbedExtensions.FromError("Slots", $"You don't have enough money available to make that bet, you have {MoneyPrefix}{user.Money.ToFormattedString()} available", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    return;
                }

                TransactionService.DoTransaction(new TransactionStruct
                {
                    Amount = bet,
                    Sender = user
                }).Then(async _=>
                {
                    await Database.SaveChangesAsync().ConfigureAwait(false);
                });
            }

            var rows = GetSlotsRows();

            var middleRow = rows[1];

            var stringRow = GetStringRows(rows);

            var message = await EmbedExtensions.FromInfo("Slots", "Please Wait, Calculating Wheels", Context).QueueMessageAsync(Context).ConfigureAwait(false);

            await Task.Delay(500).ConfigureAwait(false);

            double percentageMod = 0.0d;

            percentageMod = GetPercentageModifier(percentageMod, middleRow, SlotIcon.Cherry, .5d, 1d);
            percentageMod = GetPercentageModifier(percentageMod, middleRow, SlotIcon.Lemon, .8d, 1.5d);
            percentageMod = GetPercentageModifier(percentageMod, middleRow, SlotIcon.Melon, 1d, 2d);
            percentageMod = GetPercentageModifier(percentageMod, middleRow, SlotIcon.Bell, 1d, 4d);
            percentageMod = GetPercentageModifier(percentageMod, middleRow, SlotIcon.Crown, 1.2d, 6d);
            percentageMod = GetPercentageModifier(percentageMod, middleRow, SlotIcon.Diamond, 1.5d, 10d);
            percentageMod = GetPercentageModifier(percentageMod, middleRow, SlotIcon.Star, 2d, 12d);

            await Task.Delay(SkuldRandom.Next(50, 300)).ConfigureAwait(false);

            if (percentageMod == 0.0d)
            {
                await message.ModifyAsync(x =>
                    {
                        x.Embed = EmbedExtensions.FromMessage(
                            "Slots", 
                            $"{stringRow}\n\n" +
                            $"You lost {bet.ToFormattedString()}! " +
                            $"You now have {MoneyPrefix}`{user.Money}`",
                            Color.Red,
                            Context
                        ).Build();
                    }).ConfigureAwait(false);
            }
            else
            {
                var amount = (ulong)Math.Round(bet * percentageMod);

                TransactionService.DoTransaction(new TransactionStruct
                {
                    Amount = amount,
                    Receiver = user
                })
                    .Then(async _ =>
                    {
                        await Database.SaveChangesAsync().ConfigureAwait(false);

                        await message.ModifyAsync(x => x.Embed = EmbedExtensions.FromMessage("Slots", $"{stringRow}\n\nYou won {amount.ToFormattedString()}! You now have {MoneyPrefix}`{user.Money}`", Color.Green, Context).Build()).ConfigureAwait(false);
                    });

            }
        }

        #region SlotsMachine

        private static double GetPercentageModifier(double mod, SlotIcon[] icons, SlotIcon icon, double is2, double is3)
        {
            if (icons.Count(x => x == icon) == 3)
            {
                return is3;
            }
            else if (icons.Count(x => x == icon) == 2)
            {
                return is2;
            }
            else
            {
                return mod;
            }
        }

        private static SlotIcon[] GetSlotsRow()
            => new SlotIcon[]
            {
                slotsWeights.GetRandomWeightedValue().Value,
                slotsWeights.GetRandomWeightedValue().Value,
                slotsWeights.GetRandomWeightedValue().Value
            };

        private static SlotIcon[][] GetSlotsRows()
        {
            var middleRow = GetSlotsRow();

            var slotRow1 = GetSlotsRow();
            var slotRow3 = GetSlotsRow();

            for (int x = 0; x < middleRow.Length; x++)
            {
                if (middleRow[x] == slotRow1[x])
                {
                    slotRow1[x] = slotsWeights.GetRandomWeightedValue().Value;
                }

                if (middleRow[x] == slotRow3[x])
                {
                    slotRow3[x] = slotsWeights.GetRandomWeightedValue().Value;
                }

                if (slotRow3[x] == slotRow1[x])
                {
                    slotRow3[x] = slotsWeights.GetRandomWeightedValue().Value;
                }
            }

            return new SlotIcon[][]
            {
                slotRow1,
                middleRow,
                slotRow3
            };
        }

        private static string GetStringRow(SlotIcon[] row, bool isMiddle)
            => isMiddle ? $">> {slotIcons.GetValueOrDefault(row[0])} {slotIcons.GetValueOrDefault(row[1])} {slotIcons.GetValueOrDefault(row[2])} <<"
                         : $"{DiscordUtilities.Empty_Emote}" +
                           $"{slotIcons.GetValueOrDefault(row[0])} {slotIcons.GetValueOrDefault(row[1])} {slotIcons.GetValueOrDefault(row[2])}" +
                           $"{DiscordUtilities.Empty_Emote}";

        private static string GetStringRows(SlotIcon[][] slots)
            => $"{GetStringRow(slots[0], false)}\n{GetStringRow(slots[1], true)}\n{GetStringRow(slots[2], false)}";

        #endregion SlotsMachine

        #endregion Slots

        #region Mia

        [
            Group("mia"),
            Summary("Play a game of mia"),
            Ratelimit(20, 1, Measure.Minutes)
        ]
        public class MiaModule : InteractiveBase<ShardedCommandContext>
        {
            static readonly string PenaltyNotice = "**Note:** Re-rolling costs 25% of your bet up to 75%";

            [Command, Priority(1)]
            public async Task EntryPoint()
                => EmbedExtensions
                    .FromInfo("Mia", "", Context)
                    .Then(async x=>
                    {
                        var embed = (EmbedBuilder)x;

                        var message = new StringBuilder();

                        string Prefix = SkuldApp.MessageServiceConfig.Prefix;

                        if(!Context.IsPrivate)
                        {
                            using var Database = new SkuldDbContextFactory()
                                .CreateDbContext();

                            var gld = await Database
                                    .InsertOrGetGuildAsync(Context.Guild)
                                .ConfigureAwait(false);

                            Prefix = gld.Prefix;
                        }

                        message
                            .AppendLine($"To create a new game, use `{Prefix}mia new [amount]`")
                            .AppendLine($"To roll within a game, use `{Prefix}mia roll`")
                            .Append($"To end a game, use `{Prefix}mia stay`");

                        await 
                            embed.WithDescription(message.ToString())
                            .QueueMessageAsync(Context)
                            .ConfigureAwait(false);
                    });

            [Command("new"), Usage("250"), Priority(2)]
            public async Task NewMia(ulong bet)
            {
                try
                {
                    MiaHandler.GetSession(Context.User);
                    await
                        EmbedExtensions.FromError(
                            "Mia",
                            "You already have a game in progress",
                            Context
                        )
                        .QueueMessageAsync(Context)
                    .ConfigureAwait(false);
                }
                catch(SessionNotFoundException ex)
                {
                    using var Database = new
                        SkuldDbContextFactory().CreateDbContext();

                    if (bet <= 0)
                    {
                        await
                            EmbedExtensions.FromError(
                                "Mia",
                                $"Can't bet 0",
                                Context
                            )
                            .QueueMessageAsync(Context)
                        .ConfigureAwait(false);
                        return;
                    }

                    string MoneyPrefix = SkuldApp.MessageServiceConfig.MoneyIcon;
                    string Prefix = SkuldApp.MessageServiceConfig.Prefix;

                    if (!Context.IsPrivate)
                    {
                        var guild =
                            await
                                Database.InsertOrGetGuildAsync(Context.Guild)
                            .ConfigureAwait(false);
                        MoneyPrefix = guild.MoneyIcon;
                        Prefix = guild.Prefix;
                    }

                    if (!IsValidBet(bet))
                    {
                        await
                            EmbedExtensions.FromError(
                                "Mia",
                                "You have not specified a valid bet, " +
                                "minimum is " +
                                $"{MoneyPrefix}" +
                                $"{MinimumBet.ToFormattedString()}",
                                Context
                            ).QueueMessageAsync(Context)
                        .ConfigureAwait(false);
                        return;
                    }

                    var user = await Database.InsertOrGetUserAsync(Context.User)
                        .ConfigureAwait(false);

                    TransactionService.DoTransaction(new TransactionStruct
                    {
                        Amount = bet,
                        Sender = user
                    })
                    .IsSuccess(async z =>
                    {
                        var bot = new Dice(2);
                        var player = new Dice(2);

                        for (int x = 0; x < SkuldRandom.Next(1, 11); x++)
                        {
                            bot.Roll();
                            player.Roll();
                        }

                        string botRoll = MiaHandler.GetRollString(bot, true);
                        string plaRoll = MiaHandler.GetRollString(player, false);

                        var msg = await
                            EmbedExtensions.FromMessage(
                                "Mia",
                                $"Type `{Prefix}mia roll` or" +
                                $" `{Prefix}mia stay`\n" +
                                PenaltyNotice,
                                Context
                            )
                            .AddInlineField(
                                Context.Client.CurrentUser.Username,
                                botRoll
                            )
                            .AddInlineField(
                                Context.User.Username,
                                plaRoll
                            )
                            .WithRandomColor()
                            .QueueMessageAsync(
                                Context,
                                type: Services.Messaging.Models.MessageType.Mention
                            )
                        .ConfigureAwait(false);

                        MiaHandler.NewSession(Context.User, bet, player, bot, msg);
                    })
                    .IsError(async x =>
                    {
                        await EmbedExtensions
                            .FromError
                            (
                                "Mia",
                                "You don't have enough money available " +
                                "to make that bet, you have " +
                                $"{MoneyPrefix}{user.Money.ToFormattedString()} " +
                                "available",
                                Context
                            )
                            .QueueMessageAsync(Context)
                        .ConfigureAwait(false);
                        return;
                    });

                    await Database.SaveChangesAsync().ConfigureAwait(false);
                }
            }

            [Command("roll"), Priority(3)]
            public async Task MiaRoll()
            {
                using var Database = new
                    SkuldDbContextFactory().CreateDbContext();

                string Prefix = SkuldApp.MessageServiceConfig.Prefix;

                if (!Context.IsPrivate)
                {
                    var guild =
                        await
                            Database.InsertOrGetGuildAsync(Context.Guild)
                        .ConfigureAwait(false);
                    Prefix = guild.Prefix;
                }

                var User = await Database.InsertOrGetUserAsync(Context.User)
                    .ConfigureAwait(false);

                try
                {
                    var session = MiaHandler.GetSession(Context.User);

                    try
                    {
                        if (session.PreviousMessage != null)
                        {
                            await
                                session.PreviousMessage.DeleteAsync()
                            .ConfigureAwait(false);
                        }

                        await Context.Message.DeleteAsync().ConfigureAwait(false);
                    }
                    catch { }

                    if (session.ReRolls >= MiaHandler.MaxReRolls)
                    {
                        await
                            EmbedExtensions
                            .FromError("Mia", "You have expended the maximum amount of rerolls", Context)
                            .QueueMessageAsync(Context, timeout: 10)
                        .ConfigureAwait(false);

                        await MiaStay().ConfigureAwait(false);

                        return;
                    }
                    else
                    {
                        try
                        {
                            if (session.PreviousMessage != null)
                            {
                                await
                                    session.PreviousMessage.DeleteAsync()
                                .ConfigureAwait(false);
                            }

                            await Context.Message.DeleteAsync().ConfigureAwait(false);
                        }
                        catch { }

                        session.ReRolls += 1;

                        TransactionService.DoTransaction(new TransactionStruct
                        {
                            Sender = User,
                            Amount = MiaHandler.GetAmountFromReRolls(session.Amount, 1)
                        })
                            .IsSuccess(async _ =>
                            {
                                if (SkuldRandom.Next(1, 101) > 50)
                                {
                                    for (int x = 0; x < SkuldRandom.Next(1, 11); x++)
                                    {
                                        session.BotDice.Roll();
                                    }
                                }

                                for (int x = 0; x < SkuldRandom.Next(1, 11); x++)
                                {
                                    session.PlayerDice.Roll();
                                }

                                string botRoll = MiaHandler.GetRollString(session.BotDice, true);
                                string plaRoll = MiaHandler.GetRollString(session.PlayerDice, false);

                                var msg = await
                                    EmbedExtensions.FromMessage(
                                        "Mia",
                                        $"Type `{Prefix}mia roll`" +
                                        $" or `{Prefix}mia stay`\n" +
                                        PenaltyNotice,
                                        Context
                                    )
                                    .AddInlineField(
                                        Context.Client.CurrentUser.Username,
                                        botRoll
                                    )
                                    .AddInlineField(
                                        Context.User.Username,
                                        plaRoll
                                    )
                                    .WithRandomColor()
                                    .QueueMessageAsync(
                                        Context,
                                        type: Services.Messaging.Models.MessageType.Mention
                                    )
                                .ConfigureAwait(false);

                                session.PreviousMessage = msg;

                                MiaHandler.UpdateSession(session);
                            })
                            .IsError(async _ =>
                            {
                                await EmbedExtensions
                                    .FromError
                                    (
                                        "Mia",
                                        "You don't have enough money available, " +
                                        "I will now stay on your behalf",
                                        Context
                                    )
                                    .QueueMessageAsync(Context)
                                .ConfigureAwait(false);
                                await MiaStay().ConfigureAwait(false);
                            });

                        await Database.SaveChangesAsync().ConfigureAwait(false);
                    }
                }
                catch (SessionNotFoundException exception)
                {
                    await
                        EmbedExtensions.FromError(
                            "Mia",
                            "You don't have a session running. " +
                            $"Use `{Prefix}mia new [amount]` to start one",
                            Context
                        )
                        .QueueMessageAsync(
                            Context,
                            type: Services.Messaging.Models.MessageType.Mention
                        )
                        .ConfigureAwait(false);

                    Log.Error("Mia", exception.Message, Context, exception);
                }
            }

            [Command("stay"), Priority(4)]
            public async Task MiaStay()
            {
                try
                {
                    var session = MiaHandler.GetSession(Context.User);
                    MiaHandler.EndSession(Context.User);

                    try
                    {
                        if (session.PreviousMessage != null)
                        {
                            await
                                session.PreviousMessage.DeleteAsync()
                            .ConfigureAwait(false);
                        }

                        await Context.Message.DeleteAsync().ConfigureAwait(false);
                    }
                    catch { }

                    if (SkuldRandom.Next(1, 101) > 50)
                    {
                        for (int x = 0; x < SkuldRandom.Next(1, 11); x++)
                        {
                            session.BotDice.Roll();
                        }
                    }

                    {
                        using var Database = new
                                SkuldDbContextFactory().CreateDbContext();

                        string MoneyPrefix = SkuldApp.MessageServiceConfig.MoneyIcon;

                        if (!Context.IsPrivate)
                        {
                            var guild =
                                await
                                    Database.InsertOrGetGuildAsync(Context.Guild)
                                .ConfigureAwait(false);
                            MoneyPrefix = guild.MoneyIcon;
                        }

                        var user = await Database.InsertOrGetUserAsync(Context.User)
                            .ConfigureAwait(false);

                        EmbedBuilder embed = null;

                        switch (MiaHandler.DidPlayerWin(session.BotDice.GetDies(), session.PlayerDice.GetDies()))
                        {
                            case WinResult.PlayerWin:
                                {
                                    TransactionService.DoTransaction(
                                        new TransactionStruct
                                        {
                                            Amount = session.Amount * 2,
                                            Receiver = user
                                        }
                                    ).IsSuccess(x =>
                                    {
                                        embed = EmbedExtensions.FromSuccess(
                                            "Mia",
                                            "You Win! You now have " +
                                            $"{MoneyPrefix}" +
                                            $"{user.Money.ToFormattedString()}",
                                            Context
                                        );
                                    });
                                }
                                break;

                            case WinResult.BotWin:
                                {
                                    embed =
                                        EmbedExtensions.FromError(
                                            "Mia",
                                            "You Lost! You now have " +
                                            $"{MoneyPrefix}" +
                                            $"{user.Money.ToFormattedString()}",
                                            Context
                                        );
                                }
                                break;

                            case WinResult.Draw:
                                {
                                    TransactionService.DoTransaction(new TransactionStruct
                                    {
                                        Amount = session.Amount,
                                        Receiver = user
                                    }).IsSuccess(x =>
                                    {
                                        embed =
                                        EmbedExtensions.FromInfo(
                                            "Mia",
                                            "It's a draw!",
                                            Context
                                        );
                                    });
                                }
                                break;
                        }

                        await Database.SaveChangesAsync().ConfigureAwait(false);

                        string botRoll = MiaHandler.GetRollString(session.BotDice, false);
                        string plaRoll = MiaHandler.GetRollString(session.PlayerDice, false);

                        await embed
                                .AddInlineField(
                                    Context.Client.CurrentUser.Username,
                                    botRoll
                                )
                                .AddInlineField(
                                    Context.User.Username,
                                    plaRoll
                                )
                            .QueueMessageAsync(
                                Context,
                                type: Services.Messaging.Models.MessageType.Mention
                            )
                        .ConfigureAwait(false);
                    }

                    return;
                }
                catch (SessionNotFoundException exception)
                {
                    using var Database = new SkuldDbContextFactory()
                        .CreateDbContext();

                    string Prefix = SkuldApp.MessageServiceConfig.Prefix;

                    if (!Context.IsPrivate)
                    {
                        var guild =
                            await
                                Database.InsertOrGetGuildAsync(Context.Guild)
                            .ConfigureAwait(false);
                        Prefix = guild.Prefix;
                    }
                    await
                        EmbedExtensions.FromError(
                            "Mia",
                            "You don't have a session running. " +
                            $"Use `{Prefix}mia new [amount]` to start one",
                            Context
                        )
                        .QueueMessageAsync(
                            Context,
                            type: Services.Messaging.Models.MessageType.Mention
                        )
                    .ConfigureAwait(false);

                    Log.Error("Mia", exception.Message, Context, exception);
                }
            }
        }

        #endregion Mia
    }
}