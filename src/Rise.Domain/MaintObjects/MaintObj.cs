using Rise.Domain.Locations;
using Rise.Domain.Projects;
using Rise.Domain.Stations;

namespace Rise.Domain.MaintObjects;

public class MaintObj : Entity {
	private MaintObj() { }

	public MaintObj(string name, string serialNumber, MaintObjType maintObjType, string? brand,
		Installation installation, bool isInStock) {
		Name = Guard.Against.NullOrWhiteSpace(name);
		SerialNumber = Guard.Against.NullOrWhiteSpace(serialNumber);
		MaintObjType = maintObjType;
		Brand = brand;
		Installation = installation;
		IsInStock = isInStock;
		Location = null;
	}
	//TODO: technieker bijhouden die het laatst gescanned heeft

	public string Name { get; private set; } = null!;
	public string SerialNumber { get; private set; }
	public string? Brand { get; private set; }
	public MaintObjType MaintObjType { get; private set; }
	public MaintObjStatus MaintObjStatus { get; } = MaintObjStatus.Actief;
	public Installation Installation { get; private set; }
	public bool IsInStock { get; private set; } = true;

	// Nullable FK to MeasurementStation
	public Guid? MeasurementStationId { get; private set; }

	public MeasurementStation? Station { get; private set; }

	// Location: mirrors the assigned MeasurementStation's location, or null when in stock / unassigned
	public Location? Location { get; private set; }

	// Nullable FK to Technician:
	public Guid? TechnicianId { get; private set; }
	public Technician? Technician { get; private set; }

	public void AssignToStation(MeasurementStation station) {
		Station = Guard.Against.Null(station);
		MeasurementStationId = station.Id;
		IsInStock = false;
		Location = station.Location;
	}

	public void UnassignFromStation() {
		Station = null;
		MeasurementStationId = null;
		IsInStock = true;
		Location = null;
	}

	public void RecordTechnician(Technician technician) {
		Technician = Guard.Against.Null(technician);
		TechnicianId = technician.Id;
	}

	public void ClearTechnician() {
		Technician = null;
		TechnicianId = null;
	}
}