using Microsoft.EntityFrameworkCore;
using Serilog;
using Tessera.Wallet.Api;
using Tessera.Wallet.Api.Db;
using Tessera.Wallet.Api.Repos;
using Tessera.Wallet.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var settings = builder.Configuration.Get<ApplicationSettings>();
Console.WriteLine($"AUDIENCE = '{settings?.Authorization?.Audience}'");

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

builder.Services.AddScoped<IWalletRepository, WalletRepository>();

builder.AddKeycloakAuth(settings!.Authorization.Audience);
builder.Services.AddAuthorization();

builder.Services.AddScalar(builder.Configuration);

var app = builder.Build();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.ConfigureScalar();
app.MapApiEndpoints();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<WalletDbContext>();
    await db.Database.MigrateAsync();
}
app.Run();