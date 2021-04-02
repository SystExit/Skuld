using Skuld.Bot.Models;
using Skuld.Core;
using Skuld.Models.ThePlace;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Color = System.Drawing.Color;

namespace Skuld.Bot.Extensions
{
	public static class GenericExtensions
	{
		public static Weightable<T> GetRandomWeightedValue<T>(this IList<Weightable<T>> items)
		{
			if (items is null || items.Count == 0) return null;

			var randomWeight = SkuldRandom.Next(items.Sum(x => x.Weight));

			Weightable<T> item = null;

			foreach (var entry in items)
			{
				item = entry;

				randomWeight -= entry.Weight;

				if (randomWeight <= 0)
					break;
			}

			return item;
		}
		public static (uint width, uint height) GetWidthAndHeight(this List<PixelData> pixelData)
		{
			if (pixelData.Count > 0)
			{
				return (0, 0);
			}

			uint width = pixelData.Max(datum => datum.XPos);
			uint height = pixelData.Max(datum => datum.YPos);

			return (width, height);
		}

		public static Bitmap ResizeBitmap(this Bitmap sourceBMP, int width, int height)
		{
			Bitmap result = new(width, height);
			using (Graphics g = Graphics.FromImage(result))
			{
				g.SmoothingMode = SmoothingMode.HighQuality;
				g.PixelOffsetMode = PixelOffsetMode.Half;
				g.InterpolationMode = InterpolationMode.NearestNeighbor;
				g.DrawImage(sourceBMP, 0, 0, width, height);
			}
			return result;
		}

		public static Bitmap WritePixelDataBitmap([NotNull] this List<PixelData> pixelData)
		{
			var (width, height) = pixelData.GetWidthAndHeight();

			Bitmap b = new((int)width, (int)height);

			foreach (var pixelDatum in pixelData)
			{
				var color = Color.FromArgb(255, pixelDatum.R, pixelDatum.G, pixelDatum.B);
				b.SetPixel((int)pixelDatum.XPos - 1, (int)pixelDatum.YPos - 1, color);
			}

			return b;
		}
	}
}