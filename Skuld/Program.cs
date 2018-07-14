using Skuld.Services;

namespace Skuld
{
    public class Program
    {
        private static void Main()
            => new HostService().CreateAsync().GetAwaiter().GetResult();
    }
}