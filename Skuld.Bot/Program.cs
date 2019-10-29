using Skuld.Bot.Services;

namespace Skuld.Bot
{
    internal class Program
    {
        private static void Main()
            => new HostSerivce().CreateAsync().GetAwaiter().GetResult();
    }
}