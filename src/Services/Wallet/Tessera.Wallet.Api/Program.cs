using Microsoft.EntityFrameworkCore;
using Serilog;
using Tessera.Wallet.Api.Db;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

builder.Services.AddDbContextPool<WalletDbContext>((sp, opt) =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("walletdb"))
       .UseSnakeCaseNamingConvention());
builder.EnrichNpgsqlDbContext<WalletDbContext>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var app = builder.Build();

// Configure the HTTP request pipeline.
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