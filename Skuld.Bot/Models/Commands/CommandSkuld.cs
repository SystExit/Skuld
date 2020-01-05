using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Skuld.Bot.Models.Commands
{
    public struct CommandSkuld : IEquatable<CommandSkuld>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ParameterSkuld[] Parameters { get; set; }
        public string[] Aliases { get; set; }

        public override bool Equals(object obj)
        {
            return obj is CommandSkuld skuld && Equals(skuld);
        }

        public bool Equals([AllowNull] CommandSkuld other)
        {
            return Name == other.Name &&
                   Description == other.Description &&
                   EqualityComparer<ParameterSkuld[]>.Default.Equals(Parameters, other.Parameters) &&
                   EqualityComparer<string[]>.Default.Equals(Aliases, other.Aliases);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Description, Parameters, Aliases);
        }

        public static bool operator ==(CommandSkuld left, CommandSkuld right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CommandSkuld left, CommandSkuld right)
        {
            return !(left == right);
        }
    }
}