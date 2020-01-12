using Discord.WebSocket;
using System;

namespace Skuld.Core.Models
{
    public class User
    {
        public ulong Id { get; set; } = 0;
        public string Username { get; set; } = null;
        public ulong Money { get; set; } = 2500;
        public string Title { get; set; } = null;
        public string Description { get; set; } = null;
        public ulong LastDaily { get; set; } = 0;
        public string Language { get; set; } = "en-GB";

        /// <summary>
        /// Times been Patted
        /// </summary>
        public ulong Patted { get; set; } = 0;

        /// <summary>
        /// Times patted someone else
        /// </summary>
        public ulong Pats { get; set; } = 0;

        public Uri AvatarUrl { get; set; } = null;
        public string BanReason { get; set; } = null;
        public bool RecurringBlock { get; set; } = false;
        public bool UnlockedCustBG { get; set; } = false;
        public string Background { get; set; } = "#3F51B5";
        public ulong Flags { get; set; } = 0;
        public string TimeZone { get; set; } = TimeZoneInfo.Utc.Id;

        public bool IsUpToDate(SocketUser user)
        {
            var avi = user.GetAvatarUrl() ?? user.GetAvatarUrl();

            var username = Username == user.Username;
            bool avatar = false;

            if (AvatarUrl != null)
            {
                if (AvatarUrl.AbsoluteUri == avi)
                    avatar = true;
                else
                    avatar = false;
            }
            else
            {
                if (avi != null)
                    avatar = false;
            }

            return (username && avatar) == true;
        }
    }
}