using Shouldly;
using TestPipeline.Domain.Settings;

namespace TestPipeline.Domain.Test.Settings;

public class DatabaseSettingsTests
{
    [Test]
    public void DatabaseConnectionTest() => new DatabaseSettings
    {
        DatabaseConnection1 = "abc"
    }.DatabaseConnection1.ShouldBe("abc");
}