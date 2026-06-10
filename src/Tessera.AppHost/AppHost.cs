var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
var walletDb = postgres.AddDatabase("walletdb");

var rabbitmq = builder.AddRabbitMQ("messaging")
    .WithLifetime(ContainerLifetime.Persistent);

var keycloak = builder.AddKeycloak("keycloak", port: 50717)
    .WithDataVolume()
    .WithOtlpExporter()
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Projects.Tessera_Wallet_Api>("wallet-api")
    .WithReference(walletDb)
    .WithReference(rabbitmq)
    .WithReference(keycloak)
    .WaitFor(walletDb)
    .WaitFor(rabbitmq)
    .WaitFor(keycloak);

builder.Build().Run();