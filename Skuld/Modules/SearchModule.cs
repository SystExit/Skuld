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
using Google.Apis.Customsearch.v1;
using System.IO;
using YoutubeExplode;
using System.Collections.Generic;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using System.Web;
using Discord.Addons.Interactive;
using System.Text;

namespace Skuld.Commands
{
    [Group, Name("Search")]
    public class Search : InteractiveBase
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
                await MessageHandler.SendChannelAsync(Context.Channel,"", _embed);
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
                await MessageHandler.SendChannelAsync(Context.Channel,"", _embed);
            }
        }*/

        [Command("twitch", RunMode = RunMode.Async), Summary("Finds a twitch user")]
        public async Task TwitchSearch([Remainder]string twitchStreamer)
        {
            var twicli = Bot.NTwitchClient;
            var users = await twicli.GetUsersAsync(twitchStreamer);
            var chan = await users.FirstOrDefault().GetChannelAsync();
            var stream = await chan.GetStreamAsync();
            var embed = new EmbedBuilder()
            {
                Color = Tools.Tools.RandomColor()
            };
            if (stream != null)
            {
                embed.Author = new EmbedAuthorBuilder()
                {
                    Name = chan.DisplayName,
                    IconUrl = chan.LogoUrl,
                    Url = chan.Url
                };
                var streamimg = stream.Previews.SkipWhile(x => x.Key != "large").FirstOrDefault().Value;
                embed.Description = chan.Status;
                embed.AddField("They're currently playing", chan.Game,inline: true);
                embed.AddField("Current Viewers", $"{stream.Viewers.ToString("N0")}",inline: true);
                embed.ImageUrl = streamimg;
            }
            if (stream == null)
            {
                embed.Author = new EmbedAuthorBuilder()
                {
                    Name = chan.DisplayName,
                    IconUrl = chan.LogoUrl,
                    Url = chan.Url
                };
                if (!String.IsNullOrEmpty(chan.Status))
                { embed.AddField("Last stream title", chan.Status, inline: true); }
                else
                { embed.AddField("Last stream title", "Unset", inline: true); }
                if (!String.IsNullOrEmpty(chan.Game))
                { embed.AddField("Was last streaming", chan.Game, inline: true); }
                else
                { embed.AddField("Was last streaming", "Nothing", inline: true); }
                embed.AddField("Followers", $"{chan.Followers.ToString("N0")}",inline: true);
                embed.AddField("Total Views", $"{chan.Views.ToString("N0")}",inline: true);
                embed.ThumbnailUrl = chan.VideoBannerUrl;
            }
            await MessageHandler.SendChannelAsync(Context.Channel, "", embed.Build());
        }

		[Command("instagram", RunMode = RunMode.Async), Alias("insta"), Ratelimit(20, 1, Measure.Minutes)]
		public async Task Test(string usr)
		{
			var data = await APIS.Social.Instagram.GetInstagramUserAsync(usr);
			if (data != null)
			{
				if (!data.PrivateAccount)
				{
					if (data.Images.Images.Count() > 0)
					{
						var recentpost = data.Images.Images.FirstOrDefault();
						var embed = new EmbedBuilder
						{
							Author = new EmbedAuthorBuilder
							{
								Name = $"{data.FullName} ({data.Username})",
								IconUrl = data.ProfilePicture,
								Url = "https://instagr.am/" + data.Username
							},
							ImageUrl = recentpost.DisplaySrc,
							Description = recentpost.Caption,
							Color = Tools.Tools.RandomColor(),
							Title = "Newest Post",
							Url = "https://www.instagr.am/p/" + recentpost.Code + "/",
							Timestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(recentpost.Date),
							Footer = new EmbedFooterBuilder
							{
								Text = "Uploaded"
							}
						};
						await MessageHandler.SendChannelAsync(Context.Channel, "", embed.Build());
					}
					else
					{
						await MessageHandler.SendChannelAsync(Context.Channel, "This account has no images in their feed");
					}
				}
				else
				{
					await MessageHandler.SendChannelAsync(Context.Channel, "This account is a Private Account, so I can't access their feed.");
				}
			}
			else
			{
				await MessageHandler.SendChannelAsync(Context.Channel, $"I can't find an account named: `{usr}`. Check your spelling and try again.");
			}
		}

		//Start Search Platforms
		[Command("search", RunMode = RunMode.Async), Summary("Gets the first search on a google search"), Alias("s")]
        public async Task GetSearch(string platform, [Remainder]string query)
        {
            platform = platform.ToLowerInvariant();
            if (platform == "google" || platform == "g")
            {
                await GoogleS(query).ConfigureAwait(false);
            }
            if (platform == "youtube" || platform == "yt")
            {
                await YoutubeS(query).ConfigureAwait(false);
            }
            if (platform == "imgur" || platform == "image")
            {
                await ImgurS(query).ConfigureAwait(false);
            }
        }

        private async Task GoogleS(string query)
        {
            try
            {
                var css = new CustomsearchService(new Google.Apis.Services.BaseClientService.Initializer() { ApiKey = Bot.Configuration.GoogleAPI, ApplicationName = "Skuld" });
                var listRequest = css.Cse.List(query);
                listRequest.Cx = Bot.Configuration.GoogleCx;
                listRequest.Safe = CseResource.ListRequest.SafeEnum.High;
                var search = await listRequest.ExecuteAsync();
                var items = search.Items;
                if (items != null)
                {
                    var item = items.FirstOrDefault();
                    var item2 = items.ElementAtOrDefault(1);
                    var item3 = items.ElementAtOrDefault(2);
                    EmbedBuilder embed = null;
                    try
                    {
                        embed = new EmbedBuilder
                        {
                            Author = new EmbedAuthorBuilder
                            {
                                Name = $"Google search for: {query}",
                                IconUrl = "https://www.google.com/favicon.ico",
                                Url = $"https://google.com/search?q={query.Replace(" ", "%20")}"
                            },
                            Description = "I found this:\n" +
                                $"**{item.Title}**\n" +
                                $"<{item.Link}>\n\n" +
                                "__**Also Relevant**__\n"+
                                $"**{item2.Title}**\n<{item2.Link}>\n\n"+
                                $"**{item3.Title}**\n<{item3.Link}>\n\n"+
                                "If I didn't find what you're looking for, use this link:\n" +
                                $"https://google.com/search?q={query.Replace(" ", "%20")}",
                            Color = Tools.Tools.RandomColor()
                        };
                        await MessageHandler.SendChannelAsync(Context.Channel, $"<{item.Link}>", embed.Build());
                    }
                    //Can be ignored
                    catch
                    {
                    }
                }
                else
                {
                    await MessageHandler.SendChannelAsync(Context.Channel, "", new EmbedBuilder
                    {
                        Title = "Error with the command",
                        Description = $"I couldn't find anything matching: `{query}`, please try again.",
                        Color = Tools.Tools.RandomColor()
                    }.Build());
                    StatsdClient.DogStatsd.Increment("commands.errors.generic");
                }
            }
            catch (Exception ex)
            {
                await Bot.Logger.AddToLogs(new Models.LogMessage("GogSrch", "Error with google search", LogSeverity.Error, ex));
                await MessageHandler.SendChannelAsync(Context.Channel, "", new EmbedBuilder { Title = "Error with the command", Color = Tools.Tools.RandomColor() }.Build());
                StatsdClient.DogStatsd.Increment("commands.errors.exception");
            }
        }
        private async Task YoutubeS(string query)
        {
            try
            {
                var items = await Bot.YouTubeClient.SearchVideosAsync(query);
                var item = items.FirstOrDefault();
                var totalreactions = item.Statistics.LikeCount + item.Statistics.DislikeCount;
                double ratiog = ((double)item.Statistics.LikeCount / totalreactions)*100;
                double ratiob = ((double)item.Statistics.DislikeCount / totalreactions) * 100;

                await MessageHandler.SendChannelAsync(Context.Channel, $"<:youtube:314349922885566475> | http://youtu.be/{item.Id}\n" +
                    $"`👀: {item.Statistics.ViewCount.ToString("N0")}`\n" +
                    $"`👍: {item.Statistics.LikeCount.ToString("N0")} ({ratiog.ToString("0.0")}%)\t👎: {item.Statistics.DislikeCount.ToString("N0")} ({ratiob.ToString("0.0")}%)`\n" +
                    $"`Duration: {item.Duration}`");
            }
            catch (Exception ex)
            {
                await Bot.Logger.AddToLogs(new Models.LogMessage("YTBSrch", "Error with Youtube Search", LogSeverity.Error, ex));
                await MessageHandler.SendChannelAsync(Context.Channel, "", new EmbedBuilder { Title = "Error with the command", Description = ex.Message, Color = Tools.Tools.RandomColor() }.Build());
                StatsdClient.DogStatsd.Increment("commands.errors.exception");
            }
        }
        private async Task ImgurS(string query)
        {
            try
            {
                var client = new ImgurClient(Bot.Configuration.ImgurClientID, Bot.Configuration.ImgurClientSecret);
                var endpoint = new GalleryEndpoint(client);
                var images = await endpoint.SearchGalleryAsync(query);
                var albm = images.ElementAtOrDefault(Bot.random.Next(0,images.Count()));
                var album = (IGalleryAlbum)albm;
                if (album != null && album.Nsfw != true)
                {
                    string message = "I found this:\n" + album.Link;
                    await MessageHandler.SendChannelAsync(Context.Channel, message);
                }
                else
                    await MessageHandler.SendChannelAsync(Context.Channel, "I found nothing sorry. :/");
            }
            catch (Exception ex)
            {
                await Bot.Logger.AddToLogs(new Models.LogMessage("ImgrSch", "Error with Imgur search", LogSeverity.Error, ex));
                await MessageHandler.SendChannelAsync(Context.Channel, "", new EmbedBuilder { Title = "Error with the command", Description = ex.Message, Color = Tools.Tools.RandomColor() }.Build() );
                StatsdClient.DogStatsd.Increment("commands.errors.exception");
            }
        }

        [Command("lmgtfy",RunMode = RunMode.Async), Summary("Creates a \"lmgtfy\"(Let me google that for you) link")]
        public async Task LMGTFY(string engine, [Remainder]string query)
        {
            string url = "https://lmgtfy.com/";
            engine = engine.ToLowerInvariant();
            if (engine == "g" || engine == "google")
            { url = url + "?q=" + query.Replace(" ", "%20"); }
            if (engine == "b" || engine == "bing")
            { url = url + "?s=b&q=" + query.Replace(" ", "%20"); }
            if (engine == "y" || engine == "yahoo")
            { url = url + "?s=y&q=" + query.Replace(" ", "%20"); }
            if (engine == "a" || engine == "aol")
            { url = url + "?a=b&q=" + query.Replace(" ", "%20"); }
            if (engine == "k" || engine == "ask")
            { url = url + "?k=b&q=" + query.Replace(" ", "%20"); }
            if (engine == "d" || engine == "duckduckgo")
            { url = url + "?s=d&q=" + query.Replace(" ", "%20"); }
            if (url != "https://lmgtfy.com/")
                await MessageHandler.SendChannelAsync(Context.Channel, url);
            else
            { await MessageHandler.SendChannelAsync(Context.Channel, "", new EmbedBuilder { Author = new EmbedAuthorBuilder() { Name = "Error with command" }, Color = new Color(255, 0, 0), Description = $"Ensure your parameters are correct, example: `{Bot.Configuration.Prefix}lmgtfy g How to use lmgtfy`" }.Build());
                StatsdClient.DogStatsd.Increment("commands.errors.generic");
            }
        }
		
        [Command("urban", RunMode = RunMode.Async), Summary("Gets a thing from urban dictionary")]
        public async Task Urban([Remainder]string phrase) =>
            await Geturban(new Uri($"http://api.urbandictionary.com/v0/define?term={phrase}")).ConfigureAwait(false);

        [Command("urban", RunMode = RunMode.Async), Summary("Gets a random thing from urban dictionary")]
        public async Task Urban() => 
            await Geturban(new Uri("http://api.urbandictionary.com/v0/random")).ConfigureAwait(false);

        [Command("osu!", RunMode = RunMode.Async), Summary("Get a person's Osu! Sig")]
        public async Task OsuSig([Remainder]string User) => await SendSigAsync(0, User);

        [Command("osu!taiko", RunMode = RunMode.Async), Summary("Gets a person's Osu!Taiko Sig")]
        public async Task TaikoSig([Remainder]string User) => await SendSigAsync(1, User);

        [Command("osu!ctb", RunMode = RunMode.Async), Summary("Gets a person's Osu!CTB Sig")]
        public async Task CTBSig([Remainder]string User) => await SendSigAsync(2, User);

        [Command("osu!mania", RunMode = RunMode.Async), Summary("Gets a person's Osu!Mania Sig")]
        public async Task ManiaSig([Remainder]string User) => await SendSigAsync(3, User);

        private async Task SendSigAsync(int mode, string User)
        {
            Uri url = null;

            var folder = AppContext.BaseDirectory + "/skuld/storage/OsuSigs/";

            string msgmode="";

            if (mode == 0)
            {
                url = new Uri($"http://lemmmy.pw/osusig/sig.php?colour=pink&uname={User}&pp=1&countryrank&rankedscore&onlineindicator=undefined&xpbar");
                folder += "Standard/";
            }
            if (mode == 1)
            {
                url = new Uri($"http://lemmmy.pw/osusig/sig.php?colour=pink&uname={User}&mode=1&pp=1&countryrank&rankedscore&onlineindicator=undefined&xpbar");
                folder += "Taiko/";
                msgmode = "Taiko";
            }
            if (mode == 2)
            {
                url = new Uri($"http://lemmmy.pw/osusig/sig.php?colour=pink&uname={User}&mode=2&pp=1&countryrank&rankedscore&onlineindicator=undefined&xpbar");
                folder += "CTB/";
                msgmode = "CTB";
            }
            if (mode == 3)
            {
                url = new Uri($"http://lemmmy.pw/osusig/sig.php?colour=pink&uname={User}&mode=3&pp=1&countryrank&rankedscore&onlineindicator=undefined&xpbar");
                folder += "Mania/";
                msgmode = "Mania";
            }

            if (!Directory.Exists(folder))
            { Directory.CreateDirectory(folder); }

            var filepath = folder + User + ".png";
            await APIWebReq.DownloadFile(url, filepath);

            var file = File.OpenRead(filepath);

            if (ValidatePNG(file))
            { await MessageHandler.SendChannelAsync(Context.Channel, "", filepath); }
            else
            {
                await MessageHandler.SendChannelAsync(Context.Channel, "The user either doesn't exist or they haven't played osu!" + msgmode);
            }

            file.Close();

            File.Delete(filepath);
        }

        private bool ValidatePNG(FileStream fs)
        {
            var header = new byte[4];

            fs.Read(header, 0, 4);

            var strHeader = Encoding.ASCII.GetString(header);
            return strHeader.ToLower().EndsWith("png");
        }

        private async Task Geturban(Uri url)
        {
            var rnd = Bot.random;
            var rawresp = await APIWebReq.ReturnString(url);
            var jsonresp = JObject.Parse(rawresp);
            var lists = (JArray)jsonresp["list"];
            dynamic item = lists[rnd.Next(0, lists.Count)];
            var word = new Urban(item["word"].ToString(),
                item["definition"].ToString(),
                item["permalink"].ToString(),
                item["example"].ToString(),
                item["author"].ToString(),
                item["thumbs_up"].ToString(),
                item["thumbs_down"].ToString());
            var embed = new EmbedBuilder()
            {
                Color = Tools.Tools.RandomColor(),
                Author = new EmbedAuthorBuilder()
                {
                    Name = word.Word,
                    Url = word.PermaLink
                }
            };
            embed.AddField("Author", word.Author ?? "Not Available");
            embed.AddField("Definition", word.Definition ?? "Not Available");
            embed.AddField("Example", word.Example??"Not Available");
            embed.AddField("Upvotes", word.UpVotes??"Not Available");
            embed.AddField("Downvotes", word.DownVotes??"Not Available");
            await MessageHandler.SendChannelAsync(Context.Channel, "", embed.Build());
        }

        [Command("wikipedia", RunMode = RunMode.Async), Summary("Gets wikipedia information, supports all languages that wikipedia offers"), Alias("wiki")]
        public async Task Wiki(string langcode, [Remainder]string query) => 
            await GetWiki(langcode, query).ConfigureAwait(false);

        [Command("wikipedia", RunMode = RunMode.Async), Summary("Gets wikipedia information, supports all languages that wikipedia offers"), Alias("wiki")]
        public async Task Wiki([Remainder]string query) => 
            await GetWiki("en", query).ConfigureAwait(false);
        public async Task GetWiki(string langcode, string query)
        {
            var jsonresp = JObject.Parse((await APIWebReq.ReturnString(new Uri($"https://{langcode}.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exintro=&explaintext=&titles={query}"))));
            dynamic item = jsonresp["query"]["pages"].First.First;
            string desc = Convert.ToString(item["extract"]);
            var page = new Wiki()
            {
                Name = item["title"].ToString(),
                Description = desc.Remove(500) + "...\nRead more at the article.",
                Url = $"https://{langcode}.wikipedia.org/wiki/{query}"
            };
            var embed = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder()
                {
                    Name = page.Name,
                    Url = page.Url
                },
                Color = Tools.Tools.RandomColor()
            };
            embed.AddField("Description", page.Description??"Not Available",inline: true);
            await MessageHandler.SendChannelAsync(Context.Channel, "", embed.Build());
        }

        [Command("gif", RunMode = RunMode.Async), Summary("Gets a gif")]
        public async Task Gifcommand([Remainder]string query)
        {
            var rnd = Bot.random;
            var embed = new EmbedBuilder()
            {
                Color = Tools.Tools.RandomColor(),
                Author = new EmbedAuthorBuilder()
                {
                    Name = "Giphy",
                    IconUrl = "https://giphy.com/favicon.ico",
                    Url = "https://giphy.com/"
                }
            };
            try
            {
                query = query.Replace(" ", "%20");
                var rawresp = await APIWebReq.ReturnString(new Uri($"https://api.giphy.com/v1/gifs/search?q={query}&api_key=dc6zaTOxFJmzC"));
                var jsonresp = JObject.Parse(rawresp);
                var photo = (JArray)jsonresp["data"];
                dynamic item = photo[rnd.Next(0, photo.Count)];
                var gif = new Gif(item["id"].ToString());
                embed.Url = gif.Url;
                embed.ImageUrl = gif.Url;
                await MessageHandler.SendChannelAsync(Context.Channel, "", embed.Build());
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                { await MessageHandler.SendChannelAsync(Context.Channel, Context.User.Mention + " No results found for: `" + query + "`"); }
                StatsdClient.DogStatsd.Increment("commands.errors.generic");
            }
        }

        [Command("define", RunMode = RunMode.Async), Summary("Defines a word")]
        public async Task Define([Remainder]string word)
        {
            var stringifiedxml = await APIWebReq.ReturnString(new Uri($"http://www.stands4.com/services/v2/defs.php?uid={Bot.Configuration.STANDSUid}&tokenid={Bot.Configuration.STANDSToken}&word={word}"));
            var xml = new XmlDocument();
            xml.LoadXml(stringifiedxml);
            XObject xNode = XDocument.Parse(xml.InnerXml);
            var jobject = JObject.Parse(JsonConvert.SerializeXNode(xNode));
            dynamic item = jobject["results"]["result"].First();
            var definedword = new Define(word,item["definition"].ToString(),item["example"].ToString(),item["partofspeech"].ToString(),item["term"].ToString());
            var embed = new EmbedBuilder()
            {
                Color = Tools.Tools.RandomColor(),
                Author = new EmbedAuthorBuilder()
                {
                    Name = definedword.Word
                }
            };
            embed.AddField("Definition",definedword.Definition??"None Available");
            embed.AddField("Example",definedword.Example??"Not Available");
            embed.AddField("Part of speech",definedword.PartOfSpeech??"Not Available",inline: true);
            embed.AddField("Terms", definedword.Terms??"Not Available",inline: true);
            await MessageHandler.SendChannelAsync(Context.Channel, "", embed.Build());
        }

        [Command("reddit", RunMode = RunMode.Async), Summary("Gets a subreddit")]
        public async Task SubReddit(string subreddit)
            => await SubReddit(subreddit, 10).ConfigureAwait(false);
        [Command("reddit", RunMode = RunMode.Async), Summary("Gets a subreddit")]
        public async Task SubReddit(string subreddit, int amount)
        {
            var subReddit = await APIReddit.GetSubRedditAsync(subreddit, amount);
            var paginatedMessage = new PaginatedMessage
            {
                Title = "https://reddit.com/" + subreddit,
                Color = Tools.Tools.RandomColor(),
                Options = new PaginatedAppearanceOptions
                {
                    DisplayInformationIcon = false,
                    JumpDisplayOptions = JumpDisplayOptions.WithManageMessages
                }
            };

            var pages = new List<string>();
            var pageText = new List<string>();

            foreach (var post in subReddit.Data.Posts)
            {
                string txt = $"[{post.Data.Title}](https://reddit.com{post.Data.Permalink})\n";
                if (post.Data.Over18)
                { }
                else
                { pageText.Add(txt); }
            }

            if(pageText.Count>10)
            {
                string tempstring = null;
                int cycle = 0;
                for (int counter = 1; counter <= 10; counter++)
                {
                    if (pageText.ElementAtOrDefault((cycle + counter)-1) == null)
                    {
                        pages.Add(tempstring);
                        tempstring = null;
                        break;
                    }
                    if (counter % 10 == 0)
                    {
                        tempstring = tempstring + (cycle + counter) + ". " + pageText.ElementAtOrDefault((cycle + counter) - 1);
                        pages.Add(tempstring);
                        tempstring = null;
                        cycle = cycle + 10;
                        counter = 0;
                    }
                    else
                    { tempstring = tempstring + (cycle + counter) + ". " + pageText.ElementAtOrDefault((cycle + counter) - 1) + "\n"; }
                }
            }
            else
            {
                string tmpstring = null;
                int cntr = 1;
                foreach(var post in pageText)
                {
                    if (cntr != pageText.Count)
                    { tmpstring = tmpstring + $"{cntr++}. {post}\n"; }
                    else
                    { tmpstring = tmpstring + $"{cntr++}. {post}"; }
                }
                pages.Add(tmpstring);
            }
            
            paginatedMessage.Pages = pages;

            if (pages.Count > 1)
            { await PagedReplyAsync(paginatedMessage); }
            else
            {
                await MessageHandler.SendChannelAsync(Context.Channel, "", new EmbedBuilder()
                {
                    Title = paginatedMessage.Title,
                    Color = Tools.Tools.RandomColor(),
                    Description = pages.FirstOrDefault(),
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = "Page 1/1"
                    }
                }.Build());
            }
        }
    }
}
