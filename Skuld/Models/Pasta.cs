namespace Skuld.Models
{
    public class Pasta
    {
        public uint ID { get; set; }
        public ulong OwnerID { get; set; }
        public uint Upvotes { get; set; }
        public uint Downvotes { get; set; }
        public string Name { get; set; }
        public string Created { get; set; }
        public string Content { get; set; }
    }
}
