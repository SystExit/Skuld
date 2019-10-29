using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace Skuld.Core.Models
{
    public class SkuldDbContextFactory : IDesignTimeDbContextFactory<SkuldDatabaseContext>
    {
        public SkuldDatabaseContext CreateDbContext(string[] args = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SkuldDatabaseContext>();
            optionsBuilder.UseMySql(Environment.GetEnvironmentVariable("SKULD_CONNSTR"));

            return new SkuldDatabaseContext(optionsBuilder.Options);
        }
    }
}