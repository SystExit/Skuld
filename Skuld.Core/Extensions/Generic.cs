using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.IO;
using HtmlAgilityPack;
using System.Linq;
using Discord;

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
            ".png"
        };

        public static LogSeverity ToDiscord(this NTwitch.LogSeverity logSeverity)
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
        }

        public static ConsoleColor SeverityToColor(this LogSeverity sev)
        {
            if (sev == LogSeverity.Critical)
                return ConsoleColor.DarkRed;
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
            if (input.Contains('.') || input.Contains("www.") ||
                input.Contains("http://") || input.Contains("https://"))
            {
                return true;
            }

            return false;
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
                    pages.Add("```");
                    pagetext = "";

                    if ((x + 1) % 10 == 0)
                    {
                        pages.Add("```cs\n");
                    }
                }
            }

            return pages;
        }

        public static string GetStringfromOffset(this DateTimeOffset dateTimeOffset, DateTime dateTime)
        {
            var thing = dateTime - dateTimeOffset;
            string rtnstrng = "";
            var temp = thing.TotalDays - Math.Floor(thing.TotalDays);
            int days = (int)Math.Floor(thing.TotalDays);
            rtnstrng += days + " days ";
            return rtnstrng + "ago";
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