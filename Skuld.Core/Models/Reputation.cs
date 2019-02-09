using Skuld.Core.Extensions;
using System;

namespace Skuld.Core.Models
{
    public class Reputation
    {
        public ulong Reper;
        public ulong Timestamp = DateTime.UtcNow.ToEpoch();

        public Reputation()
        {

        }

        public Reputation(ulong ReperId)
        {
            Reper = ReperId;
        }
    }
}
