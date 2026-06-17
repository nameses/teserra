using MassTransit;
using Serilog;
using Tessera.History.Api.Db;
using Microsoft.EntityFrameworkCore;
using Tessera.History.Api.Services.CommonModels;
using Tessera.History.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);
var settings = builder.Configuration.Get<ApplicationSettings>();
var scalarSettings = new ScalarExtensions.ScalarSettings()
{
    Audience = settings!.Authorization.Audience,
    AuthorizationUrl = settings!.Scalar.Security.AuthorizationUrl,
    ClientId = settings!.Scalar.Security.ClientId
};

builder.Services.AddOpenApi();

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

builder.Services.AddDbContextPool<HistoryApiDbContext>((sp, opt) =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("historydb"))
       .UseSnakeCaseNamingConvention());
builder.EnrichNpgsqlDbContext<HistoryApiDbContext>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    //cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    //cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
});

builder.Services.AddTransient<IBetsRepository, BetsRepository>();

builder.AddKeycloakAuth(builder.Configuration["Authorization:Audience"]!);
builder.Services.AddAuthorization();

builder.Services.AddScalar(scalarSettings);

builder.Services.AddMassTransit(x =>
{
    //x.AddConsumer<ReserveFundsConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("messaging"));
        cfg.ConfigureEndpoints(ctx);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.ConfigureScalar(scalarSettings);

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HistoryApiDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
