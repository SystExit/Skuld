namespace Skuld.Core.Models
{
    public class Pasta
    {
        public uint PastaID { get; set; }
        public ulong OwnerID { get; set; }
        public uint Upvotes { get; set; }
        public uint Downvotes { get; set; }
        public string Name { get; set; }
        public ulong Created { get; set; }
        public string Content { get; set; }
    }
}