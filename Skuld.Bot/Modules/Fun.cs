using Booru.Net;
using Discord;
using Discord.Commands;
using IqdbApi;
using Skuld.APIS;
using Skuld.APIS.Animals.Models;
using Skuld.APIS.Extensions;
using Skuld.APIS.NekoLife.Models;
using Skuld.APIS.WebComics.Explosm.Models;
using Skuld.APIS.WebComics.XKCD.Models;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Globalization;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Database;
using Skuld.Discord;
using Skuld.Discord.Attributes;
using Skuld.Discord.Preconditions;
using Skuld.Discord.Utilities;
using StatsdClient;
using SysEx.Net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WenceyWang.FIGlet;

namespace Skuld.Bot.Commands
{
    [Group]
    public class Fun : SkuldBase<SkuldCommandContext>
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
        private CommandService CommandService { get => BotService.CommandService; }

        private static string[] eightball = {
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
            if (neko != null) await ReplyAsync(Context.Channel, new EmbedBuilder { ImageUrl = neko }.Build());
            else await ReplyAsync(Context.Channel, "Hmmm <:Thunk:350673785923567616>, I got an empty response.");
        }

        [Command("kitsune"), Summary("Kitsunemimi Grill"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Kitsune()
        {
            var kitsu = await SysExClient.GetKitsuneAsync();
            DogStatsd.Increment("web.get");
            await ReplyAsync(Context.Channel, new EmbedBuilder { ImageUrl = kitsu }.Build());
        }

        [Command("kitty"), Summary("kitty"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("cat", "cats", "kittycat", "kitty cat", "meow", "kitties", "kittys")]
        public async Task Kitty()
        {
            var kitty = await Animals.GetAnimalAsync(AnimalType.Kitty);
            DogStatsd.Increment("web.get");

            if (kitty.IsVideoFile())
                await ReplyAsync(Context.Channel, kitty);
            if (kitty == "https://i.ytimg.com/vi/29AcbY5ahGo/hqdefault.jpg")
                await ReplyAsync(Context.Channel, "Both the api's are down, that makes the sad a big sad. <:blobcry:350681079415439361>", new EmbedBuilder { Color = Color.Red, ImageUrl = kitty }.Build());
            else
                await ReplyAsync(Context.Channel, new EmbedBuilder { ImageUrl = kitty, Color = EmbedUtils.RandomColor() }.Build());
        }

        [Command("doggo"), Summary("doggo"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("dog", "dogs", "doggy")]
        public async Task Doggo()
        {
            var doggo = await Animals.GetAnimalAsync(AnimalType.Doggo);
            DogStatsd.Increment("web.get");
            if (doggo.IsVideoFile())
                await ReplyAsync(Context.Channel, doggo);
            if (doggo == "https://i.imgur.com/ZSMi3Zt.jpg")
                await ReplyAsync(Context.Channel, "The api is down, that makes the sad a big sad. <:blobcry:350681079415439361>", new EmbedBuilder { Color = Color.Red, ImageUrl = doggo }.Build());
            else
            {
                var embed = new EmbedBuilder { ImageUrl = doggo, Color = EmbedUtils.RandomColor() };
                await ReplyAsync(Context.Channel, embed.Build());
            }
        }

        [Command("bird"), Summary("birb"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("birb")]
        public async Task Birb()
        {
            var birb = await Animals.GetAnimalAsync(AnimalType.Bird);
            DogStatsd.Increment("web.get");
            if (birb.IsVideoFile())
                await ReplyAsync(Context.Channel, birb);
            else
            {
                var embed = new EmbedBuilder { ImageUrl = birb, Color = EmbedUtils.RandomColor() };
                await ReplyAsync(Context.Channel, embed.Build());
            }
        }

        [Command("llama"), Summary("Llama"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Llama()
        {
            var llama = await SysExClient.GetLlamaAsync();
            DogStatsd.Increment("web.get");
            await ReplyAsync(Context.Channel, new EmbedBuilder { Color = EmbedUtils.RandomColor(), ImageUrl = llama }.Build());
        }

        [Command("seal"), Summary("Seal"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Seal()
        {
            var seal = await SysExClient.GetSealAsync();
            DogStatsd.Increment("web.get");
            await ReplyAsync(Context.Channel, new EmbedBuilder { Color = EmbedUtils.RandomColor(), ImageUrl = seal }.Build());
        }

        [Command("eightball"), Summary("Eightball")]
        [Alias("8ball")]
        public async Task Eightball([Remainder]string question = null)
        {
            var usr = Context.DBUser;
            var local = Locale.GetLocale(Locale.defaultLocale);
            if (usr != null)
                local = Locale.GetLocale(usr.Language);
            await ReplyAsync(Context.Channel, $"{Context.User.Username} :8ball: says: {local.GetString(eightball[Random.Next(0, eightball.Length)])}");
        }

        [Command("roll"), Summary("Roll a die")]
        public async Task Roll(int upper)
        {
            int rand = Random.Next(1, (upper + 1));

            await ReplyAsync(Context.Channel, $"{Context.User.Mention} just rolled and got a {rand}");
        }

        [Command("pasta"), Summary("Pastas are nice"), RequireDatabase]
        public async Task Pasta(string cmd, string title, [Remainder]string content)
        {
            if (cmd == "new" || cmd == "+" || cmd == "create")
            {
                if (title == "list" || title == "help")
                {
                    await ReplyAsync(Context.Channel, "Nope");
                    DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                }
                else
                {
                    var pasta = await DatabaseClient.GetPastaAsync(title);
                    if (pasta.Successful)
                    {
                        await ReplyAsync(Context.Channel, $"Pasta already exists with name: **{title}**");
                        DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                    }
                    else
                    {
                        var resp = await DatabaseClient.InsertPastaAsync(Context.User, title, content);
                        if (resp.Successful)
                        {
                            await ReplyAsync(Context.Channel, $"Successfully added: **{title}**");
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
                            await ReplyAsync(Context.Channel, $"Successfully changed the content of **{title}**");
                        }
                    }
                    else
                    {
                        DogStatsd.Increment("commands.errors", 1, 1, new string[] { "unm-precon" });
                        await ReplyAsync(Context.Channel, "I'm sorry, but you don't own the Pasta");
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
                    await ReplyAsync(Context.Channel, embed.Build());
                }
                if (cmd == "upvote")
                {
                    var result = await DatabaseClient.CastPastaVoteAsync(Context.User, pastaLocal, true);

                    if (result.Successful)
                    {
                        await ReplyAsync(Context.Channel, "Added your vote to `" + pastaLocal.Name + "`");
                    }
                    else
                    {
                        await ReplyAsync(Context.Channel, "Error: " + result.Error);
                    }
                }
                if (cmd == "downvote")
                {
                    var result = await DatabaseClient.CastPastaVoteAsync(Context.User, pastaLocal, false);

                    if (result.Successful)
                    {
                        await ReplyAsync(Context.Channel, "Added your vote to `" + pastaLocal.Name + "`");
                    }
                    else
                    {
                        await ReplyAsync(Context.Channel, "Error: " + result.Error);
                    }
                }
                if (cmd == "delete")
                {
                    if (Convert.ToUInt64(pastaLocal.OwnerID) == Context.User.Id)
                    {
                        var resp = await DatabaseClient.DropPastaAsync(title);

                        if (resp.Successful)
                        {
                            await ReplyAsync(Context.Channel, $"Successfully deleted: **{title}**");
                        }
                    }
                }
            }
            else
            {
                DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                await ReplyAsync(Context.Channel, $"Pasta `{title}` doesn't exist. :/ Sorry.");
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

                    await ReplyAsync(Context.Channel, $"I found:\n{pastanames}");
                    return;
                }
                await ReplyAsync(Context.Channel, "No pastas exist");
            }
            else if (title == "help")
            {
                var embed = DiscordUtilities.GetCommandHelp(CommandService, Context, "pasta");
                await ReplyAsync(Context.Channel, embed);
            }
            else
            {
                var pastaResp = await DatabaseClient.GetPastaAsync(title);
                if (pastaResp.Successful) await ReplyAsync(Context.Channel, ((Pasta)pastaResp.Data).Content);
                else
                {
                    DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                    await ReplyAsync(Context.Channel, $"Whoops, `{title}` doesn't exist");
                }
            }
        }

        [Command("fuse"), Summary("Fuses 2 of the 1st generation pokemon")]
        public async Task Fuse(int int1, int int2)
        {
            if (int1 > 151 || int1 < 0) await ReplyAsync(Context.Channel, $"{int1} over/under limit. (151)");
            else if (int2 > 151 || int2 < 0) await ReplyAsync(Context.Channel, $"{int2} over/under limit. (151)");
            else
            {
                await ReplyAsync(Context.Channel, new EmbedBuilder
                {
                    Color = EmbedUtils.RandomColor(),
                    ImageUrl = $"http://images.alexonsager.net/pokemon/fused/{int1}/{int1}.{int2}.png"
                }.Build());
            }
        }

        [Command("strawpoll"), Summary("Creates Strawpoll, use quotation marks around multi word options")]
        public async Task StrawpollSend(string title, params string[] options)
        {
            var poll = await Strawpoll.SendPoll(title, options);
            DogStatsd.Increment("web.post");
            await ReplyAsync($"Strawpoll **{title}** has been created, here's the link: {poll.Url}");
        }

        [Command("strawpoll"), Summary("Gets a strawpoll")]
        public async Task StrawpollGet(string url)
        {
            if (!url.IsWebsite()) url = "https://strawpoll.me/" + url;

            var poll = await Strawpoll.GetPoll(url);
            if (poll != null)
            {
                DogStatsd.Increment("web.get");
                var embed = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = poll.Title,
                        Url = poll.Url
                    },
                    Color = EmbedUtils.RandomColor(),
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Strawpoll ID: " + poll.ID
                    },
                    Timestamp = DateTime.UtcNow
                };
                for (int z = 0; z < poll.Options.Length; z++)
                { embed.AddField(poll.Options[z], poll.Votes[z]); }

                await ReplyAsync(Context.Channel, embed.Build());
            }

            await ReplyAsync(Context.Channel, "Poll: `" + url + "` doesn't exist");
        }

        [Command("emoji"), Summary("Turns text into bigmoji")]
        public async Task Emojify([Remainder]string message)
        {
            string newmessage = "";
            var regexItem = new Regex("^[a-zA-Z0-9 ]*$");
            foreach (var character in message)
            {
                if (!regexItem.IsMatch(Convert.ToString(character)))
                    newmessage += character;
                else if (!Char.IsWhiteSpace(character))
                    newmessage += ":regional_indicator_" + character + ": ";
                else
                    newmessage += " ";
            }
            await ReplyAsync(Context.Channel, newmessage);
        }

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
                await ReplyAsync(Context.Channel, new EmbedBuilder
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
                }.Build());
            }
        }

        [Command("cah"), Summary("Gets a Random Cynaide & Happiness Comic"), Alias("cyanide&happiness", "c&h"), Ratelimit(5, 1, Measure.Minutes)]
        public async Task CAH()
        {
            try
            {
                var comic = await ComicClients.GetCAHComicAsync() as CAHComic;
                DogStatsd.Increment("web.get");
                var embed = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = "Strip done " + comic.Author,
                        Url = comic.AuthorURL,
                        IconUrl = comic.AuthorAvatar
                    },
                    ImageUrl = comic.ImageURL,
                    Color = EmbedUtils.RandomColor()
                }.Build();
                await ReplyAsync(Context.Channel, comic.URL, embed);
            }
            catch (Exception ex)
            {
                await GenericLogger.AddToLogsAsync(new Skuld.Core.Models.LogMessage("CAH-Cmd", "Error parsing website", LogSeverity.Error, ex));
            }
        }

        [Command("time"), Summary("Gets current time")]
        public async Task Time()
        {
            var offset = new DateTimeOffset(DateTime.UtcNow);
            await ReplyAsync(Context.Channel, offset.ToString());
        }

        [Command("time"), Summary("Gets time from utc with offset")]
        public async Task Time(int offset)
        {
            var ofs = Convert.ToDouble(offset);
            var ts = new TimeSpan();
            var nts = ts.Add(TimeSpan.FromHours(ofs));
            var dtOffset = new DateTimeOffset(DateTime.UtcNow);
            var ndtof = dtOffset.ToOffset(nts);
            await ReplyAsync(Context.Channel, ndtof.ToString());
        }

        [Command("roast"), Summary("\"Roasts\" a user, these are all taken as jokes, and aren't actually meant to cause harm.")]
        public async Task RoastCmd(IUser user = null)
        {
            if (user == null)
                user = Context.User;
            var roast = await SysExClient.GetRoastAsync();
            DogStatsd.Increment("web.get");
            await ReplyAsync(Context.Channel, user.Mention + " " + roast);
        }

        [Command("dadjoke"), Summary("Gives you a bad dad joke to facepalm at.")]
        public async Task DadJoke()
        {
            var joke = await SysExClient.GetDadJokeAsync();
            DogStatsd.Increment("web.get");

            await ReplyAsync(Context.Channel, new EmbedBuilder
            {
                Title = joke.Setup,
                Description = joke.Punchline,
                Color = EmbedUtils.RandomColor()
            }.Build());
        }

        [Command("pickup", RunMode = RunMode.Async), Summary("Cringe at these bad user-submitted pick up lines. (Don't actually use these or else you'll get laughed at. :3)"), Alias("pickupline")]
        public async Task PickUp()
        {
            var pickup = await SysExClient.GetPickupLineAsync();
            DogStatsd.Increment("web.get");

            await ReplyAsync(Context.Channel, new EmbedBuilder
            {
                Title = pickup.Setup,
                Description = pickup.Punchline,
                Color = EmbedUtils.RandomColor()
            }.Build());
        }

        [Command("choose"), Summary("Choose from things, eg: \"reading books\" \"playing games\"")]
        public async Task Choose(params string[] choices)
        {
            var choice = choices[Random.Next(0, choices.Length)];

            if (Char.IsWhiteSpace(choice[0])) choice = choice.Remove(choice[0], 1);
            else if (Char.IsWhiteSpace(choice[choice.Length - 1])) choice = choice.Remove(choice.Length - 1);

            await ReplyAsync(Context.Channel, $"<:blobthinkcool:350673773113901056> | __{(Context.User as IGuildUser).Nickname ?? Context.User.Username}__ I choose: **{choice}**");
        }

        [Command("yn"), Summary("Yes, no, maybe. I don't know, can you repeat the question?")]
        public async Task YN([Remainder]string question = null)
        {
            var YNResp = await YNWTFcli.AskYNWTF();
            var embed = new EmbedBuilder
            {
                Title = YNResp.Answer,
                ImageUrl = YNResp.Image,
                Color = EmbedUtils.RandomColor()
            };
            await ReplyAsync(Context.Channel, embed.Build());
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

            result = result.Substring(0, result.Length - 2);
            result += "```";

            await ReplyAsync(Context.Channel, result);
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

                await ReplyAsync(Context.Channel, $"Similarity: {mostlikely.Similarity}%\n{url}");
            }
            else
            {
                await ReplyFailedAsync(Context.Channel, "No results found");
            }
        }

        [Command("safebooru"), Summary("Gets stuff from safebooru"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("Safe")]
        public async Task Safebooru(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags()) await ReplyAsync(Context.Channel, "Your tags contains a banned tag, please remove it.");
            else
            {
                var cleantags = tags.AddBlacklistedTags();
                var posts = await BooruClient.GetSafebooruImagesAsync(cleantags);
                DogStatsd.Increment("web.get");
                var post = GetSafeImage(posts);
                if (post != null)
                {
                    await ReplyAsync(Context.Channel, post.GetMessage(post.PostUrl));
                    return;
                }
                await ReplyAsync(Context.Channel, "Couldn't find an image");
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