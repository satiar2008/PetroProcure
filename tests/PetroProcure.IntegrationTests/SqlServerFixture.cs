using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetroProcure.AI;
using PetroProcure.Application;
using PetroProcure.Infrastructure;
using PetroProcure.Infrastructure.Persistence;
using PetroProcure.Reporting;
using PetroProcure.Application.Security;

namespace PetroProcure.IntegrationTests;

public sealed class SqlServerFixture : IAsyncLifetime
{
    private readonly string _database = $"PetroProcureTests_{Guid.NewGuid():N}";
    public string RootPath { get; } = Path.Combine(Path.GetTempPath(), "PetroProcureIntegration", Guid.NewGuid().ToString("N"));
    public ServiceProvider Services { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        var connection = $"Server=(localdb)\\mssqllocaldb;Database={_database};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        var values = new Dictionary<string, string?>
        {
            ["ConnectionStrings:PetroProcureDb"] = connection,
            ["PetroProcure:FileStorage:RootPath"] = RootPath,
            ["PetroProcure:AI:Provider"] = "Mock",
            ["PetroProcure:AI:RequiredDocumentTypes:0"] = "Indent",
            ["PetroProcure:AI:RequiredDocumentTypes:1"] = "TechnicalSpecification"
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<ICurrentUserService, TestCurrentUser>();
        services.AddApplication(configuration);
        services.AddInfrastructure(configuration);
        services.AddPetroProcureReporting();
        services.AddPetroProcureAi(configuration);
        Services = services.BuildServiceProvider();
        await using var scope = Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<PetroProcureDbContext>().Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<PetroProcureDbContext>().Database.EnsureDeletedAsync();
        await Services.DisposeAsync();
        if (Directory.Exists(RootPath)) Directory.Delete(RootPath, true);
    }
}

[CollectionDefinition("sqlserver")]
public sealed class SqlServerCollection : ICollectionFixture<SqlServerFixture>;
