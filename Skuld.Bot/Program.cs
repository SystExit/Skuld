using Skuld.Bot.Services;

namespace Skuld.Bot
{
    class Program
    {
        static void Main()
            => new HostSerivce().CreateAsync().GetAwaiter().GetResult();
    }
}
