using Microsoft.EntityFrameworkCore;
using PRR.Data.DataContext;

namespace PRR.DATA.DataContextMigrations
{
    public static class DataContextHelpers
    {
        public static DbDataContext CreateDbContext(string ConnectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DbDataContext>();
            optionsBuilder.UseNpgsql(ConnectionString, b => b.MigrationsAssembly("PRR.Data.DataContextMigrations"));

            return new DbDataContext(optionsBuilder.Options);
        }

        public static void RecreateDataContext(string ConnectionString)
        {
            try
            {
                using var dbDataContext = CreateDbContext(ConnectionString);
                dbDataContext.Database.EnsureDeleted();
                dbDataContext.Database.Migrate();
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("RecreateDataContext Failed");
            }
        }
    }
}