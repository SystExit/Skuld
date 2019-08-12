using Booru.Net;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using ImageMagick;
using IqdbApi;
using Newtonsoft.Json;
using Skuld.APIS;
using Skuld.APIS.Animals.Models;
using Skuld.APIS.Extensions;
using Skuld.APIS.NekoLife.Models;
using Skuld.APIS.WebComics.Explosm.Models;
using Skuld.APIS.WebComics.XKCD.Models;
using Skuld.Bot.Extensions;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Globalization;
using Skuld.Core.Models;
using Skuld.Core.Models.Skuld;
using Skuld.Core.Utilities;
using Skuld.Database;
using Skuld.Database.Extensions;
using Skuld.Discord.Attributes;
using Skuld.Discord.Commands;
using Skuld.Discord.Extensions;
using Skuld.Discord.Services;
using Skuld.Discord.Preconditions;
using Skuld.Discord.Utilities;
using StatsdClient;
using SysEx.Net;
using SysEx.Net.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WenceyWang.FIGlet;

namespace Skuld.Bot.Commands
{
    [Group, RequireEnabledModule]
    public class Fun : InteractiveBase<SkuldCommandContext>
    {
        public Random Random { get; set; }
        public AnimalClient Animals { get; set; }
        public Locale Locale { get; set; }
        public WebComicClients ComicClients { get; set; }
        public SysExClient SysExClient { get; set; }
        public StrawPollClient Strawpoll { get; set; }
        public YNWTFClient YNWTFcli { get; set; }
        public BooruClient BooruClient { get; set; }
        public NekosLifeClient NekoLife { get; set; }
        public IqdbClient IqdbClient { get; set; }
        public BaseClient WebHandler { get; set; }
        private CommandService CommandService { get => BotService.CommandService; }

        private static readonly string[] eightball = {
            "SKULD_FUN_8BALL_YES1",
            "SKULD_FUN_8BALL_YES2",
            "SKULD_FUN_8BALL_YES3",
            "SKULD_FUN_8BALL_YES4",
            "SKULD_FUN_8BALL_YES5",
            "SKULD_FUN_8BALL_YES6",
            "SKULD_FUN_8BALL_YES7",
            "SKULD_FUN_8BALL_YES8",
            "SKULD_FUN_8BALL_YES9",
            "SKULD_FUN_8BALL_YES10",
            "SKULD_FUN_8BALL_MAYBE1",
            "SKULD_FUN_8BALL_MAYBE2",
            "SKULD_FUN_8BALL_MAYBE3",
            "SKULD_FUN_8BALL_MAYBE4",
            "SKULD_FUN_8BALL_MAYBE5",
            "SKULD_FUN_8BALL_NO1",
            "SKULD_FUN_8BALL_NO2",
            "SKULD_FUN_8BALL_NO3",
            "SKULD_FUN_8BALL_NO4",
            "SKULD_FUN_8BALL_NO5"
        };

        [Command("neko"), Summary("neko grill"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Neko()
        {
            var neko = await NekoLife.GetAsync(NekoImageType.Neko);
            DogStatsd.Increment("web.get");
            if (neko != null) await new EmbedBuilder { ImageUrl = neko }.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            else await "Hmmm <:Thunk:350673785923567616>, I got an empty response.".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
        }

        [Command("kitsune"), Summary("Kitsunemimi Grill"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Kitsune()
        {
            var kitsu = await SysExClient.GetKitsuneAsync();
            DogStatsd.Increment("web.get");
            await new EmbedBuilder { ImageUrl = kitsu }.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("kitty"), Summary("kitty"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("cat", "cats", "kittycat", "kitty cat", "meow", "kitties", "kittys")]
        public async Task Kitty()
        {
            var kitty = await Animals.GetAnimalAsync(AnimalType.Kitty);
            DogStatsd.Increment("web.get");

            if (kitty.IsVideoFile())
                await kitty.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            if (kitty == "https://i.ytimg.com/vi/29AcbY5ahGo/hqdefault.jpg")
                await new EmbedBuilder { Color = Color.Red, ImageUrl = kitty }.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel, "Both the api's are down, that makes the sad a big sad. <:blobcry:350681079415439361>");
            else
                await new EmbedBuilder { ImageUrl = kitty, Color = EmbedUtils.RandomColor() }.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("doggo"), Summary("doggo"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("dog", "dogs", "doggy")]
        public async Task Doggo()
        {
            var doggo = await Animals.GetAnimalAsync(AnimalType.Doggo);
            DogStatsd.Increment("web.get");
            if (doggo.IsVideoFile())
                await doggo.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            if (doggo == "https://i.imgur.com/ZSMi3Zt.jpg")
                await new EmbedBuilder { Color = Color.Red, ImageUrl = doggo }.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel, "The api is down, that makes the sad a big sad. <:blobcry:350681079415439361>");
            else
            {
                await new EmbedBuilder { ImageUrl = doggo, Color = EmbedUtils.RandomColor() }.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("bird"), Summary("birb"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("birb")]
        public async Task Birb()
        {
            var birb = await Animals.GetAnimalAsync(AnimalType.Bird);
            DogStatsd.Increment("web.get");
            if (birb.IsVideoFile())
                await birb.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            else
            {
                await new EmbedBuilder { ImageUrl = birb, Color = EmbedUtils.RandomColor() }.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("llama"), Summary("Llama"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Llama()
        {
            var llama = await SysExClient.GetLlamaAsync();
            DogStatsd.Increment("web.get");
            await new EmbedBuilder { Color = EmbedUtils.RandomColor(), ImageUrl = llama }.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("seal"), Summary("Seal"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Seal()
        {
            var seal = await SysExClient.GetSealAsync();
            DogStatsd.Increment("web.get");
            await new EmbedBuilder { Color = EmbedUtils.RandomColor(), ImageUrl = seal }.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("eightball"), Summary("Eightball")]
        [Alias("8ball")]
        public async Task Eightball([Remainder]string question = null)
            => await $":8ball: says: {Locale.GetLocale(Context.DBUser.Language ?? Locale.defaultLocale).GetString(eightball[Random.Next(0, eightball.Length)])}".QueueMessage(Discord.Models.MessageType.Mention, Context.User, Context.Channel);

        [Command("roll"), Summary("Roll a die")]
        public async Task Roll(int upper)
            => await $"{Context.User.Mention} just rolled and got a {Random.Next(1, (upper + 1))}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);

        [Command("pasta"), Summary("Pastas are nice"), RequireDatabase]
        public async Task Pasta(string cmd, string title, [Remainder]string content)
        {
            if (cmd == "new" || cmd == "+" || cmd == "create")
            {
                if (title == "list" || title == "help")
                {
                    await $"Cannot create a Pasta with the name: {title}".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                    DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                }
                else
                {
                    var pasta = await DatabaseClient.GetPastaAsync(title);
                    if (pasta.Successful)
                    {
                        await $"Pasta already exists with name: **{title}**".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                        DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                    }
                    else
                    {
                        var resp = await DatabaseClient.InsertPastaAsync(Context.User, title, content);
                        if (resp.Successful)
                        {
                            await $"Added: **{title}**".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                        }
                    }
                }
            }
            if (cmd == "edit" || cmd == "change" || cmd == "modify")
            {
                var pastaResp = await DatabaseClient.GetPastaAsync(title);
                if(pastaResp.Successful)
                {
                    Pasta pasta = pastaResp.Data as Pasta;
                    if (pasta.OwnerID == Context.User.Id)
                    {
                        content = content.Replace("\'", "\\\'");
                        content = content.Replace("\"", "\\\"");

                        pasta.Content = content;

                        var resp = await DatabaseClient.UpdatePastaAsync(pasta);
                        if (resp.Successful)
                        {
                            await $"Changed the content of **{title}**".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                        }
                    }
                    else
                    {
                        DogStatsd.Increment("commands.errors", 1, 1, new string[] { "unm-precon" });
                        await "I'm sorry, but you don't own the Pasta".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                    }
                }
            }
        }

        [Command("pasta"), Summary("Pastas are nice"), RequireDatabase]
        public async Task Pasta(string cmd, string title)
        {
            var pastaLocalResp = await DatabaseClient.GetPastaAsync(title);
            var user = Context.DBUser;
            if (pastaLocalResp.Successful)
            {
                Pasta pastaLocal = pastaLocalResp.Data as Pasta;
                if (cmd == "who" || cmd == "?")
                {
                    var embed = new EmbedBuilder
                    {
                        Color = EmbedUtils.RandomColor(),
                        Title = pastaLocal.Name
                    };

                    var usr = Context.Client.GetUser(pastaLocal.OwnerID);
                    if (usr != null)
                        embed.AddField("Author", usr.Username + "#" + usr.Discriminator, inline: true);
                    else
                        embed.AddField("Author", $"Unknown User ({pastaLocal.OwnerID})");
                    embed.AddField("Created", pastaLocal.Created.FromEpoch().ToString(new CultureInfo(user.Language)), inline: true);
                    embed.AddField("UpVotes", ":arrow_double_up: " + pastaLocal.Upvotes, inline: true);
                    embed.AddField("DownVotes", ":arrow_double_down: " + pastaLocal.Downvotes, inline: true);

                    await embed.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                }
                if (cmd == "upvote")
                {
                    var result = await Context.DBUser.CastPastaVoteAsync(pastaLocal, true);

                    if (result.Successful)
                    {
                        await $"Added your vote to `{pastaLocal.Name}`".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                    }
                    else
                    {
                        await result.Error.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                    }
                }
                if (cmd == "downvote")
                {
                    var result = await Context.DBUser.CastPastaVoteAsync(pastaLocal, false);

                    if (result.Successful)
                    {
                        await $"Added your vote to `{pastaLocal.Name}`".QueueMessage(Discord.Models.MessageType.Success, Context.User, Context.Channel);
                    }
                    else
                    {
                        await result.Error.QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                    }
                }
                if (cmd == "delete")
                {
                    if (Convert.ToUInt64(pastaLocal.OwnerID) == Context.User.Id)
                    {
                        var resp = await DatabaseClient.DropPastaAsync(title);

                        if (resp.Successful)
                        {
                            await $"Successfully deleted: **{title}**".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                        }
                    }
                }
            }
            else
            {
                DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                await $"Pasta `{title}` doesn't exist. :/ Sorry.".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("pasta"), Summary("Pastas are nice"), RequireDatabase]
        public async Task Pasta(string title)
        {
            if (title == "list")
            {
                var pastasResp = await DatabaseClient.GetAllPastasAsync();
                if (pastasResp.Successful)
                {
                    var pastas = pastasResp.Data as IReadOnlyList<Pasta>;
                    string pastanames = "```\n";

                    foreach (var pasta in pastas)
                    {
                        if (pasta == pastas.LastOrDefault())
                        { pastanames += pasta.Name; }
                        else
                        { pastanames += pasta.Name + ", "; }
                    }

                    pastanames += "\n```";

                    await $"I found:\n{pastanames}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    return;
                }
                await "No pastas exist".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            else if (title == "help")
            {
                await DiscordUtilities.GetCommandHelp(CommandService, Context, "pasta").QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            else
            {
                var pastaResp = await DatabaseClient.GetPastaAsync(title);
                if (pastaResp.Successful)
                {
                    await ((Pasta)pastaResp.Data).Content.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                }
                else
                {
                    DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                    await $"Whoops, `{title}` doesn't exist".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                }
            }
        }

        [Command("fuse"), Summary("Fuses 2 of the 1st generation pokemon")]
        public async Task Fuse(int int1, int int2)
        {
            if (int1 > 151 || int1 < 0) await $"{int1} over/under limit. (151)".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            else if (int2 > 151 || int2 < 0) await $"{int2} over/under limit. (151)".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            else
            {
                await new EmbedBuilder
                {
                    Color = EmbedUtils.RandomColor(),
                    ImageUrl = $"http://images.alexonsager.net/pokemon/fused/{int1}/{int1}.{int2}.png"
                }.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("emoji"), Summary("Turns text into bigmoji")]
        public async Task Emojify([Remainder]string message)
            => await message.ToRegionalIndicator().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);

        [Command("emojidance"), Summary("Dancing Emoji")]
        public async Task DanceEmoji([Remainder]string message)
            => await message.ToDancingEmoji().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);

        [Command("xkcd"), Summary("Get's Random XKCD comic"), Ratelimit(5, 1, Measure.Minutes)]
        public async Task XKCD(int comicid = -1)
        {
            if (comicid == -1)
            {
                await SendXKCD((await ComicClients.GetRandomXKCDComic()) as XKCDComic).ConfigureAwait(false);
                DogStatsd.Increment("web.get");
            }
            else
            {
                await SendXKCD((await ComicClients.GetXKCDComic(comicid)) as XKCDComic).ConfigureAwait(false);
                DogStatsd.Increment("web.get");
            }
        }

        public async Task SendXKCD(XKCDComic comic)
        {
            if (comic != null)
            {
                string datefixed;
                string monthfixed;
                DateTime dateTime;
                if (comic.day.Length == 1 && comic.month.Length == 1)
                {
                    datefixed = "0" + comic.day;
                    monthfixed = "0" + comic.month;
                    dateTime = DateTime.ParseExact($"{datefixed} {monthfixed} {comic.year}", "dd MM yyyy", CultureInfo.InvariantCulture);
                }
                else if (comic.day.Length == 1)
                {
                    datefixed = "0" + comic.day;
                    dateTime = DateTime.ParseExact($"{datefixed} {comic.month} {comic.year}", "dd MM yyyy", CultureInfo.InvariantCulture);
                }
                else if (comic.month.Length == 1)
                {
                    monthfixed = "0" + comic.month;
                    dateTime = DateTime.ParseExact($"{comic.day} {monthfixed} {comic.year}", "dd MM yyyy", CultureInfo.InvariantCulture);
                }
                else
                {
                    dateTime = DateTime.ParseExact($"{comic.day} {comic.month} {comic.year}", "dd MM yyyy", CultureInfo.InvariantCulture);
                }
                await new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = "Randall Patrick Munroe - XKCD",
                        Url = "https://xkcd.com/" + comic.num + "/",
                        IconUrl = "https://pbs.twimg.com/profile_images/602808103281692673/8lIim6cB_400x400.png"
                    },
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Strip released on"
                    },
                    Color = EmbedUtils.RandomColor(),
                    Timestamp = dateTime,
                    ImageUrl = comic.img,
                    Title = comic.safe_title,
                    Description = comic.alt
                }.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
        }

        [Command("cah"), Summary("Gets a Random Cynaide & Happiness Comic"), Alias("cyanide&happiness", "c&h"), Ratelimit(5, 1, Measure.Minutes)]
        public async Task CAH()
        {
            try
            {
                var comic = await ComicClients.GetCAHComicAsync() as CAHComic;
                DogStatsd.Increment("web.get");
                await new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = "Strip done " + comic.Author,
                        Url = comic.AuthorURL,
                        IconUrl = comic.AuthorAvatar
                    },
                    ImageUrl = comic.ImageURL,
                    Color = EmbedUtils.RandomColor()
                }.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel, comic.URL);
            }
            catch (Exception ex)
            {
                await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CAH-Cmd", "Error parsing website", LogSeverity.Error, ex));
            }
        }

        [Command("magik"), Summary("Magiks an image"), Alias("magick", "magic", "liquify"), Ratelimit(5, 1, Measure.Minutes)]
        public async Task Magik()
        {
            string url = "";

            var msgsRaw = Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, 25);
            var msgs = await msgsRaw.FlattenAsync();

            foreach (var msg in msgs)
            {
                if(msg.Attachments.Count > 0)
                {
                    url = msg.Attachments.FirstOrDefault().Url;
                    break;
                }
                if(msg.Embeds.Count > 0)
                {
                    var embed = msg.Embeds.FirstOrDefault();
                    if(embed.Image.HasValue)
                    {
                        url = embed.Image.Value.Url;
                        return;
                    }
                    if(embed.Thumbnail.HasValue)
                    {
                        url = embed.Image.Value.Url;
                        return;
                    }
                    if(embed.Author.HasValue)
                    {
                        if(!string.IsNullOrEmpty(embed.Author.Value.IconUrl))
                        {
                            url = embed.Author.Value.IconUrl;
                            return;
                        }
                    }
                }
            }

            if(url != "")
            {
                await Magik(new Uri(url));
                return;
            }

            await "Couldn't find an image".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
        }

        [Command("magik"), Summary("Magiks an image"), Alias("magick", "magic", "liquify"), Ratelimit(5, 1, Measure.Minutes)]
        public async Task Magik(Uri image)
        {
            var guid = new Guid();
            var imageLocation = Path.Combine(AppContext.BaseDirectory, "storage/magikCache");
            var imageFile = Path.Combine(imageLocation, guid + ".png");

            if (!Directory.Exists(imageLocation))
            {
                Directory.CreateDirectory(imageLocation);
            }

            await WebHandler.DownloadFileAsync(image, imageFile);

            using var magikImag = new MagickImage(imageFile);
            using var magik2 = magikImag.Clone();

            magik2.Resize(800, 600);

            magik2.LiquidRescale(new MagickGeometry
            {
                X = 1,
                Width = Convert.ToInt32(magik2.Width * 0.5),
                Height = Convert.ToInt32(magik2.Height * 0.5)
            });
            magik2.LiquidRescale(new MagickGeometry
            {
                X = 2,
                Width = Convert.ToInt32(magik2.Width * 1.5),
                Height = Convert.ToInt32(magik2.Height * 1.5)
            });

            magik2.Resize(magikImag.Width, magikImag.Height);

            magik2.Write(imageFile);

            await "".QueueMessage(Discord.Models.MessageType.File, Context.User, Context.Channel, imageFile);
        }
        [Command("magik"), Summary("Magiks an image"), Alias("magick", "magic", "liquify"), Ratelimit(5, 1, Measure.Minutes)]
        public async Task Magik([Remainder]IGuildUser user)
        {
            var avatarLocation = Path.Combine(AppContext.BaseDirectory, "storage/avatarCache");
            var avatarFile = Path.Combine(avatarLocation, user.Id + ".png");

            if (!Directory.Exists(avatarLocation))
            {
                Directory.CreateDirectory(avatarLocation);
            }

            var avatar = user.GetAvatarUrl(ImageFormat.Png, 512) ?? user.GetDefaultAvatarUrl();

            await WebHandler.DownloadFileAsync(new Uri(avatar), avatarFile);

            using var magikImag = new MagickImage(avatarFile);

            if (magikImag.Width != 512 && magikImag.Height != 512)
            {
                magikImag.Resize(512, 512);
            }

            using var magik2 = magikImag.Clone();

            magik2.Resize(800, 600);

            magik2.LiquidRescale(new MagickGeometry
            {
                X = 1,
                Width = Convert.ToInt32(magik2.Width * 0.5),
                Height = Convert.ToInt32(magik2.Height * 0.5)
            });
            magik2.LiquidRescale(new MagickGeometry
            {
                X = 2,
                Width = Convert.ToInt32(magik2.Width * 1.5),
                Height = Convert.ToInt32(magik2.Height * 1.5)
            });

            magik2.Resize(magikImag.Width, magikImag.Height);

            magik2.Write(avatarFile);

            await "".QueueMessage(Discord.Models.MessageType.File, Context.User, Context.Channel, avatarFile);
        }

        [Command("time"), Summary("Gets current time")]
        public async Task Time()
            => await new DateTimeOffset(DateTime.UtcNow).ToString().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);

        [Command("time"), Summary("Gets time from utc with offset")]
        public async Task Time(int offset)
        {
            var ofs = Convert.ToDouble(offset);
            var ts = new TimeSpan();
            var nts = ts.Add(TimeSpan.FromHours(ofs));
            var dtOffset = new DateTimeOffset(DateTime.UtcNow);
            var ndtof = dtOffset.ToOffset(nts);

            await ndtof.ToString().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("roast"), Summary("\"Roasts\" a user, these are all taken as jokes, and aren't actually meant to cause harm.")]
        public async Task RoastCmd(IUser user = null)
        {
            if (user == null)
                user = Context.User;
            var roast = await SysExClient.GetRoastAsync();
            DogStatsd.Increment("web.get");

            await roast.QueueMessage(Discord.Models.MessageType.Mention, user, Context.Channel);
        }

        [Command("dadjoke"), Summary("Gives you a bad dad joke to facepalm at.")]
        public async Task DadJoke()
        {
            var joke = await SysExClient.GetDadJokeAsync();
            DogStatsd.Increment("web.get");

            await new EmbedBuilder
            {
                Title = joke.Setup,
                Description = joke.Punchline,
                Color = EmbedUtils.RandomColor()
            }.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("pickup", RunMode = RunMode.Async), Summary("Cringe at these bad user-submitted pick up lines. (Don't actually use these or else you'll get laughed at. :3)"), Alias("pickupline")]
        public async Task PickUp()
        {
            var pickup = await SysExClient.GetPickupLineAsync();
            DogStatsd.Increment("web.get");

            await new EmbedBuilder
            {
                Title = pickup.Setup,
                Description = pickup.Punchline,
                Color = EmbedUtils.RandomColor()
            }.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("choose"), Summary("Choose from things, eg: \"reading books\" \"playing games\"")]
        public async Task Choose(params string[] choices)
        {
            var choice = choices[Random.Next(0, choices.Length)];

            if (char.IsWhiteSpace(choice[0])) choice = choice.Remove(choice[0], 1);
            else if (char.IsWhiteSpace(choice[^1])) choice = choice.Remove(choice.Length - 1);

            await $"<:blobthinkcool:350673773113901056> | __{(Context.User as IGuildUser).Nickname ?? Context.User.Username}__ I choose: **{choice}**".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("yn"), Summary("Yes, no, maybe. I don't know, can you repeat the question?")]
        public async Task YN([Remainder]string question = null)
        {
            var YNResp = await YNWTFcli.AskYNWTF();
            await new EmbedBuilder
            {
                Title = YNResp.Answer,
                ImageUrl = YNResp.Image,
                Color = EmbedUtils.RandomColor()
            }.Build().QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        private const int FIGLETWIDTH = 16;
        [Command("figlet"), Summary("Make a big ascii text lol")]
        public async Task Figlet([Remainder]string text)
        {
            var splittext = text.ToCharArray();
            var textrows = new List<string>();
            if (splittext.Count() > FIGLETWIDTH)
            {
                int count = (int)Math.Round(splittext.Count() / (decimal)FIGLETWIDTH, MidpointRounding.AwayFromZero);

                int prevamount = 0;
                for(int x = 0; x < count; x++)
                {
                    int amount = (x + 1) * FIGLETWIDTH;
                    string txt = string.Concat(splittext.Skip(prevamount).Take(amount));
                    textrows.Add(txt);
                    prevamount = amount;
                }

                if(count * FIGLETWIDTH < splittext.Count())
                {
                    var temp = splittext.ToList();
                    temp.RemoveRange(0, count * FIGLETWIDTH);

                    textrows.Add(string.Join(' ', temp));
                }
            }
            else
            {
                textrows.Add(text);
            }

            string result = "```\n";

            foreach (var row in textrows)
            {
                var figlet = new AsciiArt(row);
                foreach (var line in figlet.Result)
                {
                    result += line + "\n";
                }
                result += "\n";
            }

            result = result[0..^2];
            result += "```";

            await result.QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
        }

        [Command("iqdb"), Summary("Reverse image-search")]
        public async Task IQDB(string image)
        {
            var results = await IqdbClient.SearchUrl(image);

            if(results.IsFound)
            {
                var sorted = results.Matches.OrderByDescending(x => x.Similarity);
                var mostlikely = sorted.FirstOrDefault();
                string url = "";

                if ((!mostlikely.Url.Contains("https:") && !mostlikely.Url.Contains("http:")))
                    url = "https:" + mostlikely.Url;
                else
                    url = mostlikely.Url;

                await $"Similarity: {mostlikely.Similarity}%\n{url}".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            }
            else
            {
                await "No results found".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            }
        }

        [Command("safebooru"), Summary("Gets stuff from safebooru"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("Safe")]
        public async Task Safebooru(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags()) await "Your tags contains a banned tag, please remove it.".QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
            else
            {
                var cleantags = tags.AddBlacklistedTags();
                var posts = await BooruClient.GetSafebooruImagesAsync(cleantags);
                DogStatsd.Increment("web.get");
                var post = GetSafeImage(posts);
                if (post != null)
                {
                    await post.GetMessage(post.PostUrl).QueueMessage(Discord.Models.MessageType.Standard, Context.User, Context.Channel);
                    return;
                }
                await "Couldn't find an image".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            }
        }
        
        [Command("meme"), Summary("Does a funny haha meme"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Memay(string template = null, params string[] sources)
        {
            if(template == null && !sources.Any())
            {
                var endpoints = JsonConvert.DeserializeObject<MemeResponse>(await WebHandler.ReturnStringAsync(new Uri("https://api.skuldbot.uk/fun/meme/?endpoints"))).Endpoints;

                var paginatedMessage = new PaginatedMessage
                {
                    Title = "__Current Templates__",
                    Color = Color.Blue,
                    Options = new PaginatedAppearanceOptions
                    {
                        DisplayInformationIcon = false,
                        JumpDisplayOptions = JumpDisplayOptions.WithManageMessages
                    }
                };

                paginatedMessage.Pages = endpoints.PaginateList();

                await PagedReplyAsync(paginatedMessage);
                return;
            }

            List<string> imageLinks = new List<string>();

            foreach(var str in sources)
            {
                if(str.IsImageExtension())
                {
                    imageLinks.Add(str.TrimEmbedHiders());
                }

                if(DiscordUtilities.UserMentionRegex.IsMatch(str))
                {
                    var userid = str.Replace("<@", "").Replace(">", "");
                    ulong.TryParse(userid, out ulong useridl);

                    var user = Context.Guild.GetUser(useridl);

                    imageLinks.Add(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
                }
            }

            if (imageLinks.All(x=>x.IsImageExtension()))
            {
                var endpoints = JsonConvert.DeserializeObject<MemeResponse>(await WebHandler.ReturnStringAsync(new Uri("https://api.skuldbot.uk/fun/meme/?endpoints"))).Endpoints;

                if (endpoints.Any(x => x.Name.ToLower() == template.ToLower()))
                {
                    var endpoint = endpoints.First(x => x.Name.ToLower() == template.ToLower());
                    if (endpoint.RequiredSources == imageLinks.Count())
                    {
                        var resp = await SysExClient.GetMemeImageAsync(endpoint.Name, imageLinks.ToArray());

                        if (resp != null && resp is Stream)
                        {
                            var folderPath = Path.Combine(AppContext.BaseDirectory, "storage/meme/");

                            var filePath = Path.Combine(folderPath, $"{template}-{Context.User.Id}-{Context.Channel.Id}.png");

                            if (!Directory.Exists(folderPath))
                            {
                                Directory.CreateDirectory(folderPath);
                            }

                            using (var file = System.Drawing.Image.FromStream((Stream)resp))
                            {
                                file.Save(filePath);
                            }

                            await "".QueueMessage(Discord.Models.MessageType.File, Context.User, Context.Channel, filePath);
                        }
                    }
                    else
                    {
                        await $"You don't have enough sources. You need {endpoint.RequiredSources} source images".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                    }
                }
                else
                {
                    await $"Template `{template}` does not exist".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
                }
            }
            else
            {
                await "Sources need to be an image link".QueueMessage(Discord.Models.MessageType.Failed, Context.User, Context.Channel);
            }
        }

        private int EdgeCase;

        public SafebooruImage GetSafeImage(IReadOnlyList<SafebooruImage> posts)
        {
            var post = posts.GetRandomImage();
            EdgeCase++;
            if (EdgeCase <= 5)
            {
                if (post.Rating != Rating.Safe)
                {
                    return GetSafeImage(posts);
                }
                else
                {
                    return post;
                }
            }
            else
            {
                EdgeCase = 0;
                return null;
            }
        }
    }
}