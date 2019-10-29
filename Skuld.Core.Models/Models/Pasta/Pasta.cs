namespace Skuld.Core.Models
{
    public class Pasta
    {
        public ulong Id { get; set; }
        public ulong OwnerId { get; set; }
        public string Name { get; set; }
        public ulong Created { get; set; }
        public string Content { get; set; }
    }
}