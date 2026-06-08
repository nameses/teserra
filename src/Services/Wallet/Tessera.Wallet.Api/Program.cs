using Microsoft.EntityFrameworkCore;
using Serilog;
using Tessera.Wallet.Api;
using Tessera.Wallet.Api.Db;
using Tessera.Wallet.Api.Repos;
using Tessera.Wallet.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

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

builder.Services.AddSingleton<IWalletRepository, WalletRepository>();

var app = builder.Build();

app.MapApiEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<WalletDbContext>();
        await db.Database.MigrateAsync();
    }
}
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.MapDefaultEndpoints();
app.Run();