using Skuld.Core;

namespace Skuld.Bot.Models
{
    public class Die
    {
        public ushort Face { get; private set; }

        public Die(){}

        public Die Roll()
        {
            Face = (ushort)SkuldRandom.Next(1, 7);
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