using Rise.Domain.Projects;

namespace Rise.Domain.MaintObjects;

public class MaintObj : Entity {
	private MaintObj() { }

	public MaintObj(string name, string serialNumber, MaintObjType maintObjType, string? brand,
		Installation installation) {
		Name = Guard.Against.NullOrWhiteSpace(name);
		SerialNumber = Guard.Against.NullOrWhiteSpace(serialNumber);
		MaintObjType = maintObjType;
		Brand = brand;
		Installation = installation;
	}

	public string SerialNumber { get; private set; }
	public string Name { get; private set; } = null!;
	public MaintObjType MaintObjType { get; private set; }
	public string? Brand { get; private set; }
	public Installation Installation { get; private set; }
	public Status Status { get; } = Status.Actief;
}