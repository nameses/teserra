var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithDataVolume();
var walletDb = postgres.AddDatabase("walletdb");

var rabbitmq = builder.AddRabbitMQ("messaging");

var keycloak = builder.AddKeycloak("keycloak")
    .WithDataVolume()
    .WithOtlpExporter();

builder.AddProject<Projects.Tessera_Wallet_Api>("wallet-api")
    .WithReference(walletDb)
    .WithReference(rabbitmq)
    .WithReference(keycloak)
    .WaitFor(walletDb)
    .WaitFor(rabbitmq)
    .WaitFor(keycloak);

builder.Build().Run();