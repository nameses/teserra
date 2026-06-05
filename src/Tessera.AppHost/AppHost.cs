var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithDataVolume();
var walletDb = postgres.AddDatabase("walletdb");

var rabbitmq = builder.AddRabbitMQ("messaging");

builder.AddProject<Projects.Tessera_Wallet_Api>("wallet-api")
    .WithReference(walletDb)
    .WithReference(rabbitmq)
    .WaitFor(walletDb)
    .WaitFor(rabbitmq);

builder.Build().Run();