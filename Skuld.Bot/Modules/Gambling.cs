using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Skuld.Bot.Extensions;
using Skuld.Bot.Globalization;
using Skuld.Bot.Models;
using Skuld.Bot.Models.GamblingModule;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Discord.Extensions;
using Skuld.Discord.Handlers;
using Skuld.Discord.Preconditions;
using Skuld.Discord.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group, RequireEnabledModule]
    public class Gambling : ModuleBase<ShardedCommandContext>
    {
        public Locale Locale { get; set; }

        public Dice Dice = new Dice();

        private readonly Dictionary<string, string> coinflip = new Dictionary<string, string>
        {
            { "SKULD_COINFLIP_HEADS", "https://static.skuldbot.uk/img/flip/heads.png" },
            { "SKULD_COINFLIP_TAILS", "https://static.skuldbot.uk/img/flip/tails.png" }
        };

        private readonly Dictionary<RockPaperScissors, string> rps = new Dictionary<RockPaperScissors, string>()
        {
            { RockPaperScissors.Rock, "SKULD_RPS_ROCK" },
            { RockPaperScissors.Paper, "SKULD_RPS_PAPER" },
            { RockPaperScissors.Scissors, "SKULD_RPS_SCISSORS" }
        };

        private readonly Weightable<RockPaperScissors>[] rpsWeights = new[]
        {
            new Weightable<RockPaperScissors>
            {
                Weight = 33,
                Value = RockPaperScissors.Rock
            },
            new Weightable<RockPaperScissors>
            {
                Weight = 33,
                Value = RockPaperScissors.Paper
            },
            new Weightable<RockPaperScissors>
            {
                Weight = 33,
                Value = RockPaperScissors.Scissors
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

        private static readonly List<ushort[]> miaValues = new List<ushort[]>
        {
            new ushort[] { 2, 1 },
            new ushort[] { 1, 1 },
            new ushort[] { 2, 2 },
            new ushort[] { 3, 3 },
            new ushort[] { 4, 4 },
            new ushort[] { 5, 5 },
            new ushort[] { 6, 6 },
            new ushort[] { 6, 5 },
            new ushort[] { 6, 4 },
            new ushort[] { 6, 3 },
            new ushort[] { 6, 2 },
            new ushort[] { 6, 1 },
            new ushort[] { 5, 4 },
            new ushort[] { 5, 3 },
            new ushort[] { 5, 2 },
            new ushort[] { 5, 1 },
            new ushort[] { 4, 3 },
            new ushort[] { 4, 2 },
            new ushort[] { 4, 1 },
            new ushort[] { 3, 2 },
            new ushort[] { 3, 1 }
        };

        [Command("flip")]
        [Disabled(false, true)]
        public async Task HeadsOrTails(string guess, ulong? bet = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();
            var user = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);
            var guild = await Database.GetOrInsertGuildAsync(Context.Guild).ConfigureAwait(false);

            var ran = BotService.Services.GetRequiredService<Random>();
            var result = ran.Next(0, coinflip.Count);

            if (!bet.HasValue)
            {
                var loweredGuess = guess.ToLowerInvariant();
                switch (loweredGuess)
                {
                    case "heads":
                    case "head":
                    case "tails":
                    case "tail":
                        {
                            bool playerguess = loweredGuess == "heads" || loweredGuess == "head";

                            var res = (coinflip.Keys.ElementAt(result), coinflip.Values.ElementAt(result));

                            bool didWin = false;

                            if (result == 0 && playerguess)
                                didWin = true;
                            else if (result == 1 && !playerguess)
                                didWin = true;

                            string suffix;

                            if (didWin)
                                suffix = "You Won! <:blobsquish:350681075296501760>";
                            else
                                suffix = "You Lost! <:blobcrying:662304318531305492>";

                            await EmbedExtensions.FromImage(res.Item2, didWin ? Color.Green : Color.Red, Context)
                                .WithDescription($"Result are: {Locale.GetLocale(user.Language).GetString(res.Item1)} {suffix}")
                                .QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        break;

                    default:
                        await EmbedExtensions.FromError($"Incorrect guess value. Try; `{guild.Prefix}flip heads`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        return;
                }

                return;
            }
            else
            {
                var loweredGuess = guess.ToLowerInvariant();
                switch (loweredGuess)
                {
                    case "heads":
                    case "head":
                    case "tails":
                    case "tail":
                        {
                            if (user.Money < bet.Value)
                            {
                                return;
                            }

                            bool playerguess = loweredGuess == "heads" || loweredGuess == "head";

                            var res = (coinflip.Keys.ElementAt(result), coinflip.Values.ElementAt(result));

                            bool didWin = false;

                            if (result == 0 && playerguess)
                                didWin = true;
                            else if (result == 1 && !playerguess)
                                didWin = true;

                            if (didWin)
                                user.Money += bet.Value;
                            else
                                user.Money -= bet.Value;

                            await Database.SaveChangesAsync().ConfigureAwait(false);

                            string suffix;

                            if (didWin)
                                suffix = $"You Won! <:blobsquish:350681075296501760> Your money is now {guild.MoneyIcon}`{user.Money}`";
                            else
                                suffix = $"You Lost! <:blobcrying:662304318531305492> Your money is now {guild.MoneyIcon}`{user.Money}`";

                            await EmbedExtensions.FromImage(res.Item2, didWin ? Color.Green : Color.Red, Context)
                                .WithDescription($"Result are: {Locale.GetLocale(user.Language).GetString(res.Item1)} {suffix}")
                                .QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        break;

                    default:
                        await EmbedExtensions.FromError($"Incorrect guess value. Try; `{guild.Prefix}flip heads`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        return;
                }
                return;
            }
        }

        #region Rock Paper Scissors

        [Command("rps")]
        public async Task RPS(string shoot, ulong? bet = null)
        {
            if (bet.HasValue && bet.Value <= 0)
            {
                await EmbedExtensions.FromError("Rock Paper Scissors", $"Can't bet 0", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            using var Database = new SkuldDbContextFactory().CreateDbContext();
            var user = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

            var skuldThrow = rpsWeights.GetRandomWeightedValue().Value;

            var playerThrow = RockPaperScissorsHelper.FromString(shoot);

            var result = DidPlayerWinRPS(playerThrow, skuldThrow);

            var throwName = Locale.GetLocale(user.Language).GetString(rps.FirstOrDefault(x => x.Key == skuldThrow).Value);

            if (bet.HasValue)
            {                
                string MoneyPrefix;

                if (!Context.IsPrivate)
                {
                    var guild = await Database.GetOrInsertGuildAsync(Context.Guild).ConfigureAwait(false);
                    MoneyPrefix = guild.MoneyIcon;
                }
                else
                {
                    MoneyPrefix = MessageHandler.cmdConfig.MoneyIcon;
                }

                if (user.Money < bet.Value)
                {
                    await EmbedExtensions.FromError("Rock Paper Scissors", $"You don't have enough money available to make that bet, you have {MoneyPrefix}{user.Money.ToFormattedString()} available", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    return;
                }
                
                user.Money -= bet.Value;
                await Database.SaveChangesAsync().ConfigureAwait(false);

                switch (result)
                {
                    case WinResult.BotWin:
                        {
                            if (user.Money - bet.Value >= 0)
                                user.Money -= bet.Value;
                            else
                                user.Money = 0;

                            await Database.SaveChangesAsync().ConfigureAwait(false);

                            await EmbedExtensions.FromError("Rock Paper Scissors", $"I draw {throwName} and... You lost, you now have {MoneyPrefix}`{user.Money}`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        break;

                    case WinResult.PlayerWin:
                        {
                            if (bet.Value < ulong.MaxValue)
                                user.Money += bet.Value;
                            else
                                user.Money = ulong.MaxValue;

                            await Database.SaveChangesAsync().ConfigureAwait(false);

                            await EmbedExtensions.FromSuccess("Rock Paper Scissors", $"I draw {throwName} and... You won, you now have {MoneyPrefix}`{user.Money}`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        break;

                    case WinResult.Draw:
                        {
                            await EmbedExtensions.FromInfo("Rock Paper Scissors", $"I draw {throwName} and... It's a draw, your money has not been affected", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        break;
                }
            }
            else
            {
                switch (result)
                {
                    case WinResult.BotWin:
                        {
                            await EmbedExtensions.FromError("Rock Paper Scissors", $"I draw {throwName} and... You lost", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        break;

                    case WinResult.PlayerWin:
                        {
                            await EmbedExtensions.FromInfo("Rock Paper Scissors", $"I draw {throwName} and... You won", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        break;

                    case WinResult.Draw:
                        {
                            await EmbedExtensions.FromMessage("Rock Paper Scissors", $"I draw {throwName} and... It's a draw", DiscordUtilities.Warning_Color, Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        break;
                }
            }
        }

        private static WinResult DidPlayerWinRPS(RockPaperScissors userThrow, RockPaperScissors skuldThrow)
        {
            switch (userThrow)
            {
                case RockPaperScissors.Rock:
                    {
                        switch (skuldThrow)
                        {
                            case RockPaperScissors.Rock:
                                {
                                    return WinResult.Draw;
                                }
                            case RockPaperScissors.Paper:
                                {
                                    return WinResult.BotWin;
                                }
                            case RockPaperScissors.Scissors:
                                {
                                    return WinResult.PlayerWin;
                                }
                        }
                    }
                    break;

                case RockPaperScissors.Paper:
                    {
                        switch (skuldThrow)
                        {
                            case RockPaperScissors.Rock:
                                {
                                    return WinResult.PlayerWin;
                                }
                            case RockPaperScissors.Paper:
                                {
                                    return WinResult.Draw;
                                }
                            case RockPaperScissors.Scissors:
                                {
                                    return WinResult.BotWin;
                                }
                        }
                    }
                    break;

                case RockPaperScissors.Scissors:
                    {
                        switch (skuldThrow)
                        {
                            case RockPaperScissors.Rock:
                                {
                                    return WinResult.BotWin;
                                }
                            case RockPaperScissors.Paper:
                                {
                                    return WinResult.PlayerWin;
                                }
                            case RockPaperScissors.Scissors:
                                {
                                    return WinResult.Draw;
                                }
                        }
                    }
                    break;
            }
            return WinResult.Draw;
        }

        #endregion Rock Paper Scissors

        #region Slots

        [Command("slots")]
        public async Task Slots(ulong? bet = null)
        {
            if (bet.HasValue && bet.Value <= 0)
            {
                await EmbedExtensions.FromError("Slots", $"Can't bet 0", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            using var Database = new SkuldDbContextFactory().CreateDbContext();
            var user = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

            string MoneyPrefix;

            if (!Context.IsPrivate)
            {
                var guild = await Database.GetOrInsertGuildAsync(Context.Guild).ConfigureAwait(false);
                MoneyPrefix = guild.MoneyIcon;
            }
            else
            {
                MoneyPrefix = MessageHandler.cmdConfig.MoneyIcon;
            }

            if (bet.HasValue)
            {
                if (user.Money < bet.Value)
                {
                    await EmbedExtensions.FromError("Slots", $"You don't have enough money available to make that bet, you have {MoneyPrefix}{user.Money.ToFormattedString()} available", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    return;
                }

                user.Money -= bet.Value;
                await Database.SaveChangesAsync().ConfigureAwait(false);
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

            await Task.Delay(250).ConfigureAwait(false);

            if (bet.HasValue)
            {
                var amount = (ulong)Math.Round(bet.Value * percentageMod);

                if (percentageMod == 0.0d)
                {
                    await message.ModifyAsync(x => x.Embed = EmbedExtensions.FromMessage("Slots", $"{stringRow}\n\nYou lost {bet.Value.ToFormattedString()}! You now have {MoneyPrefix}`{user.Money}`", Color.Red, Context).Build()).ConfigureAwait(false);
                }
                else
                {
                    user.Money += amount;

                    await Database.SaveChangesAsync().ConfigureAwait(false);

                    await message.ModifyAsync(x => x.Embed = EmbedExtensions.FromMessage("Slots", $"{stringRow}\n\nYou won {amount.ToFormattedString()}! You now have {MoneyPrefix}`{user.Money}`", Color.Green, Context).Build()).ConfigureAwait(false);
                }
            }
            else
            {
                if (percentageMod == 0.0d)
                {
                    await message.ModifyAsync(x => x.Embed = EmbedExtensions.FromMessage("Slots", $"{stringRow}\n\nYou lost!", Color.Red, Context).Build()).ConfigureAwait(false);
                }
                else
                {
                    await message.ModifyAsync(x => x.Embed = EmbedExtensions.FromMessage("Slots", $"{stringRow}\n\nYou won!", Color.Green, Context).Build()).ConfigureAwait(false);
                }
            }
        }

        private static double GetPercentageModifier(double mod, SlotIcon[] icons, SlotIcon icon, double is2, double is3)
        {
            if (icons.Count(x => x == icon) == 3)
                return is3;
            else if (icons.Count(x => x == icon) == 2)
                return is2;
            else
                return mod;
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
                    slotRow1[x] = slotsWeights.GetRandomWeightedValue().Value;

                if (middleRow[x] == slotRow3[x])
                    slotRow3[x] = slotsWeights.GetRandomWeightedValue().Value;

                if (slotRow3[x] == slotRow1[x])
                    slotRow3[x] = slotsWeights.GetRandomWeightedValue().Value;
            }

            return new SlotIcon[][]
            {
                slotRow1,
                middleRow,
                slotRow3
            };
        }

        private static string GetStringRow(SlotIcon[] row, bool isMiddle)
        {
            if (!isMiddle)
            {
                return $"{DiscordUtilities.Empty_Emote}" +
                    $"{slotIcons.GetValueOrDefault(row[0])} {slotIcons.GetValueOrDefault(row[1])} {slotIcons.GetValueOrDefault(row[2])}" +
                    $"{DiscordUtilities.Empty_Emote}";
            }
            else
            {
                return $">> " +
                    $"{slotIcons.GetValueOrDefault(row[0])} {slotIcons.GetValueOrDefault(row[1])} {slotIcons.GetValueOrDefault(row[2])}" +
                    $" <<";
            }
        }

        private static string GetStringRows(SlotIcon[][] slots)
        {
            return $"{GetStringRow(slots[0], false)}\n{GetStringRow(slots[1], true)}\n{GetStringRow(slots[2], false)}";
        }

        #endregion Slots

        #region Mia

        [Command("mia"), Summary("Play a game of mia")]
        public async Task Mia(ulong? bet = null)
        {
            if(bet.HasValue && bet.Value <= 0)
            {
                await EmbedExtensions.FromError("Mia", $"Can't bet 0", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            using var Database = new SkuldDbContextFactory().CreateDbContext();

            string MoneyPrefix;

            if (!Context.IsPrivate)
            {
                var guild = await Database.GetOrInsertGuildAsync(Context.Guild).ConfigureAwait(false);
                MoneyPrefix = guild.MoneyIcon;
            }
            else
            {
                MoneyPrefix = MessageHandler.cmdConfig.MoneyIcon;
            }

            var user = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

            if (bet.HasValue)
            {
                if (user.Money < bet.Value)
                {
                    await EmbedExtensions.FromError("Mia", $"You don't have enough money available to make that bet, you have {MoneyPrefix}{user.Money.ToFormattedString()} available", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    return;
                }
            
                user.Money -= bet.Value;
                await Database.SaveChangesAsync().ConfigureAwait(false);
            }

            var bot = new Dice(2, BotService.Services.GetRequiredService<Random>());
            var player = new Dice(2, BotService.Services.GetRequiredService<Random>());

            bot.Roll();
            player.Roll();

            string botRoll = "";
            string plaRoll = "";

            foreach (var roll in bot.GetDies())
            {
                botRoll += $"{roll.Face}, ";
            }

            botRoll = botRoll[0..^2];

            foreach (var roll in player.GetDies())
            {
                plaRoll += $"{roll.Face}, ";
            }

            plaRoll = plaRoll[0..^2];

            var gameresult = DidPlayerWin(bot.GetDies(), player.GetDies());

            switch (gameresult)
            {
                case WinResult.PlayerWin:
                    {
                        if (bet.HasValue)
                        {
                            user.Money += bet.Value * 2;

                            await Database.SaveChangesAsync().ConfigureAwait(false);

                            await EmbedExtensions.FromSuccess("Mia", $"You Win! You now have {MoneyPrefix}{user.Money.ToFormattedString()}", Context)
                                .AddInlineField(Context.Client.CurrentUser.Username, botRoll)
                                .AddInlineField(Context.User.Username, plaRoll)
                                .QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        else
                        {
                            await EmbedExtensions.FromSuccess("Mia", "You Win!", Context)
                                .AddInlineField(Context.Client.CurrentUser.Username, botRoll)
                                .AddInlineField(Context.User.Username, plaRoll)
                                .QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                    }
                    break;

                case WinResult.BotWin:
                    {
                        if (bet.HasValue)
                        {
                            user.Money -= bet.Value;

                            await Database.SaveChangesAsync().ConfigureAwait(false);

                            await EmbedExtensions.FromError("Mia", $"You Lost! You now have {MoneyPrefix}{user.Money.ToFormattedString()}", Context)
                                .AddInlineField(Context.Client.CurrentUser.Username, botRoll)
                                .AddInlineField(Context.User.Username, plaRoll)
                                .QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                        else
                        {
                            await EmbedExtensions.FromError("Mia", "You Lost!", Context)
                                .AddInlineField(Context.Client.CurrentUser.Username, botRoll)
                                .AddInlineField(Context.User.Username, plaRoll)
                                .QueueMessageAsync(Context).ConfigureAwait(false);
                        }
                    }
                    break;

                case WinResult.Draw:
                    {
                        await EmbedExtensions.FromInfo("Mia", $"It's a draw!", Context)
                                .AddInlineField(Context.Client.CurrentUser.Username, botRoll)
                                .AddInlineField(Context.User.Username, plaRoll)
                                .QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                    break;
            }
        }

        private static WinResult DidPlayerWin(Die[] bot, Die[] player)
        {
            var botDies = bot.OrderByDescending(x => x.Face).ToArray();
            var playerDies = player.OrderByDescending(x => x.Face).ToArray();

            var botIndex = miaValues.FindIndex(x => x[0] == botDies[0] && x[1] == botDies[1]);
            var playerIndex = miaValues.FindIndex(x => x[0] == playerDies[0] && x[1] == playerDies[1]);

            if (playerIndex < botIndex)
                return WinResult.PlayerWin;
            else if (botIndex < playerIndex)
                return WinResult.BotWin;
            else
                return WinResult.Draw;
        }

        #endregion Mia
    }
}
