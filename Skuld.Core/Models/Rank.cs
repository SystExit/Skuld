namespace Skuld.Core.Models
{
    public struct Rank
    {
        public int Position;
        public int Total;

        public Rank(int p, int t)
        {
            Position = p;
            Total = t;
        }
    }
}
