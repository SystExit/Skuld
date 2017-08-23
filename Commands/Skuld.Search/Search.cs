using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.Linq;
using Skuld.Models.API;
using Skuld.APIS;
using Skuld.Tools;
using PokeSharp.Models;
using PokeSharp.Deserializer;
using Skuld.Models.API.MAL;
using System.Timers;
using System.Text;
using Google.Apis.Customsearch.v1;
using YoutubeSearch;
using Google.Apis.Customsearch.v1.Data;
using System.Collections.Generic;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;

namespace Skuld.Commands
{
    [Group, Name("Search")]
    public class Search : ModuleBase
    {
        /*Commented out due to potentiality of coming back to it.
        [Command("tvshow"), Summary("Gets tv show from imdb")]
        [Alias("tv")]
        public async Task Tvshow([Remainder]string tvshow)
        {
            string file = AppContext.BaseDirectory + "/storage/tvshows/" + tvshow + ".json";
            if (!File.Exists(file))
            {
                string url = $"http://www.omdbapi.com/?t={tvshow}&y=&plot=short&r=json";
                url = url.Replace("'", "%27");
                url = url.Replace(" ", "%20");

                var rawresp = await APIWebReq.ReturnString(new Uri(url));
                if (Directory.Exists(AppContext.BaseDirectory + "/storage/tvshows/") == false)
                    Directory.CreateDirectory(AppContext.BaseDirectory + "/storage/tvshows/");
                File.WriteAllText(file, rawresp);

                JObject jsonresp = JObject.Parse(rawresp);
                dynamic item = jsonresp;
                GetIMDB(item);
            }
            else
            {
                JObject jsonresp = JObject.Parse(File.ReadAllText(file));
                dynamic item = jsonresp;
                GetIMDB(item);
            }
        }
        [Command("movie"), Summary("Gets movie from imdb")]
        public async Task Movies([Remainder]string movie)
        {
            string file = AppContext.BaseDirectory + "/storage/movies/" + movie + ".json";
            if (!File.Exists(file))
            {
                string url = $"http://www.omdbapi.com/?t={movie}&y=&plot=short&r=json";
                url = url.Replace("'", "%27");
                url = url.Replace(" ", "%20");

                var rawresp = await APIWebReq.ReturnString(new Uri(url));
                if (Directory.Exists(AppContext.BaseDirectory + "/storage/movies/") == false)
                {
                    Directory.CreateDirectory(AppContext.BaseDirectory + "/storage/movies/");
                }
                File.WriteAllText(file, rawresp);

                JObject jsonresp = JObject.Parse(rawresp);
                dynamic item = jsonresp;
                GetIMDB(item);
            }
            else
            {
                JObject jsonresp = JObject.Parse(File.ReadAllText(file));
                dynamic item = jsonresp;
                GetIMDB(item);
            }
        }
        public async Task GetIMDB(dynamic item)
        {
            if (item["Type"].ToString().ToLower() == "series")
            {

                IMDb media = new IMDb(item["Awards"].ToString(), item["Country"].ToString(), item["Genre"].ToString(), item["imdbID"].ToString(), item["imdbRating"].ToString(), item["imdbVotes"].ToString(), item["Language"].ToString(), item["Plot"].ToString(), item["Poster"].ToString(), item["Released"].ToString(), item["Runtime"].ToString(), item["Title"].ToString(), item["totalSeasons"].ToString(), item["Type"].ToString(), item["Year"].ToString());
                EmbedBuilder _embed = new EmbedBuilder();
                EmbedAuthorBuilder _auth = new EmbedAuthorBuilder();
                _auth.Name = media.Title;
                _auth.Url = "http://www.imdb.com/title/" + media.imdbID;
                _embed.Author = _auth;
                _embed.ThumbnailUrl = media.Poster;
                _embed.AddInlineField("Plot", media.Plot);
                _embed.AddInlineField("Year", media.Year);
                _embed.AddInlineField("Released", media.Released);
                _embed.AddInlineField("Type", media.Type);
                _embed.AddInlineField("Seasons", media.totalSeasons);
                _embed.AddInlineField("Language", media.Language);
                _embed.AddInlineField("Awards", media.Awards);
                _embed.AddInlineField("Origin", media.Country);
                _embed.AddInlineField("Genre", media.Genre);
                _embed.AddInlineField("Rating", media.imdbRating);
                _embed.AddInlineField("Votes", media.imdbVotes);
                await MessageHandler.SendChannel(Context.Channel,"", _embed);
            }
            else
            {

                IMDb media = new IMDb(item["Awards"].ToString(), item["Country"].ToString(), item["Genre"].ToString(), item["imdbID"].ToString(), item["imdbRating"].ToString(), item["imdbVotes"].ToString(), item["Language"].ToString(), item["Plot"].ToString(), item["Poster"].ToString(), item["Released"].ToString(), item["Runtime"].ToString(), item["Title"].ToString(), null, item["Type"].ToString(), item["Year"].ToString());
                EmbedBuilder _embed = new EmbedBuilder();
                EmbedAuthorBuilder _auth = new EmbedAuthorBuilder();
                _auth.Name = media.Title;
                _auth.Url = "http://www.imdb.com/title/" + media.imdbID;
                _embed.Author = _auth;
                _embed.ThumbnailUrl = media.Poster;
                _embed.AddInlineField("Plot", media.Plot);
                _embed.AddInlineField("Year", media.Year);
                _embed.AddInlineField("Released", media.Released);
                _embed.AddInlineField("Type", media.Type);
                _embed.AddInlineField("Language", media.Language);
                _embed.AddInlineField("Awards", media.Awards);
                _embed.AddInlineField("Origin", media.Country);
                _embed.AddInlineField("Genre", media.Genre);
                _embed.AddInlineField("Rating", media.imdbRating);
                _embed.AddInlineField("Votes", media.imdbVotes);
                await MessageHandler.SendChannel(Context.Channel,"", _embed);
            }
        }*/
        [Command("twitch", RunMode = RunMode.Async), Summary("Finds a twitch user")]
        public async Task TwitchSearch([Remainder]string TwitchStreamer)
        {
            var twicli = Bot.NTwitchClient;
            var users = await twicli.GetUsersAsync(TwitchStreamer);
            var chan = await users.FirstOrDefault().GetChannelAsync();
            var stream = await chan.GetStreamAsync();
            EmbedBuilder _embed = new EmbedBuilder();
            _embed.Color = RandColor.RandomColor();
            if (stream != null)
            {
                _embed.Author = new EmbedAuthorBuilder()
                {
                    Name = chan.DisplayName,
                    IconUrl = chan.LogoUrl,
                    Url = chan.Url
                };
                var streamimg = stream.Previews.SkipWhile(x => x.Key != "large").FirstOrDefault().Value;
                _embed.Description = chan.Status;
                _embed.AddInlineField("They're currently playing", chan.Game.ToString());
                _embed.AddInlineField("Current Viewers", $"{stream.Viewers.ToString("N0")}");
                _embed.ImageUrl = streamimg;
            }
            if (stream == null)
            {
                _embed.Author = new EmbedAuthorBuilder()
                {
                    Name = chan.DisplayName,
                    IconUrl = chan.LogoUrl,
                    Url = chan.Url
                };
                if (!String.IsNullOrEmpty(chan.Status))
                    _embed.AddInlineField("Last stream title", chan.Status);
                else
                    _embed.AddInlineField("Last stream title", "Unset");
                if (!String.IsNullOrEmpty(chan.Game))
                    _embed.AddInlineField("Was last streaming", chan.Game);
                else
                    _embed.AddInlineField("Was last streaming", "Nothing");
                _embed.AddInlineField("Followers", $"{chan.Followers.ToString("N0")}");
                _embed.AddInlineField("Total Views", $"{chan.Views.ToString("N0")}");
                _embed.ThumbnailUrl = chan.VideoBannerUrl;
            }
            await MessageHandler.SendChannel(Context.Channel, "", _embed);
        }

        //Start Search Platforms
        [Command("search", RunMode = RunMode.Async), Summary("Gets the first search on a google search"), Alias("s")]
        public async Task GetSearch(string platform, [Remainder]string query)
        {
            platform = platform.ToLowerInvariant();
            if (platform == "google" || platform == "g")
            {
                await GoogleS(query);
            }
            if (platform == "youtube" || platform == "yt")
            {
                await YoutubeS(query);
            }
            if (platform == "imgur" || platform == "image")
            {
                await ImgurS(query);
            }
        }

        private async Task GoogleS(string query)
        {
            try
            {
                CustomsearchService css = new CustomsearchService(new Google.Apis.Services.BaseClientService.Initializer() { ApiKey = Config.Load().GoogleAPI, ApplicationName = "Skuld" });
                CseResource.ListRequest listRequest = css.Cse.List(query);
                listRequest.Cx = Config.Load().GoogleCx;
                listRequest.Safe = CseResource.ListRequest.SafeEnum.High;
                Google.Apis.Customsearch.v1.Data.Search search = await listRequest.ExecuteAsync();
                List<Result> items = search.Items as List<Result>;
                if (items != null)
                {
                    var item = items.FirstOrDefault();
                    var item2 = items.ElementAtOrDefault(1);
                    var item3 = items.ElementAtOrDefault(2);
                    EmbedBuilder embed = null;
                    try
                    {
                        embed = new EmbedBuilder()
                        {
                            Author = new EmbedAuthorBuilder()
                            {
                                Name = $"Google search for: {query}",
                                IconUrl = "https://www.google.com/favicon.ico",
                                Url = $"https://google.com/search?q={query.Replace(" ", "%20")}"
                            },
                            Description = "I found this:\n" +
                                $"**{item.Title}**\n" +
                                $"{item.Link}\n\n" +
                                "__**Also Relevant**__\n"+
                                $"**{item2.Title}**\n{item2.Link}\n\n"+
                                $"**{item3.Title}**\n{item3.Link}\n\n"+
                                "If I didn't find what you're looking for, use this link:\n" +
                                $"https://google.com/search?q={query.Replace(" ", "%20")}",
                            Color = RandColor.RandomColor()
                        };
                        await MessageHandler.SendChannel(Context.Channel, "", embed);
                    }
                    catch
                    {
                    }
                }
                else
                {
                    await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder()
                    {
                        Title = "Error with the command",
                        Description = $"I couldn't find anything matching: `{query}`, please try again.",
                        Color = RandColor.RandomColor()
                    });
                }
            }
            catch (Exception ex)
            {
                Bot.Logs.Add(new Models.LogMessage("GogSrch", "Error with google search", LogSeverity.Error, ex));
                await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Title = "Error with the command", Color = RandColor.RandomColor() });
            }
        }
        private async Task YoutubeS(string query)
        {
            try
            {
                VideoSearch search = new VideoSearch();
                var items = search.SearchQuery(query, 1);
                var item = items.FirstOrDefault();
                var embed = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = item.Author,
                        Url = item.Url,
                        IconUrl = "https://youtube.com/favicon.ico"
                    },
                    Title = $"{item.Title}",
                    Description = $"{item.Url}\nDuration: {item.Duration}",
                    ImageUrl = item.Thumbnail,
                    Color = RandColor.RandomColor()
                };
                await MessageHandler.SendChannel(Context.Channel, "", embed);
            }
            catch (Exception ex)
            {
                Bot.Logs.Add(new Models.LogMessage("YTBSrch", "Error with Youtube Search", LogSeverity.Error, ex));
                await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Title = "Error with the command", Description = ex.Message, Color = RandColor.RandomColor() });
            }
        }
        private async Task ImgurS(string query)
        {
            try
            {
                ImgurClient client = new ImgurClient(Config.Load().ImgurClientID, Config.Load().ImgurClientSecret);
                var endpoint = new GalleryEndpoint(client);
                var images = await endpoint.SearchGalleryAsync(query);
                var albm = images.ElementAtOrDefault(Bot.random.Next(0,images.Count()));
                var album = albm as IGalleryAlbum;
                if (album != null)
                {
                    if (album.Nsfw != true)
                    {
                        string message = "I found this:\n" + album.Link;
                        await MessageHandler.SendChannel(Context.Channel, message);
                    }
                    if (album.Nsfw == true && Context.Channel.IsNsfw == true)
                    {
                        string message = "I found this:\n" + album.Link;
                        await MessageHandler.SendChannel(Context.Channel, message);
                    }
                }
                else
                    await MessageHandler.SendChannel(Context.Channel, "I found nothing sorry. :/");
            }
            catch (Exception ex)
            {
                Bot.Logs.Add(new Models.LogMessage("ImgrSch", "Error with Imgur search", LogSeverity.Error, ex));
                await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder() { Title = "Error with the command", Description = ex.Message, Color = RandColor.RandomColor() });
            }
        }

        public string urbanphrase = null;
        [Command("urban", RunMode = RunMode.Async), Summary("Gets a thing from urban dictionary")]
        public async Task Urban([Remainder]string phrase) =>
            await Geturban(new Uri($"http://api.urbandictionary.com/v0/define?term={phrase}"));
        [Command("urban", RunMode = RunMode.Async), Summary("Gets a random thing from urban dictionary")]
        public async Task Urban() => 
            await Geturban(new Uri("http://api.urbandictionary.com/v0/random"));
        private async Task Geturban(Uri url)
        {
            var rnd = Bot.random;
            var rawresp = await APIWebReq.ReturnString(url);
            //Converts to JSon
            JObject jsonresp = JObject.Parse(rawresp);
            JArray lists = (JArray)jsonresp["list"];
            dynamic item = lists[rnd.Next(0, lists.Count)];
            Urban word = new Urban(item["word"].ToString(),
                item["definition"].ToString(),
                item["permalink"].ToString(),
                item["example"].ToString(),
                item["author"].ToString(),
                item["thumbs_up"].ToString(),
                item["thumbs_down"].ToString());
            EmbedBuilder _embed = new EmbedBuilder();
            EmbedAuthorBuilder _auth = new EmbedAuthorBuilder();
            _embed.Color = RandColor.RandomColor();
            _auth.Name = word.Word;
            _auth.Url = word.PermaLink;
            _embed.Author = _auth;
            _embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Author";
                if (!String.IsNullOrEmpty(word.Author))
                    x.Value = word.Author;
            });
            _embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Definition";
                if (!String.IsNullOrEmpty(word.Definition))
                    x.Value = word.Definition;
            });
            _embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Example";
                if (!String.IsNullOrEmpty(word.Example))
                    x.Value = word.Example;
            });
            _embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Upvotes";
                if (!String.IsNullOrEmpty(word.UpVotes))
                    x.Value = word.UpVotes;
            });
            _embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Downvotes";
                if (!String.IsNullOrEmpty(word.DownVotes))
                    x.Value = word.DownVotes;
            });
            await MessageHandler.SendChannel(Context.Channel, "", _embed);
        }

        [Command("wikipedia", RunMode = RunMode.Async), Summary("Gets wikipedia information, supports all languages that wikipedia offers")]
        [Alias("wiki")]
        public async Task Wiki(string langcode, [Remainder]string query) => 
            await GetWiki(langcode, query);
        [Command("wikipedia", RunMode = RunMode.Async), Summary("Gets wikipedia information, supports all languages that wikipedia offers")]
        [Alias("wiki")]
        public async Task Wiki([Remainder]string query) => 
            await GetWiki("en", query);
        public async Task GetWiki(string langcode, string query)
        {
            JObject jsonresp = JObject.Parse((await APIWebReq.ReturnString(new Uri($"https://{langcode}.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exintro=&explaintext=&titles={query}"))));
            dynamic item = jsonresp["query"]["pages"].First.First;
            string Desc = Convert.ToString(item["extract"]);
            Wiki page = new Wiki();
            page.Name = item["title"].ToString();
            page.Description = Desc.Remove(500) + "...\nRead more at the article.";
            page.Url = $"https://{langcode}.wikipedia.org/wiki/{query}";
            var _embed = new EmbedBuilder();
            var _auth = new EmbedAuthorBuilder();
            _embed.Color = RandColor.RandomColor();

            _auth.Name = page.Name;
            _auth.Url = page.Url;
            _embed.Author = _auth;
            if (!String.IsNullOrEmpty(page.Description))
                _embed.AddInlineField("Description", page.Description);
            else { _embed.AddInlineField("Description", "N/A"); }
            await MessageHandler.SendChannel(Context.Channel, "", _embed);
        }

        [Command("pokemon", RunMode = RunMode.Async), Summary("Gets information about a pokemon id")]
        public async Task Getpokemon(string pokemon, string group)=>
            await SendPokemon(await WebReq.GetPocketMonster(pokemon.ToLowerInvariant()), group);
        [Command("pokemon", RunMode = RunMode.Async), Summary("Gets information about a pokemon id")]
        public async Task Getpokemon(string pokemon)=>
            await SendPokemon(await WebReq.GetPocketMonster(pokemon.ToLowerInvariant()),"default");
        [Command("pokemon", RunMode = RunMode.Async), Summary("Gets information about a pokemon id")]
        public async Task Getpokemon(int pokemonid, string group)=>
            await SendPokemon(await WebReq.GetPocketMonster(pokemonid), group);
        [Command("pokemon", RunMode = RunMode.Async), Summary("Gets information about a pokemon id")]
        public async Task Getpokemon(int pokemonid)=>
            await SendPokemon(await WebReq.GetPocketMonster(pokemonid), "default");
        public async Task SendPokemon(PocketMonster pokemon, string group)
        {
            EmbedBuilder embed;
            if (pokemon==null)
            {
                embed = new EmbedBuilder()
                {
                    Color = RandColor.RandomColor(),
                    Title = "Command Error!",
                    Description = "This pokemon doesn't exist. Please try again.\nIf it is a Generation 7, pokeapi.co hasn't updated for it yet."
                };
            }
            else
            {
                var rnd = Bot.random;
                group = group.ToLower();
                string sprite = null;
                //if it equals 8 out of a random integer between 1 and 8192 then give shiny
                if (rnd.Next(1, 8193) == 8)
                    sprite = pokemon.Sprites.FrontShiny;
                else
                    sprite = pokemon.Sprites.Front;
                embed = new EmbedBuilder();
                EmbedAuthorBuilder _auth = new EmbedAuthorBuilder();
                embed.Color = RandColor.RandomColor();
                if (group == "stat" || group == "stats")
                {
                    foreach (var stat in pokemon.Stats)
                    {
                        embed.AddInlineField(stat.Stat.Name, "Base Stat: " + stat.BaseStat);
                    }
                }
                if (group == "abilities" || group == "ability")
                {
                    foreach (var ability in pokemon.Abilities)
                    {
                        embed.AddInlineField(ability.Ability.Name, "Slot: " + ability.Slot);
                    }
                }
                if (group == "helditems" || group == "hitems" || group == "hitem" || group == "items")
                {
                    if (pokemon.HeldItems.Length > 0)
                    {
                        foreach (var hitem in pokemon.HeldItems)
                        {
                            foreach (var game in hitem.VersionDetails)
                                embed.AddInlineField("Item", hitem.Item.Name + "\n**Game**\n" + game.Version.Name + "\n**Rarity**\n" + game.Rarity);
                        }
                    }
                    else
                    {
                        embed.Description = "This pokemon doesn't hold any items in the wild";
                    }
                }
                if (group == "default")
                {
                    embed.AddInlineField("Height", pokemon.Height);
                    embed.AddInlineField("Weight", pokemon.Weight + "kg");
                    embed.AddInlineField("ID", pokemon.ID);
                    embed.AddInlineField("Base Experience", pokemon.BaseExperience);
                }
                if (group == "move" || group == "moves")
                {
                    var moves = pokemon.Moves.Take(4).Select(i => i).ToArray();
                    foreach (var move in moves)
                    {
                        string mve = move.Move.Name;
                        mve += "\n**Learned at:**\n" + "Level " + move.VersionGroupDetails.FirstOrDefault().LevelLearnedAt;
                        mve += "\n**Method:**\n" + move.VersionGroupDetails.FirstOrDefault().MoveLearnMethod.Name;
                        embed.AddInlineField("Move", mve);
                    }
                    _auth.Url = "https://bulbapedia.bulbagarden.net/wiki/" + pokemon.Name + "_(Pokémon)";
                    embed.Footer = new EmbedFooterBuilder() { Text = "Click the name to view more moves, I limited it to 4 to prevent a wall of text" };
                }
                if (group == "games" || group == "game")
                {
                    string games = null;
                    foreach (var game in pokemon.GameIndices)
                    {
                        games += game.Version.Name + "\n";
                        if (game == pokemon.GameIndices.Last())
                            games += game.Version.Name;
                    }
                    embed.AddInlineField("Game", games);
                }
                string name = pokemon.Name;
                _auth.Name = char.ToUpper(name[0]) + name.Substring(1);
                embed.Author = _auth;
                embed.ThumbnailUrl = sprite;
            }
            await MessageHandler.SendChannel(Context.Channel, "", embed);
        }

        [Command("gif", RunMode = RunMode.Async), Summary("Gets a gif")]
        public async Task Gifcommand([Remainder]string query)
        {
            var rnd = Bot.random;
            var embed = new EmbedBuilder();
            embed.Color = RandColor.RandomColor();
            embed.Author = new EmbedAuthorBuilder()
            {
                Name = "Giphy",
                IconUrl = "https://giphy.com/favicon.ico",
                Url = "https://giphy.com/"
            };
            try
            {
                query = query.Replace(" ", "%20");
                var rawresp = await APIWebReq.ReturnString(new Uri($"https://api.giphy.com/v1/gifs/search?q={query}&api_key=dc6zaTOxFJmzC"));
                JObject jsonresp = JObject.Parse(rawresp);
                JArray photo = (JArray)jsonresp["data"];
                dynamic item = photo[rnd.Next(0, photo.Count)];
                var gif = new Gif(item["id"].ToString());
                embed.Url = gif.Url;
                embed.ImageUrl = gif.Url;
                await MessageHandler.SendChannel(Context.Channel, "", embed);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                    await MessageHandler.SendChannel(Context.Channel, Context.User.Mention + " No results found for: `" + query + "`");
            }
        }

        [Command("weebgif", RunMode = RunMode.Async), Summary("Gets a weeb gif")]
        public async Task WeebGif()
        {
            var gif = await APIWebReq.ReturnString(new Uri("http://lucoa.systemexit.co.uk/gifs/reactions/"));
            await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder()
            {
                Title = gif,
                ImageUrl = gif,
                Color = RandColor.RandomColor()
            });
        }

        [Command("define", RunMode = RunMode.Async), Summary("Defines a word")]
        public async Task Define([Remainder]string word)
        {
            var stringifiedxml = await APIWebReq.ReturnString(new Uri($"http://www.stands4.com/services/v2/defs.php?uid={Config.Load().STANDSUid}&tokenid={Config.Load().STANDSToken}&word={word}"));
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(stringifiedxml);
            XObject XNode = XDocument.Parse(xml.InnerXml);
            JObject jobject = JObject.Parse(JsonConvert.SerializeXNode(XNode));
            dynamic item = jobject["results"]["result"].First();
            Define definedword = new Define(word,
                item["definition"].ToString(),
                item["example"].ToString(),
                item["partofspeech"].ToString(),
                item["term"].ToString());
            EmbedBuilder _embed = new EmbedBuilder();
            EmbedAuthorBuilder _auth = new EmbedAuthorBuilder();
            _embed.Color = RandColor.RandomColor();
            _auth.Name = definedword.Word;

            _embed.AddField("Definition",definedword.Definition??"None Available");
            _embed.AddField("Example",definedword.Example??"Not Available");
            _embed.AddInlineField("Part of speech",definedword.PartOfSpeech??"Not Available");
            _embed.AddInlineField("Terms", definedword.Terms??"Not Available");

            _embed.Author = _auth;
            await MessageHandler.SendChannel(Context.Channel, "", _embed);
        }

        //Coming back to soon, will fix issues
        private MangaArr MangaArray;
        private Timer MangaTimer = new Timer(60000);
        [Command("manga", RunMode = RunMode.Async), Summary("Gets a manga from MyAnimeList.Net")]
        public async Task MangaGet([Remainder]string mangatitle)
        {
            mangatitle = mangatitle.Replace(" ", "+");
            var byteArray = new UTF8Encoding().GetBytes($"{Config.Load().MALUName}:{Config.Load().MALPassword}");
            var stringifiedxml = await APIWebReq.ReturnString(new Uri($"https://myanimelist.net/api/manga/search.xml?q={mangatitle}"), byteArray);
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(stringifiedxml);
            XObject XNode = XDocument.Parse(xml.FirstChild.NextSibling.OuterXml);
            MangaArray = JsonConvert.DeserializeObject<MangaArr>(JObject.Parse(JsonConvert.SerializeXNode(XNode))["manga"].ToString());
            int manganode = MangaArray.Entry.Count;
            if (manganode > 1)
            {
                string entries = "__Make your selection__\n";
                int count = 0;
                foreach (var item in MangaArray.Entry)
                {
                    count++;
                    entries += $"**{count}. {item.Title}**\n";
                }
                await MessageHandler.SendChannel(Context.Channel, entries);
                Bot.bot.MessageReceived += _MessageReceivedManga;
                MangaTimer.Elapsed += Timer_ElapsedManga;
                MangaTimer.Start();
            }
            else
            {
                await GetMangaAtZeroNonSearch();
            }
        }
        private void Timer_ElapsedManga(object sender, ElapsedEventArgs e)
        {
            Bot.bot.MessageReceived -= _MessageReceivedManga;
        }
        private async Task _MessageReceivedManga(Discord.WebSocket.SocketMessage arg)
        {
            if (Context.User.Id == arg.Author.Id)
            {
                MangaTimer.Stop();
                await GetMangaAtPosition(Convert.ToInt32(arg.Content));
            }
        }
        private async Task GetMangaAtZeroNonSearch()
        {

            await SendManga(MangaArray.Entry.First());
        }
        private async Task GetMangaAtPosition(int position)
        {
            Bot.bot.MessageReceived -= _MessageReceivedManga;
            position = position - 1;
            await SendManga(MangaArray.Entry.ElementAt(position));
        }
        public async Task SendManga(Manga mango)
        {
            try
            {
                EmbedBuilder _embed = new EmbedBuilder();
                EmbedAuthorBuilder _auth = new EmbedAuthorBuilder();
                _embed.Color = RandColor.RandomColor();
                _auth.Url = $"https://myanimelist.net/manga/{mango.Id}/";

                _embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "English Title";
                    if (!String.IsNullOrEmpty(mango.EnglishTitle))
                        x.Value = mango.EnglishTitle;
                    else
                        x.Value = "None Exist";
                });
                _embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Synonyms";
                    if (!String.IsNullOrEmpty(mango.Synonyms))
                        x.Value = mango.Synonyms;
                    else
                        x.Value = "None Exist";
                });
                _embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Synopsis";
                    if (!String.IsNullOrEmpty(mango.Synopsis))
                        x.Value = mango.Synopsis.Split('<')[0];
                    else
                        x.Value = "None Exist";
                });
                _embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Chapters";
                    if (!String.IsNullOrEmpty(mango.Chapters))
                        x.Value = mango.Chapters;
                    else
                        x.Value = "None Exist";
                });
                _embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Volumes";
                    if (!String.IsNullOrEmpty(mango.Volumes))
                        x.Value = mango.Volumes;
                    else
                        x.Value = "None Exist";
                });
                _embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Started";
                    if (!String.IsNullOrEmpty(mango.StartDate))
                        x.Value = mango.StartDate;
                    else
                        x.Value = "None Exist";
                });
                _embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Ended";
                    if (!String.IsNullOrEmpty(mango.EndDate))
                        x.Value = mango.EndDate;
                    else
                        x.Value = "None Exist";
                });
                _embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Score";
                    x.Value = mango.Score;
                });

                _auth.Name = mango.Title;
                _embed.Author = _auth;
                _embed.ThumbnailUrl = mango.Image;
                await MessageHandler.SendChannel(Context.Channel, "", _embed);
            }
            catch(Exception ex)
            {
            }

        }

        private AnimeArr AnimeArray;
        private Timer AnimeTimer = new Timer(60000);
        [Command("anime", RunMode = RunMode.Async), Summary("Gets an anime from MyAnimeList.Net")]
        public async Task Animuget([Remainder]string animetitle)
        {
            animetitle = animetitle.Replace(" ", "+");
            var byteArray = new UTF8Encoding().GetBytes($"{Config.Load().MALUName}:{Config.Load().MALPassword}");
            var stringifiedxml = await APIWebReq.ReturnString(new Uri($"https://myanimelist.net/api/anime/search.xml?q={animetitle}"), byteArray);
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(stringifiedxml);
            XObject XNode = XDocument.Parse(xml.FirstChild.NextSibling.OuterXml);
            AnimeArray = JsonConvert.DeserializeObject<AnimeArr>(JObject.Parse(JsonConvert.SerializeXNode(XNode))["anime"].ToString());
            int animenode = AnimeArray.Entry.Count;      
            if (animenode > 1)
            {
                string entries = "__Make your selection__\n";
                int count = 0;
                foreach (var item in AnimeArray.Entry)
                {
                    count++;
                    entries += $"**{count}. {item.Title}**\n";
                }
                await MessageHandler.SendChannel(Context.Channel, entries, null, 60);
                Bot.bot.MessageReceived += _MessageReceivedAnime;
                AnimeTimer.Elapsed += Timer_ElapsedAnime;
                AnimeTimer.Start();
            }
            else
            {
                await GetAnimeAtZeroNonSearch();
            }
        }
        private void Timer_ElapsedAnime(object sender, ElapsedEventArgs e)
        {
            Bot.bot.MessageReceived -= _MessageReceivedManga;
        }
        private async Task _MessageReceivedAnime(Discord.WebSocket.SocketMessage arg)
        {
            if (Context.User.Id == arg.Author.Id)
            {
                AnimeTimer.Stop();
                await GetAnimeAtPosition(Convert.ToInt32(arg.Content));
            }
        }
        public async Task GetAnimeAtZeroNonSearch()
        {
            await SendAnime(AnimeArray.Entry.First());
        }
        private async Task GetAnimeAtPosition(int position)
        {
            Bot.bot.MessageReceived -= _MessageReceivedAnime;
            position = position - 1;
            await SendAnime(AnimeArray.Entry.ElementAt(position));    
        }
        private async Task SendAnime(Anime animu)
        {
            EmbedBuilder _embed = new EmbedBuilder();
            EmbedAuthorBuilder _auth = new EmbedAuthorBuilder();
            _embed.Color = RandColor.RandomColor();
            _auth.Url = $"https://myanimelist.net/anime/{animu.Id}/";
            _embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "English Title";
                if (!String.IsNullOrEmpty(animu.EnglishTitle))
                    x.Value = animu.EnglishTitle;
                else
                {
                    x.Value = "None Available";
                }
            });
            _embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Synonyms";
                if (!String.IsNullOrEmpty(animu.Synonyms))
                    x.Value = animu.Synonyms;
                else
                {
                    x.Value = "Not Available";
                }
            });
            _embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Synopsis";
                if (!String.IsNullOrEmpty(animu.Synopsis))
                {
                    x.Value = animu.Synopsis.Split('<')[0];
                }
                else
                {
                    x.Value = "Not Available";
                }
            });
            _embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Episodes";
                if (!String.IsNullOrEmpty(animu.Episodes))
                    x.Value = animu.Episodes;
                else
                {
                    x.Value = "Not Available";
                }
            });
            _embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Start Date";
                if (!String.IsNullOrEmpty(animu.StartDate))
                    x.Value = animu.StartDate;
                else
                {
                    x.Value = "Not Available";
                }
            });
            _embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "End Date";
                if (!String.IsNullOrEmpty(animu.EndDate))
                    x.Value = animu.EndDate;
                else
                {
                    x.Value = "Not Available";
                }
            });
            _embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Score";
                if (!String.IsNullOrEmpty(animu.Score))
                    x.Value = animu.Score;
                else
                {
                    x.Value = "Not Available";
                }
            });
            _auth.Name = animu.Title;
            _embed.Author = _auth;
            _embed.ThumbnailUrl = animu.Image;
            await MessageHandler.SendChannel(Context.Channel, "", _embed);
        }
    }
}
