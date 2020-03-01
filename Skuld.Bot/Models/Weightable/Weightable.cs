namespace Skuld.Bot.Models
{
    public class Weightable<T>
    {
        public int Weight { get; private set; }
        public T Value { get; private set; }
    }
}