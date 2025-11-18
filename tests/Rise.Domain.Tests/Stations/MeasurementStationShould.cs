using Rise.Domain.Locations;
using Rise.Domain.Stations;

namespace Rise.Domain.Tests.Stations;

public class MeasurementStationShould {
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
			//status: "Active",
			location: location,
			installation: new Installation(installed)
		);

		station.Code.ShouldBe("STN-001");
		//station.Status.ShouldBe("Active");
		station.Location.ShouldNotBeNull();
		station.Installation.InstalledAt.ShouldBe(installed);
		station.Installation.RemovedAt.ShouldBeNull();

		station.Projects.Count.ShouldBe(1);
	}

	[Fact]
	public void AllowUpdatingStatusAndRemovalDate() {
		var station = new MeasurementStation(
			code: "STN-001",
			//status: "Active",
			location: MakeLocation(),
			installation: new Installation(DateTime.UtcNow.AddDays(-30))
		);

		//station.UpdateStatus("Inactive");
		var removedAt = DateTime.UtcNow;
		station.UpdateInstallation(new Installation(station.Installation.InstalledAt, removedAt));

		//station.Status.ShouldBe("Inactive");
		station.Installation.RemovedAt.ShouldNotBeNull();
		station.Installation.RemovedAt!.Value.ShouldBe(removedAt, TimeSpan.FromSeconds(1));
	}

	[Fact]
	public void AllowNullRemovedAt_AndMoveLocation() {
		var originalLocation = MakeLocation("Old Place");
		var station = new MeasurementStation(
			code: "STN-100",
			//status: "Active",
			location: originalLocation,
			installation: new Installation(DateTime.UtcNow.AddDays(-5))
		);

		station.Installation.RemovedAt.ShouldBeNull();

		var newLocation = MakeLocation("New Place");
		station.MoveTo(newLocation);

		station.Location.ShouldBe(newLocation);
		station.Installation.RemovedAt.ShouldBeNull();
	}
}