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
	public bool IsInStock { get; } = true;
}