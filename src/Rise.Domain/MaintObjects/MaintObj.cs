using Rise.Domain.Projects;

namespace Rise.Domain.MaintObjects;

public class MaintObj : Entity {
	private Installation _installation = null!;
	private string _serialNumber = string.Empty;

	private MaintObj() { }

	public MaintObj(string name, string serialNumber, MaintObjType maintObjType, string? brand = null) {
		Name = Guard.Against.NullOrWhiteSpace(name);
		SerialNumber = Guard.Against.NullOrWhiteSpace(serialNumber);
		MaintObjType = maintObjType;
		Brand = brand;
	}

	public string SerialNumber {
		get => _serialNumber;
		private set => _serialNumber = Guard.Against.NullOrEmpty(value);
	}

	public Installation Installation {
		get => _installation;
		private set => _installation = Guard.Against.Null(value);
	}

	public string Name { get; private set; } = null!;
	public Status Status { get; } = Status.Actief;

	public MaintObjType MaintObjType { get; private set; }

	public string? Brand { get; private set; }
}