using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Serilog;

namespace Microsoft.Extensions.Hosting;

// Adds common Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/aspire/service-defaults
public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry()
            .AddSerilogLogging()
            .AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(tracing =>
                        // Exclude health check requests from tracing
                        tracing.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                    )
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks(HealthEndpointPath);

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }

    public static TBuilder AddSerilogLogging<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddSerilog((sp, lc) => lc
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(sp)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("service", builder.Environment.ApplicationName)
            .WriteTo.Console()
            .WriteTo.OpenTelemetry(o =>
            {
                o.Endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
            }));

        return builder;
    }
    public static TBuilder AddKeycloakAuth<TBuilder>(this TBuilder builder, string audience) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddAuthentication()
            .AddKeycloakJwtBearer("keycloak", realm: "tessera", options =>
            {
                options.Audience = audience;
                options.TokenValidationParameters.ValidAudiences = new[] { audience };
                if (builder.Environment.IsDevelopment())
                    options.RequireHttpsMetadata = false;
            });
        return builder;
    }

    public static IServiceCollection AddScalar(this IServiceCollection services, DocumentationSettings settings)
    {
        services.AddOpenApi(settings.Version, options =>
        {
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
            options.UseOAuth2Authentication(settings);

            options.AddDocumentTransformer((document, context, ct) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = settings.Title,
                    Version = settings.Version //todo add versioning later
                };

                document.Servers = [];

                return Task.CompletedTask;
            });
        });

        return services;
    }

    private static OpenApiOptions UseOAuth2Authentication(this OpenApiOptions options, DocumentationSettings settings)
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
                        { settings.Audience, settings.Audience }
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
                    [new OpenApiSecuritySchemeReference("OAuth2")] = ["openid", "profile", settings.Audience ]
                }
                ];
            }
            return Task.CompletedTask;
        });

        return options;
    }

    public static WebApplication ConfigureScalar(this WebApplication app, DocumentationSettings settings)
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
                .AddDefaultScopes("OAuth2", ["openid", "profile", settings.Audience]);
        });

        return app;
    }

    public class DocumentationSettings
    {
        public required string AuthorizationUrl { get; set; }
        public required string ClientId { get; set; }
        public required string Audience { get; set; }

        public required string Title { get; set; }
        public required string Version { get; set; }
    }
}
