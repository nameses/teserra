using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace Microsoft.Extensions.Hosting;

public static class ScalarExtensions
{
    public static IServiceCollection AddScalar(this IServiceCollection services, ScalarSettings settings)
    {
        services.AddOpenApi(options =>
        {
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
            options.UseOAuth2Authentication(settings);

            options.AddDocumentTransformer((document, context, ct) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "Tessera Wallet API",
                    Version = "v1"//todo add versioning later
                };

                return Task.CompletedTask;
            });
        });

        return services;
    }

    private static OpenApiOptions UseOAuth2Authentication(this OpenApiOptions options, ScalarSettings settings)
    {
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "OAuth2 authentication using Keycloak.",
            Flows = new OpenApiOAuthFlows
            {
                Implicit = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(settings.AuthorizationUrl),
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "openid" },
                        { "profile", "profile" },
                        { "wallet.api", "wallet.api" }
                    }
                }
            }
        };

        options.AddDocumentTransformer((document, context, ct) =>
        {
            document.Components ??= new();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes["OAuth2"] = scheme;
            return Task.CompletedTask;
        });

        options.AddOperationTransformer((operation, context, ct) =>
        {
            if (context.Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().Any())
            {
                operation.Security =
                [
                    new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("OAuth2")] = ["openid", "profile", "wallet.api"]
                }
                ];
            }
            return Task.CompletedTask;
        });

        return options;
    }

    public static WebApplication ConfigureScalar(this WebApplication app, ScalarSettings settings)
    {
        app.MapOpenApi();

        app.MapScalarApiReference(options =>
        {
            options
                .WithTheme(ScalarTheme.DeepSpace)
                .WithFavicon("https://scalar.com/logo-light.svg")
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

            options.AddPreferredSecuritySchemes("OAuth2");
            options
                .AddImplicitFlow("OAuth2", flow =>
                {
                    flow.ClientId = settings.ClientId;
                    flow.AuthorizationUrl = settings.AuthorizationUrl;
                })
                .AddDefaultScopes("OAuth2", ["openid", "profile", "wallet.api"]);
        });

        return app;
    }

    public class ScalarSettings
    {
        public required string AuthorizationUrl { get; set; }
        public required string ClientId { get; set; }
        public required string Audience { get; set; }
    }
}
