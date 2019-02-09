using Skuld.Core.Extensions;
using System;

namespace Skuld.Discord.Models
{
    public class Reputation
    {
        public ulong Reper;
        public ulong Timestamp = DateTime.Now.ToEpoch();

        public Reputation(ulong ReperId)
        {
            Reper = ReperId;
        }
    }
}
