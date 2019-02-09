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
            "<a:ad:531506829470203914>",
            "<a:bd:531506829965000745>",
            "<a:cd:531506829541376000>",
            "<a:dd:531506829516210180>",
            "<a:ed:531506829646364673>",
            "<a:fd:531506829667205130>",
            "<a:gd:531506831189868544>",
            "<a:hd:531506831248326663>",
            "<a:id:531506831869214740>",
            "<a:jd:531506831403778058>",
            "<a:kd:531506831932260362>",
            "<a:ld:531506831441526795>",
            "<a:md:531506835962986534>",
            "<a:nd:531506832254959636>",
            "<a:od:531506833677090817>",
            "<a:pd:531506833400266754>",
            "<a:qd:531506833265786880>",
            "<a:rd:534392649025585153>",
            "<a:sd:531506832859201550>",
            "<a:td:531506833328701451>",
            "<a:ud:531506833328701440>",
            "<a:vd:531506833911971864>",
            "<a:wd:531506833874092072>",
            "<a:xd:531506833362255883>",
            "<a:yd:531506833471438848>",
            "<a:zd:531506835123863574>"
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

                    ret.Append(regionalIndicator[alphabet.FirstOrDefault(x => x.Key == chr.ToString().ToLower()).Value]+" ");
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

            foreach(var chr in value)
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
