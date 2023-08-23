using Shouldly;
using TestPipeline.Domain.Entities;

namespace TestPipeline.Domain.Test.Entities;

public class BaseRequestTests
{
    public class TestCase : BaseRequest
    {
        
    }

    [Test]
    public void BaseRequestTest() => new TestCase {CorrelationId = "abc"}.CorrelationId.ShouldBe("abc");
}