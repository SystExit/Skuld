using Discord;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Security.Cryptography;
using System.Text;

namespace Skuld.Core.Extensions
{
    public static class GenericExtensions
    {
        private static readonly string[] VideoExtensions = {
            ".webm",
            ".mkv",
            ".flv",
            ".vob",
            ".ogv",
            ".ogg",
            ".avi",
            ".mov",
            ".qt",
            ".wmv",
            ".mp4",
            ".m4v",
            ".mpg",
            ".mpeg"
        };

        private static readonly string[] ImageExtensions =
        {
            ".jpg",
            ".bmp",
            ".gif",
            ".png",
            ".apng"
        };

        //https://stackoverflow.com/a/1262619
        public static void Shuffle<T>(this IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /*public static LogSeverity ToDiscord(this NTwitch.LogSeverity logSeverity)
        {
            if (logSeverity == NTwitch.LogSeverity.Critical)
                return LogSeverity.Critical;
            if (logSeverity == NTwitch.LogSeverity.Debug)
                return LogSeverity.Debug;
            if (logSeverity == NTwitch.LogSeverity.Error)
                return LogSeverity.Error;
            if (logSeverity == NTwitch.LogSeverity.Info)
                return LogSeverity.Info;
            if (logSeverity == NTwitch.LogSeverity.Verbose)
                return LogSeverity.Verbose;
            if (logSeverity == NTwitch.LogSeverity.Warning)
                return LogSeverity.Warning;

            return LogSeverity.Verbose;
        }*/

        public static ConsoleColor SeverityToColor(this LogSeverity sev)
        {
            if (sev == LogSeverity.Critical)
                return ConsoleColor.Red;
            if (sev == LogSeverity.Error)
                return ConsoleColor.Red;
            if (sev == LogSeverity.Info)
                return ConsoleColor.Green;
            if (sev == LogSeverity.Warning)
                return ConsoleColor.Yellow;
            if (sev == LogSeverity.Verbose)
                return ConsoleColor.Cyan;
            return ConsoleColor.White;
        }

        public static bool ToBool(this string data)
        {
            if (data.ToLowerInvariant() == "true")
                return true;
            if (data.ToLowerInvariant() == "false")
                return false;
            if (data == "1")
                return true;
            if (data == "0")
                return false;

            throw new Exception("Cannot Convert from \"" + data + "\" to Boolean");
        }

        public static MemoryStream ToMemoryStream(this string value)
            => new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));

        public static Uri ToUri(this string value)
            => new Uri(value);

        public static bool IsImageExtension(this string input)
        {
            foreach (var ext in ImageExtensions)
            {
                if (input.Contains(ext))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsVideoFile(this string input)
        {
            foreach (var x in VideoExtensions)
            {
                if (input.Contains(x) || input.EndsWith(x))
                    return true;
            }
            return false;
        }

        public static bool IsWebsite(this string input)
        {
            try
            {
                if (new Uri(input) != null)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidOsuSig(this FileStream fs)
        {
            var header = new byte[4];

            fs.Read(header, 0, 4);

            var strHeader = Encoding.ASCII.GetString(header);
            return strHeader.ToLower().EndsWith("png");
        }

        public static string CheckEmptyWithLocale(this int? val, ResourceManager loc)
        {
            if (val.HasValue)
            {
                return Convert.ToString(val);
            }
            return loc.GetString("SKULD_GENERIC_EMPTY");
        }

        public static string CheckEmptyWithLocale(this string[] val, string seperator, ResourceManager loc)
        {
            if (val.Length == 0)
            {
                return loc.GetString("SKULD_GENERIC_EMPTY");
            }
            else
            {
                string msg = "";
                foreach (var item in val)
                {
                    var itm = item.CheckEmptyWithLocale(loc);
                    if (itm != loc.GetString("SKULD_GENERIC_EMPTY"))
                    {
                        msg += itm + seperator;
                    }
                }
                msg = msg.Remove(msg.Length - seperator.Length);
                return msg;
            }
        }

        public static string CheckEmptyWithLocale(this string val, ResourceManager loc)
            => val ?? loc.GetString("SKULD_GENERIC_EMPTY");

        public static string CheckForNull(this string s)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s))
                return null;
            else
                return s;
        }

        public static DateTime FromEpoch(this ulong epoch)
            => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToDouble(epoch));

        public static ulong ToEpoch(this DateTime dateTime)
            => (ulong)(dateTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

        public static IList<string> PaginateList(this string[] list)
        {
            var pages = new List<string>();
            string pagetext = "";

            for (int x = 0; x < list.Count(); x++)
            {
                pagetext += $"{list[x]}\n";

                if ((x + 1) % 10 == 0 || (x + 1) == list.Count())
                {
                    pages.Add(pagetext);
                    pagetext = "";
                }
            }

            return pages;
        }

        public static IList<string> PaginateCodeBlockList(this string[] list)
        {
            var pages = new List<string>();
            string pagetext = "```cs\n";

            for (int x = 0; x < list.Count(); x++)
            {
                pagetext += $"{list[x]}\n";

                if ((x + 1) % 10 == 0 || (x + 1) == list.Count())
                {
                    pagetext += "```";
                    pages.Add(pagetext);
                    pagetext = "```cs\n";
                }
            }

            return pages;
        }

        public static string GetStringfromOffset(this DateTimeOffset dateTimeOffset, DateTime dateTime)
        {
            var thing = dateTime - dateTimeOffset;
            string rtnstrng = "";
            int days = (int)Math.Floor(thing.TotalDays);
            rtnstrng += days + " days ";
            return rtnstrng + "ago";
        }

        public static double Remap(this double value, double a0, double a1, double b0, double b1)
            => b0 + (b1 - b0) * ((value - a0) / (a1 - a0));

        public static bool IsRecurring(this uint val, int startLimit)
        {
            var str = Convert.ToString(val);
            var iarr = new List<int>();
            foreach(var ch in str)
            {
                iarr.Add(Convert.ToInt32(Convert.ToString(ch)));
            }

            var same = iarr.All(x=>x == iarr[0]);

            return (same && iarr.Count() > startLimit);
        }

        //https://gist.github.com/starquake/8d72f1e55c0176d8240ed336f92116e3
        public static string StripHtml(this string value)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(value);

            if (htmlDoc == null)
                return value;

            return htmlDoc.DocumentNode.InnerText;
        }
    }
}