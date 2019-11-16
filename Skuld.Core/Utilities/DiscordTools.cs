using Discord;
using Skuld.Core.Extensions;
using System;

namespace Skuld.Core.Utilities
{
    public static class DiscordTools
    {
        #region Status Emotes

        public static readonly string Streaming_Emote = "<:streaming:614849478926794752>";
        public static readonly string Online_Emote = "<:online:614849479161544751>";
        public static readonly string Idle_Emote = "<:away:614849478847102986>";
        public static readonly string DoNotDisturb_Emote = "<:dnd:614849478482198528>";
        public static readonly string Offline_Emote = "<:offline:614849478758760479>";

        #endregion Status Emotes

        #region Response Emojis

        public static readonly string Ok_Emoji = "👌";
        public static readonly string Successful_Emoji = "✔";
        public static readonly string Failed_Emoji = "❌";
        public static readonly string Question_Emoji = "❓";
        public static readonly string Confused_Emoji = "⁉";
        public static readonly string Prohibit_Emoji = "🚫";
        public static readonly string Warning_Emoji = "⚠";
        public static readonly string Remind_Emoji = "🔔";
        public static readonly string NotNSFW_Emoji = "🔞";
        public static readonly string ATM_Emoji = "🏧";
        public static readonly string NoBotsString = "Bots are not supported.";

        #endregion Response Emojis

        #region TwitchEmotes

        public static readonly string TwitchAdmins = "<:TwitchAdmins:552666767609036825>";
        public static readonly string TwitchAffiliate = "<:TwitchAffiliate:552666767630008354>";
        public static readonly string TwitchBroadcaster = "<:TwitchBroadcaster:552666767647047680>";
        public static readonly string TwitchChatMod = "<:TwitchChatMod:552666768406216714>";
        public static readonly string TwitchPrime = "<:TwitchPrime:552666767122759722>";
        public static readonly string TwitchStaff = "<:TwitchStaff:552666767412035584>";
        public static readonly string TwitchTurbo = "<:TwitchTurbo:552666767609167873>";
        public static readonly string TwitchVerified = "<:TwitchVerified:552666767625814086>";
        public static readonly string TwitchVIP = "<:TwitchVIP:552666767416360971>";
        public static readonly string TwitchGlobalMod = "<:TwitchGlobalMod:552668468877590538>";

        #endregion TwitchEmotes

        #region NitroEmotes

        public static readonly string NitroBoostEmote = "<:boost:614875223417684126>";
        public static readonly string NitroBoostRank1Emote = "<:boostrank1:614875835123499212>";
        public static readonly string NitroBoostRank2Emote = "<:boostrank2:614875835131887795>";
        public static readonly string NitroBoostRank3Emote = "<:boostrank3:614875835102658560>";
        public static readonly string NitroBoostRank4Emote = "<:boostrank4:614875835249197076>";

        #endregion NitroEmotes

        #region Embed Colours

        public static readonly Color Ok_Color = "#339966".FromHex();
        public static readonly Color Warning_Color = "#FFFF00".FromHex();
        public static readonly Color Failed_Color = "#FF0000".FromHex();

        #endregion Embed Colours

        public const double PHI = 1.618;

        #region Levels
        public static ulong GetXPLevelRequirement(ulong level, double growthmod)
            => (ulong)Math.Round(Math.Pow(level, 2) * 50 * growthmod, MidpointRounding.AwayFromZero);

        public static ulong GetLevelFromTotalXP(ulong totalxp, double growthmod)
            => (ulong)(Math.Sqrt(totalxp / (50 * growthmod)));

        #endregion Levels

        public static int MonthsBetween(DateTime date1, DateTime date2)
            => (int)Math.Round(date1.Subtract(date2).Days / (365.25 / 12), MidpointRounding.AwayFromZero);
    }
}