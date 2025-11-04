using Rise.Domain.Projects;

namespace Rise.Domain.Tests.Projects;

public class ProjectShould {
	private static readonly DateRange DummyDateRange = new(
		new DateOnly(2025, 1, 5),
		new DateOnly(2025, 5, 10)
	);

	private static readonly Zone DummyZone = new("DummyZone", "DummyCode");
	private static readonly MeasurementNetwork DummyMeasurementNetwork = new("DummyMeasurementNetwork", DummyZone);

	[Fact]
	public void BeCreated() {
		Project project = new("Testing Project", "CODE123", DummyDateRange, DummyMeasurementNetwork);

		project.Name.ShouldBe("Testing Project");
		project.Code.ShouldBe("CODE123");
		project.DateRange.ShouldBe(DummyDateRange);
		project.MeasurementNetwork.ShouldBe(DummyMeasurementNetwork);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("   ")]
	[InlineData("")]
	public void NotBeCreatedWithAnInvalidName(string? name) {
		Should.Throw<ArgumentException>(() =>
			new Project(name!, "CODE123", DummyDateRange, DummyMeasurementNetwork)
		);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("   ")]
	[InlineData("")]
	public void NotBeCreatedWithAnInvalidCode(string? code) {
		Should.Throw<ArgumentException>(() =>
			new Project("TestProject", code!, DummyDateRange, DummyMeasurementNetwork)
		);
	}

	[Theory]
	[InlineData(null)]
	public void NotBeCreatedWithAnInvalidDateRange(DateRange? dateRange) {
		Should.Throw<ArgumentException>(() =>
			new Project("TestProject", "CODE123", dateRange, DummyMeasurementNetwork)
		);
	}

	[Theory]
	[InlineData(null)]
	public void NotBeCreatedWithAnInvalidMeasurementNetwork(MeasurementNetwork? measurementNetwork) {
		Should.Throw<ArgumentException>(() =>
			new Project("TestProject", "CODE123", DummyDateRange, measurementNetwork));
	}
}