var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);
var walletDb = postgres.AddDatabase("walletdb");
var orchestratordb = postgres.AddDatabase("orchestratordb");
var historyDb = postgres.AddDatabase("historydb");

var rabbitmq = builder.AddRabbitMQ("messaging")
    .WithLifetime(ContainerLifetime.Persistent);

var keycloak = builder.AddKeycloak("keycloak", port: 50717)
    .WithDataVolume()
    .WithOtlpExporter()
    .WithLifetime(ContainerLifetime.Persistent);

var walletApi = builder.AddProject<Projects.Tessera_Wallet_Api>("wallet-api")
    .WithReference(walletDb)
    .WithReference(rabbitmq)
    .WithReference(keycloak)
    .WaitFor(walletDb)
    .WaitFor(rabbitmq)
    .WaitFor(keycloak);

var orchestrator = builder.AddProject<Projects.Tessera_Orchestrator>("orchestrator")
    .WithReference(orchestratordb)
    .WithReference(rabbitmq)
    .WithReference(keycloak)
    .WaitFor(orchestratordb)
    .WaitFor(rabbitmq)
    .WaitFor(keycloak);

var historyApi = builder.AddProject<Projects.Tessera_History_Api>("history-api")
    .WithReference(historyDb)
    .WithReference(rabbitmq)
    .WithReference(keycloak)
    .WaitFor(historyDb)
    .WaitFor(rabbitmq)
    .WaitFor(keycloak);

var notifications = builder.AddProject<Projects.Tessera_Notifications>("notifications")
    .WithReference(rabbitmq)
    .WithReference(keycloak)
    .WaitFor(rabbitmq)
    .WaitFor(keycloak);

builder.AddProject<Projects.Tessera_Gateway>("gateway")
    .WithReference(walletApi)
    .WithReference(orchestrator)
    .WithReference(historyApi)
    .WithReference(notifications)
    .WaitFor(walletApi)
    .WaitFor(orchestrator)
    .WaitFor(historyApi)
    .WaitFor(notifications)
    .WithExternalHttpEndpoints();

builder.Build().Run();