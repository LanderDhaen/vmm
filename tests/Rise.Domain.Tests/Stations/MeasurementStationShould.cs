using System;
using System.Linq;

using Rise.Domain.Locations;
using Rise.Domain.Networks;
using Rise.Domain.Projects;
using Rise.Domain.Stations;
using Rise.Domain.Stations.ValueObjects;

using Shouldly;

using Xunit;

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

	private static readonly Project DummyProject =
		new Project("dummyproject", "DP-1", DummyDateRange, DummyMeasurementNetwork);

	private static Location MakeLocation(string name = "Test Water")
		=> new Location(
			name,
			new Address("Main St 1", "Bus 1", "Brussels", "1000"),
			new Coordinate(4.35, 50.85));

	[Fact]
	public void BeCreated() {
		var location = MakeLocation();
		var installed = DateTime.UtcNow.AddDays(-10);

		var station = new MeasurementStation(
			code: "STN-001",
			status: "Active",
			location: location,
			installation: new Installation(installed),
			projects: new[] { DummyProject }
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
		var station = new MeasurementStation(
			code: "STN-001",
			status: "Active",
			location: MakeLocation(),
			installation: new Installation(DateTime.UtcNow.AddDays(-30)),
			projects: new[] { DummyProject }
		);

		station.UpdateStatus("Inactive");
		var removedAt = DateTime.UtcNow;
		station.UpdateInstallation(new Installation(station.Installation.InstalledAt, removedAt));

		station.Status.ShouldBe("Inactive");
		station.Installation.RemovedAt.ShouldNotBeNull();
		station.Installation.RemovedAt!.Value.ShouldBe(removedAt, TimeSpan.FromSeconds(1));

		station.Projects.ShouldContain(DummyProject);
	}

	[Fact]
	public void AllowNullRemovedAt_AndMoveLocation() {
		var originalLocation = MakeLocation("Old Place");
		var station = new MeasurementStation(
			code: "STN-100",
			status: "Active",
			location: originalLocation,
			installation: new Installation(DateTime.UtcNow.AddDays(-5)),
			projects: new[] { DummyProject }
		);

		station.Installation.RemovedAt.ShouldBeNull();

		var newLocation = MakeLocation("New Place");
		station.MoveTo(newLocation);

		station.Location.ShouldBe(newLocation);
		station.Installation.RemovedAt.ShouldBeNull();
		station.Projects.ShouldContain(DummyProject);
	}
}