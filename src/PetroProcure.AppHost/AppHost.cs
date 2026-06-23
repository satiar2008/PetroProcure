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

builder.Build().Run();
