using Discord;
using System.IO;
using System.Text;
using System;
using System.Resources;

namespace Skuld.Tools
{
    public class Tools
    {
        private static readonly string[] _validExtensions = { ".jpg", ".bmp", ".gif", ".png" };
        public static bool IsImageExtension(string url)
        {
            foreach (var ext in _validExtensions)
            {
                if (url.Contains(ext))
                {
                    return true;
                }
            }

            return false;
        }
        public static Color RandomColor()
        {
            var bytes = new byte[3];
            Bot.random.NextBytes(bytes);
            return new Color(bytes[0], bytes[1], bytes[2]);
        }
        public static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }
        public static int ParseInt32OrDefault(string s)
        {
            if (Int32.TryParse(s, out int tmp))
                return tmp;
            else
                return 0;
        }
        public static uint ParseUInt32OrDefault(string s)
        {
            if (UInt32.TryParse(s, out uint tmp))
                return tmp;
            else
                return 0;
        }
        public static UInt64 ParseUInt64OrDefault(string input)
        {
            if (UInt64.TryParse(input, out ulong tmp))
                return tmp;
            else
                return 0;
        }

        public static string CheckForEmpty(string s)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s))
                return "SKULD_GENERIC_EMPTY";
            else
                return s;
        }
        public static string CheckForEmptyWithLocale(string s, ResourceManager locale)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s))
                return locale.GetString("SKULD_GENERIC_EMPTY");
            else
                return s;
        }

        public static string CheckForNull(string s)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s))
                return null;
            else
                return s;
        }

        public static ConsoleColor ColorBasedOnSeverity(LogSeverity sev)
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
    }
}
