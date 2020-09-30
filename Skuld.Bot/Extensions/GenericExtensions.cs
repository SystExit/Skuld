using Skuld.Bot.Models;
using Skuld.Core;
using Skuld.Models.ThePlace;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Drawing.Drawing2D;

namespace Skuld.Bot.Extensions
{
    public static class GenericExtensions
    {
        public static Weightable<T> GetRandomWeightedValue<T>(this IList<Weightable<T>> items)
        {
            if (items == null || items.Count == 0) return null;

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
        public static (int width, int height) GetWidthAndHeight(this List<PixelData> pixelData)
        {
            uint width = 0;
            uint height = 0;

            foreach (var pixelDatum in pixelData)
            {
                if (pixelDatum.XPos > width)
                {
                    width = pixelDatum.XPos;
                }
                if (pixelDatum.YPos > height)
                {
                    height = pixelDatum.YPos;
                }
            }

            return ((int)width, (int)height);
        }

        public static Bitmap ResizeBitmap(this Bitmap sourceBMP, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(sourceBMP, 0, 0, width, height);
            }
            return result;
        }

        public static Bitmap WritePixelDataBitmap(this List<PixelData> pixelData)
        {
            var (width, height) = pixelData.GetWidthAndHeight();

            Bitmap b = new Bitmap(width, height);

            foreach (var pixelDatum in pixelData)
            {
                var color = Color.FromArgb(255, pixelDatum.R, pixelDatum.G, pixelDatum.B);
                b.SetPixel((int)pixelDatum.XPos - 1, (int)pixelDatum.YPos - 1, color);
            }

            return b;
        }
    }
}