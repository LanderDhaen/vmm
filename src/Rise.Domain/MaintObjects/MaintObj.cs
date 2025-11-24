using Rise.Domain.Stations;

namespace Rise.Domain.MaintObjects;

public class MaintObj : Entity {
	private MaintObj() { }

	//TODO: locatie toevoegen (van meetstation), zo niet dan stock(bool)
	//TODO: meetstation heeft een lijst van maintobj(actief) + maintobj hoort bij een meetstation(actief)/stock(inactief)
	public MaintObj(string name, string serialNumber, MaintObjType maintObjType, string? brand,
		Installation installation, bool isInStock) {
		Name = Guard.Against.NullOrWhiteSpace(name);
		SerialNumber = Guard.Against.NullOrWhiteSpace(serialNumber);
		MaintObjType = maintObjType;
		Brand = brand;
		Installation = installation;
		IsInStock = isInStock;
	}
	//TODO: technieker bijhouden die het laatst gescanned heeft
	//TODO: meetstation toevoegen

	public string SerialNumber { get; private set; }
	public Installation Installation { get; private set; }
	public string Name { get; private set; } = null!;
	public MaintObjStatus Status { get; } = MaintObjStatus.Actief;
	public MaintObjType MaintObjType { get; private set; }
	public string? Brand { get; private set; }
	public bool IsInStock { get; private set; } = true;

	// Nullable FK to MeasurementStation: when null the maint-object is 'in stock'
	public Guid? MeasurementStationId { get; private set; }
	public MeasurementStation? Station { get; private set; }

	// Helper to assign/unassign a maint object to a station in a controlled way
	public void AssignToStation(MeasurementStation station) {
		Station = Guard.Against.Null(station);
		MeasurementStationId = station.Id;
		IsInStock = false;
	}

	public void UnassignFromStation() {
		Station = null;
		MeasurementStationId = null;
		IsInStock = true;
	}
}