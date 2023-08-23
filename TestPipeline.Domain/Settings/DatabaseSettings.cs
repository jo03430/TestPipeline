using JetBrains.Annotations;

namespace TestPipeline.Domain.Settings;

[PublicAPI]
public class DatabaseSettings
{
    public string? DatabaseConnection1 { get; set; }
}