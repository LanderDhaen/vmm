using Rise.Domain.Projects;
using Rise.Domain.Locations;
using Shouldly;
using Xunit;

namespace Rise.Domain.Tests.Projects;

public class ProjectShould {

    private static readonly DateRange DummyDateRange = new(
        new DateOnly(2025, 1, 5),
        new DateOnly(2025, 5, 10)
    );

    [Fact]
    public void BeCreated()
    {
        var project = new Project("Testing Project", "CODE123", DummyDateRange);

        project.Name.ShouldBe("Testing Project");
        project.Code.ShouldBe("CODE123");
        project.DateRange.ShouldBe(DummyDateRange);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("")]
    public void NotBeCreatedWithAnInvalidName(string? name)
    {
        Should.Throw<ArgumentException>(() =>
            new Project(name!, "CODE123", DummyDateRange)
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("")]
    public void NotBeCreatedWithAnInvalidCode(string? code)
    {
        Should.Throw<ArgumentException>(() =>
            new Project("TestProject", code!, DummyDateRange)
        );
    }

    [Theory]
    [InlineData(null)]
    public void NotBeCreatedWithAnInvalidDateRange(DateRange? dateRange)
    {
        Should.Throw<ArgumentException>(() =>
            new Project("TestProject", "CODE123", dateRange!)
        );
    }
}
