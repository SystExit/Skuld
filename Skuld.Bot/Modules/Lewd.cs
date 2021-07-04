﻿using Booru.Net;
using Discord;
using Discord.Commands;
using Skuld.APIS;
using Skuld.APIS.NekoLife.Models;
using Skuld.Bot.Discord.Attributes;
using Skuld.Bot.Discord.Preconditions;
using Skuld.Bot.Extensions;
using Skuld.Core.Extensions;
using Skuld.Services.Messaging.Extensions;
using StatsdClient;
using SysEx.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Skuld.Bot.Modules
{
	[
		Group,
		Name("Lewd"),
		RequireNsfw,
		RequireEnabledModule,
		Remarks("🔞 No Sex™")
	]
	public class LewdModule : ModuleBase<ShardedCommandContext>
	{
		public SysExClient SysExClient { get; set; }
		public DanbooruClient DanbooruClient { get; set; }
		public GelbooruClient GelbooruClient { get; set; }
		public Rule34Client Rule34Client { get; set; }
		public E621Client E621Client { get; set; }
		public KonaChanClient KonaChanClient { get; set; }
		public YandereClient YandereClient { get; set; }
		public RealbooruClient RealbooruClient { get; set; }
		public SafebooruClient SafebooruClient { get; set; }
		public NekosLifeClient NekosLifeClient { get; set; }

		[Disabled(true, true, "TOS Concerns")]
		[Command("lewdneko"), Summary("Lewd Neko Grill"), Ratelimit(20, 1, Measure.Minutes)]
		public async Task LewdNeko()
		{
			var neko = await NekosLifeClient.GetAsync(NekoImageType.LewdNeko).ConfigureAwait(false);
			StatsdClient.DogStatsd.Increment("web.get");
			if (neko is not null)
				await EmbedExtensions.FromImage(neko, Color.Purple, Context).QueueMessageAsync(Context).ConfigureAwait(false);
			else
				await EmbedExtensions.FromError("Hmmm <:Thunk:350673785923567616> I got an empty response. Try again.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("lewdkitsune"), Summary("Lewd Kitsunemimi Grill"), Ratelimit(20, 1, Measure.Minutes)]
		public async Task LewdKitsune()
		{
			var kitsu = await SysExClient.GetLewdKitsuneAsync().ConfigureAwait(false);
			StatsdClient.DogStatsd.Increment("web.get");
			await EmbedExtensions.FromMessage(Context).WithImageUrl(kitsu).QueueMessageAsync(Context).ConfigureAwait(false);
		}

		#region ImageBoards

		public static async Task ContainsIllegalTags(
			IEnumerable<string> tags,
			ShardedCommandContext Context
		)
			=> await new StringBuilder("Your tags")
					.Append(" contains ")
					.Append(tags.Count())
					.Append(" banned tag")
					.Append(tags.Count() > 1 ? "s" : "")
					.AppendLine(", please remove them.")
					.Append("Banned Tags Found: ")
					.Append(string.Join(", ", tags))
					.ToString()
					.QueueMessageAsync(Context)
					.ConfigureAwait(false);

		[Command("danbooru"), Summary("Gets stuff from danbooru"), Ratelimit(20, 1, Measure.Minutes)]
		[Alias("dan")]
		[Usage("wholesome")]
		public async Task Danbooru(params string[] tags)
		{
			tags
				.ContainsBlacklistedTags()
				.IsSuccessAsync(x => ContainsIllegalTags(x.Data.As<IEnumerable<string>>(), Context))
				.IsErrorAsync(async x =>
					{
						var posts = await DanbooruClient.GetImagesAsync(tags).ConfigureAwait(false);
						StatsdClient.DogStatsd.Increment("web.get");

						if (posts is null || !posts.Any())
						{
							await EmbedExtensions.FromError("Couldn't find an image.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
							return;
						}

						var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).Random();

						await post.GetMessage(Context).QueueMessageAsync(Context).ConfigureAwait(false);
					}
				);
		}

		[Command("gelbooru"), Summary("Gets stuff from gelbooru"), Ratelimit(20, 1, Measure.Minutes)]
		[Alias("gel")]
		[Usage("wholesome")]
		public async Task Gelbooru(params string[] tags)
		{
			tags
				.ContainsBlacklistedTags()
				.IsSuccessAsync(x => ContainsIllegalTags(x.Data.As<IEnumerable<string>>(), Context))
				.IsErrorAsync(async x =>
					{
						var posts = await GelbooruClient.GetImagesAsync(tags).ConfigureAwait(false);
						StatsdClient.DogStatsd.Increment("web.get");

						if (posts is null || !posts.Any())
						{
							await EmbedExtensions.FromError("Couldn't find an image.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
							return;
						}

						var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).Random();

						await post.GetMessage(Context).QueueMessageAsync(Context).ConfigureAwait(false);
					}
				);
		}

		[Command("rule34"), Summary("Gets stuff from rule34"), Ratelimit(20, 1, Measure.Minutes)]
		[Alias("r34")]
		[Usage("wholesome")]
		public async Task R34(params string[] tags)
		{
			tags
				.ContainsBlacklistedTags()
				.IsSuccessAsync(x => ContainsIllegalTags(x.Data.As<IEnumerable<string>>(), Context))
				.IsErrorAsync(async x =>
					{
						var posts = await Rule34Client.GetImagesAsync(tags).ConfigureAwait(false);
						StatsdClient.DogStatsd.Increment("web.get");

						if (posts is null || !posts.Any())
						{
							await EmbedExtensions.FromError("Couldn't find an image.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
							return;
						}

						var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).Random();

						await post.GetMessage(Context).QueueMessageAsync(Context).ConfigureAwait(false);
					}
				);
		}

		[Command("e621"), Summary("Gets stuff from e621"), Ratelimit(20, 1, Measure.Minutes)]
		[Usage("wholesome")]
		public async Task E621(params string[] tags)
		{
			tags
				.ContainsBlacklistedTags()
				.IsSuccessAsync(x => ContainsIllegalTags(x.Data.As<IEnumerable<string>>(), Context))
				.IsErrorAsync(async x =>
					{
						var posts = await E621Client.GetImagesAsync(tags).ConfigureAwait(false);
						StatsdClient.DogStatsd.Increment("web.get");

						if (posts is null || !posts.Any())
						{
							await EmbedExtensions.FromError("Couldn't find an image.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
							return;
						}

						var post = posts.Where(x => x.Tags.All(z => !z.Value.ContainsBlacklistedTags().Successful)).Random();

						await post.GetMessage(Context).QueueMessageAsync(Context).ConfigureAwait(false);
					}
				);
		}

		[Command("konachan"), Summary("Gets stuff from konachan"), Ratelimit(20, 1, Measure.Minutes)]
		[Alias("kona", "kc")]
		[Usage("wholesome")]
		public async Task KonaChan(params string[] tags)
		{
			if (tags.Length > 6)
			{
				await $"Cannot process more than 6 tags at a time".QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}
			tags
				.ContainsBlacklistedTags()
				.IsSuccessAsync(x => ContainsIllegalTags(x.Data.As<IEnumerable<string>>(), Context))
				.IsErrorAsync(async x =>
					{
						var posts = await KonaChanClient.GetImagesAsync(tags).ConfigureAwait(false);
						StatsdClient.DogStatsd.Increment("web.get");

						if (posts is null || !posts.Any())
						{
							await EmbedExtensions.FromError("Couldn't find an image.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
							return;
						}

						var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).Random();

						await post.GetMessage(Context).QueueMessageAsync(Context).ConfigureAwait(false);
					}
				);
		}

		[Command("yandere"), Summary("Gets stuff from yandere"), Ratelimit(20, 1, Measure.Minutes)]
		[Usage("wholesome")]
		public async Task Yandere(params string[] tags)
		{
			tags
				.ContainsBlacklistedTags()
				.IsSuccessAsync(x => ContainsIllegalTags(x.Data.As<IEnumerable<string>>(), Context))
				.IsErrorAsync(async x =>
					{
						var posts = await YandereClient.GetImagesAsync(tags).ConfigureAwait(false);
						StatsdClient.DogStatsd.Increment("web.get");

						if (posts is null || !posts.Any())
						{
							await EmbedExtensions.FromError("Couldn't find an image.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
							return;
						}

						var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).Random();

						await post.GetMessage(Context).QueueMessageAsync(Context).ConfigureAwait(false);
					}
				);
		}

		[
			Command("real"),
			Summary("Gets stuff from Realbooru"),
			Ratelimit(20, 1, Measure.Minutes),
			Usage("wholesome")
		]
		public async Task Realbooru(params string[] tags)
		{
			tags
				.ContainsBlacklistedTags()
				.IsSuccessAsync(x => ContainsIllegalTags(x.Data.As<IEnumerable<string>>(), Context))
				.IsErrorAsync(async x =>
					{
						var posts = await RealbooruClient.GetImagesAsync(tags).ConfigureAwait(false);
						StatsdClient.DogStatsd.Increment("web.get");

						if (posts is null || !posts.Any())
						{
							await EmbedExtensions.FromError("Couldn't find an image.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
							return;
						}

						var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).Random();

						await post.GetMessage(Context).QueueMessageAsync(Context).ConfigureAwait(false);
					}
				);
		}

		[
			Command("hentaibomb"),
			Summary("\"bombs\" the chat with images from all boorus"),
			Ratelimit(20, 1, Measure.Minutes),
			Usage("wholesome")
		]
		public async Task HentaiBomb([Remainder] string tags = null)
		{
			string[] localTags = Array.Empty<string>();

			if (tags is not null)
			{
				localTags = tags.Split(' ').ToArray();
			}

			localTags
				.ContainsBlacklistedTags()
				.IsSuccessAsync(x => ContainsIllegalTags(x.Data.As<IEnumerable<string>>(), Context))
				.IsError(x =>
					{
						""
						.ThenAsync(async x =>
						{
							var posts = await YandereClient.GetImagesAsync(tags).ConfigureAwait(false);
							StatsdClient.DogStatsd.Increment("web.get");
							if (posts is not null && posts.Any())
							{
								var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).Random();
								if (post is not null)
								{
									x = $"Yande.re: {post.GetMessage(Context, true)}\n";
								}
							}

							return x;
						})
						.ThenAsync(async x =>
						{
							if (localTags.Length < 6)
							{
								var posts = await KonaChanClient.GetImagesAsync(tags).ConfigureAwait(false);
								StatsdClient.DogStatsd.Increment("web.get");
								if (posts is not null && posts.Any())
								{
									var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).Random();
									if (post is not null)
									{
										x += $"Konachan {post.GetMessage(Context, true)}\n";
									}
								}
							}

							return x;
						})
						.ThenAsync(async x =>
						{
							var posts = await E621Client.GetImagesAsync(tags).ConfigureAwait(false);
							StatsdClient.DogStatsd.Increment("web.get");
							if (posts is not null && posts.Any())
							{
								var post = posts.Where(x => !x.Tags.All(z => z.Value.ContainsBlacklistedTags().Successful)).Random();
								if (post is not null)
								{
									x += $"E621: {post.GetMessage(Context, true)}\n";
								}
							}

							return x;
						})
						.ThenAsync(async x =>
						{
							var posts = await Rule34Client.GetImagesAsync(tags).ConfigureAwait(false);
							StatsdClient.DogStatsd.Increment("web.get");
							if (posts is not null && posts.Any())
							{
								var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).Random();
								if (post is not null)
								{
									x += $"R34: {post.GetMessage(Context, true)}\n";
								}
							}

							return x;
						})
						.ThenAsync(async x =>
						{
							var posts = await GelbooruClient.GetImagesAsync(tags).ConfigureAwait(false);
							StatsdClient.DogStatsd.Increment("web.get");
							if (posts is not null && posts.Any())
							{
								var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).Random();
								if (post is not null)
								{
									x += $"Gelbooru: {post.GetMessage(Context, true)}\n";
								}
							}

							return x;
						})
						.ThenAsync(async x =>
						{
							var posts = await DanbooruClient.GetImagesAsync(tags).ConfigureAwait(false);
							StatsdClient.DogStatsd.Increment("web.get");
							if (posts is not null && posts.Any())
							{
								var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).Random();
								if (post is not null)
								{
									x += $"Danbooru: {post.GetMessage(Context, true)}\n";
								}
							}

							return x;
						})
						.Then(async x =>
						{
							if (!string.IsNullOrEmpty((string)x))
							{
								await x.QueueMessageAsync(Context).ConfigureAwait(false);
							}
						});
					}
				);
		}


		[Command("safebooru"), Summary("Gets stuff from safebooru"), Ratelimit(20, 1, Measure.Minutes)]
		[Alias("safe")]
		public async Task Safebooru(params string[] tags)
		{
			tags
				.ContainsBlacklistedTags()
				.IsSuccessAsync(x => ContainsIllegalTags(x.Data.As<IEnumerable<string>>(), Context))
				.IsErrorAsync(async x =>
				{
					var posts = await SafebooruClient
						.GetImagesAsync(tags)
					.ConfigureAwait(false);

					DogStatsd.Increment("web.get");

					if (posts is null || !posts.Any())
					{
						await EmbedExtensions
							.FromError(
								"Couldn't find an image.",
								Context
							)
							.QueueMessageAsync(Context)
						.ConfigureAwait(false);
						return;
					}

					var post = posts.Where(x => !x.Tags.ContainsBlacklistedTags().Successful).Random();

					await post
							.GetMessage(Context)
							.QueueMessageAsync(Context)
							.ConfigureAwait(false);
				});
		}

		#endregion ImageBoards
	}
}
