namespace TestPipeline.Domain.Settings;

public class BasicAuthenticationSettings
{
    public string Token { get; init; } = string.Empty;
    public string HashType { get; init; } = string.Empty;
}