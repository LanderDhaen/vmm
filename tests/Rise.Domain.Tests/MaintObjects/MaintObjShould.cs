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

	[Fact]
	public void BeCreatedWithExpectedDefaults() {
		Guid typeId = Guid.NewGuid();
		MaintObj mo = new("Sensor-1", "SN-123", typeId, "BrandX", null, true);

		mo.Name.ShouldBe("Sensor-1");
		mo.SerialNumber.ShouldBe("SN-123");
		mo.MaintObjTypeId.ShouldBe(typeId);
		mo.Brand.ShouldBe("BrandX");
		mo.IsInStock.ShouldBeTrue();
		mo.Location.ShouldBeNull();
		mo.MeasurementStationId.ShouldBeNull();
		mo.TechnicianId.ShouldBeNull();
		mo.MaintObjStatus.ShouldBe(MaintObjStatus.Inactief);
	}

	[Fact]
	public void AssignAndUnassignStationUpdateLocationAndStockFlag() {
		MaintObj mo = new("Sensor-1", "SN-123", Guid.NewGuid(), null, null, true);
		MeasurementStation station = MakeStation("STN-100");

		mo.AssignToStation(station);

		mo.Station.ShouldBe(station);
		mo.MeasurementStationId.ShouldBe(station.Id);
		mo.IsInStock.ShouldBeFalse();
		mo.Location.ShouldBe(station.Location);

		mo.UnassignFromStation();

		mo.Station.ShouldBeNull();
		mo.MeasurementStationId.ShouldBeNull();
		mo.IsInStock.ShouldBeTrue();
		mo.Location.ShouldBeNull();
	}

	[Fact]
	public void RecordAndClearTechnician() {
		MaintObj mo = new("Sensor-1", "SN-123", Guid.NewGuid(), null, null, true);
		Technician tech = MakeTechnician();

		mo.RecordTechnician(tech);

		mo.Technician.ShouldBe(tech);
		mo.TechnicianId.ShouldBe(tech.Id);

		mo.ClearTechnician();

		mo.Technician.ShouldBeNull();
		mo.TechnicianId.ShouldBeNull();
	}

	[Fact]
	public void AllowChangingStatus() {
		MaintObj mo = new("Sensor-1", "SN-123", Guid.NewGuid(), null, null, true);

		mo.SetStatus(MaintObjStatus.Defect);
		mo.MaintObjStatus.ShouldBe(MaintObjStatus.Defect);

		mo.SetStatus(MaintObjStatus.Inactief);
		mo.MaintObjStatus.ShouldBe(MaintObjStatus.Inactief);
	}

	[Fact]
	public void ConstructorGuardsAgainstInvalidArguments() {
		Should.Throw<ArgumentException>(() => new MaintObj("", "SN-1", Guid.NewGuid(), null, null, true));
		Should.Throw<ArgumentException>(() => new MaintObj("Name", "", Guid.NewGuid(), null, null, true));
	}
}