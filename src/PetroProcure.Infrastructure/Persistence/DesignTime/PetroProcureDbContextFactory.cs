using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PetroProcure.Infrastructure.Persistence.DesignTime;

public sealed class PetroProcureDbContextFactory : IDesignTimeDbContextFactory<PetroProcureDbContext>
{
    public PetroProcureDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PetroProcureDb")
            ?? Environment.GetEnvironmentVariable("PETROPROCURE_CONNECTIONSTRINGS__PETROPROCUREDB")
            ?? "Server=(localdb)\\mssqllocaldb;Database=PetroProcureDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<PetroProcureDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(PetroProcureDbContext).Assembly.FullName);
        });

        return new PetroProcureDbContext(optionsBuilder.Options);
    }
}
