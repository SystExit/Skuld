using Skuld.Services;

namespace Skuld
{
    public class Program
    {
        static void Main()
            =>  new HostService().CreateAsync().GetAwaiter().GetResult();
    }
}