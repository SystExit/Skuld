using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Skuld.Bot.Models.Commands
{
    public struct ModuleSkuld : IEquatable<ModuleSkuld>
    {
        public string Name { get; set; }
        public List<CommandSkuld> Commands { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ModuleSkuld skuld && Equals(skuld);
        }

        public bool Equals([AllowNull] ModuleSkuld other)
        {
            return Name == other.Name &&
                   EqualityComparer<List<CommandSkuld>>.Default.Equals(Commands, other.Commands);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Commands);
        }

        public static bool operator ==(ModuleSkuld left, ModuleSkuld right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ModuleSkuld left, ModuleSkuld right)
        {
            return !(left == right);
        }
    }
}