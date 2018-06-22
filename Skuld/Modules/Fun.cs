using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Skuld.Tools;
using Discord;
using Discord.Addons.Interactive;
using StatsdClient;
using Skuld.Services;
using Skuld.APIS;
using Skuld.Globalization;
using SysEx.Net;
using Booru.Net;
using Skuld.Extensions;
using Skuld.Utilities;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Skuld.Modules
{
    [Group]
    public class Fun : InteractiveBase<ShardedCommandContext>
    {
        public DatabaseService Database { get; set; }
        public Random Random { get; set; }
        public LoggingService Logger { get; set; }
        public AnimalAPIS Animals { get; set; }
        public Locale Locale { get; set; }
        public WebComicClients ComicClients { get; set; }
        public SysExClient SysExClient { get; set; }
        public Strawpoll Strawpoll { get; set; }
        public YNWTF YNWTFcli { get; set; }
        public BooruClient BooruClient { get; set; }

        static string[] eightball = {
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
            var neko = await Animals.GetNekoAsync();
            if (neko != null)
                await Context.Channel.ReplyAsync(new EmbedBuilder { ImageUrl = neko }.Build());
            else
                await Context.Channel.ReplyAsync("Hmmm <:Thunk:350673785923567616>, I got an empty response.");
        }

        [Command("kitsune"), Summary("Kitsunemimi Grill"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Kitsune()
        {
            var kitsu = await SysExClient.GetKitsuneAsync();
            DogStatsd.Increment("web.get");
            await Context.Channel.ReplyAsync(new EmbedBuilder { ImageUrl = kitsu }.Build());
        }

        [Command("kitty"), Summary("kitty"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("cat", "cats", "kittycat", "kitty cat", "meow", "kitties", "kittys")]
        public async Task Kitty()
        {
            var kitty = await Animals.GetKittyAsync();

            if (kitty.IsVideoFile())
                await Context.Channel.ReplyAsync(kitty);
            if (kitty == "https://i.ytimg.com/vi/29AcbY5ahGo/hqdefault.jpg")
                await Context.Channel.ReplyAsync("Both the api's are down, that makes the sad a big sad. <:blobcry:350681079415439361>", new EmbedBuilder { Color = Color.Red, ImageUrl = kitty }.Build());
            else
            {
                await Context.Channel.ReplyAsync(new EmbedBuilder { ImageUrl = kitty, Color = EmbedUtils.RandomColor() }.Build());
            }
        }

        [Command("doggo"), Summary("doggo"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("dog", "dogs", "doggy")]
        public async Task Doggo()
        {
            var doggo = await Animals.GetDoggoAsync();
            if (doggo.IsVideoFile())
                await Context.Channel.ReplyAsync(doggo);
            if (doggo == "https://i.imgur.com/ZSMi3Zt.jpg")
                await Context.Channel.ReplyAsync("The api is down, that makes the sad a big sad. <:blobcry:350681079415439361>", new EmbedBuilder { Color = Color.Red, ImageUrl = doggo }.Build());
            else
            {
                var embed = new EmbedBuilder { ImageUrl = doggo, Color = EmbedUtils.RandomColor() };
                await Context.Channel.ReplyAsync(embed.Build());
            }
        }

        [Command("bird"), Summary("birb"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("birb")]
        public async Task Birb()
        {
            var birb = await Animals.GetBirbAsync();
            if (birb.IsVideoFile())
                await Context.Channel.ReplyAsync(birb);
            else
            {
                var embed = new EmbedBuilder { ImageUrl = birb, Color = EmbedUtils.RandomColor() };
                await Context.Channel.ReplyAsync(embed.Build());
            }            
        }

        [Command("llama"), Summary("Llama"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Llama()
        {
            var llama = await SysExClient.GetLlamaAsync();
            await Context.Channel.ReplyAsync(new EmbedBuilder { Color = EmbedUtils.RandomColor(), ImageUrl = llama }.Build());
        }

        [Command("seal"), Summary("Seal"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Seal()
        {
            var seal = await SysExClient.GetSealAsync();
            await Context.Channel.ReplyAsync(new EmbedBuilder { Color = EmbedUtils.RandomColor(), ImageUrl = seal }.Build());
        }

        [Command("eightball"), Summary("Eightball")]
        [Alias("8ball")]
        public async Task Eightball([Remainder]string question = null)
        {
            var usr = await Database.GetUserAsync(Context.User.Id);
            var local = Locale.GetLocale(Locale.defaultLocale);
            if (usr != null)
                local = Locale.GetLocale(usr.Language);
            await Context.Channel.ReplyAsync($"{Context.User.Username} :8ball: says: {local.GetString(eightball[Random.Next(0, eightball.Length)])}");
        }

        [Command("roll"), Summary("Roll a die")]
        public async Task Roll(int upper)
        {
            try
            {
                int rand = Random.Next(1, (upper + 1));
                if (rand == 1)
                {
                    if (Database.CanConnect)
                    {
                        var oldusr = await Database.GetUserAsync(Context.User.Id);
                        oldusr.LuckFactor = Math.Round((oldusr.LuckFactor / 1.1), 4);

                        if (oldusr.LuckFactor < 0.1)
                            oldusr.LuckFactor = 0.1;

                        await Database.UpdateUserAsync(oldusr);
                    }

                    await Context.Channel.ReplyAsync($"{Context.User.Mention} just rolled and got a {rand} :weary:");
                }
                else
                    await Context.Channel.ReplyAsync($"{Context.User.Mention} just rolled and got a {rand}");
            }
            catch (FormatException)
            {
                await Context.Channel.ReplyAsync($"{Context.User.Mention} this command only accepts numbers, try again");
                DogStatsd.Increment("commands.errors", 1, 1, new string[] { "exception" });
            }
        }

        [Command("pasta"), Summary("Pastas are nice"), RequireDatabase]
        public async Task Pasta(string cmd, string title, [Remainder]string content)
        {
            if (cmd == "new" || cmd == "+" || cmd == "create")
            {
                if (title == "list" || title == "help")
                {
                    await Context.Channel.ReplyAsync("Nope");
                    DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                }
                else
                {
                    var pasta = await Database.GetPastaAsync(title);
                    if (pasta != null)
                    {
                        await Context.Channel.ReplyAsync($"Pasta already exists with name: **{title}**");
                        DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                    }
                    else
                    {
                        var resp = await Database.InsertPastaAsync(Context.User, title, content);
                        if (resp.Successful)
                        {
                            await Context.Channel.ReplyAsync($"Successfully added: **{title}**");
                        }
                    }
                }
            }
            if (cmd == "edit" || cmd == "change" || cmd == "modify")
            {
                var pasta = await Database.GetPastaAsync(title);
                if (pasta.OwnerID == Context.User.Id)
                {
                    content = content.Replace("\'", "\\\'");
                    content = content.Replace("\"", "\\\"");

                    pasta.Content = content;

                    var resp = await Database.UpdatePastaAsync(pasta);
                    if (resp.Successful)
                    {
                        await Context.Channel.ReplyAsync($"Successfully changed the content of **{title}**");
                    }
                }
                else
                {
                    DogStatsd.Increment("commands.errors", 1, 1, new string[] { "unm-precon" });
                    await Context.Channel.ReplyAsync("I'm sorry, but you don't own the Pasta");
                }
            }
        }

        [Command("pasta"), Summary("Pastas are nice"), RequireDatabase]
        public async Task Pasta(string cmd, string title)
        {
            var pastaLocal = await Database.GetPastaAsync(title);
            if (pastaLocal != null)
            {
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
                    embed.AddField("Created", pastaLocal.Created, inline: true);
                    embed.AddField("UpVotes", ":arrow_double_up: " + pastaLocal.Upvotes, inline: true);
                    embed.AddField("DownVotes", ":arrow_double_down: " + pastaLocal.Downvotes, inline: true);
                    await Context.Channel.ReplyAsync(embed.Build());
                }
                if (cmd == "upvote")
                {
                    var result = await Database.CastPastaVoteAsync(Context.User, title, true);

                    if (result.Successful)
                    {
                        await Context.Channel.ReplyAsync("Added your vote to `" + pastaLocal.Name + "`");
                    }
                    else
                    {
                        await Context.Channel.ReplyAsync("Error: " + result.Error);
                    }
                }
                if (cmd == "downvote")
                {
                    var result = await Database.CastPastaVoteAsync(Context.User, title, false);

                    if (result.Successful)
                    {
                        await Context.Channel.ReplyAsync("Added your vote to `" + pastaLocal.Name + "`");
                    }
                    else
                    {
                        await Context.Channel.ReplyAsync("Error: " + result.Error);
                    }
                }
                if (cmd == "delete")
                {
                    if (Convert.ToUInt64(pastaLocal.OwnerID) == Context.User.Id)
                    {
                        var resp = await Database.DropPastaAsync(title);

                        if (resp.Successful)
                        {
                            await Context.Channel.ReplyAsync($"Successfully deleted: **{title}**");
                        }
                    }
                }
            }
            else
            {
                DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                await Context.Channel.ReplyAsync($"Pasta `{title}` doesn't exist. :/ Sorry.");
            }
        }

        [Command("pasta"), Summary("Pastas are nice"), RequireDatabase]
        public async Task Pasta(string title)
        {
            if (title == "list")
            {
                var pastas = await Database.GetAllPastasAsync();
                if (pastas != null)
                {
                    string pastanames = "```\n";

                    foreach (var pasta in pastas)
                    {
                        if (pasta == pastas.LastOrDefault())
                        { pastanames += pasta.Name; }
                        else
                        { pastanames += pasta.Name + ", "; }
                    }

                    pastanames += "\n```";

                    await Context.Channel.ReplyAsync("I found:\n{pastanames}");
                }
                await Context.Channel.ReplyAsync("No pastas exist");
            }
            else if (title == "help")
            {
                string help = "Here's how to do stuff with **pasta**:\n\n" +
                    "```cs\n" +
                    "   give   : Give a user your pasta\n" +
                    "   list   : List all pasta\n" +
                    "   edit   : Change the content of your pasta\n" +
                    "  change  : Same as above\n" +
                    "   new    : Creates a new pasta\n" +
                    "    +     : Same as above\n" +
                    "   who    : Gets information about a pasta\n" +
                    "    ?     : Same as above\n" +
                    "  upvote  : Upvotes a pasta\n" +
                    " downvote : Downvotes a pasta\n" +
                    "  delete  : deletes a pasta```";
                await Context.Channel.ReplyAsync(help);
            }
            else
            {
                var pasta = await Database.GetPastaAsync(title);
                if (pasta != null)
                {
                    await Context.Channel.ReplyAsync(pasta.Content);
                }
                else
                {
                    DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                    await Context.Channel.ReplyAsync($"Whoops, `{title}` doesn't exist");
                }
            }
        }

        [Command("fuse"), Summary("Fuses 2 of the 1st generation pokemon")]
        public async Task Fuse(int int1, int int2)
        {
            if (int1 > 151 || int1 < 0)
            {
                await Context.Channel.ReplyAsync($"{int1} over/under limit. (151)");
            }
            else if (int2 > 151 || int2 < 0)
            {
                await Context.Channel.ReplyAsync($"{int2} over/under limit. (151)");
            }
            else
            {
                await Context.Channel.ReplyAsync(new EmbedBuilder
                {
                    Color = EmbedUtils.RandomColor(),
                    ImageUrl = $"http://images.alexonsager.net/pokemon/fused/{int1}/{int1}.{int2}.png"
                }.Build());
            }
        }

        [Command("strawpoll"), Summary("Creates Strawpoll")]
        public async Task StrawpollSend(string title, [Remainder]string options)
        {
            var optionsLocal = options.Split(',');
            var poll = await Strawpoll.SendPoll(title, optionsLocal);
            await Context.Channel.ReplyAsync($"Strawpoll **{title}** has been created, here's the link: {poll.Url}");
        }

        [Command("strawpoll"), Summary("Gets a strawpoll")]
        public async Task StrawpollGet(string url)
        {
            if (!url.IsWebsite())
            {
                url = "https://strawpoll.me/" + url;
            }
            var poll = await Strawpoll.GetPoll(url);
            if (poll != null)
            {
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

                await Context.Channel.ReplyAsync(embed.Build());
            }

            await Context.Channel.ReplyAsync("Poll: `" + url + "` doesn't exist");
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
            await Context.Channel.ReplyAsync(newmessage);
        }

        /*[Command("slots", RunMode= RunMode.Async),Summary("Test your luck")]
		public async Task Slots(int bet)
		{
			Random rand = new Random();
			double luckfactor = Convert.ToDouble((await Sql.GetAsync($"SELECT luckfactor FROM accounts where ID = {Context.User.Id}")).Rows[0][0]);
            string[] reels = new string[] { ":banana:", ":apple:", ":lemon:", ":peach:", ":tangerine:", ":cherries:", ":grapes:", ":star:" };
			string[,] reelpayout = new string[,] { { "", "" }, { "", "" } };
			int reellngth = reels.Length;
			int reel1 = 0, reel2 = 0, reel3 = 0;
			for(int i=0; i<10;i++)
			{
				reel1 = rand.Next(1, reellngth);
				reel2 = rand.Next(1, reellngth);
				reel3 = rand.Next(1, reellngth);
			}
			if(reels[reel1] == reels[reel2])
			{

			}
			if (reels[reel1] == reels[reel2] && reels[reel2] == reels[reel3])
			{

			}
		}*/

        [Command("xkcd"), Summary("Get's Random XKCD comic"), Ratelimit(5, 1, Measure.Minutes)]
        public async Task XKCD(int comicid = -1)
        {
            if (comicid == -1)
            {
                await SendXKCD((await ComicClients.GetRandomXKCDComicAsync())).ConfigureAwait(false);
            }
            else
            {
                await SendXKCD((await ComicClients.GetXKCDComicAsync(comicid))).ConfigureAwait(false);
            }
        }
        public async Task SendXKCD(Models.API.XKCDComic comic)
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
                await Context.Channel.ReplyAsync(new EmbedBuilder
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
                var comic = await ComicClients.GetCAHComicAsync();
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
                await Context.Channel.ReplyAsync(comic.URL, embed);
            }
            catch (Exception ex)
            {
                await Logger.AddToLogsAsync(new Models.LogMessage("CAH-Cmd", "Error parsing website", LogSeverity.Error, ex));
            }
        }

        [Command("time"), Summary("Gets current time")]
        public async Task Time()
        {
            var offset = new DateTimeOffset(DateTime.UtcNow);
            await Context.Channel.ReplyAsync(offset.ToString());
        }

        [Command("time"), Summary("Gets time from utc with offset")]
        public async Task Time(int offset)
        {
            var ofs = Convert.ToDouble(offset);
            var ts = new TimeSpan();
            var nts = ts.Add(TimeSpan.FromHours(ofs));
            var dtOffset = new DateTimeOffset(DateTime.UtcNow);
            var ndtof = dtOffset.ToOffset(nts);
            await Context.Channel.ReplyAsync(ndtof.ToString());
        }

        [Command("roast"), Summary("\"Roasts\" a user, these are all taken as jokes, and aren't actually meant to cause harm.")]
        public async Task RoastCmd(IUser user = null)
        {
            if (user == null)
                user = Context.User;
            var roast = await SysExClient.GetRoastAsync();
            await Context.Channel.ReplyAsync(user.Mention + " " + roast);
        }

        [Command("dadjoke"), Summary("Gives you a bad dad joke to facepalm at.")]
        public async Task DadJoke()
        {
            var joke = await SysExClient.GetDadJokeAsync();

            await Context.Channel.ReplyAsync(new EmbedBuilder
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

            await Context.Channel.ReplyAsync(new EmbedBuilder
            {
                Title = pickup.Setup,
                Description = pickup.Punchline,
                Color = EmbedUtils.RandomColor()
            }.Build());
        }

        [Command("apod"), Summary("Gets NASA's \"Astronomy Picture of the Day\""), Ratelimit(20, 1, Measure.Minutes)]
        public async Task APOD()
        {
            var aPOD = await NASAClient.GetAPODAsync();

            if(!aPOD.HDUrl.IsVideoFile())
            {
                var embed = new EmbedBuilder
                {
                    Color = EmbedUtils.RandomColor(),
                    Title = aPOD.Title,
                    Url = "https://apod.nasa.gov/",
                    ImageUrl = aPOD.HDUrl,
                    Timestamp = Convert.ToDateTime(aPOD.Date),
                    Author = new EmbedAuthorBuilder
                    {
                        Name = aPOD.CopyRight
                    }
                };
                await Context.Channel.ReplyAsync(embed.Build());
            }
            else
            {
                await Context.Channel.ReplyAsync(aPOD.HDUrl);
            }
        }

        [Command("choose"), Summary("Choose from things, eg: books | games")]
        public async Task Choose([Remainder]string choices)
        {
            var choicearr = choices.Split('|');
            var choice = choicearr[Random.Next(0, choicearr.Length)];
            if (Char.IsWhiteSpace(choice[0]))
            { choice = choice.Remove(choice[0], 1); }
            else if (Char.IsWhiteSpace(choice[choice.Length - 1]))
            { choice = choice.Remove(choice.Length - 1); }
            await Context.Channel.ReplyAsync($"<:blobthinkcool:350673773113901056> | __{(Context.User as IGuildUser).Nickname ?? Context.User.Username}__ I choose: **{choice}**");
        }

        [Command("yn"), Summary("Yes? or No?")]
        public async Task YN([Remainder]string question = null)
        {
            Models.API.YNWTF YNResp = await YNWTFcli.AskYNWTF();
            var embed = new EmbedBuilder
            {
                Title = YNResp.Answer,
                ImageUrl = YNResp.Image,
                Color = EmbedUtils.RandomColor()
            };
            await Context.Channel.ReplyAsync(embed.Build());
        }

        [Command("safebooru"), Summary("Gets stuff from safebooru"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("Safe")]
        public async Task Safebooru(params string[] tags)
        {
            if (tags.ContainsBlacklistedTags())
            {
                await Context.Channel.ReplyAsync("Your tags contains a banned tag, please remove it.");
            }
            else
            {
                var cleantags = tags.AddBlacklistedTags();
                var posts = await BooruClient.GetSafebooruImagesAsync(cleantags);
                StatsdClient.DogStatsd.Increment("web.get");
                var post = GetSafeImage(posts);
                if (post != null)
                {
                    await Context.Channel.ReplyAsync(Tools.Tools.GetBooruMessage(post.Score, post.ImageUrl, post.PostUrl, post.ImageUrl.IsVideoFile()));
                    return;
                }
                await Context.Channel.ReplyAsync("Couldn't find an image");
            }
        }
        int EdgeCase;
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

        [Command("weeb")]
        public async Task WeebTag(string tag)
        {
            await Task.Delay(1);
        }
    }
}
