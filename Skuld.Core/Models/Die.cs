using System;

namespace Skuld.Core.Models
{
    public class Die
    {
        private Random Random;
        public ushort Face { get; private set; }

        public Die()
        {
            Random = new Random();
        }

        public Die(Random random)
        {
            Random = random;
        }

        public Die Roll()
        {
            ushort counter = 0;

            while (counter <= 10)
            {
                Face = (ushort)Random.Next(1, 7);
                counter++;
            }

            return this;
        }

        public static bool operator ==(Die left, Die right)
            => left.Face == right.Face;

        public static bool operator !=(Die left, Die right)
            => !(left == right);

        public static bool operator ==(ushort value, Die die)
            => value == die.Face;

        public static bool operator !=(ushort value, Die die)
            => !(value == die);

        public static bool Equals(Die left, Die right)
            => left == right;
    }
}