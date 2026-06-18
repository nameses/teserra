using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var serviceScopes = builder.Configuration
      .GetSection("Scalar:ServiceScopes")
      .Get<string[]>() ?? [];

builder.AddServiceDefaults();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.MapReverseProxy();

app.MapOpenApi();

app.MapScalarApiReference(options =>
{
    options.AddDocuments(serviceScopes);

    options
        .WithTitle("Tessera Platform API")
        .WithTheme(ScalarTheme.DeepSpace)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

    options.AddPreferredSecuritySchemes("OAuth2");
    options
        .AddImplicitFlow("OAuth2", flow =>
        {
            flow.ClientId = builder.Configuration["Scalar:ClientId"]!;
            flow.AuthorizationUrl = builder.Configuration["Scalar:AuthorizationUrl"]!;
        })
        .AddDefaultScopes("OAuth2", ["openid", "profile", ..serviceScopes]);
});

app.Run();
