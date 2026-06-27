var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .AddDatabase("PetroProcureDb");
var jwtSigningKey = builder.AddParameter("jwt-signing-key", secret: true);
var bootstrapAdminPassword = builder.AddParameter("bootstrap-admin-password", secret: true);

var api = builder.AddProject<Projects.PetroProcure_Api>("api")
    .WithReference(sql)
    .WithEnvironment("Authentication__Jwt__SigningKey", jwtSigningKey)
    .WithEnvironment("Security__BootstrapAdmin__Enabled", "true")
    .WithEnvironment("Security__BootstrapAdmin__Password", bootstrapAdminPassword)
    .WaitFor(sql);

builder.AddProject<Projects.PetroProcure_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WaitFor(api);

// The background worker processes AI evaluation jobs from the same database. It must receive the
// PetroProcureDb connection string from Aspire; without this reference it throws on startup
// ("Connection string 'PetroProcureDb' was not found.").
builder.AddProject<Projects.PetroProcure_Worker>("worker")
    .WithReference(sql)
    .WaitFor(sql);

builder.Build().Run();
