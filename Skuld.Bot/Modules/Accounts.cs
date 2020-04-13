using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using ImageMagick;
using Microsoft.EntityFrameworkCore.Internal;
using NodaTime;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Conversion;
using Skuld.Core.Extensions.Formatting;
using Skuld.Core.Utilities;
using Skuld.Models;
using Skuld.Services.Accounts.Banking.Models;
using Skuld.Services.Banking;
using Skuld.Services.Discord.Attributes;
using Skuld.Services.Discord.Preconditions;
using Skuld.Services.Extensions;
using Skuld.Services.Messaging.Extensions;
using StatsdClient;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group, Name("Accounts"), RequireEnabledModule, RequireDatabase]
    public class AccountModule : InteractiveBase<ShardedCommandContext>
    {
        public SkuldConfig Configuration { get; set; }
        const int PrestigeRequirement = 100;

        [Command("prestige"), Summary("Prestiges")]
        public async Task Prestige()
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var usr = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

            var nextPrestige = usr.PrestigeLevel + 1;

            var xp = Database.UserXp.ToList().Where(x => x.UserId == usr.Id);

            ulong globalLevel = 0;

            xp.ToList().ForEach(x =>
            {
                globalLevel += x.Level;
            });

            if(globalLevel > PrestigeRequirement * nextPrestige)
            {
                await
                    EmbedExtensions.FromInfo("Prestige Corner", "Please respond with y/n to confirm you want to prestige\n\n**__PLEASE NOTE__**\nThis will remove **ALL** experience gotten in every server with experience enabled", Context)
                    .QueueMessageAsync(Context)
                .ConfigureAwait(false);

                var next = await NextMessageAsync().ConfigureAwait(false);

                try
                {
                    if (next.Content.ToBool())
                    {
                        Database.UserXp.RemoveRange(Database.UserXp.ToList().Where(x => x.UserId == Context.User.Id));
                        await Database.SaveChangesAsync().ConfigureAwait(false);

                        usr.PrestigeLevel = usr.PrestigeLevel.Add(1);

                        await Database.SaveChangesAsync().ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    DogStatsd.Increment("commands.errors", 1, 1, new[] { "err:parse-fail" });
                }
            }
            else
            {
                await
                    EmbedExtensions.FromInfo("Prestige Corner", $"You lack the amount of levels required to prestige. For your prestige level ({usr.PrestigeLevel}), it is: {PrestigeRequirement * nextPrestige}", Context)
                    .QueueMessageAsync(Context)
                .ConfigureAwait(false);
            }
        }

        [Command("money"), Summary("Gets a user's money")]
        [Alias("balance", "credits")]
        public async Task Money([Remainder]IGuildUser user = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (user != null && (user.IsBot || user.IsWebhook))
            {
                await EmbedExtensions.FromError("SkuldBank - Account Information", DiscordUtilities.NoBotsString, Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            if (user == null)
                user = (IGuildUser)Context.User;

            var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);
            var dbusr = await Database.InsertOrGetUserAsync(user).ConfigureAwait(false);

            await
                EmbedExtensions.FromMessage("SkuldBank - Account Information", $"{user.Mention} has {gld.MoneyIcon}{dbusr.Money.ToFormattedString()} {gld.MoneyName}", Context)
            .QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("profile"), Summary("Get a users profile")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task Profile([Remainder]IGuildUser user = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (user != null && (user.IsBot || user.IsWebhook))
            {
                await EmbedExtensions.FromError(DiscordUtilities.NoBotsString, Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            if (user == null)
            {
                user = Context.User as IGuildUser;
            }

            var profileuser = await Database.InsertOrGetUserAsync(user).ConfigureAwait(false);

            var fontsFolder = SkuldAppContext.FontDirectory;
            var fontFile = Path.Combine(fontsFolder, "NotoSans-Regular.ttf");

            if (!Directory.Exists(fontsFolder))
            {
                Directory.CreateDirectory(fontsFolder);
                await HttpWebClient.DownloadFileAsync(new Uri("https://static.skuldbot.uk/fonts/NotoSans-Regular.ttf"), fontFile).ConfigureAwait(false);
            }

            using MagickImage image = new MagickImage(new MagickColor("#212121"), 600, 510)
            {
                Format = MagickFormat.Png
            };

            if (string.IsNullOrEmpty(profileuser.Background))
            {
                image.Draw(new DrawableFillColor(new MagickColor("#3F51B5")), new DrawableRectangle(0, 0, 600, 228));
            }
            else if (profileuser.Background.StartsWith('#'))
            {
                image.Draw(new DrawableFillColor(new MagickColor(profileuser.Background)), new DrawableRectangle(0, 0, 600, 228));
            }
            else
            {
                using MagickImage img2 = new MagickImage(await HttpWebClient.ReturnStreamAsync(new Uri(profileuser.Background)).ConfigureAwait(false))
                {
                    FilterType = FilterType.Quadratic
                };
                img2.Resize(600, 0);
                img2.Crop(600, 228, Gravity.Center);
                image.Composite(img2);
            }

            var avatar = user.GetAvatarUrl(ImageFormat.Png) ?? user.GetDefaultAvatarUrl();

            using (MagickImage profileBackground = new MagickImage(await HttpWebClient.ReturnStreamAsync(new Uri(avatar)).ConfigureAwait(false), new MagickReadSettings
            {
                Format = MagickFormat.Png,
                BackgroundColor = MagickColors.None,
                Width = 128,
                Height = 128
            }))
            {
                using (var mask = new MagickImage("xc:none", 128, 128))
                {
                    mask.Draw(new DrawableFillColor(MagickColors.White), new DrawableCircle(64, 64, 62, 126));

                    profileBackground.Composite(mask, CompositeOperator.CopyAlpha);
                }

                using (MagickImage statusBackground = new MagickImage($"xc:{user.Status.HexFromStatus()}", 32, 32))
                {
                    statusBackground.BackgroundColor = MagickColors.None;

                    using (var mask = new MagickImage("xc:none", 32, 32))
                    {
                        mask.Draw(new DrawableFillColor(MagickColors.White), new DrawableCircle(16, 16, 14, 30));

                        statusBackground.Composite(mask, CompositeOperator.CopyAlpha);
                    }

                    profileBackground.Composite(statusBackground, 96, 96, CompositeOperator.Over);
                }

                image.Composite(profileBackground, 236, 32, CompositeOperator.Over);
            }

            var font = new DrawableFont(fontFile);
            var encoding = new DrawableTextEncoding(System.Text.Encoding.Unicode);
            var fontsize = new DrawableFontPointSize(20);
            var white = new DrawableFillColor(new MagickColor(65535, 65535, 65535));

            int ylevel1 = 365, ylevel2 = 405, ylevel3 = 445;

            //Bar
            image.Draw(new DrawableFillColor(new MagickColor(0, 0, 0, 52428)), new DrawableRectangle(0, 188, 600, 228));

            //Rep
            using (MagickImage label = new MagickImage($"label:{Database.Reputations.Count(x => x.Repee == profileuser.Id)} Rep", new MagickReadSettings
            {
                BackgroundColor = MagickColors.Transparent,
                FillColor = MagickColors.White,
                Width = 580,
                Height = 30,
                TextGravity = Gravity.West,
                FontPointsize = 30,
                Font = fontFile
            }))
            {
                image.Composite(label, 20, 193, CompositeOperator.Over);
            }

            string moneyIcon = "₩";

            if(!Context.IsPrivate)
            {
                var dbGuild = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

                if(!moneyIcon.StartsWith("<") || !moneyIcon.StartsWith(":"))
                {
                    moneyIcon = dbGuild.MoneyIcon;
                }
            }

            var moneyAmount = profileuser.Money.ToFormattedString();

            var money = moneyIcon + moneyAmount;


            //Money
            using (MagickImage label2 = new MagickImage($"label:{money}", 
                new MagickReadSettings
            {
                BackgroundColor = MagickColors.Transparent,
                FillColor = MagickColors.White,
                Width = 580,
                Height = 30,
                TextGravity = Gravity.East,
                FontPointsize = 30,
                Font = fontFile
            }))
            {
                image.Composite(label2, 0, 193, CompositeOperator.Over);
            }

            //Username
            using (MagickImage label3 = new MagickImage($"label:{user.FullName()}", 
                new MagickReadSettings
            {
                BackgroundColor = MagickColors.Transparent,
                FillColor = MagickColors.White,
                Width = 600,
                Height = 40,
                TextGravity = Gravity.Center,
                Font = fontFile
            }))
            {
                image.Composite(label3, 0, 230, CompositeOperator.Over);
            }

            //Title
            if (!string.IsNullOrEmpty(profileuser.Title))
            {
                using MagickImage label4 = new MagickImage($"label:{profileuser.Title}", 
                    new MagickReadSettings
                {
                    BackgroundColor = MagickColors.Transparent,
                    FillColor = MagickColors.White,
                    Width = 600,
                    Height = 40,
                    TextGravity = Gravity.Center,
                    Font = fontFile
                });
                image.Composite(label4, 0, 270, CompositeOperator.Over);
            }

            //YLevel 1
            var dailyText = $"Daily: {profileuser.LastDaily.FromEpoch().ToDMYString()}";
            var dmetr = image.FontTypeMetrics(dailyText, true);
            var rightPos = 600 - (dmetr.TextWidth * 2);

            var rankraw = Database.GetOrderedGlobalExperienceLeaderboard();
            var exp = rankraw.FirstOrDefault(x => x.UserId == profileuser.Id);

            if (rankraw != null && rankraw.Any(x => x.UserId == profileuser.Id))
            {
                image.Draw(font, fontsize, encoding, white, new DrawableText(22, ylevel1, $"Global Rank: {(rankraw.IndexOf(exp)+1).ToFormattedString()}/{rankraw.Count().ToFormattedString()}"));
            }
            else
            {
                image.Draw(font, fontsize, encoding, white, new DrawableText(22, ylevel1, $"Global Rank: -1/{(rankraw == null ? -1 : rankraw.Count())}"));
            }

            image.Draw(font, fontsize, encoding, white, new DrawableText(rightPos, ylevel1, dailyText));

            //YLevel 2
            image.Draw(font, fontsize, encoding, white, new DrawableText(22, ylevel2, $"Pasta Karma: {Database.GetPastaKarma(profileuser.Id).ToFormattedString()}"));
            var favcommand = (await Database.UserCommandUsage.AsAsyncEnumerable().Where(x => x.UserId == profileuser.Id).OrderByDescending(x => x.Usage).ToListAsync()).FirstOrDefault();
            image.Draw(font, fontsize, encoding, white, new DrawableText(rightPos, ylevel2, $"Fav. Cmd: {(favcommand == null ? "N/A" : favcommand.Command)} ({(favcommand == null ? "0" : favcommand.Usage.ToFormattedString())})"));

            //YLevel 3
            image.Draw(font, fontsize, encoding, white, new DrawableText(22, ylevel3, $"Level: {exp.Level.ToFormattedString()} ({exp.TotalXP.ToFormattedString()})"));
            image.Draw(font, fontsize, encoding, white, new DrawableText(rightPos, ylevel3, $"Pats: {profileuser.Pats.ToFormattedString()}/Patted: {profileuser.Patted.ToFormattedString()}"));

            ulong xpToNextLevel = DatabaseUtilities.GetXPLevelRequirement(exp.Level + 1, DiscordUtilities.PHI);

            //Progressbar
            image.Draw(new DrawableFillColor(new MagickColor("#212121")), new DrawableRectangle(20, 471, 580, 500));
            image.Draw(new DrawableFillColor(new MagickColor("#dfdfdf")), new DrawableRectangle(22, 473, 578, 498));

            var percentage = (double)exp.XP / xpToNextLevel * 100;
            var mapped = percentage.Remap(0, 100, 22, 578);

            image.Draw(new DrawableFillColor(new MagickColor("#009688")), new DrawableRectangle(22, 473, mapped, 498));

            //Current XP
            image.Draw(font, fontsize, encoding, new DrawableText(25, 493, (exp.XP).ToFormattedString() + "XP"));

            //XP To Next
            using (MagickImage label5 = new MagickImage($"label:{xpToNextLevel.ToFormattedString()}XP", 
                new MagickReadSettings
            {
                BackgroundColor = MagickColors.Transparent,
                FillColor = MagickColors.Black,
                Width = 575,
                Height = 20,
                TextGravity = Gravity.East,
                FontPointsize = 20,
                Font = fontFile
            }))
            {
                image.Composite(label5, 0, 475, CompositeOperator.Over);
            }

            MemoryStream outputStream = new MemoryStream();

            image.Write(outputStream);

            outputStream.Position = 0;

            await "".QueueMessageAsync(Context, outputStream, type: Services.Messaging.Models.MessageType.File).ConfigureAwait(false);
        }

        [Command("daily"), Summary("Daily Money")]
        public async Task Daily(IGuildUser user = null)
        {
            if (user != null && (user.IsBot || user.IsWebhook))
            {
                await EmbedExtensions.FromError(DiscordUtilities.NoBotsString, Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var self = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

            var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

            User target = null;

            if (user != null)
            {
                target = await Database.InsertOrGetUserAsync(user).ConfigureAwait(false);
            }

            ulong MoneyAmount = self.GetDailyAmount(Configuration);

            var previousAmount = user == null ? self.Money : target.Money;

            if (self.IsStreakReset(Configuration))
            {
                self.Streak = 0;
            }

            if ((user == null ? self : target).ProcessDaily(MoneyAmount, self))
            {
                string desc = $"You just got your daily of {gld.MoneyIcon}{MoneyAmount}";

                if(target != null)
                {
                    desc = $"You just gave your daily of {gld.MoneyIcon}{MoneyAmount.ToFormattedString()} to {user.Mention}";
                }

                var embed =
                    EmbedExtensions
                    .FromMessage("SkuldBank - Daily",
                        desc,
                        Context
                    )
                    .AddInlineField(
                        "Previous Amount",
                        $"{gld.MoneyIcon}{previousAmount.ToFormattedString()}"
                    )
                    .AddInlineField(
                        "New Amount",
                        $"{gld.MoneyIcon}{self.Money.ToFormattedString()}"
                    );

                if (self.Streak > 0)
                {
                    embed.AddField("Streak", $"You're on a streak of {self.Streak} days!!");
                }

                self.Streak = self.Streak.Add(1);

                if (self.MaxStreak < self.Streak)
                {
                    self.MaxStreak = self.Streak;
                }

                await
                    embed.QueueMessageAsync(Context)
                .ConfigureAwait(false);
            }
            else
            {
                TimeSpan remain = DateTime.Today.Date.AddDays(1) - DateTime.UtcNow;

                await
                    EmbedExtensions.FromError(
                        "SkuldBank - Daily",
                        $"You must wait `{remain.ToDifferenceString()}`",
                        Context
                    ).QueueMessageAsync(Context)
                .ConfigureAwait(false);
            }

            await Database.SaveChangesAsync().ConfigureAwait(false);
        }

        [Command("give"), Summary("Give your money to people")]
        public async Task Give(IGuildUser user, ulong amount)
        {
            if (user == Context.User)
            {
                await EmbedExtensions.FromError("SkuldBank - Transaction", "Can't give yourself money", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }
            if (user != null && (user.IsBot || user.IsWebhook))
            {
                await EmbedExtensions.FromError("SkuldBank - Transaction", DiscordUtilities.NoBotsString, Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var usr = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

            if (usr.Money >= amount)
            {
                var usr2 = await Database.InsertOrGetUserAsync(user).ConfigureAwait(false);

                TransactionService.DoTransaction(new TransactionStruct
                {
                    Amount = amount,
                    Sender = usr,
                    Receiver = usr2
                });

                await Database.SaveChangesAsync().ConfigureAwait(false);

                var dbGuild = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

                await
                    EmbedExtensions.FromMessage("Skuld Bank - Transaction", $"{Context.User.Mention} just gave {user.Mention} {dbGuild.MoneyIcon}{amount.ToFormattedString()}", Context)
                    .QueueMessageAsync(Context).ConfigureAwait(false);

                DogStatsd.Increment("bank.transactions");
                DogStatsd.Increment("bank.money.transferred", (int)amount);
            }
            else
            {
                await EmbedExtensions.FromError($"{Context.User.Mention} you can't give more than you have", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("rank"), Summary("Gets your or someone's current level")]
        [Alias("exp")]
        [Ratelimit(20, 1, Measure.Minutes)]
        public async Task Level(IGuildUser user = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (user == null)
            {
                user = (IGuildUser)Context.User;
            }

            var usr = await Database.InsertOrGetUserAsync(user).ConfigureAwait(false);

            if (!Database.Features.FirstOrDefault(x => x.Id == Context.Guild.Id).Experience)
            {
                await EmbedExtensions.FromError("Module `Experience` is disabled for this guild, ask an administrator to enable it using: `sk!guild-feature experience 1`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            if (user != null && (user.IsBot || user.IsWebhook))
            {
                await EmbedExtensions.FromError(DiscordUtilities.NoBotsString, Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var fontFile = Path.Combine(SkuldAppContext.FontDirectory, "NotoSans-Regular.ttf");

            if (!Directory.Exists(SkuldAppContext.FontDirectory))
            {
                Directory.CreateDirectory(SkuldAppContext.FontDirectory);
                await HttpWebClient.DownloadFileAsync(new Uri("https://static.skuldbot.uk/fonts/NotoSans-Regular.ttf"), fontFile).ConfigureAwait(false);
            }

            using var image = new MagickImage(new MagickColor("#212121"), 750, 300)
            {
                Format = MagickFormat.Png
            };

            if (string.IsNullOrEmpty(usr.Background))
            {
                image.Draw(new DrawableFillColor(new MagickColor("#3F51B5")), new DrawableRectangle(0, 0, 750, 300));
            }
            else if (usr.Background.StartsWith('#'))
            {
                image.Draw(new DrawableFillColor(new MagickColor(usr.Background)), new DrawableRectangle(0, 0, 750, 300));
            }
            else
            {
                using MagickImage img2 = new MagickImage(await HttpWebClient.ReturnStreamAsync(new Uri(usr.Background)).ConfigureAwait(false))
                {
                    FilterType = FilterType.Quadratic
                };
                img2.Resize(750, 0);
                img2.Crop(750, 300, Gravity.Center);
                image.Composite(img2);
            }

            //Box
            image.Draw(new DrawableFillColor(new MagickColor(0, 0, 0, 52428)), new DrawableRectangle(20, 20, 730, 280));

            var avatar = user.GetAvatarUrl(ImageFormat.Png) ?? user.GetDefaultAvatarUrl();

            using (MagickImage profileBackground = new MagickImage(await HttpWebClient.ReturnStreamAsync(new Uri(avatar)).ConfigureAwait(false), new MagickReadSettings
            {
                Format = MagickFormat.Png,
                BackgroundColor = MagickColors.None,
                Width = 128,
                Height = 128
            }))
            {
                using (var mask = new MagickImage("xc:none", 128, 128))
                {
                    mask.Draw(new DrawableFillColor(MagickColors.White), new DrawableCircle(64, 64, 62, 126));

                    profileBackground.Composite(mask, CompositeOperator.CopyAlpha);
                }

                using (MagickImage statusBackground = new MagickImage($"xc:{user.Status.HexFromStatus()}", 32, 32))
                {
                    statusBackground.BackgroundColor = MagickColors.None;

                    using (var mask = new MagickImage("xc:none", 36, 36))
                    {
                        mask.Draw(new DrawableFillColor(MagickColors.White), new DrawableCircle(16, 16, 14, 30));

                        statusBackground.Composite(mask, CompositeOperator.CopyAlpha);
                    }

                    profileBackground.Composite(statusBackground, 96, 96, CompositeOperator.Over);
                }

                image.Composite(profileBackground, 48, 84, CompositeOperator.Over);
            }

            var font = new DrawableFont(fontFile);
            var encoding = new DrawableTextEncoding(System.Text.Encoding.Unicode);
            var fontmed = new DrawableFontPointSize(30);
            var fontmedd = new DrawableFontPointSize(26);
            var white = new DrawableFillColor(new MagickColor(65535, 65535, 65535));

            var experiences = await Database.UserXp.ToListAsync().ConfigureAwait(false);
                
            var xps = experiences.Where(x => x.GuildId == Context.Guild.Id && x.IsVoiceExperience == false);

            xps = xps.OrderByDescending(x => x.TotalXP).ToList();

            ulong index = 0;

            foreach (var x in xps)
            {
                if (x.UserId == usr.Id)
                    break;
                else
                    index++;
            }

            var xp = experiences.GetJoinedExperience(user.Id,
                Context.IsPrivate ? null : Context.Guild);

            //Username
            using (MagickImage label3 = new MagickImage($"label:{user.FullName()}", new MagickReadSettings
            {
                BackgroundColor = MagickColors.Transparent,
                FillColor = MagickColors.White,
                Width = 510,
                Height = 60,
                TextGravity = Gravity.West,
                Font = fontFile
            }))
            {
                image.Composite(label3, 220, 80, CompositeOperator.Over);
            }

            image.Draw(font, fontmed, encoding, white, new DrawableText(220, 170, $"Rank {index + 1}/{experiences.Count}"));
            image.Draw(font, fontmed, encoding, white, new DrawableText(220, 210, $"Level: {xp.Level} ({xp.TotalXP.ToFormattedString()})"));

            ulong xpToNextLevel = DatabaseUtilities.GetXPLevelRequirement(xp.Level + 1, DiscordUtilities.PHI);

            int innerHeight = 256;

            //Progressbar
            image.Draw(new DrawableFillColor(new MagickColor("#212121")), new DrawableRectangle(20, innerHeight - 2, 730, 280));
            image.Draw(new DrawableFillColor(new MagickColor("#dfdfdf")), new DrawableRectangle(22, innerHeight, 728, 278));

            var percentage = (double)xp.XP / xpToNextLevel * 100;
            var mapped = percentage.Remap(0, 100, 22, 728);

            image.Draw(new DrawableFillColor(new MagickColor("#009688")), new DrawableRectangle(22, innerHeight, mapped, 278));

            //Current XP
            image.Draw(font, fontmedd, encoding, new DrawableText(25, 277, (xp.XP).ToFormattedString() + "XP"));

            //XP To Next
            using (MagickImage label5 = new MagickImage($"label:{(xpToNextLevel).ToFormattedString()}XP", new MagickReadSettings
            {
                BackgroundColor = MagickColors.Transparent,
                FillColor = MagickColors.Black,
                Width = 725,
                Height = 30,
                TextGravity = Gravity.East,
                FontPointsize = 26,
                Font = fontFile
            }))
            {
                image.Composite(label5, 0, 252, CompositeOperator.Over);
            }

            MemoryStream outputStream = new MemoryStream();

            image.Write(outputStream);

            outputStream.Position = 0;

            await "".QueueMessageAsync(Context, outputStream, type: Services.Messaging.Models.MessageType.File).ConfigureAwait(false);
        }

        [Command("rep"), Summary("Gives someone rep or checks your rep")]
        [Ratelimit(20, 1, Measure.Minutes)]
        [Usage("@person")]
        public async Task GiveRep([Remainder]IGuildUser user = null)
        {
            if (user != null && (user.IsBot || user.IsWebhook))
            {
                await EmbedExtensions.FromError(DiscordUtilities.NoBotsString, Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if ((user != null && user.Id == Context.User.Id) || user == null)
            {
                var count = Database.Reputations.Count(x => x.Repee == Context.User.Id);

                if (count > 0)
                {
                    var ordered = (await Database.Reputations.ToListAsync()).OrderByDescending(x => x.Timestamp);
                    var mostRecent = ordered.FirstOrDefault(x => x.Repee == Context.User.Id);

                    await $"Your repuation is at: {count}rep\nYour most recent rep was by {Context.Client.GetUser(mostRecent.Reper).FullName()} at {mostRecent.Timestamp.FromEpoch()}"
                        .QueueMessageAsync(Context).ConfigureAwait(false);
                }
                else
                {
                    await "You have no reputation".QueueMessageAsync(Context).ConfigureAwait(false);
                }
                return;
            }

            var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);
            var usr = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);
            var repee = await Database.InsertOrGetUserAsync(user).ConfigureAwait(false);

            if (Database.Reputations.Any(x => x.Repee == repee.Id && x.Reper == Context.User.Id))
            {
                await "You have already given this person a reputation point.".QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            Database.Reputations.Add(new Reputation
            {
                Repee = repee.Id,
                Reper = usr.Id,
                Timestamp = DateTime.UtcNow.ToEpoch()
            });

            await Database.SaveChangesAsync().ConfigureAwait(false);

            await $"You gave rep to {user.Mention}".QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("unrep"), Summary("Removes a rep")]
        [Ratelimit(20, 1, Measure.Minutes)]
        [Usage("@person")]
        public async Task RemoveRep([Remainder]IGuildUser user)
        {
            if (user != null && (user.IsBot || user.IsWebhook))
            {
                await EmbedExtensions.FromError(DiscordUtilities.NoBotsString, Context).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if ((user != null && user.Id == Context.User.Id) || user == null)
            {
                await "You can't modify your own reputation".QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);
            var usr = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

            if (Database.Reputations.Any(x => x.Repee == user.Id && x.Reper == Context.User.Id))
            {
                Database.Reputations.Remove(Database.Reputations.FirstOrDefault(x => x.Reper == usr.Id && x.Repee == user.Id));

                await Database.SaveChangesAsync().ConfigureAwait(false);

                await $"You gave rep to {user.Mention}".QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            await "You haven't given this person a reputation point.".QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("title"), Summary("Sets Title"), RequireDatabase]
        [Usage("The village thief")]
        public async Task SetTitle([Remainder]string title = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var usr = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

            if (title == null)
            {
                usr.Title = "";

                await Database.SaveChangesAsync().ConfigureAwait(false);

                await EmbedExtensions.FromSuccess("Successfully cleared your title.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                usr.Title = title;

                await Database.SaveChangesAsync().ConfigureAwait(false);

                await EmbedExtensions.FromSuccess($"Successfully set your title to **{title}**", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Name("Blocking"), Group("block")]
        public class Block : InteractiveBase<ShardedCommandContext>
        {
            [Command("recurring"), Summary("Blocks people from patting you on recurring digits"), RequireDatabase]
            public async Task BlockRecurring()
            {
                using var Database = new SkuldDbContextFactory().CreateDbContext();

                var usr = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

                usr.RecurringBlock = !usr.RecurringBlock;

                await Database.SaveChangesAsync().ConfigureAwait(false);

                await EmbedExtensions.FromSuccess($"Set RecurringBlock to: {usr.RecurringBlock}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }

            [Command("actions"), Summary("Blocks people from performing actions"), RequireDatabase]
            [Usage("@person")]
            public async Task BlockActions([Remainder] IUser user)
            {
                using var database = new SkuldDbContextFactory().CreateDbContext();

                var res = database.BlockedActions.ToList().FirstOrDefault(x => x.Blocker == Context.User.Id && x.Blockee == user.Id);

                if (res != null)
                {
                    database.BlockedActions.Remove(res);

                    await EmbedExtensions.FromSuccess($"Unblocked {user.Mention}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                }
                else
                {
                    database.BlockedActions.Add(new BlockedAction
                    {
                        Blocker = Context.User.Id,
                        Blockee = user.Id
                    });

                    await EmbedExtensions.FromSuccess($"Blocked {user.Mention}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                }

                await database.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        [Name("Background"), Group("background")]
        public class CustomBG : InteractiveBase<ShardedCommandContext>
        {
            public SkuldConfig Configuration { get; set; }

            [Command("buy"), Summary("Buy permanent custom backgrounds"), RequireDatabase]
            public async Task BuyCBG()
            {
                using var Database = new SkuldDbContextFactory().CreateDbContext();

                var usr = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);
                var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

                if (!usr.UnlockedCustBG)
                {
                    if (usr.Money >= 40000)
                    {
                        TransactionService.DoTransaction(new TransactionStruct
                        {
                            Amount = 40000,
                            Sender = usr
                        });
                        usr.UnlockedCustBG = true;

                        await Database.SaveChangesAsync().ConfigureAwait(false);

                        await EmbedExtensions.FromSuccess($"You've successfully unlocked custom backgrounds, use: {gld.Prefix ?? Configuration.Prefix}set-custombg [URL] to set your background", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                    else
                    {
                        await EmbedExtensions.FromError($"You need at least {gld.MoneyIcon}40,000 to unlock custom backgrounds", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                }
                else
                {
                    await EmbedExtensions.FromInfo($"You already unlocked custom backgrounds, use: {gld.Prefix ?? Configuration.Prefix}set-custombg [URL] to set your background", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                }
            }

            [Command("set"), Summary("Sets your custom background Image"), RequireDatabase]
            [Usage("https://example.com/SuperAwesomeImage.png", "#ff0000")]
            public async Task SetCBG(string link = null)
            {
                using var Database = new SkuldDbContextFactory().CreateDbContext();

                var usr = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);
                var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

                if (Uri.TryCreate(link, UriKind.Absolute, out var res))
                {
                    if (usr.Money >= 900)
                    {
                        TransactionService.DoTransaction(new TransactionStruct
                        {
                            Amount = 900,
                            Sender = usr
                        });
                        usr.Background = res.OriginalString;

                        await Database.SaveChangesAsync().ConfigureAwait(false);

                        await EmbedExtensions.FromSuccess("Set your Background", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                    else
                    {
                        await EmbedExtensions.FromError($"You need at least {gld.MoneyIcon}900 to change your background", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                    return;
                }
                else if (link.ToLowerInvariant() == "reset")
                {
                    usr.Background = "#3F51B5";

                    await Database.SaveChangesAsync().ConfigureAwait(false);

                    await EmbedExtensions.FromSuccess($"Reset your background to: {usr.Background}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    return;
                }
                else if (int.TryParse(link[0] != '#' ? link : link.Remove(0, 1), System.Globalization.NumberStyles.HexNumber, null, out _))
                {
                    if (usr.Money >= 300)
                    {
                        TransactionService.DoTransaction(new TransactionStruct
                        {
                            Amount = 300,
                            Sender = usr
                        });

                        usr.Background = link[0] != '#' ? "#" + link : link;

                        await Database.SaveChangesAsync().ConfigureAwait(false);

                        await EmbedExtensions.FromSuccess("Set your Background", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                    else
                    {
                        await EmbedExtensions.FromError($"You need at least {gld.MoneyIcon}300 to change your background", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    }
                }
                else
                {
                    await EmbedExtensions.FromError($"Malformed Entry", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    return;
                }
            }

            [Command("preview"), Summary("Previews a custom bg")]
            [Ratelimit(20, 1, Measure.Minutes)]
            [Usage("https://example.com/SuperAwesomeImage.png", "#445566")]
            public async Task PreviewCustomBG(string link)
            {
                var fontsFolder = SkuldAppContext.FontDirectory;
                var fontFile = Path.Combine(fontsFolder, "NotoSans-Regular.ttf");

                if (!Directory.Exists(fontsFolder))
                {
                    Directory.CreateDirectory(fontsFolder);
                    await HttpWebClient.DownloadFileAsync(new Uri("https://static.skuldbot.uk/fonts/NotoSans-Regular.ttf"), fontFile).ConfigureAwait(false);
                }

                using MagickImage image = new MagickImage(new MagickColor("#212121"), 600, 510)
                {
                    Format = MagickFormat.Png
                };

                if (string.IsNullOrEmpty(link))
                {
                    image.Draw(new DrawableFillColor(new MagickColor("#3F51B5")), new DrawableRectangle(0, 0, 600, 228));
                }
                else if (int.TryParse(link[0] != '#' ? link : link.Remove(0, 1), System.Globalization.NumberStyles.HexNumber, null, out _))
                {
                    string hex = link;

                    if (link[0] != '#')
                        hex = $"#{hex}";

                    image.Draw(new DrawableFillColor(new MagickColor(hex)), new DrawableRectangle(0, 0, 600, 228));
                }
                else
                {
                    using MagickImage img2 = new MagickImage(await HttpWebClient.ReturnStreamAsync(new Uri(link)).ConfigureAwait(false))
                    {
                        FilterType = FilterType.Quadratic
                    };
                    img2.Resize(600, 0);
                    img2.Crop(600, 228, Gravity.Center);
                    image.Composite(img2);
                }

                var avatar = Context.User.GetAvatarUrl(ImageFormat.Png) ?? Context.User.GetDefaultAvatarUrl();

                using (MagickImage profileBackground = new MagickImage(await HttpWebClient.ReturnStreamAsync(new Uri(avatar)).ConfigureAwait(false), new MagickReadSettings
                {
                    Format = MagickFormat.Png,
                    BackgroundColor = MagickColors.None,
                    Width = 128,
                    Height = 128
                }))
                {
                    using (var mask = new MagickImage("xc:none", 128, 128))
                    {
                        mask.Draw(new DrawableFillColor(MagickColors.White), new DrawableCircle(64, 64, 62, 126));

                        profileBackground.Composite(mask, CompositeOperator.CopyAlpha);
                    }

                    using (MagickImage statusBackground = new MagickImage($"xc:{Context.User.Status.HexFromStatus()}", 32, 32))
                    {
                        statusBackground.BackgroundColor = MagickColors.None;

                        using (var mask = new MagickImage("xc:none", 32, 32))
                        {
                            mask.Draw(new DrawableFillColor(MagickColors.White), new DrawableCircle(16, 16, 14, 30));

                            statusBackground.Composite(mask, CompositeOperator.CopyAlpha);
                        }

                        profileBackground.Composite(statusBackground, 96, 96, CompositeOperator.Over);
                    }

                    image.Composite(profileBackground, 236, 32, CompositeOperator.Over);
                }

                var font = new DrawableFont(fontFile);
                var encoding = new DrawableTextEncoding(System.Text.Encoding.Unicode);
                var fontsize = new DrawableFontPointSize(20);
                var white = new DrawableFillColor(new MagickColor(65535, 65535, 65535));

                var exp = new UserExperience
                {
                    TotalXP = 1234567890
                };

                exp.Level = DatabaseUtilities.GetLevelFromTotalXP(exp.TotalXP, DiscordUtilities.PHI);

                exp.XP = DatabaseUtilities.GetXPLevelRequirement(exp.Level, DiscordUtilities.PHI) / 2;

                int ylevel1 = 365, ylevel2 = 405, ylevel3 = 445;

                //Bar
                image.Draw(new DrawableFillColor(new MagickColor(0, 0, 0, 52428)), new DrawableRectangle(0, 188, 600, 228));

                //Rep
                using (MagickImage label = new MagickImage($"label:1,234 Rep", new MagickReadSettings
                {
                    BackgroundColor = MagickColors.Transparent,
                    FillColor = MagickColors.White,
                    Width = 580,
                    Height = 30,
                    TextGravity = Gravity.West,
                    FontPointsize = 30,
                    Font = fontFile
                }))
                {
                    image.Composite(label, 20, 193, CompositeOperator.Over);
                }

                //Money
                using (MagickImage label2 = new MagickImage($"label:₩123,456,789", new MagickReadSettings
                {
                    BackgroundColor = MagickColors.Transparent,
                    FillColor = MagickColors.White,
                    Width = 580,
                    Height = 30,
                    TextGravity = Gravity.East,
                    FontPointsize = 30,
                    Font = fontFile
                }))
                {
                    image.Composite(label2, 0, 193, CompositeOperator.Over);
                }

                //Username
                using (MagickImage label3 = new MagickImage($"label:{Context.User.FullName()}", new MagickReadSettings
                {
                    BackgroundColor = MagickColors.Transparent,
                    FillColor = MagickColors.White,
                    Width = 600,
                    Height = 40,
                    TextGravity = Gravity.Center,
                    Font = fontFile
                }))
                {
                    image.Composite(label3, 0, 230, CompositeOperator.Over);
                }

                //Title
                using MagickImage label4 = new MagickImage($"label:Preview Card", new MagickReadSettings
                {
                    BackgroundColor = MagickColors.Transparent,
                    FillColor = MagickColors.White,
                    Width = 600,
                    Height = 40,
                    TextGravity = Gravity.Center,
                    Font = fontFile
                });
                image.Composite(label4, 0, 270, CompositeOperator.Over);

                //YLevel 1
                var dailyText = $"Daily: {((ulong)0).FromEpoch().ToString("yyyy/MM/dd HH:mm:ss")}";
                var dmetr = image.FontTypeMetrics(dailyText, true);
                var rightPos = 600 - (dmetr.TextWidth * 2);

                image.Draw(font, fontsize, encoding, white, new DrawableText(22, ylevel1, $"Global Rank: 420/-420"));

                image.Draw(font, fontsize, encoding, white, new DrawableText(rightPos, ylevel1, dailyText));

                //YLevel 2
                image.Draw(font, fontsize, encoding, white, new DrawableText(22, ylevel2, $"Pasta Karma: 123,456,789"));
                image.Draw(font, fontsize, encoding, white, new DrawableText(rightPos, ylevel2, $"Fav. Cmd: profile (123,456,789)"));

                //YLevel 3
                image.Draw(font, fontsize, encoding, white, new DrawableText(22, ylevel3, $"Level: {exp.Level.ToFormattedString()} ({exp.TotalXP.ToFormattedString()})"));
                image.Draw(font, fontsize, encoding, white, new DrawableText(rightPos, ylevel3, $"Pats: 7,777/Patted: 7,777"));

                ulong xpToNextLevel = DatabaseUtilities.GetXPLevelRequirement(exp.Level + 1, DiscordUtilities.PHI);

                //Progressbar
                image.Draw(new DrawableFillColor(new MagickColor("#212121")), new DrawableRectangle(20, 471, 580, 500));
                image.Draw(new DrawableFillColor(new MagickColor("#dfdfdf")), new DrawableRectangle(22, 473, 578, 498));

                var percentage = (double)exp.XP / xpToNextLevel * 100;
                var mapped = percentage.Remap(0, 100, 22, 578);

                image.Draw(new DrawableFillColor(new MagickColor("#009688")), new DrawableRectangle(22, 473, mapped, 498));

                //Current XP
                image.Draw(font, fontsize, encoding, new DrawableText(25, 493, (exp.XP).ToFormattedString() + "XP"));

                //XP To Next
                using (MagickImage label5 = new MagickImage($"label:{(xpToNextLevel).ToFormattedString()}XP", new MagickReadSettings
                {
                    BackgroundColor = MagickColors.Transparent,
                    FillColor = MagickColors.Black,
                    Width = 575,
                    Height = 20,
                    TextGravity = Gravity.East,
                    FontPointsize = 20,
                    Font = fontFile
                }))
                {
                    image.Composite(label5, 0, 475, CompositeOperator.Over);
                }

                {
                    var col = MagickColors.Red;
                    col.A = ushort.MaxValue / 2;

                    using MagickImage watermark = new MagickImage($"label:PREVIEW", new MagickReadSettings
                    {
                        BackgroundColor = MagickColors.Transparent,
                        FillColor = col,
                        Width = 600,
                        Height = 300,
                        TextGravity = Gravity.Center,
                        FontPointsize = 100,
                        FontWeight = FontWeight.ExtraBold,
                        Font = fontFile
                    });

                    watermark.Rotate(-45);

                    image.Composite(watermark, Gravity.Center, 0, 0, CompositeOperator.Over);
                }

                MemoryStream outputStream = new MemoryStream();

                image.Write(outputStream);

                outputStream.Position = 0;

                await "".QueueMessageAsync(Context, outputStream, type: Services.Messaging.Models.MessageType.File).ConfigureAwait(false);
            }
        }

        [Command("settimezone"), Summary("Sets your timezone")]
        [Usage("UTC")]
        public async Task SetTimeZone([Remainder]DateTimeZone timezone)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var user = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

            user.TimeZone = timezone.Id;

            await Database.SaveChangesAsync().ConfigureAwait(false);

            await
                new EmbedBuilder()
                .AddFooter(Context)
                .AddAuthor(Context.Client)
                .WithDescription($"Set your timezone to: {timezone.Id}")
            .QueueMessageAsync(Context).ConfigureAwait(false);
        }
    }
}