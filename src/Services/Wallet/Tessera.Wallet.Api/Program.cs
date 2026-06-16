using MassTransit;

using Microsoft.EntityFrameworkCore;

using Serilog;

using Tessera.Wallet.Api;
using Tessera.Wallet.Api.Consumers;
using Tessera.Wallet.Api.Db;
using Tessera.Wallet.Api.Repos;
using Tessera.Wallet.Api.Services;
using Tessera.Wallet.Api.Services.CommonModels;

var builder = WebApplication.CreateBuilder(args);
var settings = builder.Configuration.Get<ApplicationSettings>();
var scalarSettings = new ScalarExtensions.ScalarSettings() 
{ 
    Audience = settings!.Authorization.Audience, 
    AuthorizationUrl = settings!.Scalar.Security.AuthorizationUrl, 
    ClientId = settings!.Scalar.Security.ClientId
};

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

builder.Services.AddDbContextPool<WalletDbContext>((sp, opt) =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("walletdb"))
       .UseSnakeCaseNamingConvention());
builder.EnrichNpgsqlDbContext<WalletDbContext>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
});

builder.Services.AddTransient<IWalletRepository, WalletRepository>();

builder.AddKeycloakAuth(settings!.Authorization.Audience);
builder.Services.AddAuthorization();

builder.Services.AddScalar(scalarSettings);

builder.Services.AddGrpc();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ReserveFundsConsumer>();
    x.AddConsumer<SettleBetConsumer>();
    x.AddConsumer<RefundBetConsumer>();

    x.AddEntityFrameworkOutbox<WalletDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("messaging"));
        cfg.ConfigureEndpoints(ctx);
    });
});

var app = builder.Build();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.ConfigureScalar(scalarSettings);
app.MapApiEndpoints();
app.MapGrpcService<WalletGrpcService>();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<WalletDbContext>();
    await db.Database.MigrateAsync();
}
app.Run();

