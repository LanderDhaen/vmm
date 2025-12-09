using Rise.Domain.Common;
using Rise.Domain.Locations;
using Rise.Domain.Networks;
using Rise.Domain.Projects;
using Rise.Domain.Stations;

namespace Rise.Domain.Tests.Projects;

public class ProjectShould {
	private static readonly DateRange DummyDateRange = new(
		new DateOnly(2025, 1, 5),
		new DateOnly(2025, 5, 10)
	);

	private static readonly Location DummyLocation = new(
		"Aalst",
		new Address("Koningstraat 12", "Bus 3A", "Brussel", "1000"),
		new Coordinate(6.0, 7.0)
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

	[Fact]
	public void HaveStatus_Afgesloten_WhenEndDateIsInThePast() {
		Project project = new("Past Project", "CODE123", DummyDateRange, DummyMeasurementNetwork);
		project.Status.ShouldBe(Status.Afgesloten);
	}

	[Fact]
	public void HaveStatus_Actief_WhenNoEndDateIsProvided() {
		DateRange openRange = new(new DateOnly(2025, 1, 1));
		Project project = new("Open Project", "CODE123", openRange, DummyMeasurementNetwork);
		project.Status.ShouldBe(Status.Actief);
	}

	[Fact]
	public void HaveStatus_Actief_WhenEndDateIsInTheFuture() {
		DateOnly futureEnd = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30);
		DateRange futureRange = new(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1), futureEnd);
		Project project = new("Future Project", "CODE123", futureRange, DummyMeasurementNetwork);
		project.Status.ShouldBe(Status.Actief);
	}

	// removing end date to reopen project
	[Fact]
	public void UpdateStatus_ReopensProject_ByRemovingEndDate() {
		DateRange pastRange = new(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-20),
			DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1));
		Project project = new("Closed Project", "CODE123", pastRange, DummyMeasurementNetwork);
		project.Status.ShouldBe(Status.Afgesloten);

		project.UpdateStatus(Status.Actief);

		project.Status.ShouldBe(Status.Actief);
		project.DateRange.EndDate.HasValue.ShouldBeFalse();
	}

	//edge: end date equal to today should be considered Afgesloten (<= today)
	[Fact]
	public void HaveStatus_Afgesloten_WhenEndDateIsToday() {
		DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
		DateRange range = new(today.AddDays(-5), today);
		Project project = new("Today Project", "CODE123", range, DummyMeasurementNetwork);
		project.Status.ShouldBe(Status.Afgesloten);
	}

	//edge: updating to the same status should be a no-operation and not change the end date
	[Fact]
	public void UpdateStatus_NoOp_WhenSameStatus() {
		DateRange openRange = new(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-10));
		Project project = new("Open Project", "CODE123", openRange, DummyMeasurementNetwork);

		project.UpdateStatus(Status.Actief);

		project.Status.ShouldBe(Status.Actief);
		project.DateRange.EndDate.HasValue.ShouldBeFalse();
	}

	//closed projects cannot be edited
	[Fact]
	public void Edit_Throws_WhenProjectIsClosed() {
		DateRange pastRange = new(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30),
			DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1));
		Project project = new("Closed Project", "CODE123", pastRange, DummyMeasurementNetwork);
		project.Status.ShouldBe(Status.Afgesloten);

		Should.Throw<InvalidOperationException>(() =>
			project.Edit("New Name", "NEWCODE", new DateRange(DateOnly.FromDateTime(DateTime.UtcNow)))
		);
	}

	[Fact]
	public void UpdateMeasurementStations_AddMeasurementStationToProject() {
		DateRange pastRange = new(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30));
		Project project = new("Closed Project", "CODE123", pastRange, DummyMeasurementNetwork);
		project.Status.ShouldBe(Status.Actief);

		MeasurementStation measurementStation = new("MS001", DummyLocation,
			new Installation(DateTime.UtcNow));

		project.UpdateMeasurementStationList(new List<MeasurementStation> { measurementStation });

		project.Stations.Count.ShouldBe(1);
		project.Stations.ShouldContain(measurementStation);
	}

	[Fact]
	public void UpdateMeasurementStations_DoesNothing_MeasurementStationAlreadyAdded() {
		DateRange pastRange = new(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30));
		Project project = new("Closed Project", "CODE123", pastRange, DummyMeasurementNetwork);
		project.Status.ShouldBe(Status.Actief);

		MeasurementStation measurementStation = new("MS001", DummyLocation,
			new Installation(DateTime.UtcNow));

		project.UpdateMeasurementStationList(new List<MeasurementStation> { measurementStation });
		project.Stations.Count.ShouldBe(1);
		project.Stations.ShouldContain(measurementStation);

		project.UpdateMeasurementStationList(new List<MeasurementStation> { measurementStation });

		project.Stations.Count.ShouldBe(1);
		project.Stations.ShouldContain(measurementStation);
	}

	[Fact]
	public void UpdateMeasurementStations_DeletesMeasurementStation() {
		DateRange pastRange = new(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30));
		Project project = new("Closed Project", "CODE123", pastRange, DummyMeasurementNetwork);
		project.Status.ShouldBe(Status.Actief);

		MeasurementStation measurementStation = new("MS001", DummyLocation,
			new Installation(DateTime.UtcNow));

		project.UpdateMeasurementStationList(new List<MeasurementStation> { measurementStation });
		project.Stations.Count.ShouldBe(1);
		project.Stations.ShouldContain(measurementStation);

		project.UpdateMeasurementStationList(new List<MeasurementStation>());

		project.Stations.Count.ShouldBe(0);
		project.Stations.ShouldNotContain(measurementStation);
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
			project.UpdateMeasurementStationList(new List<MeasurementStation> { measurementStation })
		);
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