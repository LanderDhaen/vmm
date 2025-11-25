using Rise.Domain.Locations;
using Rise.Domain.Projects;
using Rise.Domain.Stations;

namespace Rise.Domain.MaintObjects;

public class MaintObj : Entity {
	private MaintObj() { }

	public MaintObj(string name, string serialNumber, MaintObjType maintObjType, string? brand,
		Installation? installation, bool isInStock) {
		Name = Guard.Against.NullOrWhiteSpace(name);
		SerialNumber = Guard.Against.NullOrWhiteSpace(serialNumber);
		MaintObjType = maintObjType;
		Brand = brand;
		Installation = installation;
		IsInStock = isInStock;
		Location = null;
	}

	public string Name { get; private set; } = null!;
	public string SerialNumber { get; private set; } = null!;
	public string? Brand { get; private set; }
	public MaintObjType MaintObjType { get; private set; }
	public MaintObjStatus MaintObjStatus { get; private set; } = MaintObjStatus.Inactief;
	public Installation? Installation { get; private set; }
	public bool IsInStock { get; private set; } = true;

	public MeasurementStation? Station { get; private set; }

	// Location: mirrors the assigned MeasurementStation's location, or null when in stock / unassigned
	public Location? Location { get; private set; }

	public Technician? Technician { get; private set; }

	public void AssignToStation(MeasurementStation station) {
		Station = Guard.Against.Null(station);
		IsInStock = false;
		Location = station.Location;
	}

	public void UnassignFromStation() {
		Station = null;
		IsInStock = true;
		Location = null;
	}

	public void RecordTechnician(Technician technician) {
		Technician = Guard.Against.Null(technician);
	}

	public void ClearTechnician() {
		Technician = null;
	}

	public void SetStatus(MaintObjStatus status) {
		MaintObjStatus = status;
	}
}