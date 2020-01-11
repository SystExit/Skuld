using Akitaux.Twitch.Helix.Requests;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using PokeAPI;
using Skuld.APIS;
using Skuld.APIS.Extensions;
using Skuld.APIS.Pokemon.Models;
using Skuld.Bot.Extensions;
using Skuld.Bot.Services;
using Skuld.Core.Extensions;
using Skuld.Core.Generic.Models;
using Skuld.Core.Models;
using Skuld.Discord.Extensions;
using Skuld.Discord.Preconditions;
using Skuld.Discord.Services;
using SteamStoreQuery;
using SteamWebAPI2.Interfaces;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Voltaic;

namespace Skuld.Bot.Commands
{
    [Group, RequireEnabledModule]
    public class Search : InteractiveBase<ShardedCommandContext>
    {
        public SocialAPIS Social { get; set; }
        public Random Random { get; set; }
        public SteamStore SteamStoreClient { get; set; }
        public UrbanDictionaryClient UrbanDictionary { get; set; }
        public WikipediaClient Wikipedia { get; set; }
        public GiphyClient Giphy { get; set; }
        public Stands4Client Stands4 { get; set; }
        public SkuldConfig Configuration { get => HostSerivce.Configuration; }

        #region SocialMedia

        [Command("twitch"), Summary("Finds a twitch user")]
        public async Task TwitchSearch([Remainder]string twitchStreamer)
        {
            var channel = await BotService.TwitchClient.GetUsersAsync(new GetUsersParams
            {
                UserNames = new[] { new Utf8String(twitchStreamer) }
            }).ConfigureAwait(false);

            var user = channel.Data.FirstOrDefault();

            if (user != null)
                await (await user.GetEmbedAsync(BotService.TwitchClient).ConfigureAwait(false)).QueueMessageAsync(Context).ConfigureAwait(false);
            else
                await EmbedExtensions.FromError($"Couldn't find user `{twitchStreamer}`. Check your spelling and try again", Context).QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("reddit"), Summary("Gets a subreddit")]
        public async Task SubReddit(string subreddit)
        {
            var channel = (ITextChannel)Context.Channel;

            var subReddit = await Social.GetSubRedditAsync(subreddit).ConfigureAwait(false);

            var embed = new EmbedBuilder
            {
                Title = "https://reddit.com/" + subreddit,
                Description = subReddit.Data.Posts.PaginatePosts(channel, 25)[0],
                Footer = new EmbedFooterBuilder
                {
                    Text = "Page 1/1"
                },
                Color = Color.Blue
            };

            await embed.Build().QueueMessageAsync(Context).ConfigureAwait(false);
        }

        #endregion SocialMedia

        #region SearchEngines

        [Command("search"), Summary("Use \"g\" as a short cut for google,\n\"yt\" for youtube,\nor search for images on imgur"), Alias("s")]
        public async Task GetSearch(string platform, [Remainder]string query)
        {
            platform = platform.ToLowerInvariant();
            if (platform == "google" || platform == "g")
            {
                await $"🔍 Searching Google for: {query}".QueueMessageAsync(Context).ConfigureAwait(false);
                var result = await SearchClient.SearchGoogleAsync(query).ConfigureAwait(false);

                if (result == null)
                {
                    await EmbedExtensions.FromError($"I couldn't find anything matching: `{query}`, please try again.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                    return;
                }

                var item1 = result.FirstOrDefault();
                var item2 = result.ElementAt(1);
                var item3 = result.LastOrDefault();

                string desc = "I found this:\n" +
                    $"**{item1.Title}**\n" +
                    $"<{item1.Link}>\n\n" +
                    "__**Also Relevant**__\n" +
                    $"**{item2.Title}**\n<{item2.Link}>\n\n" +
                    $"**{item3.Title}**\n<{item3.Link}>\n\n" +
                    "If I didn't find what you're looking for, use this link:\n" +
                    $"https://google.com/search?q={query.Replace(" ", "%20")}";

                await
                    new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithName($"Google search for: {query}")
                        .WithIconUrl("https://upload.wikimedia.org/wikipedia/commons/0/09/IOS_Google_icon.png")
                        .WithUrl($"https://google.com/search?q={query.Replace(" ", "%20")}")
                    )
                    .AddFooter(Context)
                    .WithDescription(desc)
                    .QueueMessageAsync(Context).ConfigureAwait(false);
            }
            if (platform == "youtube" || platform == "yt")
            {
                await $"🔍 Searching Youtube for: {query}".QueueMessageAsync(Context).ConfigureAwait(false);
                var result = await SearchClient.SearchYoutubeAsync(query);
                await result.QueueMessageAsync(Context).ConfigureAwait(false);
            }
            if (platform == "imgur")
            {
                await "🔍 Searching Imgur for: {query}".QueueMessageAsync(Context).ConfigureAwait(false);
                var result = await SearchClient.SearchImgurAsync(query);
                await result.QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("lmgtfy"), Summary("Creates a \"lmgtfy\"(Let me google that for you) link")]
        public async Task LMGTFY(string engine, [Remainder]string query)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            var prefix = (await Database.GetGuildAsync(Context.Guild).ConfigureAwait(false)).Prefix ?? Configuration.Prefix;

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
                await url.QueueMessageAsync(Context).ConfigureAwait(false);
            else
            {
                await EmbedExtensions.FromError($"Ensure your parameters are correct, example: `{Configuration.Prefix}lmgtfy g How to use lmgtfy`", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                StatsdClient.DogStatsd.Increment("commands.errors", 1, 1, new[] { "generic" });
            }
        }

        #endregion SearchEngines

        #region Definitions/Information

        [Command("urban"), Summary("Gets a thing from urban dictionary if empty, it gets a random thing"), RequireNsfw]
        public async Task Urban([Remainder]string phrase = null)
        {
            if (phrase == null)
                await (await UrbanDictionary.GetRandomWordAsync().ConfigureAwait(false)).ToEmbed().QueueMessageAsync(Context).ConfigureAwait(false);
            else
                await (await UrbanDictionary.GetPhraseAsync(phrase).ConfigureAwait(false)).ToEmbed().QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("wikipedia"), Summary("Gets wikipedia information, supports all languages that wikipedia offers"), Alias("wiki")]
        public async Task Wiki(string langcode, [Remainder]string query)
        {
            var page = await Wikipedia.GetArticleAsync(langcode, query).ConfigureAwait(false);
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = page.Name,
                    Url = page.Url
                },
                Color = EmbedExtensions.RandomEmbedColor()
            };
            embed.AddField("Description", page.Description ?? "Not Available", true);
            await embed.Build().QueueMessageAsync(Context).ConfigureAwait(false);
        }

        [Command("define"), Summary("Defines a word")]
        public async Task Define([Remainder]string word)
        {
            var definedword = await Stands4.GetWordAsync(word).ConfigureAwait(false);

            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = definedword.Word
                },
                Color = EmbedExtensions.RandomEmbedColor()
            };
            embed.AddField("Definition", definedword.Definition ?? "None Available");
            embed.AddField("Example", definedword.Example ?? "Not Available");
            embed.AddField("Part of speech", definedword.PartOfSpeech ?? "Not Available", inline: true);
            embed.AddField("Terms", definedword.Terms ?? "Not Available", inline: true);

            await embed.Build().QueueMessageAsync(Context).ConfigureAwait(false);
        }

        #endregion Definitions/Information

        #region Games

        [Command("steam"), Summary("Searches steam store")]
        public async Task SteamStore([Remainder]string game)
        {
            var steam = Query.Search(game);

            if (steam.Capacity > 0)
            {
                if (steam.Count > 1)
                {
                    var Pages = steam.PaginateList(25);

                    await $"Type the number of what you want:\n{string.Join("\n", Pages)}".QueueMessageAsync(Context).ConfigureAwait(false);

                    var message = await NextMessageAsync();

                    int.TryParse(message.Content, out int selectedapp);
                    if (selectedapp <= 0)
                    {
                        await EmbedExtensions.FromError("Incorrect input", Context).QueueMessageAsync(Context).ConfigureAwait(false);
                        return;
                    }

                    selectedapp--;

                    await (await steam[selectedapp].GetEmbedAsync().ConfigureAwait(false)).QueueMessageAsync(Context).ConfigureAwait(false);
                }
                else
                {
                    await (await steam[0].GetEmbedAsync().ConfigureAwait(false)).QueueMessageAsync(Context).ConfigureAwait(false);
                }
            }
            else
            {
                await EmbedExtensions.FromError("I found nothing from steam", Context).QueueMessageAsync(Context).ConfigureAwait(false);
            }
        }

        [Command("pokemon"), Summary("Gets information about a pokemon id")]
        public async Task GetPokemon(string pokemon, string group = null)
            => await SendPokemonAsync(await DataFetcher.GetNamedApiObject<PokemonSpecies>(pokemon.ToLowerInvariant()), group ?? "default").ConfigureAwait(false);

        [Command("pokemon"), Summary("Gets information about a pokemon id")]
        public async Task GetPokemon(int pokemonid, string group = null)
            => await SendPokemonAsync(await DataFetcher.GetApiObject<PokemonSpecies>(pokemonid), group ?? "default").ConfigureAwait(false);

        public async Task SendPokemonAsync(PokemonSpecies pokemon, string group)
        {
            if (pokemon == null)
            {
                StatsdClient.DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });

                await
                    EmbedExtensions.FromError("This pokemon doesn't exist. Please try again.", Context)
                    .QueueMessageAsync(Context)
                    .ConfigureAwait(false);
            }
            else
            {
                switch (group.ToLowerInvariant())
                {
                    case "stat":
                    case "stats":
                        await (await pokemon.GetEmbedAsync(PokemonDataGroup.Stats).ConfigureAwait(false)).QueueMessageAsync(Context).ConfigureAwait(false);
                        break;

                    case "abilities":
                    case "ability":
                        await (await pokemon.GetEmbedAsync(PokemonDataGroup.Abilities).ConfigureAwait(false)).QueueMessageAsync(Context).ConfigureAwait(false);
                        break;

                    case "helditems":
                    case "hitems":
                    case "hitem":
                    case "items":
                        await (await pokemon.GetEmbedAsync(PokemonDataGroup.HeldItems).ConfigureAwait(false)).QueueMessageAsync(Context).ConfigureAwait(false);
                        break;

                    case "move":
                    case "moves":
                        await (await pokemon.GetEmbedAsync(PokemonDataGroup.Moves).ConfigureAwait(false)).QueueMessageAsync(Context).ConfigureAwait(false);
                        break;

                    case "games":
                    case "game":
                        await (await pokemon.GetEmbedAsync(PokemonDataGroup.Games).ConfigureAwait(false)).QueueMessageAsync(Context).ConfigureAwait(false);
                        break;

                    case "default":
                    default:
                        await (await pokemon.GetEmbedAsync(PokemonDataGroup.Default).ConfigureAwait(false)).QueueMessageAsync(Context).ConfigureAwait(false);
                        break;
                }
            }
        }

        #endregion Games
    }
}