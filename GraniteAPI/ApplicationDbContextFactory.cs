using GraniteAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GraniteAPI.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Use the Render PostgreSQL connection string
            optionsBuilder.UseNpgsql(
     "Host=dpg-d48skg3ipnbc73dmhetg-a.singapore-postgres.render.com;Port=5432;Database=granitedb;Username=granitedb_user;Password=o24A54vGzT5AMyT0mL56zN5x84EVv5nQ;SSL Mode=Require;Trust Server Certificate=true"
 );


            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
