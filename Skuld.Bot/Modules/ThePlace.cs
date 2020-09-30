using Discord;
using Discord.Commands;
using ImageMagick;
using Microsoft.EntityFrameworkCore;
using Skuld.Bot.Extensions;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Conversion;
using Skuld.Core.Utilities;
using Skuld.Models;
using Skuld.Models.ThePlace;
using Skuld.Services.Accounts.Banking.Models;
using Skuld.Services.Banking;
using Skuld.Bot.Discord.Attributes;
using Skuld.Bot.Discord.Models;
using Skuld.Bot.Discord.Preconditions;
using Skuld.Services.Messaging.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace Skuld.Bot.Modules
{
    [Group("theplace"), Name("ThePlace"), RequireEnabledModule]
    public class ThePlaceModule : ModuleBase<ShardedCommandContext>
    {
        [Command("view"), Summary("View the current image")]
        [Ratelimit(10, 1, Measure.Minutes)]
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

            using MemoryStream outputStream = new MemoryStream();

            image.Save(outputStream, ImageFormat.Png);

            outputStream.Position = 0;

            await msg.DeleteAsync().ConfigureAwait(false);

            await Context.Channel.SendFileAsync(outputStream, "theplace.png").ConfigureAwait(false);
        }

        [Command("place"), Summary("Place a pixel")]
        public async Task PlacePixel(uint x, uint y, [Remainder]System.Drawing.Color colour)
        {
            if(x <= 0 || y <= 0)
            {
                await EmbedExtensions.FromError("I can't process coordinates below 0", Context).QueueMessageAsync(Context);
                return;
            }
            if(x > SkuldAppContext.PLACEIMAGESIZE || y > SkuldAppContext.PLACEIMAGESIZE)
            {
                await EmbedExtensions.FromError("I can't process coordinates above "+SkuldAppContext.PLACEIMAGESIZE, Context).QueueMessageAsync(Context);
                return;
            }

            ulong pixelCost = GetPixelCost(x, y);

            using var Database = new SkuldDbContextFactory().CreateDbContext();

            string prefix = (await Database.InsertOrGetConfigAsync(SkuldAppContext.ConfigurationId)).Prefix;

            if(!Context.IsPrivate)
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
                    await "Lmao you have not enough currency, get a job kekw".QueueMessageAsync(Context).ConfigureAwait(false);
                });

            await Database.SaveChangesAsync().ConfigureAwait(false);
        }

        ulong GetPixelCost(ulong x, ulong y)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            PixelData pixel = Database.PlacePixelData.FirstOrDefault(p => p.XPos == x && p.YPos == y);

            List<PixelHistory> history = Database.PlacePixelHistory.ToList();

            if(history.Any(p => p.PixelId == pixel.Id))
            {
                history = history
                    .Where(p => p.PixelId == pixel.Id)
                    .Where(p => p.ChangedTimestamp >= DateTime.UtcNow.Subtract(TimeSpan.FromDays(7))
                    .ToEpoch())
                    .ToList();

                if (history.Any())
                {
                    return 90 + (ulong)Math.Round(DiscordUtilities.PHI * history.Count, 0);
                }
            }

            return 90;
        }
    }
}
