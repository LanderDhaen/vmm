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

	private static MaintObjType MakeType(string code = "Sonde", string name = "Sonde") {
		return new MaintObjType(Guid.NewGuid(), code, name, 30, 180);
	}

	[Fact]
	public void BeCreatedWithExpectedDefaults() {
		MaintObjType type = MakeType();
		Location location = MakeLocation();
		MaintObj mo = new("Sensor-1", "SN-123", type, "BrandX", location);

		mo.Name.ShouldBe("Sensor-1");
		mo.SerialNumber.ShouldBe("SN-123");
		mo.MaintObjType.ShouldBe(type);
		mo.Brand.ShouldBe("BrandX");
		mo.Location.ShouldBe(location);
		mo.Station.ShouldBeNull();
		mo.LastScannedBy.ShouldBeNull();
		mo.MaintObjStatus.ShouldBe(MaintObjStatus.Inactief);
	}

	[Fact]
	public void AssignAndUnassignStationUpdateLocationAndStockFlag() {
		Location location = MakeLocation();
		MaintObj mo = new("Sensor-1", "SN-123", MakeType(), null, location);
		MeasurementStation station = MakeStation("STN-100");

		mo.AssignToStation(station);

		mo.Station.ShouldBe(station);
		mo.Station!.Id.ShouldBe(station.Id);
		mo.Location.ShouldBe(station.Location);

		mo.UnassignFromStation();

		mo.Station.ShouldBeNull();
	}

	[Fact]
	public void RecordAndClearTechnician() {
		Location location = MakeLocation();
		MaintObj mo = new("Sensor-1", "SN-123", MakeType(), null, location);
		Technician tech = MakeTechnician();

		mo.RecordTechnician(tech);

		mo.LastScannedBy.ShouldBe(tech);

		mo.ClearTechnician();

		mo.LastScannedBy.ShouldBeNull();
	}

	[Fact]
	public void AllowChangingStatus() {
		Location location = MakeLocation();
		MaintObj mo = new("Sensor-1", "SN-123", MakeType(), null, location);

		mo.SetStatus(MaintObjStatus.Defect);
		mo.MaintObjStatus.ShouldBe(MaintObjStatus.Defect);

		mo.SetStatus(MaintObjStatus.Inactief);
		mo.MaintObjStatus.ShouldBe(MaintObjStatus.Inactief);
	}

	[Fact]
	public void ConstructorGuardsAgainstInvalidArguments() {
		Location location = MakeLocation();
		Should.Throw<ArgumentException>(() => new MaintObj("", "SN-1", MakeType(), null, location));
		Should.Throw<ArgumentException>(() => new MaintObj("Name", "", MakeType(), null, location));
	}
}