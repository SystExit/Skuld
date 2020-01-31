using Skuld.Bot.Services;

namespace Skuld.Bot
{
    internal class Program
    {
        private static void Main(string[] args)
            => HostSerivce.CreateAsync(args).GetAwaiter().GetResult();
    }
}