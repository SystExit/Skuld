using System;
using System.Diagnostics.CodeAnalysis;

namespace Skuld.Bot.Models.Services.WebSocket
{
    public struct WebSocketGuild : IEquatable<WebSocketGuild>
    {
        public string Name { get; internal set; }
        public string GuildIconUrl { get; internal set; }
        public ulong Id { get; internal set; }

        public override bool Equals(object obj)
        {
            return obj is WebSocketGuild guild && Equals(guild);
        }

        public bool Equals([AllowNull] WebSocketGuild other)
        {
            return Name == other.Name &&
                   GuildIconUrl == other.GuildIconUrl &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, GuildIconUrl, Id);
        }

        public static bool operator ==(WebSocketGuild left, WebSocketGuild right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WebSocketGuild left, WebSocketGuild right)
        {
            return !(left == right);
        }
    }
}