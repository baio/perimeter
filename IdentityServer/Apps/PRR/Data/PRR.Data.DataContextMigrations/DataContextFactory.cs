using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PRR.Data.DataContext;
using PRR.Data.DataContextMigrations;

namespace PRR.Data.DataContextMigrations
{
    public class DataContextFactory : IDesignTimeDbContextFactory<DbDataContext>
    {
        public DbDataContext CreateDbContext(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = config.GetConnectionString("PostgreSQL");

            return DataContextHelpers.CreateDbContext(connectionString);
        }
    }
}