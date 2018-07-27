using Discord;
using Google.Apis.Customsearch.v1;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using Skuld.Core.Models;
using Skuld.Core.Services;
using Skuld.Extensions;
using Skuld.Utilities.Discord;
using System;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;

namespace Skuld.Services
{
    public class SearchService
    {
        public CustomsearchService GoogleSearchService { get; internal set; }
        public YoutubeClient Youtube { get; set; }
        public ImgurClient ImgurClient { get; set; }
        public GenericLogger Logger { get; set; }
        public SkuldConfig Configuration { get; set; }

        public SearchService(GenericLogger log, SkuldConfig conf)
        {
            GoogleSearchService = new CustomsearchService();
            ImgurClient = new ImgurClient(conf.APIS.ImgurClientID, conf.APIS.ImgurClientSecret);
            Youtube = new YoutubeClient();
            Logger = log;
            Configuration = conf;
        }

        public void BuildGoogleClient()
        {
            GoogleSearchService = new CustomsearchService(new Google.Apis.Services.BaseClientService.Initializer { ApiKey = Configuration.APIS.GoogleAPI, ApplicationName = "Skuld" });
        }

        public async Task<string> SearchImgurAsync(string query)
        {
            try
            {
                var endpoint = new GalleryEndpoint(ImgurClient);
                var images = await endpoint.SearchGalleryAsync(query);
                var albm = images.GetRandomItem();
                var album = (IGalleryAlbum)albm;
                if (album != null && album.Nsfw.GetValueOrDefault())
                {
                    return "I found this:\n" + album.Link;
                }
                else
                {
                    return "I found nothing sorry. :/";
                }
            }
            catch (Exception ex)
            {
                StatsdClient.DogStatsd.Increment("commands.errors", 1, 1, new string[] { "exception" });
                await Logger.AddToLogsAsync(new Core.Models.LogMessage("ImgrSch", "Error with Imgur search", LogSeverity.Error, ex));
                return $"Error with search: {ex.Message}";
            }
        }

        public async Task<string> SearchYoutubeAsync(string query)
        {
            try
            {
                var items = await Youtube.SearchVideosAsync(query);
                var item = items.FirstOrDefault();
                var totalreactions = item.Statistics.LikeCount + item.Statistics.DislikeCount;
                double ratiog = ((double)item.Statistics.LikeCount / totalreactions) * 100;
                double ratiob = ((double)item.Statistics.DislikeCount / totalreactions) * 100;

                return $"<:youtube:314349922885566475> | http://youtu.be/{item.Id}\n" +
                    $"`👀: {item.Statistics.ViewCount.ToString("N0")}`\n" +
                    $"`👍: {item.Statistics.LikeCount.ToString("N0")} ({ratiog.ToString("0.0")}%)\t👎: {item.Statistics.DislikeCount.ToString("N0")} ({ratiob.ToString("0.0")}%)`\n" +
                    $"`Duration: {item.Duration}`";
            }
            catch (Exception ex)
            {
                StatsdClient.DogStatsd.Increment("commands.errors", 1, 1, new string[] { "exception" });
                await Logger.AddToLogsAsync(new Core.Models.LogMessage("YTBSrch", "Error with Youtube Search", LogSeverity.Error, ex));
                return $"Error with search: {ex.Message}";
            }
        }

        public async Task<Embed> SearchGoogleAsync(string query)
        {
            try
            {
                var listRequest = GoogleSearchService.Cse.List(query);
                listRequest.Cx = Configuration.APIS.GoogleCx;
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
                                IconUrl = "https://upload.wikimedia.org/wikipedia/commons/0/09/IOS_Google_icon.png",
                                Url = $"https://google.com/search?q={query.Replace(" ", "%20")}"
                            },
                            Description = "I found this:\n" +
                                $"**{item.Title}**\n" +
                                $"<{item.Link}>\n\n" +
                                "__**Also Relevant**__\n" +
                                $"**{item2.Title}**\n<{item2.Link}>\n\n" +
                                $"**{item3.Title}**\n<{item3.Link}>\n\n" +
                                "If I didn't find what you're looking for, use this link:\n" +
                                $"https://google.com/search?q={query.Replace(" ", "%20")}",
                            Color = EmbedUtils.RandomColor()
                        };
                        return embed.Build();
                    }
                    //Can be ignored
                    catch
                    {
                    }
                }
                else
                {
                    StatsdClient.DogStatsd.Increment("commands.errors", 1, 1, new string[] { "generic" });
                    return new EmbedBuilder
                    {
                        Title = "Error with the command",
                        Description = $"I couldn't find anything matching: `{query}`, please try again.",
                        Color = Color.Red
                    }.Build();
                }
            }
            catch (Exception ex)
            {
                StatsdClient.DogStatsd.Increment("commands.errors", 1, 1, new string[] { "exception" });
                await Logger.AddToLogsAsync(new Core.Models.LogMessage("GogSrch", "Error with google search", LogSeverity.Error, ex));
                return new EmbedBuilder
                {
                    Title = "Error with the command",
                    Color = Color.Red
                }.Build();
            }
            return new EmbedBuilder
            {
                Title = "Error with the command",
                Color = Color.Red
            }.Build();
        }
    }
}