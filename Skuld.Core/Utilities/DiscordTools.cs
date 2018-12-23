using Discord;
using Skuld.Core.Extensions;

namespace Skuld.Core.Utilities
{
    public static class DiscordTools
    {
        public static readonly string Streaming_Emote = "<:streaming:313956277132853248>";
        public static readonly string Online_Emote = "<:online:313956277808005120>";
        public static readonly string Idle_Emote = "<:away:313956277220802560>";
        public static readonly string DoNotDisturb_Emote = "<:dnd:313956276893646850>";
        public static readonly string Invisible_Emote = "<:invisible:313956277107556352>";
        public static readonly string Offline_Emote = "<:offline:313956277237710868>";
        public static readonly string Warning_Emote = "<:BOSHY:485761951251562497>";
        public static readonly string Confused_Emote = "<:blobconfused:350681076588478474>";

        public static readonly string Ok_Emoji = "👌";
        public static readonly string Successful_Emoji = "✅";
        public static readonly string Failed_Emoji = "❎";
        public static readonly string Question_Emoji = "❓";

        public static readonly Color Ok_Color = "#339966".FromHex();
        public static readonly Color Warning_Color = "#FFFF00".FromHex();
        public static readonly Color Failed_Color = "#FF0000".FromHex();
    }
}