namespace Tessera.History.Api.Services.CommonModels;

public class ApplicationSettings
{
    public ScalarSettings Scalar { get; set; } = new();
    public AuthorizationSettings Authorization { get; set; } = new();
}

public class AuthorizationSettings
{
    public string Audience { get; set; } = string.Empty;
}

public class ScalarSettings
{
    public SecuritySettings Security { get; set; } = new();
}

public class SecuritySettings
{
    public string AuthorizationUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
}

