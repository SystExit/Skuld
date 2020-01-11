namespace Skuld.Core.Models
{
    public class Pasta
    {
        public ulong Id { get; set; }
        public ulong OwnerId { get; set; }
        public string Name { get; set; }
        public ulong Created { get; set; }
        public string Content { get; set; }

        public bool IsOwner(User user)
        {
            return OwnerId == user.Id || (user.Flags & 1 << 0) != 0 || (user.Flags & 1 << 1) != 0;
        }
    }
}