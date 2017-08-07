using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using Skuld.Models.API;
using Skuld.Tools;
using Discord;
using Skuld.Models;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using MySql.Data.MySqlClient;

namespace Skuld.Fun
{
    [Group,Name("Fun")]
    public class Fun : ModuleBase
    {
        static string[] eightball = { "It is certain", "It is decidedly so", "Without a doubt", "Yes, definitely", "You may rely on it", "As I see it, yes", "Most likely", "Outlook good", "Yes", "Signs point to yes", "Reply hazy, try again", "Ask again later", "Better not tell you now", "Cannot predict now", "Concentrate and ask again", "Don't count on it", "My reply is no", "My sources say no", "Outlook not so good", "Very doubtful" };

        [Command("neko", RunMode = RunMode.Async),Summary("neko grill")]
        public async Task Neko()
        {
            var rawresp = await APIS.APIWebReq.ReturnString(new Uri("https://nekos.life/api/neko"));
            JObject jsonresp = JObject.Parse(rawresp);
            dynamic item = jsonresp;
            if (item["neko"].ToString() != null)
            {
                var neko = item["neko"];
                await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Title = neko, ImageUrl = neko });
            }
        }

        [Command("kitty", RunMode = RunMode.Async), Summary("kitty")]
        [Alias("cat", "cats", "kittycat", "kitty cat", "meow", "kitties", "kittys")]
        public async Task Kitty() { var kitty = await APIS.Kitty.WebReq.GetKitty(); await MessageHandler.SendChannel(Context.Channel,"", new EmbedBuilder() { Color = RandColor.RandomColor(), Title = kitty.ImageURL, ImageUrl = kitty.ImageURL }); }
        [Command("doggo", RunMode = RunMode.Async), Summary("doggo")]
        [Alias("dog", "dogs", "doggy")]
        public async Task Doggo() { var doggo = await APIS.Doggo.WebReq.GetDoggo(); await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Color = RandColor.RandomColor(), Title = doggo.ImageURL, ImageUrl = doggo.ImageURL }); }
        
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
                    Double currluckfact= Convert.ToDouble(await Sql.GetSingleAsync(cmd));
                    currluckfact = currluckfact / 1.1;
                    currluckfact = Math.Round(currluckfact, 4);
                    if (currluckfact < 0.1)
                        currluckfact = 0.1;
                    cmd = new MySqlCommand("UPDATE accounts SET luckfactor = @luckfactor where ID = @userid");
                    cmd.Parameters.AddWithValue("@userid", Context.User.Id);
                    cmd.Parameters.AddWithValue("@luckfactor", currluckfact);
                    await Sql.InsertAsync(cmd);
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
            var Pasta = new Pasta();
            var command = new MySqlCommand("SELECT * FROM pasta WHERE pastaname = @pastaname");
            command.Parameters.AddWithValue("@pastaname", title);
            var reader = await Sql.GetAsync(command);
            while (reader.Read())
            {
                Pasta.PastaName = reader["pastaname"].ToString();
                Pasta.Content = reader["content"].ToString();
                Pasta.Username = reader["username"].ToString();
                Pasta.Created = reader["created"].ToString();
                Pasta.Upvotes = Convert.ToUInt32(reader["upvotes"].ToString());
                Pasta.Downvotes = Convert.ToUInt32(reader["downvotes"].ToString());
                Pasta.OwnerID = Convert.ToUInt64(reader["ownerid"].ToString());
            }
            reader.Close();
            await Sql.getconn.CloseAsync();
            if (Pasta != null)
            {
                if (cmd == "who" || cmd == "?")
                {
                    Console.WriteLine(Context.User.Id);
                    Console.WriteLine(Pasta.OwnerID);
                    EmbedBuilder _embed = new EmbedBuilder();
                    _embed.Color = RandColor.RandomColor();
                    _embed.Title = Pasta.PastaName;
                    _embed.AddField(x =>
                    {
                        x.Name = "Author";
                        x.Value = Pasta.Username;
                    });
                    _embed.AddField(x =>
                    {
                        x.Name = "Created";
                        x.Value = Pasta.Created;
                    });
                    _embed.AddField(x =>
                    {
                        x.Name = "UpVotes";
                        x.Value = Pasta.Upvotes;
                    });
                    _embed.AddField(x =>
                    {
                        x.Name = "DownVotes";
                        x.Value = Pasta.Downvotes;
                    });
                    await MessageHandler.SendChannel(Context.Channel, "", _embed);
                }
                if (cmd == "upvote")
                {
                    command = new MySqlCommand("SELECT upvotes FROM pasta WHERE pastaname = @title");
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
                        await MessageHandler.SendChannel(Context.Channel, $"Upvote added to **{title}** it currently has: {upvotes} Upvotes");
                }
                if (cmd == "downvote")
                {
                    command = new MySqlCommand("SELECT downvotes FROM pasta WHERE pastaname = @title");
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
                        await MessageHandler.SendChannel(Context.Channel, $"Downvote added to **{title}** it currently has: {downvotes} Downvote");
                }
                if (cmd == "delete")
                {
                    if(Convert.ToUInt64(Pasta.OwnerID) == Context.User.Id)
                    {
                        command = new MySqlCommand("DELETE FROM pasta WHERE pastaname = @title");
                        command.Parameters.AddWithValue("@title", title);
                        await Sql.InsertAsync(command).ContinueWith(async x =>
                        {
                            Console.WriteLine(x);
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
            if (cmd == "new" || cmd == "+")
            {
                if (title == "list")
                {
                    await MessageHandler.SendChannel(Context.Channel, "Nope");
                }
                else
                {
                    var command = new MySqlCommand("SELECT pastaname FROM pasta WHERE pastaname = @pastatitle");
                    command.Parameters.AddWithValue("@pastatitle", title);
                    var pastaname = await Sql.GetSingleAsync(command);
                    if (!string.IsNullOrEmpty(pastaname))
                    {
                        await MessageHandler.SendChannel(Context.Channel, $"Pasta already exists with name: **{title}**");
                    }
                    else
                    {
                        content = content.Replace("\'", "\\\'");
                        content = content.Replace("\"", "\\\"");

                        command = new MySqlCommand("INSERT INTO pasta (content,ownerid,created,username,pastaname) VALUES ( @content , @ownerid , @created , @username , @pastatitle )");
                        command.Parameters.AddWithValue("@content", content);
                        command.Parameters.AddWithValue("@ownerid", Context.User);
                        command.Parameters.AddWithValue("@created", DateTime.UtcNow);
                        command.Parameters.AddWithValue("@username", Context.User.Username.Replace("\"", "\\\"").Replace("\'", "\\'") + "#" + Context.User.DiscriminatorValue);
                        command.Parameters.AddWithValue("@pastatitle", title);

                        await Sql.InsertAsync(command).ContinueWith(async x =>
                        {
                            command = new MySqlCommand("SELECT pastaname FROM pasta WHERE pastaname = @pastatitle");
                            command.Parameters.AddWithValue("@pastatitle", title);
                            var newpasta = await Sql.GetSingleAsync(command);
                            if (x.IsCompleted && !string.IsNullOrEmpty(newpasta))
                            {
                                await MessageHandler.SendChannel(Context.Channel, $"Successfully added: **{title}**");
                            }
                        });
                    }
                }
            }
            if (cmd == "edit" || cmd == "change")
            {
                var command = new MySqlCommand("SELECT ownerid from pasta where pastaname = @title");
                command.Parameters.AddWithValue("@title", title);
                var ownerid = await Sql.GetSingleAsync(command);
                if (Convert.ToUInt64(ownerid) == Context.User.Id)
                {
                    command = new MySqlCommand("SELECT content FROM pasta where pastaname = @title");
                    command.Parameters.AddWithValue("@title", title);
                    var oldcontent = await Sql.GetSingleAsync(command);
                    content.Replace("\'", "\\\'");
                    content.Replace("\"", "\\\"");
                    command = new MySqlCommand("UPDATE pasta SET content = @content WHERE pastaname = @title");
                    command.Parameters.AddWithValue("@content", content);
                    command.Parameters.AddWithValue("@title", title);
                    await Sql.InsertAsync(command).ContinueWith(async x =>
                    {
                        command = new MySqlCommand("SELECT content FROM pasta where pastaname = @title");
                        command.Parameters.AddWithValue("@title", title);
                        var respnew = await Sql.GetSingleAsync(command);
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
                var reader = await Sql.GetAsync(new MySqlCommand($"SELECT PastaName FROM pasta"));
                while (await reader.ReadAsync())
                {
                    rows.Add(reader["PastaName"].ToString());
                }
                reader.Close();
                await Sql.getconn.CloseAsync();
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
            else
            {
                var command = new MySqlCommand("SELECT content FROM pasta WHERE pastaname = @title");
                command.Parameters.AddWithValue("@title", title);
                string response = await Sql.GetSingleAsync(command);
                if (!String.IsNullOrEmpty(response))
                    await MessageHandler.SendChannel(Context.Channel, response);
                else
                    await MessageHandler.SendChannel(Context.Channel, $"Whoops, `{title}` doesn't exist");
            }
        }
        [Command("pasta", RunMode = RunMode.Async), Summary("Pastas are nice")]
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
                    command = new MySqlCommand("UPDATE pasta SET ownerid = {user.Id}, username = \'{}\' WHERE ownerid = {} AND pastaname = \'{title}\'");
                    command.Parameters.AddWithValue("@ownerid", user.Id);
                    command.Parameters.AddWithValue("@username", user.Username.Replace("\"", "\\\"").Replace("\'", "\\'")+"#"+user.Discriminator);
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
        }

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
            var poll = await APIS.Strawpoll.WebReq.SendPoll(Title, options);            
            await MessageHandler.SendChannel(Context.Channel,$"Strawpoll **{Title}** has been created, here's the link: {poll.Url}");
        }
        [Command("strawpoll", RunMode = RunMode.Async), Summary("Gets a strawpoll")]
        public async Task StrawpollGet(int id)
        {
            var poll = await APIS.Strawpoll.WebReq.GetPoll(id);
            EmbedBuilder _embed = new EmbedBuilder();
            EmbedAuthorBuilder _author = new EmbedAuthorBuilder();
            _embed.Color = RandColor.RandomColor();
            _author.Name = poll.Title;
            _author.Url = poll.Url;
            _embed.Author = _author;
            EmbedFooterBuilder _footer = new EmbedFooterBuilder();
            _footer.Text = "Strawpoll ID: " + poll.ID;
            _embed.Timestamp = DateTime.UtcNow;
            _embed.Footer = _footer;

            for (int z = 0; z < poll.Options.Length; z++)
            {
                _embed.AddField(x =>
                {
                    x.Name = poll.Options[z];
                    x.Value = poll.Votes[z];
                });
            }

            await MessageHandler.SendChannel(Context.Channel,"", _embed);
        }
        [Command("strawpoll", RunMode = RunMode.Async), Summary("Gets a strawpoll")]
        public async Task StrawpollGet(string url)
        {
            var poll = await APIS.Strawpoll.WebReq.GetPoll(url);
            EmbedBuilder _embed = new EmbedBuilder();
            EmbedAuthorBuilder _author = new EmbedAuthorBuilder();
            _embed.Color = RandColor.RandomColor();
            _author.Name = poll.Title;
            _author.Url = poll.Url;
            _embed.Author = _author;
            EmbedFooterBuilder _footer = new EmbedFooterBuilder();
            _footer.Text = "Strawpoll ID: " + poll.ID;
            _embed.Timestamp = DateTime.UtcNow;
            _embed.Footer = _footer;

            for (int z = 0; z < poll.Options.Length; z++)
            {
                _embed.AddField(x =>
                {
                    x.Name = poll.Options[z];
                    x.Value = poll.Votes[z];
                });
            }

            await MessageHandler.SendChannel(Context.Channel, "", _embed);
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
            try
            {
                int randComic = 0;
                await APIS.XKCD.WebReq.GetLastPage();
                for (int x = 0; x < 10; x++)
                {
                    randComic = Bot.random.Next(0, APIS.XKCD.WebReq.LastPage.Value);
                }
                await sendXKCD((await APIS.XKCD.WebReq.GetComic(randComic)));
            }
            catch(Exception ex)
            {
            }
        }
        public async Task sendXKCD(XKCDComic comic)
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
    }
}
