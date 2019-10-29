using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using ImageMagick;
using Microsoft.EntityFrameworkCore.Internal;
using Skuld.APIS;
using Skuld.Bot.Services;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Generic.Models;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Discord.Extensions;
using Skuld.Discord.Preconditions;
using Skuld.Discord.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Bot.Commands
{
    [Group, RequireEnabledModule, RequireDatabase]
    public class Profiles : InteractiveBase<ShardedCommandContext>
    {
        public SkuldConfig Configuration { get => HostSerivce.Configuration; }
        public BaseClient WebHandler { get; set; }

        [Command("money"), Summary("Gets a user's money")]
        public async Task Money([Remainder]IGuildUser user = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (user != null && (user.IsBot || user.IsWebhook))
            {
                await DiscordTools.NoBotsString.QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
                return;
            }

            if (user == null)
                user = (IGuildUser)Context.User;

            var gld = await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false);
            var dbusr = await Database.GetUserAsync(user).ConfigureAwait(false);

            await $"{user.Mention} has {gld.MoneyIcon}{dbusr.Money.ToString("N0")}".QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("profile"), Summary("Get a users profile")]
        public async Task Profile([Remainder]IGuildUser user = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (user != null && (user.IsBot || user.IsWebhook))
            {
                await DiscordTools.NoBotsString.QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
                return;
            }

            if (user == null)
                user = Context.User as IGuildUser;

            var profileuser = await Database.GetUserAsync(user).ConfigureAwait(false);

            var imagickCache = SkuldAppContext.IMagickCache;
            var folder = SkuldAppContext.ProfileDirectory;
            var fontsFolder = SkuldAppContext.FontDirectory;
            var fontFile = Path.Combine(fontsFolder, "NotoSans-Regular.ttf");

            if (!Directory.Exists(fontsFolder))
            {
                Directory.CreateDirectory(fontsFolder);
                await WebHandler.DownloadFileAsync(new Uri("https://static.skuldbot.uk/fonts/NotoSans-Regular.ttf"), fontFile).ConfigureAwait(false);
            }

            if (!Directory.Exists(imagickCache))
            {
                Directory.CreateDirectory(imagickCache);
            }

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            MagickAnyCPU.CacheDirectory = imagickCache;

            var imageLocation = folder + user.Id + ".png";

            var imageBackgroundFolder = Path.Combine(AppContext.BaseDirectory, "storage/backgroundCache/");

            var imageBackgroundFile = Path.Combine(imageBackgroundFolder, profileuser.Id + "_background.png");

            if (!Directory.Exists(imageBackgroundFolder))
            {
                Directory.CreateDirectory(imageBackgroundFolder);
            }

            if (!string.IsNullOrEmpty(profileuser.Background))
            {
                if (!profileuser.Background.StartsWith('#'))
                {
                    await WebHandler.DownloadFileAsync(new Uri(profileuser.Background), imageBackgroundFile).ConfigureAwait(false);
                }
            }

            using (MagickImage image = new MagickImage(new MagickColor("#212121"), 600, 510))
            {
                image.Format = MagickFormat.Png;
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
                    using MagickImage img2 = new MagickImage(imageBackgroundFile)
                    {
                        FilterType = FilterType.Quadratic
                    };
                    img2.Resize(600, 0);
                    img2.Crop(600, 228, Gravity.Center);
                    image.Composite(img2);
                }

                var avatar = user.GetAvatarUrl(ImageFormat.Png) ?? user.GetDefaultAvatarUrl();

                var avatarLocation = Path.Combine(AppContext.BaseDirectory, "storage/avatarCache");
                var avatarFile = Path.Combine(avatarLocation, user.Id + ".png");

                if (!Directory.Exists(avatarLocation))
                {
                    Directory.CreateDirectory(avatarLocation);
                }

                await WebHandler.DownloadFileAsync(new Uri(avatar), avatarFile).ConfigureAwait(false);

                using (MagickImage profileBackground = new MagickImage(avatarFile, 128, 128))
                {
                    profileBackground.BackgroundColor = MagickColors.None;

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

                    if (Context.Client.GetUser(user.Id).MutualGuilds.Any(x => x.GetUser(user.Id).PremiumSince.HasValue))
                    {
                        var prem = Context.Client.GetUser(user.Id).MutualGuilds.FirstOrDefault(x => x.GetUser(user.Id).PremiumSince.HasValue).GetUser(user.Id).PremiumSince;

                        var months = DiscordTools.MonthsBetween(DateTime.UtcNow, prem.Value.Date);
                        string emblem = "";

                        if (months <= 1)
                        {
                            emblem = DiscordUtilities.Level1UserBoost;
                        }
                        if (months == 2)
                        {
                            emblem = DiscordUtilities.Level2UserBoost;
                        }
                        if (months >= 3)
                        {
                            emblem = DiscordUtilities.Level3UserBoost;
                        }

                        using MagickImage img3 = new MagickImage(Path.Combine(AppContext.BaseDirectory, "storage/boost", emblem), 256, 256);

                        img3.Settings.BackgroundColor = MagickColors.None;
                        img3.BackgroundColor = MagickColors.None;
                        img3.Alpha(AlphaOption.Set);
                        img3.ColorFuzz = new Percentage(10);
                        img3.FloodFill(MagickColors.None, 1, 1);

                        img3.Resize(48, 48);

                        profileBackground.Composite(img3, -4, 90, CompositeOperator.Over);
                    }

                    image.Composite(profileBackground, 236, 32, CompositeOperator.Over);
                }

                var font = new DrawableFont(fontFile);
                var encoding = new DrawableTextEncoding(System.Text.Encoding.Unicode);
                var fontsize = new DrawableFontPointSize(20);
                var white = new DrawableFillColor(new MagickColor(65535, 65535, 65535));

                var experiences = Database.UserXp.Where(x => x.UserId == profileuser.Id);

                var exp = new UserExperience();

                foreach (var experience in experiences)
                {
                    exp.TotalXP += experience.TotalXP;
                    exp.XP += experience.XP;
                    exp.Level += exp.Level;
                }

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

                //Money
                using (MagickImage label2 = new MagickImage($"label:{(await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false)).MoneyIcon}{profileuser.Money.ToString("N0")}", new MagickReadSettings
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
                using (MagickImage label3 = new MagickImage($"label:{user.FullName()}", new MagickReadSettings
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
                    using MagickImage label4 = new MagickImage($"label:{profileuser.Title}", new MagickReadSettings
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
                var dailyText = $"Daily: {profileuser.LastDaily.FromEpoch().ToString("yyyy/MM/dd HH:mm:ss")}";
                var dmetr = image.FontTypeMetrics(dailyText, true);
                var rightPos = 600 - (dmetr.TextWidth * 2);

                var rankraw = Database.GetOrderedGlobalExperienceLeaderboard();

                (ulong, ulong) rank = ((ulong)rankraw.IndexOf(rankraw.FirstOrDefault(x => x.UserId == profileuser.Id)) + 1, (ulong)rankraw.Count());

                image.Draw(font, fontsize, encoding, white, new DrawableText(22, ylevel1, $"Global Rank: {rank.Item1}/{rank.Item2}"));
                image.Draw(font, fontsize, encoding, white, new DrawableText(rightPos, ylevel1, dailyText));

                //YLevel 2
                image.Draw(font, fontsize, encoding, white, new DrawableText(22, ylevel2, $"Pasta Karma: {Database.GetPastaKarma(profileuser.Id).ToString("N0")}"));
                var favcommand = Database.UserCommandUsage.Where(x => x.UserId == profileuser.Id).OrderByDescending(x => x.Usage).FirstOrDefault();
                image.Draw(font, fontsize, encoding, white, new DrawableText(rightPos, ylevel2, $"Fav. Cmd: {(favcommand == null ? "N/A" : favcommand.Command)} ({(favcommand == null ? "0" : favcommand.Usage.ToString("N0"))})"));

                //YLevel 3
                image.Draw(font, fontsize, encoding, white, new DrawableText(22, ylevel3, $"Level: {exp.Level} ({exp.TotalXP.ToString("N0")})"));
                image.Draw(font, fontsize, encoding, white, new DrawableText(rightPos, ylevel3, $"Pats: {profileuser.Pats.ToString("N0")}/Patted: {profileuser.Patted.ToString("N0")}"));

                ulong xpToNextLevel = DiscordTools.GetXPLevelRequirement(exp.Level + 1, DiscordTools.PHI);

                //Progressbar
                image.Draw(new DrawableFillColor(new MagickColor("#212121")), new DrawableRectangle(20, 471, 580, 500));
                image.Draw(new DrawableFillColor(new MagickColor("#dfdfdf")), new DrawableRectangle(22, 473, 578, 498));

                var percentage = (double)exp.XP / xpToNextLevel * 100;
                var mapped = percentage.Remap(0, 100, 22, 578);

                image.Draw(new DrawableFillColor(new MagickColor("#009688")), new DrawableRectangle(22, 473, mapped, 498));

                //Current XP
                image.Draw(font, fontsize, encoding, new DrawableText(25, 493, (exp.XP).ToString("N0") + "XP"));

                //XP To Next
                using (MagickImage label5 = new MagickImage($"label:{(xpToNextLevel).ToString("N0")}XP", new MagickReadSettings
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

                image.Write(imageLocation);

                image.Dispose();
            }

            await "".QueueMessageAsync(Context, Discord.Models.MessageType.File, imageLocation).ConfigureAwait(false);
        }

        [Command("daily"), Summary("Daily Money")]
        public async Task Daily(IGuildUser user = null)
        {
            if (user != null && (user.IsBot || user.IsWebhook))
            {
                await DiscordTools.NoBotsString.QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
                return;
            }

            using var Database = new SkuldDbContextFactory().CreateDbContext();
            var gld = await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false);

            if (user == null)
                user = (IGuildUser)Context.User;

            bool isSelf = false;

            if (user.Id == Context.User.Id)
                isSelf = true;

            var self = await Database.GetUserAsync(Context.User).ConfigureAwait(false);
            var target = await Database.GetUserAsync(user).ConfigureAwait(false);

            if (self.LastDaily == 0)
            {
                self.LastDaily = DateTime.UtcNow.ToEpoch();
                target.Money += Configuration.Preferences.DailyAmount;
                await Database.SaveChangesAsync().ConfigureAwait(false);

                await $"You {(!isSelf ? $"just gave {user.Mention}" : "just got")} your daily of {gld.MoneyIcon}{Configuration.Preferences.DailyAmount}".QueueMessageAsync(Context).ConfigureAwait(false);

                return;
            }
            else
            {
                if (self.LastDaily < DateTime.UtcNow.Date.ToEpoch())
                {
                    self.LastDaily = DateTime.UtcNow.ToEpoch();
                    target.Money += Configuration.Preferences.DailyAmount;
                    await Database.SaveChangesAsync().ConfigureAwait(false);

                    await $"You {(!isSelf ? $"just gave {user.Mention}" : "just got")} your daily of {gld.MoneyIcon}{Configuration.Preferences.DailyAmount}".QueueMessageAsync(Context).ConfigureAwait(false);

                    return;
                }
                else
                {
                    TimeSpan remain = TimeSpan.FromTicks((DateTime.Today.AddDays(1).ToEpoch() - DateTime.UtcNow.ToEpoch()).FromEpoch().Ticks);
                    string remaining = remain.Hours + " Hours " + remain.Minutes + " Minutes " + remain.Seconds + " Seconds";
                    await $"You must wait `{remaining}`".QueueMessageAsync(Context).ConfigureAwait(false);
                }
            }
        }

        [Command("give"), Summary("Give your money to people")]
        public async Task Give(IGuildUser user, ulong amount)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (user != null && (user.IsBot || user.IsWebhook))
            {
                await DiscordTools.NoBotsString.QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
                return;
            }

            var usr = await Database.GetUserAsync(Context.User).ConfigureAwait(false);

            if (usr.Money - amount > 0)
            {
                var usr2 = await Database.GetUserAsync(user).ConfigureAwait(false);

                usr.Money -= amount;
                usr2.Money += amount;

                await Database.SaveChangesAsync().ConfigureAwait(false);

                await $"{Context.User.Mention} just gave {user.Mention} {(await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false)).MoneyIcon}{amount.ToString("N0")}"
                    .QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                await $"{Context.User.Mention} you can't give more than you have".QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
            }
        }

        [Command("rank"), Summary("Gets your or someone's current level")]
        [Alias("exp")]
        public async Task Level(IGuildUser user = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (user == null)
                user = (IGuildUser)Context.User;

            var usr = await Database.GetUserAsync(user).ConfigureAwait(false);

            if (!Database.Features.FirstOrDefault(x => x.Id == Context.Guild.Id).Experience)
            {
                await "Module `Experience` is disabled for this guild, ask an administrator to enable it using: `sk!guild-feature experience 1`".QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
                return;
            }

            if (user != null && (user.IsBot || user.IsWebhook))
            {
                await DiscordTools.NoBotsString.QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false); ;
                return;
            }

            if (user == null)
            {
                user = Context.User as IGuildUser;
            }

            var folder = Path.Combine(AppContext.BaseDirectory, "/storage/exp/");
            var fontFile = Path.Combine(SkuldAppContext.FontDirectory, "NotoSans-Regular.ttf");

            if (!Directory.Exists(SkuldAppContext.FontDirectory))
            {
                Directory.CreateDirectory(SkuldAppContext.FontDirectory);
                await WebHandler.DownloadFileAsync(new Uri("https://static.skuldbot.uk/fonts/NotoSans-Regular.ttf"), fontFile).ConfigureAwait(false);
            }

            if (!Directory.Exists(SkuldAppContext.IMagickCache))
                Directory.CreateDirectory(SkuldAppContext.IMagickCache);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            MagickAnyCPU.CacheDirectory = SkuldAppContext.IMagickCache;

            var imageLocation = folder + user.Id + ".png";

            var imageBackgroundFolder = Path.Combine(AppContext.BaseDirectory, "/storage/backgroundCache/");

            var imageBackgroundFile = Path.Combine(imageBackgroundFolder, usr.Id + "_background.png");

            if (!Directory.Exists(imageBackgroundFolder))
                Directory.CreateDirectory(imageBackgroundFolder);

            if (!usr.Background.StartsWith('#') && !string.IsNullOrEmpty(usr.Background))
            {
                await WebHandler.DownloadFileAsync(new Uri(usr.Background), imageBackgroundFile).ConfigureAwait(false);
            }

            using (var image = new MagickImage(new MagickColor("#212121"), 750, 300))
            {
                image.Format = MagickFormat.Png;

                if (usr.Background.StartsWith('#'))
                {
                    var col = usr.Background.FromHex();
                    image.Draw(new DrawableFillColor(new MagickColor(col.R, col.G, col.B)), new DrawableRectangle(0, 0, 750, 300));
                }
                else if (string.IsNullOrEmpty(usr.Background))
                {
                    image.Draw(new DrawableFillColor(new MagickColor("#3F51B5")), new DrawableRectangle(0, 0, 750, 300));
                }
                else
                {
                    using MagickImage img2 = new MagickImage(imageBackgroundFile)
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

                var avatarLocation = Path.Combine(AppContext.BaseDirectory, "storage/avatarCache");
                var avatarFile = Path.Combine(avatarLocation, user.Id + ".png");

                if (!Directory.Exists(avatarLocation))
                {
                    Directory.CreateDirectory(avatarLocation);
                }

                await WebHandler.DownloadFileAsync(new Uri(avatar), avatarFile).ConfigureAwait(false);

                using (MagickImage profileBackground = new MagickImage(avatarFile, 128, 128))
                {
                    profileBackground.BackgroundColor = MagickColors.None;

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

                    image.Composite(profileBackground, 64, 84, CompositeOperator.Over);
                }

                var font = new DrawableFont(fontFile);
                var encoding = new DrawableTextEncoding(System.Text.Encoding.Unicode);
                var fontmed = new DrawableFontPointSize(30);
                var fontmedd = new DrawableFontPointSize(26);
                var white = new DrawableFillColor(new MagickColor(65535, 65535, 65535));

                var experiences = Database.UserXp.Where(x => x.GuildId == Context.Guild.Id);

                ulong index = 0;

                foreach (var x in experiences)
                {
                    if (x.UserId == usr.Id)
                        break;
                    else
                        index++;
                }

                var xp = experiences.FirstOrDefault(x => x.UserId == usr.Id);

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

                image.Draw(font, fontmed, encoding, white, new DrawableText(220, 170, $"Rank {index + 1}/{experiences.Count()}"));
                image.Draw(font, fontmed, encoding, white, new DrawableText(220, 210, $"Level: {xp.Level} ({xp.TotalXP.ToString("N0")})"));

                ulong xpToNextLevel = DiscordTools.GetXPLevelRequirement(xp.Level + 1, DiscordTools.PHI);

                int innerHeight = 256;

                //Progressbar
                image.Draw(new DrawableFillColor(new MagickColor("#212121")), new DrawableRectangle(20, innerHeight - 2, 730, 280));
                image.Draw(new DrawableFillColor(new MagickColor("#dfdfdf")), new DrawableRectangle(22, innerHeight, 728, 278));

                var percentage = (double)xp.XP / xpToNextLevel * 100;
                var mapped = percentage.Remap(0, 100, 22, 728);

                image.Draw(new DrawableFillColor(new MagickColor("#009688")), new DrawableRectangle(22, innerHeight, mapped, 278));

                //Current XP
                image.Draw(font, fontmedd, encoding, new DrawableText(25, 275, (xp.XP).ToString("N0") + "XP"));

                //XP To Next
                using (MagickImage label5 = new MagickImage($"label:{(xpToNextLevel).ToString("N0")}XP", new MagickReadSettings
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
                    image.Composite(label5, 0, 250, CompositeOperator.Over);
                }

                image.Write(imageLocation);
            }

            await "".QueueMessageAsync(Context, Discord.Models.MessageType.File, imageLocation).ConfigureAwait(false);
        }

        [Command("heal"), Summary("Shows you how much you can heal by")]
        public async Task HealAmount()
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();
            var usr = await Database.GetUserAsync(Context.User).ConfigureAwait(false);

            var amnt = Math.Round(Math.Ceiling(usr.Money * 0.8));
            await $"You can heal for: `{Math.Floor(amnt)}`HP".QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("heal"), Summary("Heal yourself or others here")]
        public async Task Heal(uint hp, [Remainder] IGuildUser user = null)
        {
            if (user != null && (user.IsBot || user.IsWebhook))
            {
                await DiscordTools.NoBotsString.QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
                return;
            }

            if (user == null)
                user = (IGuildUser)Context.User;

            using var Database = new SkuldDbContextFactory().CreateDbContext();
            var gld = await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false);
            var usr = await Database.GetUserAsync(Context.User).ConfigureAwait(false);
            var target = await Database.GetUserAsync(user).ConfigureAwait(false);

            string pref = $"{(user == (IGuildUser)Context.User ? "You" : "They")}";

            if (target.HP == 10000)
            {
                await $"{pref}'re already at max health".QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }
            var amount = GetCostOfHP(hp);
            if (usr.Money < amount)
            {
                await "You don't have enough money for this action".QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }
            if (hp > (10000 - target.HP))
            {
                await ($"{pref} only need to heal by: " + (10000 - target.HP)).QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            target.HP += (int)hp;
            usr.Money -= amount;

            if (usr.HP > 10000)
                usr.HP = 10000;

            await Database.SaveChangesAsync().ConfigureAwait(false);

            if (pref[^1] == 'y')
            {
                pref = "Their";
            }
            else
            {
                pref += "r";
            }

            await $"You have healed {pref} HP by {hp} for {gld.MoneyIcon}{amount.ToString("N0")}".QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("rep"), Summary("Gives someone rep or checks your rep")]
        public async Task GiveRep([Remainder]IGuildUser user = null)
        {
            if (user != null && (user.IsBot || user.IsWebhook))
            {
                await DiscordTools.NoBotsString.QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
                return;
            }

            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if ((user != null && user.Id == Context.User.Id) || user == null)
            {
                var count = Database.Reputations.Count(x => x.Repee == Context.User.Id);

                if (count > 0)
                {
                    var ordered = Database.Reputations.OrderByDescending(x => x.Timestamp);
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

            var gld = await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false);
            var usr = await Database.GetUserAsync(Context.User).ConfigureAwait(false);
            var repee = await Database.GetUserAsync(user).ConfigureAwait(false);

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
        public async Task RemoveRep([Remainder]IGuildUser user)
        {
            if (user != null && (user.IsBot || user.IsWebhook))
            {
                await DiscordTools.NoBotsString.QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
                return;
            }

            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if ((user != null && user.Id == Context.User.Id) || user == null)
            {
                await "You can't modify your own reputation".QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            var gld = await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false);
            var usr = await Database.GetUserAsync(Context.User).ConfigureAwait(false);

            if (Database.Reputations.Any(x => x.Repee == user.Id && x.Reper == Context.User.Id))
            {
                Database.Reputations.Remove(Database.Reputations.FirstOrDefault(x => x.Reper == usr.Id && x.Repee == user.Id));

                await Database.SaveChangesAsync().ConfigureAwait(false);

                await $"You gave rep to {user.Mention}".QueueMessageAsync(Context).ConfigureAwait(false);
                return;
            }

            await "You haven't given this person a reputation point.".QueueMessageAsync(Context).ConfigureAwait(false);
        }

        private ulong GetCostOfHP(uint hp)
            => (ulong)Math.Round(Math.Ceiling(hp / 0.8));
    }

    [Group, Name("Accounts"), RequireDatabase]
    public class Account : InteractiveBase<ShardedCommandContext>
    {
        public SkuldConfig Configuration { get => HostSerivce.Configuration; }

        [Command("title"), Summary("Sets Title")]
        public async Task SetTitle([Remainder]string title = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var usr = await Database.GetUserAsync(Context.User).ConfigureAwait(false);

            if (title == null)
            {
                usr.Title = "";

                await Database.SaveChangesAsync().ConfigureAwait(false);

                await "Successfully cleared your title.".QueueMessageAsync(Context).ConfigureAwait(false);
            }
            else
            {
                usr.Title = title;

                await Database.SaveChangesAsync().ConfigureAwait(false);

                await $"Successfully set your title to **{title}**".QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("recurring-block"), Summary("Blocks people from patting you on recurring digits")]
        public async Task BlockRecurring(bool action)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var usr = await Database.GetUserAsync(Context.User).ConfigureAwait(false);

            usr.RecurringBlock = action;

            await Database.SaveChangesAsync().ConfigureAwait(false);

            await $"Set RecurringBlock to: {action}".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
        }

        [Command("set-hexbg"), Summary("Sets your background to a Hex Color"), RequireDatabase]
        public async Task SetHexBG(string Hex = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var usr = await Database.GetUserAsync(Context.User).ConfigureAwait(false);
            var gld = await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false);

            if (Hex != null)
            {
                if (usr.Money >= 300)
                {
                    usr.Money -= 300;
                    if (int.TryParse((Hex[0] != '#' ? Hex : Hex.Remove(0, 1)), System.Globalization.NumberStyles.HexNumber, null, out _))
                    {
                        usr.Background = (Hex[0] != '#' ? "#" + Hex : Hex);

                        await Database.SaveChangesAsync().ConfigureAwait(false);

                        await "Set your Background".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                    }
                    else
                    {
                        await $"Malformed Entry".QueueMessageAsync(Context, Discord.Models.MessageType.Failed).ConfigureAwait(false);
                        return;
                    }
                }
                else
                {
                    await $"You need at least {gld.MoneyIcon}300 to change your background".QueueMessageAsync(Context).ConfigureAwait(false);
                }
            }
            else
            {
                usr.Background = "#3F51B5";

                await Database.SaveChangesAsync().ConfigureAwait(false);

                await $"Reset your background to: {usr.Background}".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
            }
        }

        [Command("buy-custombg"), Summary("Buy permanent custom backgrounds"), RequireDatabase]
        public async Task BuyCBG()
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var usr = await Database.GetUserAsync(Context.User).ConfigureAwait(false);
            var gld = await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false);

            if (!usr.UnlockedCustBG)
            {
                if (usr.Money >= 40000)
                {
                    usr.Money -= 40000;
                    usr.UnlockedCustBG = true;

                    await Database.SaveChangesAsync().ConfigureAwait(false);

                    await $"You've successfully unlocked custom backgrounds, use: {gld.Prefix ?? Configuration.Discord.Prefix}set-custombg [URL] to set your background".QueueMessageAsync(Context).ConfigureAwait(false);
                }
                else
                {
                    await $"You need at least {gld.MoneyIcon}40,000 to unlock custom backgrounds".QueueMessageAsync(Context).ConfigureAwait(false);
                }
            }
            else
            {
                await $"You already unlocked custom backgrounds, use: {gld.Prefix ?? Configuration.Discord.Prefix}set-custombg [URL] to set your background".QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("set-custombg"), Summary("Sets your custom background Image"), RequireDatabase]
        public async Task SetCBG(Uri link = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var usr = await Database.GetUserAsync(Context.User).ConfigureAwait(false);
            var gld = await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false);

            if (link != null)
            {
                if (usr.Money >= 900)
                {
                    usr.Money -= 900;
                    usr.Background = link.OriginalString;

                    await Database.SaveChangesAsync().ConfigureAwait(false);

                    await "Set your Background".QueueMessageAsync(Context, Discord.Models.MessageType.Success).ConfigureAwait(false);
                }
                else
                {
                    await $"You need at least {gld.MoneyIcon}900 to change your background".QueueMessageAsync(Context).ConfigureAwait(false);
                }
            }
            else
            {
                usr.Background = "#3F51B5";

                await Database.SaveChangesAsync().ConfigureAwait(false);

                await $"Reset your background to: {usr.Background}".QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }
    }
}