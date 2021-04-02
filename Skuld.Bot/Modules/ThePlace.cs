using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Skuld.Bot.Discord.Attributes;
using Skuld.Bot.Discord.Preconditions;
using Skuld.Bot.Extensions;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Verification;
using Skuld.Core.Utilities;
using Skuld.Models;
using Skuld.Models.ThePlace;
using Skuld.Services.Accounts.Banking.Models;
using Skuld.Services.Banking;
using Skuld.Services.Messaging.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace Skuld.Bot.Modules
{
	[
		Group("theplace"),
		Name("ThePlace"),
		RequireDatabase,
		RequireEnabledModule,
		Remarks("🎨 Draw on The Place")
	]
	public class ThePlaceModule : ModuleBase<ShardedCommandContext>
	{
		[
			Command("view"),
			Summary("View the current image"),
			Usage("theplace view"),
			Ratelimit(10, 1, Measure.Minutes)
		]
		public async Task ViewImage()
		{
			var msg = await "Please wait... Generating the place"
				.QueueMessageAsync(Context)
				.ConfigureAwait(false);

			using var Database = new SkuldDbContextFactory().CreateDbContext();

			List<PixelData> pixelData = Database.PlacePixelData
				.AsNoTracking()
				.ToList();

			int size = SkuldAppContext.PLACEIMAGESIZE * 4;

			using Bitmap image = pixelData
				.WritePixelDataBitmap()
				.ResizeBitmap(size, size);

			using MemoryStream outputStream = new();

			image.Save(outputStream, ImageFormat.Png);

			outputStream.Position = 0;

			await msg.DeleteAsync().ConfigureAwait(false);

			await Context.Channel.SendFileAsync(outputStream, "theplace.png").ConfigureAwait(false);
		}

		[
			Command("place"),
			Summary("Place a pixel"),
			Usage("theplace place 5 10 #00ff00"),
			Ratelimit(10, 1, Measure.Minutes)
		]
		public async Task PlacePixel(int x, int y, [Remainder] Color colour)
		{
			if (x <= 0 || y <= 0)
			{
				await EmbedExtensions.FromError("I can't process coordinates below 0", Context).QueueMessageAsync(Context);
				return;
			}
			if (x > SkuldAppContext.PLACEIMAGESIZE || y > SkuldAppContext.PLACEIMAGESIZE)
			{
				await EmbedExtensions.FromError("I can't process coordinates above " + SkuldAppContext.PLACEIMAGESIZE, Context).QueueMessageAsync(Context);
				return;
			}

			ulong pixelCost = GetPixelCost(x, y);

			using var Database = new SkuldDbContextFactory().CreateDbContext();

			string prefix = (await Database.InsertOrGetConfigAsync(SkuldAppContext.ConfigurationId)).Prefix;

			if (!Context.IsPrivate)
			{
				prefix = (await Database.InsertOrGetGuildAsync(Context.Guild)).Prefix;
			}

			var dbUser = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

			TransactionService.DoTransaction(new TransactionStruct
			{
				Sender = dbUser,
				Amount = pixelCost
			})
				.IsSuccessAsync(async _ =>
				{
					using var db = new SkuldDbContextFactory().CreateDbContext();

					var pixel = db.PlacePixelData.FirstOrDefault(p => p.XPos == x && p.YPos == y);

					pixel.R = colour.R;
					pixel.G = colour.G;
					pixel.B = colour.B;

					await db.SaveChangesAsync().ConfigureAwait(false);

					db.PlacePixelHistory.Add(new PixelHistory
					{
						PixelId = pixel.Id,
						ChangedTimestamp = DateTime.UtcNow.ToEpoch(),
						CostToChange = pixelCost,
						ModifierId = Context.User.Id
					});

					await db.SaveChangesAsync().ConfigureAwait(false);

					await $"Set it, use `{prefix}theplace view` to view it".QueueMessageAsync(Context).ConfigureAwait(false);
				})
				.IsErrorAsync(async _ =>
				{
					await "You don't have enough currency".QueueMessageAsync(Context).ConfigureAwait(false);
				});

			await Database.SaveChangesAsync().ConfigureAwait(false);
		}

		[
			Command("place"),
			Summary("Place an image"),
			Usage("theplace place 5 10 https://example.com/exampleImage.png"),
			Ratelimit(10, 1, Measure.Minutes)
		]
		public async Task PlaceImage(int x, int y, [Remainder] string image)
		{
			if (x <= 0 || y <= 0)
			{
				await EmbedExtensions.FromError("I can't process coordinates below 0", Context).QueueMessageAsync(Context);
				return;
			}
			if (x > SkuldAppContext.PLACEIMAGESIZE || y > SkuldAppContext.PLACEIMAGESIZE)
			{
				await EmbedExtensions.FromError("I can't process coordinates above " + SkuldAppContext.PLACEIMAGESIZE, Context).QueueMessageAsync(Context);
				return;
			}

			if (!image.IsWebsite() && !image.IsImageExtension())
			{
				await EmbedExtensions.FromError("You haven't provided an image link", Context).QueueMessageAsync(Context);
				return;
			}

			Bitmap bitmapImage;

			try
			{
				bitmapImage = new(await HttpWebClient.GetStreamAsync(new Uri(image)));
			}
			catch (Exception ex)
			{
				await EmbedExtensions.FromError("Couldn't process image", Context).QueueMessageAsync(Context);
				Log.Error("ThePlace", ex.Message, Context, ex);
				return;
			}

			if (bitmapImage is null)
			{
				await EmbedExtensions.FromError("Couldn't process image", Context).QueueMessageAsync(Context);
				Log.Error("ThePlace", "Couldn't load image", Context);
				return;
			}

			double aspectRatio = (double)bitmapImage.Width / bitmapImage.Height;

			if (bitmapImage.Width > SkuldAppContext.PLACEIMAGESIZE - x)
			{
				double otherAspect = (double)bitmapImage.Height / bitmapImage.Width;

				int newHeight = (int)Math.Min(Math.Round(SkuldAppContext.PLACEIMAGESIZE - y * otherAspect), SkuldAppContext.PLACEIMAGESIZE - y);
				bitmapImage = bitmapImage.ResizeBitmap(SkuldAppContext.PLACEIMAGESIZE - x, newHeight);
			}

			if (bitmapImage.Height > SkuldAppContext.PLACEIMAGESIZE - y)
			{
				int newWidth = (int)Math.Min(Math.Round(SkuldAppContext.PLACEIMAGESIZE - x * aspectRatio), SkuldAppContext.PLACEIMAGESIZE - x);
				bitmapImage = bitmapImage.ResizeBitmap(newWidth, SkuldAppContext.PLACEIMAGESIZE - y);
			}

			ulong pixelCost = 0;

			for (int bx = 0; bx < bitmapImage.Width; bx++)
			{
				for (int by = 0; by < bitmapImage.Height; by++)
				{
					pixelCost += GetPixelCost(x + bx, y + by);
				}
			}

			using var Database = new SkuldDbContextFactory().CreateDbContext();

			string prefix = (await Database.InsertOrGetConfigAsync(SkuldAppContext.ConfigurationId)).Prefix;

			if (!Context.IsPrivate)
			{
				prefix = (await Database.InsertOrGetGuildAsync(Context.Guild)).Prefix;
			}

			var dbUser = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

			TransactionService.DoTransaction(new TransactionStruct
			{
				Sender = dbUser,
				Amount = pixelCost
			})
				.IsSuccessAsync(async _ =>
				{
					using var PixelDb = new SkuldDbContextFactory().CreateDbContext();
					using var historyDb = new SkuldDbContextFactory().CreateDbContext();

					for (int bx = 0; bx < bitmapImage.Width; bx++)
					{
						for (int by = 0; by < bitmapImage.Height; by++)
						{
							var pixel = PixelDb.PlacePixelData.FirstOrDefault(p => p.XPos == x + bx && p.YPos == y + by);

							var colour = bitmapImage.GetPixel(bx, by);

							pixel.R = colour.R;
							pixel.G = colour.G;
							pixel.B = colour.B;

							historyDb.PlacePixelHistory.Add(new PixelHistory
							{
								PixelId = pixel.Id,
								ChangedTimestamp = DateTime.UtcNow.ToEpoch(),
								CostToChange = pixelCost,
								ModifierId = Context.User.Id
							});
						}
					}

					await PixelDb.SaveChangesAsync().ConfigureAwait(false);
					await historyDb.SaveChangesAsync().ConfigureAwait(false);

					await $"Set it, use `{prefix}theplace view` to view it".QueueMessageAsync(Context).ConfigureAwait(false);
				})
				.IsErrorAsync(async _ =>
				{
					await "You don't have enough currency".QueueMessageAsync(Context).ConfigureAwait(false);
				});

			await Database.SaveChangesAsync().ConfigureAwait(false);
		}

		static ulong GetPixelCost(int x, int y)
			=> GetPixelCost((ulong)x, (ulong)y);

		static ulong GetPixelCost(ulong x, ulong y)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			PixelData pixel = Database.PlacePixelData.AsNoTracking().FirstOrDefault(p => p.XPos == x && p.YPos == y);

			List<PixelHistory> history = Database.PlacePixelHistory.AsNoTracking().ToList();

			if (history.Any(p => p.PixelId == pixel.Id))
			{
				history = history
					.Where(p => p.PixelId == pixel.Id)
					.Where(p => p.ChangedTimestamp >= DateTime.UtcNow.Subtract(TimeSpan.FromDays(7))
					.ToEpoch())
					.ToList();

				if (history.Any())
				{
					return 90 + (ulong)Math.Round((DiscordUtilities.PHI * history.Count) * 5, 0);
				}
			}

			return 90;
		}
	}
}
