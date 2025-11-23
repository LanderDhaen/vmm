using Rise.Domain.Locations;
using Rise.Domain.Networks;
using Rise.Domain.Projects;
using Rise.Domain.Stations;

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
			location,
			new Installation(installed)
		);

		station.Code.ShouldBe("STN-001");
		station.Location.ShouldNotBeNull();
		station.Installation.InstalledAt.ShouldBe(installed);
		station.Installation.RemovedAt.ShouldBeNull();
	}

	[Fact]
	public void AllowUpdatingRemovalDate() {
		MeasurementStation station = new(
			"STN-001",
			MakeLocation(),
			new Installation(DateTime.UtcNow.AddDays(-30))
		);

		DateTime removedAt = DateTime.UtcNow;
		station.UpdateInstallation(new Installation(station.Installation.InstalledAt, removedAt));

		station.Installation.RemovedAt.ShouldNotBeNull();
		station.Installation.RemovedAt!.Value.ShouldBe(removedAt, TimeSpan.FromSeconds(1));
	}

	[Fact]
	public void AllowNullRemovedAt_AndMoveLocation() {
		Location originalLocation = MakeLocation("Old Place");
		MeasurementStation station = new(
			"STN-100",
			originalLocation,
			new Installation(DateTime.UtcNow.AddDays(-5))
		);

		station.Installation.RemovedAt.ShouldBeNull();

		Location newLocation = MakeLocation("New Place");
		station.MoveTo(newLocation);

		station.Location.ShouldBe(newLocation);
		station.Installation.RemovedAt.ShouldBeNull();
	}

	[Fact]
	public void UpdateProjects_AddProjectsToMeasurementStation() {
		DateRange pastRange = new(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30));
		Project project = new("Closed Project", "CODE123", pastRange, DummyMeasurementNetwork);
		project.Status.ShouldBe(Status.Actief);

		MeasurementStation measurementStation = new("MS001", DummyLocation,
			new Installation(DateTime.UtcNow));

		measurementStation.UpdateProjectList(new List<Project> { project });

		measurementStation.Projects.Count.ShouldBe(1);
		measurementStation.Projects.ShouldContain(project);
	}

	[Fact]
	public void UpdateProjects_DoesNothing_ProjectAlreadyAdded() {
		DateRange pastRange = new(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30));
		Project project = new("Closed Project", "CODE123", pastRange, DummyMeasurementNetwork);
		project.Status.ShouldBe(Status.Actief);

		MeasurementStation measurementStation = new("MS001", DummyLocation,
			new Installation(DateTime.UtcNow));

		measurementStation.UpdateProjectList(new List<Project> { project });
		measurementStation.Projects.Count.ShouldBe(1);
		measurementStation.Projects.ShouldContain(project);

		measurementStation.UpdateProjectList(new List<Project> { project });
		measurementStation.Projects.Count.ShouldBe(1);
		measurementStation.Projects.ShouldContain(project);
	}

	[Fact]
	public void UpdateMeasurementStations_DeletesMeasurementStation() {
		DateRange pastRange = new(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30));
		Project project = new("Closed Project", "CODE123", pastRange, DummyMeasurementNetwork);
		project.Status.ShouldBe(Status.Actief);

		MeasurementStation measurementStation = new("MS001", DummyLocation,
			new Installation(DateTime.UtcNow));

		measurementStation.UpdateProjectList(new List<Project> { project });
		measurementStation.Projects.Count.ShouldBe(1);
		measurementStation.Projects.ShouldContain(project);

		measurementStation.UpdateProjectList(new List<Project> { });
		measurementStation.Projects.Count.ShouldBe(0);
		measurementStation.Projects.ShouldNotContain(project);
	}

	[Fact]
	public void UpdateMeasurementStations_Throws_WhenProjectIsClosed() {
		DateRange pastRange = new(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30),
			DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1));
		Project project = new("Closed Project", "CODE123", pastRange, DummyMeasurementNetwork);
		project.Status.ShouldBe(Status.Afgesloten);

		MeasurementStation measurementStation = new("MS001", DummyLocation,
			new Installation(DateTime.UtcNow));

		Should.Throw<InvalidOperationException>(() =>
			measurementStation.UpdateProjectList(new List<Project> { project })
		);
	}
}