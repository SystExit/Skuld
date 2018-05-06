using System.Threading.Tasks;
using Discord.Commands;
using Skuld.APIS;
using Skuld.Tools;
using Discord;
using System.Text.RegularExpressions;
using Discord.Addons.Interactive;
using Skuld.Services;
using Skuld.Extensions;

namespace Skuld.Modules
{
	[Group, Name("Lewd")]
    public class Lewd : InteractiveBase<ShardedCommandContext>
    {
		public MessageService MessageService { get; set; }
		public AnimalAPIS Animals { get; set; }
		public SysExClient SysExClient { get; set; }
		public BooruClient BooruClient { get; set; }
		
		[Command("lewdneko"), Summary("Lewd Neko Grill"), RequireNsfw, Ratelimit(20, 1, Measure.Minutes)]
		public async Task LewdNeko()
		{
			var neko = await Animals.GetLewdNekoAsync();
			if (neko != null)
				await MessageService.SendChannelAsync(Context.Channel, "", new EmbedBuilder { ImageUrl = neko }.Build());
			else
				await MessageService.SendChannelAsync(Context.Channel, "Hmmm <:Thunk:350673785923567616>, I got an empty response.");
		}

		[Command("lewdkitsune"), Summary("Lewd Kitsunemimi Grill"), RequireNsfw, Ratelimit(20, 1, Measure.Minutes)]
		public async Task LewdKitsune()
		{
			var kitsu = await SysExClient.GetLewdKitsuneAsync();
			await MessageService.SendChannelAsync(Context.Channel, "", new EmbedBuilder { ImageUrl = kitsu }.Build());
		}

		[Command("danbooru"), Summary("Gets stuff from danbooru"), Ratelimit(20, 1, Measure.Minutes)]
		[Alias("dan")]
		public async Task Danbooru(params string[] tags)
		{
			if (BooruClient.ContainsBlacklistedTags(tags))
			{
				await MessageService.SendChannelAsync(Context.Channel, "Your tags contains a banned tag, please remove it.");
			}
			else
			{
				var posts = await BooruClient.GetDanbooruImagesAsync(tags);
				if(posts!=null)
				{
					var post = posts.GetRandomImage();
					if (post != null)
					{
						string message = "<" + post.PostUrl + ">\n" + post.ImageUrl;
						await MessageService.SendChannelAsync(Context.Channel, message);
					}
				}
				await MessageService.SendChannelAsync(Context.Channel, "Couldn't find an image");				
			}
		}

		[Command("gelbooru"), Summary("Gets stuff from gelbooru"), RequireNsfw, Ratelimit(20, 1, Measure.Minutes)]
		[Alias("gel")]
		public async Task Gelbooru(params string[] tags)
		{
			if (BooruClient.ContainsBlacklistedTags(tags))
			{
				await MessageService.SendChannelAsync(Context.Channel, "Your tags contains a banned tag, please remove it.");
				return;
			}
			else
			{
				var posts = await BooruClient.GetGelbooruImagesAsync(tags);
				if (posts != null)
				{
					var post = posts.GetRandomImage();
					if (post != null)
					{
						string message = "<" + post.PostUrl + ">\n" + post.ImageUrl;
						await MessageService.SendChannelAsync(Context.Channel, message);
					}
				}
				await MessageService.SendChannelAsync(Context.Channel, "Couldn't find an image");
			}
		}

		[Command("rule34"), Summary("Gets stuff from rule34"), RequireNsfw, Ratelimit(20, 1, Measure.Minutes)]
		[Alias("r34")]
		public async Task R34(params string[] tags)
		{
			if (BooruClient.ContainsBlacklistedTags(tags))
			{
				await MessageService.SendChannelAsync(Context.Channel, "Your tags contains a banned tag, please remove it.");
				return;
			}
			else
			{
				var posts = await BooruClient.GetRule34ImagesAsync(tags);
				if (posts != null)
				{
					var post = posts.GetRandomImage();
					if (post != null)
					{
						string message = "<" + post.PostUrl + ">\n" + post.ImageUrl;
						await MessageService.SendChannelAsync(Context.Channel, message);
					}
				}
				await MessageService.SendChannelAsync(Context.Channel, "Couldn't find an image");
			}
		}

		[Command("e621"), Summary("Gets stuff from e621"), RequireNsfw, Ratelimit(20, 1, Measure.Minutes)]
		public async Task E621(params string[] tags)
		{
			if (BooruClient.ContainsBlacklistedTags(tags))
			{
				await MessageService.SendChannelAsync(Context.Channel, "Your tags contains a banned tag, please remove it.");
				return;
			}
			else
			{
				var posts = await BooruClient.GetE621ImagesAsync(tags);
				if (posts != null)
				{
					var post = posts.GetRandomImage();
					if (post != null)
					{
						string message = "<" + post.PostUrl + ">\n" + post.ImageUrl;
						await MessageService.SendChannelAsync(Context.Channel, message);
					}
				}
				await MessageService.SendChannelAsync(Context.Channel, "Couldn't find an image");
			}
		}

		[Command("konachan"), Summary("Gets stuff from konachan"), RequireNsfw, Ratelimit(20, 1, Measure.Minutes)]
		[Alias("kona", "kc")]
		public async Task KonaChan(params string[] tags)
		{
			if (BooruClient.ContainsBlacklistedTags(tags))
			{
				await MessageService.SendChannelAsync(Context.Channel, "Your tags contains a banned tag, please remove it.");
				return;
			}
			else
			{
				var posts = await BooruClient.GetKonaChanImagesAsync(tags);
				if (posts != null)
				{
					var post = posts.GetRandomImage();
					if (post != null)
					{
						string message = "<" + post.PostUrl + ">\n" + post.ImageUrl;
						await MessageService.SendChannelAsync(Context.Channel, message);
					}
				}
				await MessageService.SendChannelAsync(Context.Channel, "Couldn't find an image");
			}
		}

		[Command("yandere"), Summary("Gets stuff from yandere"), RequireNsfw, Ratelimit(20, 1, Measure.Minutes)]
		public async Task Yandere(params string[] tags)
		{
			if (BooruClient.ContainsBlacklistedTags(tags))
			{
				await MessageService.SendChannelAsync(Context.Channel, "Your tags contains a banned tag, please remove it.");
				return;
			}
			else
			{
				var posts = await BooruClient.GetYandereImagesAsync(tags);
				if (posts != null)
				{
					var post = posts.GetRandomImage();
					if (post != null)
					{
						string message = "<" + post.PostUrl + ">\n" + post.ImageUrl;
						await MessageService.SendChannelAsync(Context.Channel, message);
					}
				}
				await MessageService.SendChannelAsync(Context.Channel, "Couldn't find an image");
			}
		}
	}
}
