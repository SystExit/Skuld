using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using Skuld.APIS;
using Skuld.Models.API;
using Skuld.Tools;
using Discord;
using System.Globalization;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Discord.Addons.Interactive;
using Imgur.API.Endpoints.Impl;

namespace Skuld.Commands
{
    [Group,Name("Fun")]
    public class Fun : InteractiveBase
    {        
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
        static string[] videoext = {
            ".webm",
            ".mkv",
            ".flv",
            ".vob",
            ".ogv",
            ".ogg",
            ".avi",
            ".mov",
            ".qt",
            ".wmv",
            ".mp4",
            ".m4v",
            ".mpg",
            ".mpeg"
        };
		static List<Models.API.Reddit.Post> KitsunePosts;
		static int kitsucount = 0;
        
        [Command("neko", RunMode = RunMode.Async),Summary("neko grill"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Neko()
        {
            var neko = await NekoLife.GetNekoAsync();
            if (neko != null)
            {
                await MessageHandler.SendChannelAsync(Context.Channel, "", new EmbedBuilder { ImageUrl = neko }.Build());
            }
        }
        [RequireNsfw]
        [Command("lewdneko", RunMode = RunMode.Async), Summary("Lewd Neko Grill"), Ratelimit(20, 1, Measure.Minutes,false,true)]
        public async Task LewdNeko()
        {
            var neko = await NekoLife.GetLewdNekoAsync();
            if (neko != null)
            {
                await MessageHandler.SendChannelAsync(Context.Channel, "", new EmbedBuilder { ImageUrl = neko }.Build());
            }
		}
		[Command("kitsune", RunMode = RunMode.Async), Summary("Kitsunemimi Grill"), Ratelimit(20, 1, Measure.Minutes, false, true)]
		public async Task Kitsune()
		{
			if (KitsunePosts == null || kitsucount >= KitsunePosts.Count)
			{
				var data = await APIReddit.GetSubRedditAsync("r/kitsunemimi", 200);
				if (data != null)
				{
					KitsunePosts = data.Data.Posts.ToList();
				}
			}
			var vettedposts = KitsunePosts
						.Where(x => x.Data.Over18 == false && x.Data.Url != null && !x.Data.Stickied && x.Data.Domain != "reddit.com");
			if (vettedposts != null)
			{
				var rnd = Bot.random.Next(vettedposts.Count());
				var post = vettedposts.ElementAtOrDefault(rnd);
				if (post != null)
				{
					string imgurl = post.Data.Url;
					if (post.Data.Url.StartsWith("https://imgur.com"))
					{
						var albumep = new AlbumEndpoint(Bot.imgurClient);
						var album = await albumep.GetAlbumImagesAsync(post.Data.Url.Remove(0, "https://imgur.com/a/".Count()));
						imgurl = album.FirstOrDefault().Link;
					}
					await MessageHandler.SendChannelAsync(Context.Channel, "", new EmbedBuilder { ImageUrl = imgurl }.Build());
					kitsucount++;
				}
			}
		}
		[RequireNsfw]
		[Command("lewdkitsune", RunMode = RunMode.Async), Summary("Lewd Kitsunemimi Grill"), Ratelimit(20, 1, Measure.Minutes, false, true)]
		public async Task LewdKitsune()
		{
			if (KitsunePosts == null || kitsucount >= KitsunePosts.Count)
			{
				var data = await APIReddit.GetSubRedditAsync("r/kitsunemimi", 200);
				if (data != null)
				{
					KitsunePosts = data.Data.Posts.ToList();
				}
			}
			var vettedposts = KitsunePosts
						.Where(x => x.Data.Over18 == true && x.Data.Url != null && !x.Data.Stickied && x.Data.Domain != "reddit.com");
			if (vettedposts != null)
			{
				var rnd = Bot.random.Next(vettedposts.Count());
				var post = vettedposts.ElementAtOrDefault(rnd);
				if (post != null)
				{
					string imgurl = post.Data.Url;
					if (post.Data.Url.StartsWith("https://imgur.com"))
					{
						var albumep = new AlbumEndpoint(Bot.imgurClient);
						var album = await albumep.GetAlbumImagesAsync(post.Data.Url.Remove(0, "https://imgur.com/a/".Count()));
						imgurl = album.FirstOrDefault().Link;
					}
					await MessageHandler.SendChannelAsync(Context.Channel, "", new EmbedBuilder { ImageUrl = imgurl }.Build());
					kitsucount++;
				}
			}
		}

		[Command("kitty", RunMode = RunMode.Async), Summary("kitty"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("cat", "cats", "kittycat", "kitty cat", "meow", "kitties", "kittys")]
        public async Task Kitty()
        {
            var kitty = await APIS.Kitty.WebReq.GetKitty();
            if (videoext.Any(kitty.ImageURL.Contains))
            { await MessageHandler.SendChannelAsync(Context.Channel, kitty.ImageURL); }
            else
            { await MessageHandler.SendChannelAsync(Context.Channel, "", new EmbedBuilder { Color = Tools.Tools.RandomColor(), ImageUrl = kitty.ImageURL }.Build()); }
        }

        [Command("doggo", RunMode = RunMode.Async), Summary("doggo"), Ratelimit(20, 1, Measure.Minutes)]
        [Alias("dog", "dogs", "doggy")]
        public async Task Doggo()
        {            
            var doggo = await APIS.Doggo.WebReq.GetDoggo();
            if (videoext.Any(doggo.ImageURL.Contains))
            { await MessageHandler.SendChannelAsync(Context.Channel, doggo.ImageURL); }
            else
            { await MessageHandler.SendChannelAsync(Context.Channel, "", new EmbedBuilder { Color = Tools.Tools.RandomColor(), ImageUrl = doggo.ImageURL }.Build()); }
        }

        [Command("llama", RunMode = RunMode.Async), Summary("Llama"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Llama()
        {
            var llama = JsonConvert.DeserializeObject<Animal>(await APIWebReq.ReturnString(new Uri("https://api.systemexit.co.uk/get/llama/random")));
            await MessageHandler.SendChannelAsync(Context.Channel, "", new EmbedBuilder { Color = Tools.Tools.RandomColor(), ImageUrl = llama.FileUrl }.Build());
        }

        [Command("seal", RunMode = RunMode.Async), Summary("Seal"), Ratelimit(20, 1, Measure.Minutes)]
        public async Task Seal()
        {
            var seal = JsonConvert.DeserializeObject<Animal>(await APIWebReq.ReturnString(new Uri("https://api.systemexit.co.uk/get/seal/random")));
            await MessageHandler.SendChannelAsync(Context.Channel, "", new EmbedBuilder { Color = Tools.Tools.RandomColor(), ImageUrl = seal.FileUrl }.Build());
        }

        [Command("eightball", RunMode = RunMode.Async), Summary("Eightball")]
        [Alias("8ball")]
#pragma warning disable RECS0154 // Parameter is never used
        public async Task Eightball([Remainder]string whatisyourquestion) => await Eightball();
#pragma warning restore RECS0154 // Parameter is never used        

        [Command("eightball", RunMode = RunMode.Async), Summary("Eightball")]
        [Alias("8ball")]
        public async Task Eightball()
        {
            var usr = await Bot.Database.GetUserAsync(Context.User.Id);
            var locale = Locale.GetLocale(usr.Language);
            await MessageHandler.SendChannelAsync(Context.Channel, $"{Context.User.Username} :8ball: says: {locale.GetString(eightball[Bot.random.Next(0, eightball.Length)])}");
        }

        [Command("roll", RunMode = RunMode.Async), Summary("Roll a die")]
        public async Task Roll(int upper)
        {
            try
            {
                int rand = Bot.random.Next(1, (upper + 1));
                if (rand == 1)
                {
                    var cmd = new MySqlCommand("SELECT luckfactor FROM accounts where ID = @userid");
                    cmd.Parameters.AddWithValue("@userid", Context.User.Id);
                    var currluckfact = Convert.ToDouble(await Bot.Database.GetSingleAsync(cmd));
                    currluckfact = currluckfact / 1.1;
                    currluckfact = Math.Round(currluckfact, 4);
                    if (currluckfact < 0.1)
                    { currluckfact = 0.1; }
                    cmd = new MySqlCommand("UPDATE accounts SET luckfactor = @luckfactor where ID = @userid");
                    cmd.Parameters.AddWithValue("@userid", Context.User.Id);
                    cmd.Parameters.AddWithValue("@luckfactor", currluckfact);
                    await Bot.Database.NonQueryAsync(cmd);
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} just rolled and got a {rand} :weary:");
                }
                else
                { await Context.Channel.SendMessageAsync($"{Context.User.Mention} just rolled and got a {rand}"); }
            }
            catch (FormatException)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} this command only accepts numbers, try again");
                StatsdClient.DogStatsd.Increment("commands.errors.exception");
            }
        }

        [Command("pasta", RunMode = RunMode.Async), Summary("Pastas are nice")]
        public async Task Pasta(string cmd, string title)
        {
            var pastaLocal = await Bot.Database.GetPastaAsync(title);
            pastaLocal.Username = Context.Client.GetUser(pastaLocal.OwnerID).Username??"Unknown";
            if (pastaLocal != null)
            {
                if (cmd == "who" || cmd == "?")
                {
                    var embed = new EmbedBuilder()
                    {
                        Color = Tools.Tools.RandomColor(),
                        Title = pastaLocal.PastaName
                    };
                    embed.AddField("Author", pastaLocal.Username, inline: true);
                    embed.AddField("Created", pastaLocal.Created, inline: true);
                    embed.AddField("UpVotes", ":arrow_double_up: " + pastaLocal.Upvotes, inline: true);
                    embed.AddField("DownVotes", ":arrow_double_down: " + pastaLocal.Downvotes, inline: true);
                    await MessageHandler.SendChannelAsync(Context.Channel, "", embed.Build());
                }
                if (cmd == "upvote")
                {
                    await MessageHandler.SendChannelAsync(Context.Channel, "This function is currently disabled due to a new update coming soon. :( Sorry for the inconvenience");
                    /*command = new MySqlCommand("SELECT upvotes FROM pasta WHERE pastaname = @title");
                    command.Parameters.AddWithValue("@title", title);
                    uint upvotes = Convert.ToUInt32(await Sql.GetSingleAsync(command));
                    upvotes = upvotes+1;
                    command = new MySqlCommand("UPDATE pasta SET upvotes = @upvotes WHERE pastaname = @title");
                    command.Parameters.AddWithValue("@title", title);
                    command.Parameters.AddWithValue("@upvotes", upvotes);
                    await Sql.NonQueryAsync(command);
                    command = new MySqlCommand("SELECT upvotes FROM pasta WHERE pastaname = @title");
                    command.Parameters.AddWithValue("@title", title);
                    uint upvote = Convert.ToUInt32(await Sql.GetSingleAsync(command));
                    if (upvotes == upvote)
                        await MessageHandler.SendChannelAsync(Context.Channel, $"Upvote added to **{title}** it currently has: {upvotes} Upvotes");*/
                }
                if (cmd == "downvote")
                {
                    await MessageHandler.SendChannelAsync(Context.Channel, "This function is currently disabled due to a new update coming soon. :( Sorry for the inconvenience");
                    /*command = new MySqlCommand("SELECT downvotes FROM pasta WHERE pastaname = @title");
                    command.Parameters.AddWithValue("@title", title);
                    uint downvotes = Convert.ToUInt32(await Sql.GetSingleAsync(command));
                    downvotes = downvotes + 1;
                    command = new MySqlCommand("UPDATE pasta SET downvotes = @downvotes WHERE pastaname = @title");
                    command.Parameters.AddWithValue("@title", title);
                    command.Parameters.AddWithValue("@downvotes", downvotes);
                    await Sql.NonQueryAsync(command);
                    command = new MySqlCommand("SELECT downvotes FROM pasta WHERE pastaname = @title");
                    command.Parameters.AddWithValue("@title", title);
                    uint Downvote = Convert.ToUInt32(await Sql.GetSingleAsync(command));
                    if (downvotes == Downvote)
                        await MessageHandler.SendChannelAsync(Context.Channel, $"Downvote added to **{title}** it currently has: {downvotes} Downvote");*/
                }
                if (cmd == "delete")
                {
                    if (Convert.ToUInt64(pastaLocal.OwnerID) == Context.User.Id)
                    {
                        var command = new MySqlCommand("DELETE FROM pasta WHERE pastaname = @title");
                        command.Parameters.AddWithValue("@title", title);
                        await Bot.Database.NonQueryAsync(command).ContinueWith(async x =>
                        {
                            if (x.IsCompleted)
                            { await MessageHandler.SendChannelAsync(Context.Channel, $"Successfully deleted: **{title}**"); }
                        });
                    }
                }
            }
            else
            {
                StatsdClient.DogStatsd.Increment("commands.errors.generic");
                await MessageHandler.SendChannelAsync(Context.Channel, $"Pasta `{title}` doesn't exist. :/ Sorry.");
            }
        }

        [Command("pasta", RunMode = RunMode.Async), Summary("Pastas are nice")]
        public async Task Pasta(string cmd, string title, [Remainder]string content)
        {
            if (cmd == "new" || cmd == "+" || cmd == "create")
            {
                if (title == "list" || title == "help")
                {
                    await MessageHandler.SendChannelAsync(Context.Channel, "Nope");
                    StatsdClient.DogStatsd.Increment("commands.errors.generic");
                }
                else
                {
                    var command = new MySqlCommand("SELECT pastaname FROM pasta WHERE pastaname = @pastatitle");
                    command.Parameters.AddWithValue("@pastatitle", title);
                    var pastaname = await Bot.Database.GetSingleAsync(command);
                    if (!string.IsNullOrEmpty(pastaname))
                    {
                        await MessageHandler.SendChannelAsync(Context.Channel, $"Pasta already exists with name: **{title}**");
                        StatsdClient.DogStatsd.Increment("commands.errors.generic");
                    }
                    else
                    {
                        content = content.Replace("\'", "\\\'");
                        content = content.Replace("\"", "\\\"");

                        command = new MySqlCommand("INSERT INTO pasta (content,username,ownerid,created,pastaname) VALUES ( @content , @username , @ownerid , @created , @pastatitle )");
                        command.Parameters.AddWithValue("@content", content);
                        command.Parameters.AddWithValue("@ownerid", Context.User.Id);
                        command.Parameters.AddWithValue("@created", DateTime.UtcNow);
                        command.Parameters.AddWithValue("@pastatitle", title);

                        await Bot.Database.NonQueryAsync(command).ContinueWith(async x =>
                        {
                            command = new MySqlCommand("SELECT pastaname FROM pasta WHERE pastaname = @pastatitle");
                            command.Parameters.AddWithValue("@pastatitle", title);
                            var newpasta = await Bot.Database.GetSingleAsync(command);
                            if (x.IsCompleted && !string.IsNullOrEmpty(newpasta))
                            {
                                await MessageHandler.SendChannelAsync(Context.Channel, $"Successfully added: **{title}**");
                            }
                        });
                    }
                }
            }
            if (cmd == "edit" || cmd == "change" || cmd == "modify")
            {
                var command = new MySqlCommand("SELECT ownerid from pasta where pastaname = @title");
                command.Parameters.AddWithValue("@title", title);
                var ownerid = await Bot.Database.GetSingleAsync(command);
                if (Convert.ToUInt64(ownerid) == Context.User.Id)
                {
                    command = new MySqlCommand("SELECT content FROM pasta where pastaname = @title");
                    command.Parameters.AddWithValue("@title", title);
                    var oldcontent = await Bot.Database.GetSingleAsync(command);
                    content = content.Replace("\'", "\\\'");
                    content = content.Replace("\"", "\\\"");
                    command = new MySqlCommand("UPDATE pasta SET content = @content WHERE pastaname = @title");
                    command.Parameters.AddWithValue("@content", content);
                    command.Parameters.AddWithValue("@title", title);
                    await Bot.Database.NonQueryAsync(command).ContinueWith(async x =>
                    {
                        command = new MySqlCommand("SELECT content FROM pasta where pastaname = @title");
                        command.Parameters.AddWithValue("@title", title);
                        var respnew = await Bot.Database.GetSingleAsync(command);
                        if (x.IsCompleted && respnew != oldcontent)
                            await MessageHandler.SendChannelAsync(Context.Channel, $"Successfully changed the content of **{title}**");
                    });
                }
                else
                {
                    StatsdClient.DogStatsd.Increment("commands.errors.unm-precon");
                    await MessageHandler.SendChannelAsync(Context.Channel, "I'm sorry, but you don't own the Pasta");
                }
            }
        }

        [Command("pasta", RunMode = RunMode.Async), Summary("Pastas are nice")]
        public async Task Pasta([Remainder]string title)
        {
            if (title == "list")
            {
				var pastas = await Bot.Database.GetAllPastasAsync();

				string pastanames = "```\n";

				foreach(var pasta in pastas)
				{
					if (pasta == pastas.LastOrDefault())
					{ pastanames += pasta.PastaName; }
					else
					{ pastanames += pasta.PastaName + ", "; }
				}

				pastanames += "\n```";

                await MessageHandler.SendChannelAsync(Context.Channel, $"I found:\n{pastanames}");
            }
            else if (title == "help")
            {
                string help = "Here's how to do stuff with **pasta**:\n\n" +
                    "```cs\n"+
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
                await MessageHandler.SendChannelAsync(Context.Channel, help);
            }
            else
            {
                var command = new MySqlCommand("SELECT content FROM pasta WHERE pastaname = @title");
                command.Parameters.AddWithValue("@title", title);
                string response = await Bot.Database.GetSingleAsync(command);
                if (!String.IsNullOrEmpty(response))
                { await MessageHandler.SendChannelAsync(Context.Channel, response); }
                else
                {
                    StatsdClient.DogStatsd.Increment("commands.errors.generic");
                    await MessageHandler.SendChannelAsync(Context.Channel, $"Whoops, `{title}` doesn't exist");
                }
            }
        }
        
        [Command("fuse", RunMode = RunMode.Async), Summary("Fuses 2 of the 1st generation pokemon")]
        public async Task Fuse(int int1, int int2)
        {
            if (int1 > 151 || int1 < 0)
            { await MessageHandler.SendChannelAsync(Context.Channel, $"{int1} over/under limit. (151)"); }
            else if (int2 > 151 || int2 < 0)
            { await MessageHandler.SendChannelAsync(Context.Channel, $"{int2} over/under limit. (151)"); }
            else
            { await MessageHandler.SendChannelAsync(Context.Channel, "", new EmbedBuilder() { Color = Tools.Tools.RandomColor(), ImageUrl = $"http://images.alexonsager.net/pokemon/fused/{int1}/{int1}.{int2}.png" }.Build()); }
        }

        [Command("strawpoll", RunMode = RunMode.Async), Summary("Creates Strawpoll")]
        public async Task StrawpollSend(string title, [Remainder]string options)
        {
            var optionsLocal = options.Split(',');
            var poll = await APIWebReq.SendPoll(title, optionsLocal);            
            await MessageHandler.SendChannelAsync(Context.Channel,$"Strawpoll **{title}** has been created, here's the link: {poll.Url}");
        }

        [Command("strawpoll", RunMode = RunMode.Async), Summary("Gets a strawpoll")]
        public async Task StrawpollGet(int id) =>
            await StrawpollGet("http://www.strawpoll.me/" + id).ConfigureAwait(false);

        [Command("strawpoll", RunMode = RunMode.Async), Summary("Gets a strawpoll")]
        public async Task StrawpollGet(string url)
        {
            var poll = await APIWebReq.GetPoll(url);
            var embed = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = poll.Title,
                    Url = poll.Url
                },
                Color = Tools.Tools.RandomColor(),
                Footer = new EmbedFooterBuilder()
                {
                    Text = "Strawpoll ID: " + poll.ID
                },
                Timestamp = DateTime.UtcNow
            };
            for (int z = 0; z < poll.Options.Length; z++)
            { embed.AddField(poll.Options[z], poll.Votes[z]); }

            await MessageHandler.SendChannelAsync(Context.Channel, "", embed.Build());
        }

        [Command("emoji", RunMode = RunMode.Async), Summary("Turns text into bigmoji")]
        public async Task Emojify([Remainder]string message)
        {
            string newmessage = "";
            var regexItem = new Regex("^[a-zA-Z0-9 ]*$");
            foreach (var character in message)
            {
                if (!regexItem.IsMatch(Convert.ToString(character)))
                { newmessage += character; }
                if (!Char.IsWhiteSpace(character))
                { newmessage += ":regional_indicator_" + character + ": "; }
                else
                { newmessage += " "; }
            }
            await MessageHandler.SendChannelAsync(Context.Channel, newmessage);
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

        [Command("xkcd", RunMode = RunMode.Async), Summary("Get's random XKCD comic"), Ratelimit(5, 1, Measure.Minutes)]
        public async Task XKCD()
        {
            int randComic = 0;
            await APIWebReq.GetXKCDLastPage();
            for (int x = 0; x < 10; x++)
            {
                randComic = Bot.random.Next(0, APIWebReq.XKCDLastPage.Value);
            }
            await SendXKCD((await APIWebReq.GetXKCDComic(randComic)));
        }
        public async Task SendXKCD(XKCDComic comic)
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
            await MessageHandler.SendChannelAsync(Context.Channel,string.Empty, new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = "Randall Patrick Munroe - XKCD",
                    Url = "https://xkcd.com/" + comic.num + "/",
                    IconUrl = "https://xkcd.com/favicon.ico"
                },
                Footer = new EmbedFooterBuilder()
                {
                    Text = "Strip released on"
                },
                Color = Tools.Tools.RandomColor(),
                Timestamp = dateTime,
                ImageUrl = comic.img,
                Title = comic.safe_title,
                Description = comic.alt
            }.Build());
        }

        [Command("cah", RunMode = RunMode.Async), Summary("Gets a random Cynaide & Happiness Comic"), Alias("cyanide&happiness","c&h"), Ratelimit(5, 1, Measure.Minutes)]
        public async Task CAH()
        {
            try
            {
                var doc = await APIWebReq.ScrapeUrl(new Uri("http://explosm.net/comics/random"));
                var author = doc.DocumentNode.Descendants("div").FirstOrDefault(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.Contains("small-2 medium-2 large-2 columns"));
                var authorblock = doc.DocumentNode.Descendants("div").FirstOrDefault(z => z.Attributes.Contains("class") && z.Attributes["class"].Value.Contains("small-8 medium-9 large-8 columns"));
                var authorblocksection = author.Descendants().Skip(1).FirstOrDefault();
                var authorurl = "http://explosm.net" + authorblocksection.Attributes["href"].Value;
                var authoravatar = "http:" + authorblocksection.ChildNodes.FirstOrDefault().Attributes["src"].Value;
                var image = "http:" + doc.GetElementbyId("main-comic").Attributes["src"].Value;
                var embed = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = "Strip done "+ authorblock.InnerText.Split('\n')[2],
                        Url = authorurl,
                        IconUrl = authoravatar
                    },
                    ImageUrl = image,
                    Color = Tools.Tools.RandomColor()
                }.Build();
                await MessageHandler.SendChannelAsync(Context.Channel, doc.GetElementbyId("permalink").GetAttributeValue("value", ""), embed);
            }
            catch(Exception ex)
            {
                await Bot.Logger.AddToLogs(new Models.LogMessage("CAH-Cmd", "Error parsing website", LogSeverity.Error, ex));
            }
        }

        [Command("time", RunMode = RunMode.Async), Summary("Gets current time")]
        public async Task Time()
        {
            var offset = new DateTimeOffset(DateTime.UtcNow);
            await MessageHandler.SendChannelAsync(Context.Channel,offset.ToString());
        }

        [Command("time", RunMode = RunMode.Async), Summary("Gets time from utc with offset")]
        public async Task Time(int offset)
        {
            var ofs = Convert.ToDouble(offset);
            var ts = new TimeSpan();
            var nts = ts.Add(TimeSpan.FromHours(ofs));
            var dtOffset = new DateTimeOffset(DateTime.UtcNow);
            var ndtof = dtOffset.ToOffset(nts);
            await MessageHandler.SendChannelAsync(Context.Channel,ndtof.ToString());
        }

        [Command("roast", RunMode = RunMode.Async), Summary("\"Roasts\" a user, these are all taken as jokes, and aren't actually meant to cause harm.")]
        public async Task RoastCmd(IGuildUser user) =>
            await Roast(user).ConfigureAwait(false);

        [Command("roastme", RunMode = RunMode.Async), Summary("\"Roast\" yourself, these are all taken as jokes, and aren't actually meant to cause harm.")]
        public async Task RoastYourselfCmd() =>
            await Roast(Context.User as IGuildUser).ConfigureAwait(false);
        public async Task Roast(IGuildUser user)
        {
            var roast = JsonConvert.DeserializeObject<Roasts>(await APIWebReq.ReturnString(new Uri("https://api.systemexit.co.uk/get/roasts/random")));
            await MessageHandler.SendChannelAsync(Context.Channel, user.Mention + " " + roast.Roast);
        }

        [Command("dadjoke", RunMode = RunMode.Async), Summary("Gives you a bad dad joke to facepalm at.")]
        public async Task DadJoke()
        {
            var joke = JsonConvert.DeserializeObject<PickupLine>(await APIWebReq.ReturnString(new Uri("https://api.systemexit.co.uk/get/dadjokes/random")));

            await MessageHandler.SendChannelAsync(Context.Channel, "", new EmbedBuilder
            {
                Title = joke.Setup,
                Description = Tools.Tools.CheckForNull(joke.Punchline),
                Color = Tools.Tools.RandomColor()
            }.Build());
        }

        [Command("pickup",RunMode = RunMode.Async),Summary("Cringe at these bad user-submitted pick up lines. (Don't actually use these or else you'll get laughed at. :3)"),Alias("pickupline")]
        public async Task PickUp()
        {
            var pickup = JsonConvert.DeserializeObject<PickupLine>(await APIWebReq.ReturnString(new Uri("https://api.systemexit.co.uk/get/pickuplines/random")));

            await MessageHandler.SendChannelAsync(Context.Channel, "", new EmbedBuilder
            {
                Title = pickup.Setup,
                Description = Tools.Tools.CheckForNull(pickup.Punchline),
                Color = Tools.Tools.RandomColor()
            }.Build());
        }   

        [Command("apod", RunMode = RunMode.Async), Summary("Gets NASA's \"Astronomy Picture of the Day\""), Ratelimit(20, 1, Measure.Minutes)]
        public async Task APOD()
        {
            var aPOD = await APIWebReq.NasaAPOD();
            var embed = new EmbedBuilder
            {
                Color = Tools.Tools.RandomColor(),
                Title = aPOD.Title,
                Url = "https://apod.nasa.gov/",
                ImageUrl = aPOD.HDUrl,
                Timestamp = Convert.ToDateTime(aPOD.Date),
                Author = new EmbedAuthorBuilder
                {
                    Name = aPOD.CopyRight
                }
            };
            await MessageHandler.SendChannelAsync(Context.Channel, "", embed.Build());
        }

        [Command("choose", RunMode = RunMode.Async), Summary("Choose from things")]
        public async Task Choose([Remainder]string choices)
        {
            var choicearr = choices.Split('|');
            var choice = choicearr[Bot.random.Next(0, choicearr.Length)];
            if (Char.IsWhiteSpace(choice[0]))
            { choice = choice.Remove(choice[0], 1); }
            else if (Char.IsWhiteSpace(choice[choice.Length - 1]))
            { choice = choice.Remove(choice.Length - 1); }
            await MessageHandler.SendChannelAsync(Context.Channel, $"<:blobthinkcool:350673773113901056> | __{(Context.User as IGuildUser).Nickname??Context.User.Username}__ I choose: **{choice}**");
        }

        [Command("yn", RunMode = RunMode.Async), Summary("Yes? or No?")]
        public async Task YN([Remainder]string question) => await YN();

        [Command("yn", RunMode = RunMode.Async), Summary("Yes? or No?")]
        public async Task YN()
        {
            var YNResp = await APIWebReq.AskYNWTF();
            var embed = new EmbedBuilder
            {
                Color = Tools.Tools.RandomColor(),
                Title = YNResp.Answer,
                ImageUrl = YNResp.Image
            };
            await MessageHandler.SendChannelAsync(Context.Channel, "", embed.Build());
        }

        [Command("heal", RunMode = RunMode.Async), Summary("Did you run out of health? Here's the healing station")]
        public async Task Heal()
            => await MessageHandler.SendChannelAsync(Context.Channel, "You need to supply how much to heal by.");

        [Command("heal", RunMode = RunMode.Async), Summary("Did you run out of health? Here's the healing station")]
        public async Task Heal(uint hp)
            => await Heal(hp, Context.User as IGuildUser);

        [Command("heal", RunMode = RunMode.Async), Summary("Did you run out of health? Here's the healing station")]
        public async Task Heal(uint hp, [Remainder]IGuildUser User)
        {
            var user = await Bot.Database.GetUserAsync(User.Id);
            if (user != null)
            {
                var offset = 10000 - user.HP;
                if (hp > offset)
                {
                    if (User == Context.User)
                        await MessageHandler.SendChannelAsync(Context.Channel, "You sure you wanna do that? You only need to heal by: `"+offset+"` HP");
                    else
                        await MessageHandler.SendChannelAsync(Context.Channel, "You sure you wanna do that? They only need to heal by: `" + offset + "` HP");
                    return;
                }

                var cost = GetCostOfHP(hp);
                if (user.Money >= cost)
                {
                    if (user.HP == 10000)
                    {
                        await MessageHandler.SendChannelAsync(Context.Channel, "You're already at max health");
                        return;
                    }

                    user.Money -= cost;
                    user.HP += hp;

                    if (user.HP > 10000)
                        user.HP = 10000;

                    await Bot.Database.UpdateUserAsync(user);

                    if (User == Context.User)
                        await MessageHandler.SendChannelAsync(Context.Channel, $"You have healed your hp by {hp} for {Bot.Configuration.MoneySymbol}{cost.ToString("N0")}");
                    else
                        await MessageHandler.SendChannelAsync(Context.Channel, $"You have healed {User.Nickname ?? User.Username}'s health by {hp} for {Bot.Configuration.MoneySymbol}{cost.ToString("N0")}");
                }
                else
                {
                    await MessageHandler.SendChannelAsync(Context.Channel, "You don't have enough money for this action.");
                }
            }
        }

        ulong GetCostOfHP(uint hp)
        {
            return (ulong)(hp / 0.8);
        }
    }
}
