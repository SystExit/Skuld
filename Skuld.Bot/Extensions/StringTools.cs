using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skuld.Bot.Extensions
{
    public static class StringTools
    {
        private static readonly Dictionary<string, int> alphabet = new Dictionary<string, int>
        {
            { "a", 0 },
            { "b", 1 },
            { "c", 2 },
            { "d", 3 },
            { "e", 4 },
            { "f", 5 },
            { "g", 6 },
            { "h", 7 },
            { "i", 8 },
            { "j", 9 },
            { "k", 10 },
            { "l", 11 },
            { "m", 12 },
            { "n", 13 },
            { "o", 14 },
            { "p", 15 },
            { "q", 16 },
            { "r", 17 },
            { "s", 18 },
            { "t", 19 },
            { "u", 20 },
            { "v", 21 },
            { "w", 22 },
            { "x", 23 },
            { "y", 24 },
            { "z", 25 }
        };

        private static readonly string[] alphaDance =
        {
            "<a:ad:552574586705936414>",
            "<a:bd:552574587326693380>",
            "<a:cd:552574587200602115>",
            "<a:dd:552574586835959819>",
            "<a:ed:552574587636940808>",
            "<a:fd:552574588798763029>",
            "<a:gd:552574587762638866>",
            "<a:hd:552574587917828097>",
            "<a:id:552574587846524949>",
            "<a:jd:552574595866165253>",
            "<a:kd:552574595736141834>",
            "<a:ld:552574590887657512>",
            "<a:md:552574597527240715>",
            "<a:nd:552574596667408384>",
            "<a:od:552574596071555073>",
            "<a:pd:552574595799187465>",
            "<a:qd:552574590270832661>",
            "<a:rd:552574596499505164>",
            "<a:sd:552574596239458304>",
            "<a:td:552574595866034176>",
            "<a:ud:552574596096851978>",
            "<a:vd:552574596595843087>",
            "<a:wd:552574596625203260>",
            "<a:xd:552574596608425999>",
            "<a:yd:552574595660775426>",
            "<a:zd:552574595845324830>"
        };

        private static readonly string[] regionalIndicator =
        {
            "🇦",
            "🇧",
            "🇨",
            "🇩",
            "🇪",
            "🇫",
            "🇬",
            "🇭",
            "🇮",
            "🇯",
            "🇰",
            "🇱",
            "🇲",
            "🇳",
            "🇴",
            "🇵",
            "🇶",
            "🇷",
            "🇸",
            "🇹",
            "🇺",
            "🇻",
            "🇼",
            "🇽",
            "🇾",
            "🇿"
        };

        //https://stackoverflow.com/a/8809437
        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static string ToFormattedString(this ulong Value)
            => Value.ToString("N0");
        public static string ToFormattedString(this long Value)
            => Value.ToString("N0");
        public static string ToFormattedString(this int Value)
            => Value.ToString("N0");
        public static string ToFormattedString(this uint Value)
            => Value.ToString("N0");

        public static string ToRegionalIndicator(this string value)
        {
            StringBuilder ret = new StringBuilder();

            foreach (var chr in value)
            {
                if (!char.IsWhiteSpace(chr))
                {
                    if (!char.IsLetter(chr))
                    {
                        ret.Append(chr);
                        continue;
                    }

                    ret.Append(regionalIndicator[alphabet.FirstOrDefault(x => x.Key == chr.ToString().ToLower()).Value] + " ");
                }
                else
                {
                    ret.Append("  ");
                }
            }
            return ret.ToString();
        }

        public static string ToDancingEmoji(this string value)
        {
            StringBuilder ret = new StringBuilder();

            foreach (var chr in value)
            {
                if (!char.IsWhiteSpace(chr))
                {
                    if (!char.IsLetter(chr))
                    {
                        ret.Append(chr);
                        continue;
                    }

                    ret.Append(alphaDance[alphabet.FirstOrDefault(x => x.Key == chr.ToString().ToLower()).Value]);
                }
                else
                {
                    ret.Append("  ");
                }
            }
            return ret.ToString();
        }
    }
}