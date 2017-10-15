using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.Linq;
using Skuld.APIS;
using Skuld.Tools;
using System.Text;
using System.Collections.Generic;
using System.Web;
using Discord.Addons.Interactive;
using Skuld.Models.API.MAL;

namespace Skuld.Modules
{
    [Group, Name("Search")]
    public class WeebModule : InteractiveBase
    {
        private MangaArr MangaArray;
        [Command("manga", RunMode = RunMode.Async), Summary("Gets a manga from MyAnimeList.Net")]
        public async Task MangaGet([Remainder]string mangatitle)
        {
            mangatitle = mangatitle.Replace(" ", "+");
            var byteArray = new UTF8Encoding().GetBytes($"{Config.Load().MALUName}:{Config.Load().MALPassword}");
            var stringifiedxml = await APIWebReq.ReturnString(new Uri($"https://myanimelist.net/api/manga/search.xml?q={mangatitle}"), byteArray);
            var xml = new XmlDocument();
            xml.LoadXml(stringifiedxml);
            XObject xNode = XDocument.Parse(xml.FirstChild.NextSibling.OuterXml);
            MangaArray = JsonConvert.DeserializeObject<MangaArr>(JObject.Parse(JsonConvert.SerializeXNode(xNode))["manga"].ToString());
            int manganode = MangaArray.Entry.Count;
            if (manganode > 1)
            {
                IMessage msg = null;
                var msgs = new List<IMessage>();
                string entrymessage = "__Make your selection__ Make your selection within 10 seconds\n";
                int count = 0;
                var entries = new List<string>();
                foreach (var item in MangaArray.Entry)
                {
                    count++;
                    entries.Add($"**{count}. {item.Title}**");
                }
                if (entries.Count >= 20)
                {
                    entrymessage += String.Join(Environment.NewLine, entries.Take(entries.Count / 2).ToArray());
                    msgs.Add(await MessageHandler.SendChannel(Context.Channel, entrymessage));
                    entrymessage = "";
                    entrymessage += String.Join(Environment.NewLine, entries.Skip(entries.Count / 2).ToArray());
                    msgs.Add(await MessageHandler.SendChannel(Context.Channel, entrymessage));
                }
                else
                {
                    entrymessage += String.Join(Environment.NewLine, entries.ToArray());
                    msg = await MessageHandler.SendChannel(Context.Channel, entrymessage);
                }
                var response = await NextMessageAsync(fromSourceUser: true, inSourceChannel: true, timeout:TimeSpan.FromSeconds(10));
                await GetMangaAtPosition(Convert.ToInt32(response.Content), response.Channel);

                if (Context.Guild.CurrentUser.GuildPermissions.ManageMessages)
                {
                    await Context.Message.DeleteAsync();
                    await response.DeleteAsync();
                }
                if (msgs.Count > 0)
                    foreach (var m in msgs)
                        await m.DeleteAsync();
                if (msg != null)
                    await msg.DeleteAsync();
            }
            else
            {
                await GetMangaAtZeroNonSearch();
            }
        }
        private async Task GetMangaAtZeroNonSearch()
        {
            await SendManga(MangaArray.Entry.First());
        }
        private async Task GetMangaAtPosition(int position, IMessageChannel channel) =>
            await SendManga(MangaArray.Entry.ElementAt(position - 1), channel);
        public async Task SendManga(Manga mango, IMessageChannel channel = null)
        {
            var embed = new EmbedBuilder()
            {
                Color = Tools.Tools.RandomColor(),
                Author = new EmbedAuthorBuilder()
                {
                    Url = $"https://myanimelist.net/manga/{mango.Id}/",
                    Name = mango.Title
                },
                ThumbnailUrl = mango.Image
            };
            embed.AddField("English Title", Tools.Tools.CheckIfStringIsNull(mango.EnglishTitle), inline: true);
            embed.AddField("Synonyms", Tools.Tools.CheckIfStringIsNull(mango.Synonyms), inline: true);
            embed.AddField("Chapters", Tools.Tools.CheckIfStringIsNull(mango.Chapters), inline: true);
            embed.AddField("Volumes", Tools.Tools.CheckIfStringIsNull(mango.Volumes), inline: true);
            embed.AddField("Start Date", Tools.Tools.CheckIfStringIsNull(mango.StartDate), inline: true);
            embed.AddField("End Date", Tools.Tools.CheckIfStringIsNull(mango.EndDate), inline: true);
            embed.AddField("Score", Tools.Tools.CheckIfStringIsNull(mango.Score), inline: true);
            embed.AddField("Synopsis", Tools.Tools.CheckIfStringIsNull(HttpUtility.HtmlDecode(mango.Synopsis.Split('<')[0])), inline: true);
            await MessageHandler.SendChannel(channel ?? Context.Channel, "", embed);
        }

        private AnimeArr AnimeArray;
        [Command("anime", RunMode = RunMode.Async), Summary("Gets an anime from MyAnimeList.Net")]
        public async Task Animuget([Remainder]string animetitle)
        {
            IMessage msg = null;
            var msgs = new List<IMessage>();
            animetitle = animetitle.Replace(" ", "+");
            var byteArray = new UTF8Encoding().GetBytes($"{Config.Load().MALUName}:{Config.Load().MALPassword}");
            var stringifiedxml = await APIWebReq.ReturnString(new Uri($"https://myanimelist.net/api/anime/search.xml?q={animetitle}"), byteArray);
            var xml = new XmlDocument();
            xml.LoadXml(stringifiedxml);
            XObject xNode = XDocument.Parse(xml.FirstChild.NextSibling.OuterXml);
            AnimeArray = JsonConvert.DeserializeObject<AnimeArr>(JObject.Parse(JsonConvert.SerializeXNode(xNode))["anime"].ToString());
            int animenode = AnimeArray.Entry.Count;
            if (animenode > 1)
            {
                string entrymessage = "__Make your selection__ Make your selection within 10 seconds\n";
                int count = 0;
                var entries = new List<string>();
                foreach (var item in AnimeArray.Entry)
                {
                    count++;
                    entries.Add($"**{count}. {item.Title}**");
                }
                if (entries.Count >= 20)
                {
                    entrymessage += String.Join(Environment.NewLine, entries.Take(entries.Count / 2).ToArray());
                    msgs.Add(await MessageHandler.SendChannel(Context.Channel, entrymessage));
                    entrymessage = "";
                    entrymessage += String.Join(Environment.NewLine, entries.Skip(entries.Count / 2).ToArray());
                    msgs.Add(await MessageHandler.SendChannel(Context.Channel, entrymessage));
                }
                else
                {
                    entrymessage += String.Join(Environment.NewLine, entries.ToArray());
                    msg = await MessageHandler.SendChannel(Context.Channel, entrymessage);
                }                
                var response = await NextMessageAsync(fromSourceUser: true, inSourceChannel: true, timeout: TimeSpan.FromSeconds(10));
                await GetAnimeAtPosition(Convert.ToInt32(response.Content), response.Channel);

                if (Context.Guild.CurrentUser.GuildPermissions.ManageMessages)
                {
                    await Context.Message.DeleteAsync();
                    await response.DeleteAsync();
                }
                if (msgs.Count > 0)
                    foreach (var m in msgs)
                        await m.DeleteAsync();
                if (msg != null)
                    await msg.DeleteAsync();
            }
            else
            {
                await GetAnimeAtZeroNonSearch();
            }
        }
        public async Task GetAnimeAtZeroNonSearch()
        {
            await SendAnime(AnimeArray.Entry.First());
        }
        private async Task GetAnimeAtPosition(int position, IMessageChannel channel) =>
            await SendAnime(AnimeArray.Entry[position - 1], channel);
        private async Task SendAnime(Anime animu, IMessageChannel channel = null)
        {
            var embed = new EmbedBuilder
            {
                Color = Tools.Tools.RandomColor(),
                Author = new EmbedAuthorBuilder()
                {
                    Url = $"https://myanimelist.net/anime/{animu.Id}/",
                    Name = animu.Title
                },
                ThumbnailUrl = animu.Image
            };
            embed.AddField("English Title", Tools.Tools.CheckIfStringIsNull(animu.EnglishTitle), inline: true);
            embed.AddField("Synonyms", Tools.Tools.CheckIfStringIsNull(animu.Synonyms), inline: true);
            embed.AddField("Episodes", Tools.Tools.CheckIfStringIsNull(animu.Episodes), inline: true);
            embed.AddField("Start Date", Tools.Tools.CheckIfStringIsNull(animu.StartDate), inline: true);
            embed.AddField("End Date", Tools.Tools.CheckIfStringIsNull(animu.EndDate), inline: true);
            embed.AddField("Score", Tools.Tools.CheckIfStringIsNull(animu.Score), inline: true);
            embed.AddField("Synopsis", Tools.Tools.CheckIfStringIsNull(HttpUtility.HtmlDecode(animu.Synopsis.Split('<')[0])), inline: true);

            await MessageHandler.SendChannel(channel ?? Context.Channel, "", embed);
        }
                
        [Command("weebgif", RunMode = RunMode.Async), Summary("Gets a weeb gif")]
        public async Task WeebGif()
        {
            await MessageHandler.SendChannel(Context.Channel, "", new EmbedBuilder()
            {
                ImageUrl = await APIWebReq.ReturnString(new Uri("http://lucoa.systemexit.co.uk/gifs/reactions/")),
                Color = Tools.Tools.RandomColor()
            });
        }
    }
}
