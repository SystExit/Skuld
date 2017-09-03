using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using Skuld.APIS;
using Skuld.Models.API;
using Skuld.Tools;
using Discord;
using Skuld.Models;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using MySql.Data.MySqlClient;
using CowsaySharp.Library;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Skuld.Commands
{
    [Group,Name("Fun")]
    public class Fun : ModuleBase
    {
        
        static string[] eightball = {
            "It is certain",
            "It is decidedly so",
            "Without a doubt",
            "Yes, definitely",
            "You may rely on it",
            "As I see it, yes",
            "Most likely",
            "Outlook good",
            "Yes",
            "Signs point to yes",
            "Reply hazy, try again",
            "Ask again later",
            "Better not tell you now",
            "Cannot predict now",
            "Concentrate and ask again",
            "Don't count on it",
            "My reply is no",
            "My sources say no",
            "Outlook not so good",
            "Very doubtful"
        };
        string[] videoext = new string[] {
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
        string[] Roasts = new string[] {
            "You suck.",
            "Keep rolling your eyes. Maybe one day you'll find a brain back there.",
            "I'm no proctologist, but I know an asshole when I see one.",
            "If you are going to be two faced, at least make one of them pretty.",
            "You look like a before picture.",
            "So, a thought crossed your mind? Must've been a long and lonely journey.",
            "The smartass in me really doesn't like the dumbass in you.",
            "100,000 sperm, you were the fastest?",
            "Are you made of Gallium, Yitrium, Boron, Oxygen and Iodine? Because damn you are GA Y B O I.",
            "You're the reason the gene pool needs a lifeguard." };
        string[,] DadJokes = new string[,] { 
            {"What do you call a pile of cats?", "A \"meow\"ntain." },
            { "Why did the picture go to jail?", "Because it was framed." },
            { "You're 'Merican when you go into the bathroom and you're 'Merican when you come out. But do you know what you are while you're in there?","You're a \"peeing\"" },
            {"What did the ocean say to the shore?","Nothing. It just waved." },
            {"What do you do when you see a space man?","You park, man." },
            {"Why do bananas need sunscreen?","Because they peel." },
            {"Why does a chicken coup only have 2 doors?","Because if it had 4 doors, it would be a chicken sedan." },
            {"You've heard the rumor going around about butter?","Nevermind. I shouldn't spread it." },
            {"Why did the can-crusher quit his job?","Because it was \"soda\"pressing." },
            {"What do you call a man who never toots in public?","A private tooter." },
            {"What time did the man go to the dentist?","Tooth-hurty." },
            {"What's brown and sticky?","A stick." },
            {"What does an annoying pepper do?","It gets \"Jalapeno\" face." },
            {"What do you call a pony with a sore throat?", "A little \"Hoarse\"." },
            {"What happened when frogs parked illegally?","They get \"Toad\"." },
            {"What did the buffalo say to his son when he left for college?","Bison." },
            {"If the first french fries weren't cooked in France, where were they cooked?","They were cooked in Greece." },
            {"Did you hear about the circus fire?", "It was \"in tents\"." },
            {"What did the horse say after it tripped?","\"Help, I've fallen and I can't giddyup.\"" },
            {"I had a dream that I was a muffler last night.", "I woke up exhausted." },
            {"What do you call cheese that isn't yours?","Nacho Cheese." },
            {"How do you make Holy Water?","You boil the *Hell* out of it." },
            { "What do you can a fish with two knees?","A two-knee fish." },
            {"What's Forest Gump's password?", "1forest1" },
            {"How many tickles does it take to make an octopus laugh?","Ten-tickles" },
            {"I bought some shoes from a drug dealer","I don't know what he laced them with, but I was tripping all day." },
            {"Where does Fonzie like to go to eat?","Chick-fil-AAAAYYYY!" }
        };
        string[,] PickUpLines = new string[,] { 
            {"Is your dad a terrorist?", "Because you're the bomb." },
            {"Did you sit on a pile of sugar?","Because that is a sweet ass you got there." },
            {"Did you fall from heaven?","Because you look like you're in pain." },
            {"Are you a Butcher?","Because you seem good with meat." },
            {"Are you from Tennessee?","Because you're the only 10 I see" },
            {"Are you from Japan?", "Because I wanna get in Japanties." },
            {"Baby, you make my floppy disk turn into a hard drive.",null },
            {"Baby, you turn my software into hardware",null },
            {"Are you from Hell?", "Because you're hot AF." },
            {"Are you a magnet?", "Because I'm attracted to you." },
            {"Are you Mexican?","Because you're the Mex I Can handle." },
            {"If you were a chicken, you'd be impeccable.",null },
            {"You're like a candy bar;", "Half sweet, and half nuts."},
            {"You must be Jamaican,","because Jamaican me crazy." },
            {"Charmanders are red, mudkips are blue;","If you were a Pokemon, I'd choose you." }
        };        

        [Command("neko", RunMode = RunMode.Async),Summary("neko grill")]
        public async Task Neko()
        {
            var rawresp = await APIS.APIWebReq.ReturnString(new Uri("https://nekos.life/api/neko"));
            JObject jsonresp = JObject.Parse(rawresp);
            dynamic item = jsonresp;
            if (item["neko"].ToString() != null)
            {
                var neko = item["neko"];
                await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { ImageUrl = neko });
            }
        }

        [Command("kitty", RunMode = RunMode.Async), Summary("kitty")]
        [Alias("cat", "cats", "kittycat", "kitty cat", "meow", "kitties", "kittys")]
        public async Task Kitty()
        {
            var kitty = await APIS.Kitty.WebReq.GetKitty();
            if (videoext.Any(kitty.ImageURL.Contains))
                await MessageHandler.SendChannel(Context.Channel, kitty.ImageURL);
            else
                await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Color = RandColor.RandomColor(), ImageUrl = kitty.ImageURL });
        }
        [Command("doggo", RunMode = RunMode.Async), Summary("doggo")]
        [Alias("dog", "dogs", "doggy")]
        public async Task Doggo()
        {            
            var doggo = await APIS.Doggo.WebReq.GetDoggo();
            if (videoext.Any(doggo.ImageURL.Contains))
                await MessageHandler.SendChannel(Context.Channel, doggo.ImageURL);
            else
                await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Color = RandColor.RandomColor(), ImageUrl = doggo.ImageURL });
        }
        [Command("llama", RunMode = RunMode.Async), Summary("Llama")]
        public async Task Llama()
        {
            var llama = JsonConvert.DeserializeObject<Animal>(await APIWebReq.ReturnString(new Uri("https://api.systemexit.co.uk/animals/llama/random")));
            await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Color = RandColor.RandomColor(), ImageUrl = llama.FileUrl });
        }
        
        [Command("eightball", RunMode = RunMode.Async), Summary("Eightball")]
        [Alias("8ball")]
        public async Task Eightball([Remainder] string whatisyourquestion) { await MessageHandler.SendChannel(Context.Channel, $"{Context.User.Mention} :8ball: says: {eightball[Bot.random.Next(0, eightball.Length)]}"); }
        [Command("eightball", RunMode = RunMode.Async), Summary("Eightball")]
        [Alias("8ball")]
        public async Task Eightball() { await MessageHandler.SendChannel(Context.Channel, $"{Context.User.Mention} :8ball: says: {eightball[Bot.random.Next(0, eightball.Length)]}"); }

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
                    Double currluckfact= Convert.ToDouble(await SqlTools.GetSingleAsync(cmd));
                    currluckfact = currluckfact / 1.1;
                    currluckfact = Math.Round(currluckfact, 4);
                    if (currluckfact < 0.1)
                        currluckfact = 0.1;
                    cmd = new MySqlCommand("UPDATE accounts SET luckfactor = @luckfactor where ID = @userid");
                    cmd.Parameters.AddWithValue("@userid", Context.User.Id);
                    cmd.Parameters.AddWithValue("@luckfactor", currluckfact);
                    await SqlTools.InsertAsync(cmd);
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} just rolled and got a {rand} :weary:");
                }
                else
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} just rolled and got a {rand}");
            }
            catch (FormatException)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} this command only accepts numbers, try again");
            }
        }

        [Command("pasta", RunMode = RunMode.Async), Summary("Pastas are nice")]
        public async Task Pasta(string cmd, string title)
        {
            var Pasta = await SqlTools.GetPasta(title);
            if (Pasta != null)
            {
                if (cmd == "who" || cmd == "?")
                {
                    EmbedBuilder _embed = new EmbedBuilder()
                    {
                        Color = RandColor.RandomColor(),
                        Title = Pasta.PastaName
                    };
                    _embed.AddInlineField("Author",Pasta.Username);
                    _embed.AddInlineField("Created", Pasta.Created);
                    _embed.AddInlineField("UpVotes", Pasta.Upvotes);
                    _embed.AddInlineField("DownVotes", Pasta.Downvotes);
                    await MessageHandler.SendChannel(Context.Channel, "", _embed);
                }
                if (cmd == "upvote")
                {
                    await MessageHandler.SendChannel(Context.Channel, "This function is currently disabled due to a new update coming soon. :( Sorry for the inconvenience");
                    /*command = new MySqlCommand("SELECT upvotes FROM pasta WHERE pastaname = @title");
                    command.Parameters.AddWithValue("@title", title);
                    uint upvotes = Convert.ToUInt32(await Sql.GetSingleAsync(command));
                    upvotes = upvotes+1;
                    command = new MySqlCommand("UPDATE pasta SET upvotes = @upvotes WHERE pastaname = @title");
                    command.Parameters.AddWithValue("@title", title);
                    command.Parameters.AddWithValue("@upvotes", upvotes);
                    await Sql.InsertAsync(command);
                    command = new MySqlCommand("SELECT upvotes FROM pasta WHERE pastaname = @title");
                    command.Parameters.AddWithValue("@title", title);
                    uint upvote = Convert.ToUInt32(await Sql.GetSingleAsync(command));
                    if (upvotes == upvote)
                        await MessageHandler.SendChannel(Context.Channel, $"Upvote added to **{title}** it currently has: {upvotes} Upvotes");*/
                }
                if (cmd == "downvote")
                {
                    await MessageHandler.SendChannel(Context.Channel, "This function is currently disabled due to a new update coming soon. :( Sorry for the inconvenience");
                    /*command = new MySqlCommand("SELECT downvotes FROM pasta WHERE pastaname = @title");
                    command.Parameters.AddWithValue("@title", title);
                    uint downvotes = Convert.ToUInt32(await Sql.GetSingleAsync(command));
                    downvotes = downvotes + 1;
                    command = new MySqlCommand("UPDATE pasta SET downvotes = @downvotes WHERE pastaname = @title");
                    command.Parameters.AddWithValue("@title", title);
                    command.Parameters.AddWithValue("@downvotes", downvotes);
                    await Sql.InsertAsync(command);
                    command = new MySqlCommand("SELECT downvotes FROM pasta WHERE pastaname = @title");
                    command.Parameters.AddWithValue("@title", title);
                    uint Downvote = Convert.ToUInt32(await Sql.GetSingleAsync(command));
                    if (downvotes == Downvote)
                        await MessageHandler.SendChannel(Context.Channel, $"Downvote added to **{title}** it currently has: {downvotes} Downvote");*/
                }
                if (cmd == "delete")
                {
                    if(Convert.ToUInt64(Pasta.OwnerID) == Context.User.Id)
                    {
                        var command = new MySqlCommand("DELETE FROM pasta WHERE pastaname = @title");
                        command.Parameters.AddWithValue("@title", title);
                        await SqlTools.InsertAsync(command).ContinueWith(async x =>
                        {
                            if (x.IsCompleted)
                                await MessageHandler.SendChannel(Context.Channel, $"Successfully deleted: **{title}**");
                        });
                    }
                }
            }
            else
            {
                await MessageHandler.SendChannel(Context.Channel, $"Pasta `{title}` doesn't exist. :/ Sorry.");
            }
        }
        [Command("pasta", RunMode = RunMode.Async), Summary("Pastas are nice")]
        public async Task Pasta(string cmd, string title, [Remainder]string content)
        {
            if (cmd == "new" || cmd == "+" || cmd == "create")
            {
                if (title == "list" || title == "help")
                {
                    await MessageHandler.SendChannel(Context.Channel, "Nope");
                }
                else
                {
                    var command = new MySqlCommand("SELECT pastaname FROM pasta WHERE pastaname = @pastatitle");
                    command.Parameters.AddWithValue("@pastatitle", title);
                    var pastaname = await SqlTools.GetSingleAsync(command);
                    if (!string.IsNullOrEmpty(pastaname))
                    {
                        await MessageHandler.SendChannel(Context.Channel, $"Pasta already exists with name: **{title}**");
                    }
                    else
                    {
                        content = content.Replace("\'", "\\\'");
                        content = content.Replace("\"", "\\\"");

                        command = new MySqlCommand("INSERT INTO pasta (content,username,ownerid,created,pastaname) VALUES ( @content , @username , @ownerid , @created , @pastatitle )");
                        command.Parameters.AddWithValue("@content", content);
                        command.Parameters.AddWithValue("@username", Context.User.Username.Replace("\'", "\\\'").Replace("\"", "\\\"") + "#" + Context.User.DiscriminatorValue);                    
                        command.Parameters.AddWithValue("@ownerid", Context.User.Id);
                        command.Parameters.AddWithValue("@created", DateTime.UtcNow);
                        command.Parameters.AddWithValue("@pastatitle", title);

                        await SqlTools.InsertAsync(command).ContinueWith(async x =>
                        {
                            command = new MySqlCommand("SELECT pastaname FROM pasta WHERE pastaname = @pastatitle");
                            command.Parameters.AddWithValue("@pastatitle", title);
                            var newpasta = await SqlTools.GetSingleAsync(command);
                            if (x.IsCompleted && !string.IsNullOrEmpty(newpasta))
                            {
                                await MessageHandler.SendChannel(Context.Channel, $"Successfully added: **{title}**");
                            }
                        });
                    }
                }
            }
            if (cmd == "edit" || cmd == "change" || cmd == "modify")
            {
                var command = new MySqlCommand("SELECT ownerid from pasta where pastaname = @title");
                command.Parameters.AddWithValue("@title", title);
                var ownerid = await SqlTools.GetSingleAsync(command);
                if (Convert.ToUInt64(ownerid) == Context.User.Id)
                {
                    command = new MySqlCommand("SELECT content FROM pasta where pastaname = @title");
                    command.Parameters.AddWithValue("@title", title);
                    var oldcontent = await SqlTools.GetSingleAsync(command);
                    content.Replace("\'", "\\\'");
                    content.Replace("\"", "\\\"");
                    command = new MySqlCommand("UPDATE pasta SET content = @content WHERE pastaname = @title");
                    command.Parameters.AddWithValue("@content", content);
                    command.Parameters.AddWithValue("@title", title);
                    await SqlTools.InsertAsync(command).ContinueWith(async x =>
                    {
                        command = new MySqlCommand("SELECT content FROM pasta where pastaname = @title");
                        command.Parameters.AddWithValue("@title", title);
                        var respnew = await SqlTools.GetSingleAsync(command);
                        if (x.IsCompleted && respnew != oldcontent)
                            await MessageHandler.SendChannel(Context.Channel, $"Successfully changed the content of **{title}**");
                    });
                }
                else
                {
                    await MessageHandler.SendChannel(Context.Channel, "I'm sorry, but you don't own the Pasta");
                }

            }
        }
        [Command("pasta", RunMode = RunMode.Async), Summary("Pastas are nice")]
        public async Task Pasta([Remainder]string title)
        {
            if (title == "list")
            {
                string columndata = null;
                List<string> rows = new List<string>();
                var reader = await SqlTools.GetAsync(new MySqlCommand($"SELECT PastaName FROM pasta"));
                while (await reader.ReadAsync())
                {
                    rows.Add(reader["PastaName"].ToString());
                }
                reader.Close();
                await SqlTools.getconn.CloseAsync();
                if (rows != null)
                {
                    foreach (var item in rows)
                    {
                        if (item != rows.Last())
                            columndata += $"`{item}`, ";
                        else
                            columndata += $"`{item}`";
                    }
                }
                await MessageHandler.SendChannel(Context.Channel, $"I found:\n{columndata}");
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
                await MessageHandler.SendChannel(Context.Channel, help);
            }
            else
            {
                var command = new MySqlCommand("SELECT content FROM pasta WHERE pastaname = @title");
                command.Parameters.AddWithValue("@title", title);
                string response = await SqlTools.GetSingleAsync(command);
                if (!String.IsNullOrEmpty(response))
                    await MessageHandler.SendChannel(Context.Channel, response);
                else
                    await MessageHandler.SendChannel(Context.Channel, $"Whoops, `{title}` doesn't exist");
            }
        }
        /*[Command("pasta", RunMode = RunMode.Async), Summary("Pastas are nice")]
        public async Task Pasta(string cmd, IUser user, string title)
        {
            if (cmd == "give")
            {
                var command = new MySqlCommand("SELECT ownerid FROM pasta where pastaname = @title");
                command.Parameters.AddWithValue("@title", title);
                var id = Convert.ToUInt64(await Sql.GetSingleAsync(command));
                var OldUser = Bot.bot.GetUser(id);
                if (Context.User.Id == OldUser.Id)
                {
                    command = new MySqlCommand("UPDATE pasta SET ownerid = @ownerid WHERE ownerid = @oldownerid AND pastaname = @title");
                    command.Parameters.AddWithValue("@ownerid", user.Id);
                    command.Parameters.AddWithValue("@oldownerid", Context.User.Id);
                    command.Parameters.AddWithValue("@title",title);
                    await Sql.InsertAsync(command).ContinueWith(async x =>
                    {
                        command = new MySqlCommand("SELECT ownerid FROM pasta where pastaname = @title");
                        command.Parameters.AddWithValue("@title", title);
                        if (Context.User.Id != Convert.ToUInt64(await Sql.GetSingleAsync(command)))
                            await MessageHandler.SendChannel(Context.Channel, $"Successfully gave {user.Mention} pasta `{title}`");
                    });
                }
            }
        }*/

        [Command("fuse", RunMode = RunMode.Async), Summary("Fuses 2 of the 1st generation pokemon")]
        public async Task Fuse(int int1, int int2)
        {
            if (int1 > 151 || int1 < 0)
                await MessageHandler.SendChannel(Context.Channel,$"{int1} over/under limit. (151)");
            else if (int2 > 151 || int2 < 0)
                await MessageHandler.SendChannel(Context.Channel,$"{int2} over/under limit. (151)");
            else
                await MessageHandler.SendChannel(Context.Channel,"", new EmbedBuilder() { Color = RandColor.RandomColor(), ImageUrl = $"http://images.alexonsager.net/pokemon/fused/{int1}/{int1}.{int2}.png" });
        }

        [Command("strawpoll", RunMode = RunMode.Async), Summary("Creates Strawpoll")]
        public async Task StrawpollSend(string Title, [Remainder]string Options)
        {
            string[] options = Options.Split(',');
            var poll = await APIWebReq.SendPoll(Title, options);            
            await MessageHandler.SendChannel(Context.Channel,$"Strawpoll **{Title}** has been created, here's the link: {poll.Url}");
        }
        [Command("strawpoll", RunMode = RunMode.Async), Summary("Gets a strawpoll")]
        public async Task StrawpollGet(int id) =>
            await StrawpollGet("http://www.strawpoll.me/" + id);
        [Command("strawpoll", RunMode = RunMode.Async), Summary("Gets a strawpoll")]
        public async Task StrawpollGet(string url)
        {
            var poll = await APIWebReq.GetPoll(url);
            EmbedBuilder _embed = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = poll.Title,
                    Url = poll.Url
                },
                Color = RandColor.RandomColor(),
                Footer = new EmbedFooterBuilder()
                {
                    Text = "Strawpoll ID: " + poll.ID
                },
                Timestamp = DateTime.UtcNow
            };
            for (int z = 0; z < poll.Options.Length; z++)
                _embed.AddField(poll.Options[z], poll.Votes[z]);

            await MessageHandler.SendChannel(Context.Channel, "", _embed);
        }

        [Command("emoji", RunMode = RunMode.Async), Summary("Turns text into bigmoji")]
        public async Task Emojify([Remainder]string message)
        {
            string newmessage = "";
            var regexItem = new Regex("^[a-zA-Z0-9 ]*$");
            foreach (var character in message)
            {
                if(!regexItem.IsMatch(Convert.ToString(character)))
                    newmessage += character;
                if (!Char.IsWhiteSpace(character))
                    newmessage += ":regional_indicator_" + character + ": ";
                else
                    newmessage += " ";
            }
            await MessageHandler.SendChannel(Context.Channel, newmessage);
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

        [Command("xkcd", RunMode = RunMode.Async), Summary("Get's random XKCD comic")]
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

            EmbedBuilder _embed = new EmbedBuilder();
            EmbedAuthorBuilder _author = new EmbedAuthorBuilder();
            EmbedFooterBuilder _footer = new EmbedFooterBuilder();
            _embed.Color = RandColor.RandomColor();
            _author.Name = "Randall Patrick Munroe - XKCD";
            _author.Url = "https://xkcd.com/" + comic.num + "/";
            _author.IconUrl = "https://xkcd.com/favicon.ico";
            _footer.Text = "Strip released on";
            _embed.Timestamp = dateTime;

            _embed.ImageUrl = comic.img;
            _embed.Title = comic.safe_title;
            _embed.Description = comic.alt;

            _embed.Author = _author;
            _embed.Footer = _footer;

            await MessageHandler.SendChannel(Context.Channel,string.Empty, _embed);
        }

        [Command("time", RunMode = RunMode.Async), Summary("Gets current time")]
        public async Task Time()
        {
            DateTimeOffset offset = new DateTimeOffset(DateTime.UtcNow);
            await MessageHandler.SendChannel(Context.Channel,offset.ToString());
        }
        [Command("time", RunMode = RunMode.Async), Summary("Gets time from utc with offset")]
        public async Task Time(int offset)
        {
            double ofs = Convert.ToDouble(offset);
            TimeSpan ts = new TimeSpan();
            var nts = ts.Add(TimeSpan.FromHours(ofs));
            DateTimeOffset DTOffset = new DateTimeOffset(DateTime.UtcNow);
            var ndtof = DTOffset.ToOffset(nts);
            await MessageHandler.SendChannel(Context.Channel,ndtof.ToString());
        }

        [Command("roast", RunMode = RunMode.Async), Summary("\"Roasts\" a user, these are all taken as jokes, and aren't actually meant to cause harm.")]
        public async Task RoastCmd(IGuildUser user) =>
            await Roast(user);
        [Command("roastme", RunMode = RunMode.Async), Summary("\"Roast\" yourself, these are all taken as jokes, and aren't actually meant to cause harm.")]
        public async Task RoastYourselfCmd() =>
            await Roast((Context.User as IGuildUser));
        public async Task Roast(IGuildUser user) =>
            await MessageHandler.SendChannel(Context.Channel, user.Mention + " " + Roasts[Bot.random.Next(0, Roasts.Length)]);
        [Command("dadjoke", RunMode = RunMode.Async), Summary("Gives you a bad dad joke to facepalm at.")]
        public async Task DadJoke()
        {
            int index = Bot.random.Next(0, DadJokes.GetLength(0));
            string joke = DadJokes[index, 0];
            string punchline = DadJokes[index, 1]??"";
            await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder()
            {
                Title = joke,
                Description = punchline,
                Color = RandColor.RandomColor()
            });
        }
        [Command("pickup",RunMode = RunMode.Async),Summary("Cringe at these bad user-submitted pick up lines. (Don't actually use these or else you'll get laughed at. :3)"),Alias("pickupline")]
        public async Task PickUp()
        {
            int index = Bot.random.Next(0, PickUpLines.GetLength(0));
            index = Bot.random.Next(0, PickUpLines.GetLength(0));
            string part1 = PickUpLines[index, 0];
            string part2 = PickUpLines[index, 1];
            await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder()
            {
                Title = part1,
                Description = part2??"",
                Color = RandColor.RandomColor()
            });
        }
        [Command("cowsay",RunMode = RunMode.Async),Summary("Make an ascii cow say some things.")]
        public async Task CowSay([Remainder]string message)
        {
            string cowdir = Path.Combine(AppContext.BaseDirectory, "skuld", "storage", "cows");
            string[] cows = Directory.GetFiles(cowdir);
            string cow = null;
            if (message.StartsWith("-b"))
            { 
                cow = GetCow.ReturnCow(cowdir + "\\default.cow", false, new CowFace("=="));
                message = message.Remove(0, 3);
            }
            if (message.StartsWith("-d"))
            {
                cow = GetCow.ReturnCow(cowdir + "\\default.cow", false, new CowFace("XX"));
                message = message.Remove(0, 3);
            }                    
            if (message.StartsWith("-g"))
            {
                cow = GetCow.ReturnCow(cowdir + "\\default.cow", false, new CowFace("$$"));
                message = message.Remove(0, 3);
            }                    
            if (message.StartsWith("-p"))
            {
                cow = GetCow.ReturnCow(cowdir + "\\default.cow", false, new CowFace("@@"));
                message = message.Remove(0, 3);
            }                    
            if (message.StartsWith("-s"))
            {
                cow = GetCow.ReturnCow(cowdir + "\\default.cow", false, new CowFace("**"));
                message = message.Remove(0, 3);
            }
            if (message.StartsWith("--think"))
            {
                cow = GetCow.ReturnCow(cowdir + "\\default.cow", true, new CowFace("oo"));
                message = message.Remove(0, 8);
            }
            if (message.StartsWith("-t"))
            {
                cow = GetCow.ReturnCow(cowdir + "\\default.cow", false, new CowFace("--"));
                message = message.Remove(0, 3);
            }                    
            if (message.StartsWith("-w"))
            {
                cow = GetCow.ReturnCow(cowdir + "\\default.cow", false, new CowFace("00"));
                message = message.Remove(0, 3);
            }                    
            if (message.StartsWith("-y"))
            {
                cow = GetCow.ReturnCow(cowdir + "\\default.cow", false, new CowFace(".."));
                message = message.Remove(0, 3);
            }                    
            if (message.StartsWith("-T"))
            {
                cow = GetCow.ReturnCow(cowdir + "\\default.cow", false, new CowFace("oo", message.Split(null)[1]));
                message = message.Remove(0, message.Split(null)[1].Length+4);
            }                    
            if (message.StartsWith("-e"))
            {
                cow = GetCow.ReturnCow(cowdir + "\\default.cow", false, new CowFace(message.Split(null)[1]));
                message = message.Remove(0, message.Split(null)[1].Length+4);
            }                    
            if (message.StartsWith("-f"))
            {
                string cowfile = message.Split(null)[1] + ".cow";
                if (cows.Contains(cowdir+"\\"+cowfile))
                {
                    cow = GetCow.ReturnCow(cowdir + "\\" +cowfile, false, new CowFace());
                    message = message.Remove(0, message.Split(null)[1].Length+4);
                }
                else
                {
                    throw new FileNotFoundException("Cannot find cow \"" + cowfile + "\" in \"" + cowdir + "\"");
                }
            }
            string cowsay = "```\n"+message+"\n";
            if (!String.IsNullOrEmpty(cow))
                cowsay += cow + "```";
            else
                cowsay += GetCow.ReturnCow(cowdir+"\\default.cow", false, new CowFace("oo")) + "```";
            if (cowsay.Length > 2000)
            {
                cowsay = cowsay.Remove(0, 3);
                cowsay = cowsay.Remove(cowsay.Length - 3, 3);
                File.WriteAllText(AppContext.BaseDirectory + "\\cowsay.txt", cowsay);
                await Context.Channel.SendFileAsync(AppContext.BaseDirectory + "\\cowsay.txt", "It's over 2000 characters D: Here's the file.");
            }
            else
                await MessageHandler.SendChannel(Context.Channel, cowsay);
        }        

        [Command("apod", RunMode = RunMode.Async), Summary("Gets NASA's \"Astronomy Picture of the Day\"")]
        public async Task APOD()
        {
            var PictureOfTheDay = await APIWebReq.NasaAPOD();
            var embed = new EmbedBuilder()
            {
                Color = RandColor.RandomColor(),
                Title = PictureOfTheDay.Title,
                Url = "https://apod.nasa.gov/",
                ImageUrl = PictureOfTheDay.HDUrl,
                Timestamp = Convert.ToDateTime(PictureOfTheDay.Date)
            };
            await MessageHandler.SendChannel(Context.Channel, "", embed);
        }
        [Command("choose", RunMode = RunMode.Async), Summary("Choose from things")]
        public async Task Choose([Remainder]string choices)
        {
            var choicearr = choices.Split('|');
            await MessageHandler.SendChannel(Context.Channel, $"<:blobthinkcool:350673773113901056> | __{(Context.User as IGuildUser).Nickname??Context.User.Username}__ I choose: **{choicearr[Bot.random.Next(0,choicearr.Length)]}**");
        }
    }
}
