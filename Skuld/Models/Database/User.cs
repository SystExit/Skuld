using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Linq;

namespace Skuld.Models.Database
{
    public class SkuldUser
    {
        public ulong ID { get; set; }
        public bool Banned { get; set; }
        public string Description { get; set; }
        public bool CanDM { get; set; }
        public ulong Money { get; set; }
		public string Language { get; set; }
        public uint HP { get; set; }
        public uint Patted { get; set; }
        public uint Pats { get; set; }
        public uint GlaredAt { get; set; }
        public uint Glares { get; set; }
        public ulong Daily { get; set; }
        public string AvatarUrl { get; set; }
        public string FavCmd { get; set; }
        public ulong FavCmdUsg { get; set; }
        public async Task<long> GetPastaKarma()
        {
            long returnkarma = 0;

            var db = Services.HostService.Services.GetRequiredService<Services.DatabaseService>();

            var pastas = await db.GetAllPastasAsync();
            if(pastas != null && pastas.Count > 0)
            {
                var ownedpastas = pastas.Where(x => x.OwnerID == ID);

                if (ownedpastas != null)
                {
                    long upkarma = 0;
                    long downkarma = 0;
                    foreach (var pasta in ownedpastas)
                    {
                        upkarma += pasta.Upvotes;
                        downkarma += pasta.Downvotes;
                    }
                    returnkarma = upkarma - (downkarma / 5);
                }
            }

            return returnkarma;
        }
    }
}
