using System.Collections.Generic;
using Discord;

namespace Skuld.Core.Models
{
    public class SkuldUser
    {
        public ulong ID { get; set; }
        public bool Banned { get; set; }
        public string Username { get; set; }
        public string Title { get; set; }
        public bool CanDM { get; set; }
        public ulong Money { get; set; }
        public string Language { get; set; }
        public uint HP { get; set; }
        public uint Patted { get; set; }
        public uint Pats { get; set; }
        public ulong Daily { get; set; }
        public string AvatarUrl { get; set; }
        public bool RecurringBlock { get; set; }
        public bool UnlockedCustBG { get; set; }
        public string Background { get; set; }

        public List<GuildExperience> GuildExperience { get; set; }
        public List<int> UpvotedPastas { get; set; }
        public List<int> DownvotedPastas { get; set; }
        public List<CommandUsage> CommandUsage { get; set; }
        public List<Reputation> Reputation { get; set; }

        public static bool operator ==(SkuldUser user, IUser duser) => user.ID == duser.Id && user.Username == duser.Username && user.AvatarUrl == (duser.GetAvatarUrl() ?? duser.GetDefaultAvatarUrl());
        public static bool operator !=(SkuldUser user, IUser duser) => user.ID != duser.Id && user.Username != duser.Username && user.AvatarUrl != (duser.GetAvatarUrl() ?? duser.GetDefaultAvatarUrl());
        public static bool operator ==(IUser duser, SkuldUser user) => user == duser;
        public static bool operator !=(IUser duser, SkuldUser user) => user != duser;
        public bool Equals(IUser user)
        {
            return this.ID == user.Id &&
                this.Username == user.Username;
        }
    }}