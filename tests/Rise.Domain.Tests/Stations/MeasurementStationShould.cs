using Rise.Domain.Locations;
using Rise.Domain.Networks;
using Rise.Domain.Projects;
using Rise.Domain.Stations;
using Rise.Domain.Stations.ValueObjects;

namespace Rise.Domain.Tests.Stations;

public class MeasurementStationShould {
	private static readonly Location DummyLocation = new(
		"Aalst",
		new Address("Koningstraat 12", "Bus 3A", "Brussel", "1000"),
		new Coordinate(6.0, 7.0)
	);

	private static readonly DateRange DummyDateRange = new(
		new DateOnly(2025, 1, 5),
		new DateOnly(2025, 5, 10)
	);

	private static readonly Zone DummyZone = new("DummyZone", "DummyCode");
	private static readonly MeasurementNetwork DummyMeasurementNetwork = new("DummyMeasurementNetwork", DummyZone);

	private static readonly Project DummyProject = new("dummyproject", "DP-1", DummyDateRange, DummyMeasurementNetwork);

	private static Location MakeLocation(string name = "Test Water") {
		return new Location(
			name,
			new Address("Main St 1", "Bus 1", "Brussels", "1000"),
			new Coordinate(4.35, 50.85));
	}

	[Fact]
	public void BeCreated() {
		Location location = MakeLocation();
		DateTime installed = DateTime.UtcNow.AddDays(-10);

		MeasurementStation station = new(
			"STN-001",
			"Active",
			location,
			new Installation(installed),
			new[] { DummyProject }
		);

		station.Code.ShouldBe("STN-001");
		station.Status.ShouldBe("Active");
		station.Location.ShouldNotBeNull();
		station.Installation.InstalledAt.ShouldBe(installed);
		station.Installation.RemovedAt.ShouldBeNull();

		station.Projects.Count.ShouldBe(1);
		station.Projects.Single().ShouldBe(DummyProject);
	}

	[Fact]
	public void AllowUpdatingStatusAndRemovalDate() {
		MeasurementStation station = new(
			"STN-001",
			"Active",
			MakeLocation(),
			new Installation(DateTime.UtcNow.AddDays(-30)),
			new[] { DummyProject }
		);

		station.UpdateStatus("Inactive");
		DateTime removedAt = DateTime.UtcNow;
		station.UpdateInstallation(new Installation(station.Installation.InstalledAt, removedAt));

		station.Status.ShouldBe("Inactive");
		station.Installation.RemovedAt.ShouldNotBeNull();
		station.Installation.RemovedAt!.Value.ShouldBe(removedAt, TimeSpan.FromSeconds(1));

		station.Projects.ShouldContain(DummyProject);
	}

	[Fact]
	public void AllowNullRemovedAt_AndMoveLocation() {
		Location originalLocation = MakeLocation("Old Place");
		MeasurementStation station = new(
			"STN-100",
			"Active",
			originalLocation,
			new Installation(DateTime.UtcNow.AddDays(-5)),
			new[] { DummyProject }
		);

		station.Installation.RemovedAt.ShouldBeNull();

		Location newLocation = MakeLocation("New Place");
		station.MoveTo(newLocation);

		station.Location.ShouldBe(newLocation);
		station.Installation.RemovedAt.ShouldBeNull();
		station.Projects.ShouldContain(DummyProject);
	}
}