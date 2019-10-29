using System;

namespace Skuld.Core.Utilities
{
    public static class ConversionTools
    {
        public static ulong GetEpochMs()
            => (ulong)new DateTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;

        public static int ParseInt32OrDefault(object input)
        {
            if (int.TryParse(Convert.ToString(input), out int tmp))
                return tmp;
            else
                return 0;
        }

        public static uint ParseUInt32OrDefault(object input)
        {
            if (uint.TryParse(Convert.ToString(input), out uint tmp))
                return tmp;
            else
                return 0;
        }

        public static ulong ParseUInt64OrDefault(object input)
        {
            if (ulong.TryParse(Convert.ToString(input), out ulong tmp))
                return tmp;
            else
                return 0;
        }
    }
}