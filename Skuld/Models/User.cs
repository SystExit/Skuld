using Microsoft.Extensions.DependencyInjection;
using Skuld.Services;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Skuld.Models
{
    public class SkuldUser
    {
        public ulong ID { get; set; }
        public ulong Money { get; set; }
		public string Language { get; set; }
        public string Description { get; set; }
        public DateTime? Daily { get; set; }
        public double LuckFactor { get; set; }
        public bool DMEnabled { get; set; }
        public uint Petted { get; set; }
        public uint Pets { get; set; }
        public uint HP { get; set; }
        public uint GlaredAt { get; set; }
        public uint Glares { get; set; }
        public string FavCmd { get; set; }
        public ulong FavCmdUsg { get; set; }
        public async Task<long> GetPastaKarma()
        {
            long returnkarma = 0;

            var db = Bot.services.GetRequiredService<DatabaseService>();

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
