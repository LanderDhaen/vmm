using Rise.Domain.Common;
using Rise.Domain.Locations;
using Rise.Domain.MaintObjects;
using Rise.Domain.Projects;
using Rise.Domain.Stations;

namespace Rise.Domain.Tests.MaintObjects;

public class MaintObjShould {
	private static Location MakeLocation(string name = "Test Place") {
		return new Location(
			name,
			new Address("Main St 1", "Bus 1", "Brussels", "1000"),
			new Coordinate(4.35, 50.85)
		);
	}

	private static MeasurementStation MakeStation(string code = "STN-001") {
		return new MeasurementStation(code, MakeLocation(), new Installation(DateTime.UtcNow.AddDays(-5)));
	}

	private static Technician MakeTechnician() {
		return new Technician("John", "Doe", "account-1");
	}

	private static MaintObjPlan MakeType(string code = "Sonde", string name = "Sonde") {
		return new MaintObjPlan(name, 30, 180);
	}

	[Fact]
	public void BeCreatedWithExpectedDefaults() {
		MaintObjPlan plan = MakeType();
		Location location = MakeLocation();
		MaintObj mo = new("Sensor-1", "SN-123", MaintObjType.Sensor, plan, "BrandX", location);

		mo.Name.ShouldBe("Sensor-1");
		mo.SerialNumber.ShouldBe("SN-123");
		mo.MaintObjPlan.ShouldBe(plan);
		mo.Brand.ShouldBe("BrandX");
		mo.Location.ShouldBe(location);
		mo.Station.ShouldBeNull();
		mo.LastScannedBy.ShouldBeNull();
		mo.MaintObjStatus.ShouldBe(MaintObjStatus.Inactive);
	}

	[Fact]
	public void AssignAndUnassignStationUpdateLocationAndStockFlag() {
		Location location = new("Stock", new Address("Main St 1", "Bus 1", "Brussels", "1000"),
			new Coordinate(4.35, 50.85), true);
		MaintObj mo = new("Sensor-1", "SN-123", MaintObjType.Sensor, MakeType(), null, location);
		MeasurementStation station = MakeStation("STN-100");

		mo.AssignToStation(station);

		mo.Station.ShouldBe(station);
		mo.Station!.Id.ShouldBe(station.Id);
		mo.Location.ShouldBe(station.Location);

		mo.MoveToStockLocation(location);

		mo.Station.ShouldBeNull();
	}

	[Fact]
	public void RecordAndClearTechnician() {
		Location location = MakeLocation();
		MaintObj mo = new("Sensor-1", "SN-123", MaintObjType.Sensor, MakeType(), null, location);
		Technician tech = MakeTechnician();

		mo.ScanBy(tech);
		mo.LastScannedBy.ShouldBe(tech);
	}

	[Fact]
	public void AllowChangingStatus() {
		Location location = MakeLocation();
		MaintObj mo = new("Sensor-1", "SN-123", MaintObjType.Sensor, MakeType(), null, location);

		mo.MarkAsBroken();
		mo.MaintObjStatus.ShouldBe(MaintObjStatus.Broken);

		mo.MarkAsFixed();
		mo.MaintObjStatus.ShouldBe(MaintObjStatus.Inactive);
	}

	[Fact]
	public void ConstructorGuardsAgainstInvalidArguments() {
		Location location = MakeLocation();
		Should.Throw<ArgumentException>(() => new MaintObj("", "SN-1", MaintObjType.Sensor, MakeType(), null, location));
		Should.Throw<ArgumentException>(() => new MaintObj("Name", "", MaintObjType.Sensor, MakeType(), null, location));
	}
}