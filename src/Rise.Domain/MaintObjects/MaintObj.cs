using System.Data;

using Rise.Domain.Locations;
using Rise.Domain.Projects;
using Rise.Domain.Stations;

namespace Rise.Domain.MaintObjects;

public class MaintObj : Entity {
	private MaintObj() { }

	public MaintObj(string name, string serialNumber, MaintObjType maintObjType, MaintObjPlan maintObjPlan, string? brand, Location location) {
		Name = Guard.Against.NullOrWhiteSpace(name);
		SerialNumber = Guard.Against.NullOrWhiteSpace(serialNumber);
		MaintObjType = Guard.Against.Null(maintObjType);
		MaintObjPlan = Guard.Against.Null(maintObjPlan);
		Brand = brand;
		Location = Guard.Against.Null(location);
	}

	public string Name { get; private set; }
	public string SerialNumber { get; private set; }
	public string? Brand { get; private set; }

	public MaintObjType MaintObjType { get; private set; }
	public MaintObjStatus MaintObjStatus { get; private set; } = MaintObjStatus.Inactive;
	public MaintObjPlan MaintObjPlan { get; private set; }
	public Installation? Installation { get; private set; }

	public MeasurementStation? Station { get; private set; }
	public Location Location { get; private set; }

	public Technician? LastScannedBy { get; private set; }

	public void AssignToStation(MeasurementStation station) {
		Station = Guard.Against.Null(station);
		Installation = new Installation(Station.Installation.InstalledAt, Station.Installation.InstalledAt);
		Location = station.Location;
		SetStatus(MaintObjStatus.Active);
	}

	public void UnassignFromStation() {
		Installation = null;
		Station = null;
		SetStatus(MaintObjStatus.Inactive);
	}

	public void MoveToStockLocation(Location location) {
		if (Location.Equals(location)) {
			return;
		}

		Location = Guard.Against.Null(location);
		UnassignFromStation();
	}

	public void ScanBy(Technician technician) {
		LastScannedBy = Guard.Against.Null(technician);
	}


	public void SetStatus(MaintObjStatus status) {
		MaintObjStatus = status;
	}

	public void Edit(string name, string serialNumber, MaintObjPlan maintObjPlan, string? brand,
		Installation installation) {
		Name = Guard.Against.NullOrWhiteSpace(name);
		SerialNumber = Guard.Against.NullOrWhiteSpace(serialNumber);
		MaintObjPlan = maintObjPlan;
		Brand = brand;
		Installation = installation;
	}
}