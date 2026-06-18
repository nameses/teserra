using MassTransit;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Tessera.Contracts.Betting;
using Tessera.Orchestrator;
using Tessera.Orchestrator.Db;
using Tessera.Orchestrator.Services;
using Tessera.Orchestrator.Services.CommonModels;

var builder = WebApplication.CreateBuilder(args);
var settings = builder.Configuration.Get<ApplicationSettings>();
var scalarSettings = new Extensions.DocumentationSettings()
{
    Audience = settings!.Authorization.Audience,
    AuthorizationUrl = settings!.Scalar.Security.AuthorizationUrl,
    ClientId = settings!.Scalar.Security.ClientId,
    Title = settings!.Docs.Title,
    Version = settings!.Docs.Version,
};

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

builder.Services.AddDbContextPool<OrchestratorDbContext>((sp, opt) =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("orchestratordb"))
       .UseSnakeCaseNamingConvention());
builder.EnrichNpgsqlDbContext<OrchestratorDbContext>();

builder.AddKeycloakAuth(scalarSettings.Audience); 
builder.Services.AddAuthorization();

builder.Services.AddScalar(scalarSettings);

EndpointConvention.Map<ReserveFundsCommand>(new Uri("queue:reserve-funds"));
EndpointConvention.Map<SettleBetCommand>(new Uri("queue:settle-bet"));
EndpointConvention.Map<RefundBetCommand>(new Uri("queue:refund-bet"));

builder.Services.AddQuartz(q =>
{
    q.UsePersistentStore(s =>
    {
        s.UseProperties = true;
        s.UsePostgres(builder.Configuration.GetConnectionString("orchestratordb")!);
        s.UseSystemTextJsonSerializer();
        s.SetProperty("quartz.jobStore.tablePrefix", "quartz.QRTZ_");
    });
});
builder.Services.AddQuartzHostedService(o => o.WaitForJobsToComplete = true);

builder.Services.AddMassTransit(config =>
{
    config.AddSagaStateMachine<BetStateMachine, BetState>()
        .EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Optimistic;
            r.ExistingDbContext<OrchestratorDbContext>();
            r.UsePostgres();
        });

    config.AddEntityFrameworkOutbox<OrchestratorDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    config.AddQuartzConsumers();

    config.UsingRabbitMq((r, c) =>
    {
        c.Host(builder.Configuration.GetConnectionString("messaging"));
        c.UseMessageScheduler(new Uri("queue:quartz"));
        c.ConfigureEndpoints(r);
    });
});

var app = builder.Build();

app.ConfigureScalar(scalarSettings);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

app.MapApiEndpoints();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<OrchestratorDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
