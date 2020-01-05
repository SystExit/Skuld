using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Skuld.Core.Utilities;

namespace Skuld.Core.Models
{
    public class SkuldDbContextFactory : IDesignTimeDbContextFactory<SkuldDatabaseContext>
    {
        public SkuldDatabaseContext CreateDbContext(string[] args = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SkuldDatabaseContext>();
            optionsBuilder.UseMySql(SkuldAppContext.GetEnvVar(Utils.ConStrEnvVar));

            return new SkuldDatabaseContext(optionsBuilder.Options);
        }
    }
}