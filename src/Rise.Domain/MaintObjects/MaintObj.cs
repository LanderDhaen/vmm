using Rise.Domain.Locations;
using Rise.Domain.Projects;
using Rise.Domain.Stations;

namespace Rise.Domain.MaintObjects;

public class MaintObj : Entity {
	private MaintObj() { }

	public MaintObj(string name, string serialNumber, MaintObjType maintObjType, string? brand, Location location) {
		Name = Guard.Against.NullOrWhiteSpace(name);
		SerialNumber = Guard.Against.NullOrWhiteSpace(serialNumber);
		MaintObjType = Guard.Against.Null(maintObjType);
		Brand = brand;
		Location = Guard.Against.Null(location);
	}

	public string Name { get; private set; }
	public string SerialNumber { get; private set; }
	public string? Brand { get; private set; }
	public MaintObjType MaintObjType { get; private set; }
	public MaintObjStatus MaintObjStatus { get; private set; } = MaintObjStatus.Inactief;
	public Installation? Installation { get; private set; }

	public MeasurementStation? Station { get; private set; }
	public Location Location { get; private set; }

	public Technician? LastScannedBy { get; private set; }

	public void AssignToStation(MeasurementStation station) {
		Station = Guard.Against.Null(station);
		Location = station.Location;
	}

	public void UnassignFromStation() {
		Station = null;
	}

	public void RecordTechnician(Technician technician) {
		LastScannedBy = Guard.Against.Null(technician);
	}

	public void ClearTechnician() {
		LastScannedBy = null;
	}

	public void SetStatus(MaintObjStatus status) {
		MaintObjStatus = status;
	}

	public void Edit(string name, string serialNumber, MaintObjType maintObjType, string? brand,
		Installation installation, Location location) {
		Name = Guard.Against.NullOrWhiteSpace(name);
		SerialNumber = Guard.Against.NullOrWhiteSpace(serialNumber);
		MaintObjType = maintObjType;
		Brand = brand;
		Installation = installation;
		Location = location;
	}
}