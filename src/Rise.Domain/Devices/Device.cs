using Rise.Domain.Projects;

namespace Rise.Domain.Devices;

public class Device : Entity {
	private Installation _installation = null!;
	private string _serialNumber = string.Empty;

	private Device() { }

	public Device(string name, string serialNumber, DeviceType deviceType, string? brand = null) {
		Name = Guard.Against.NullOrWhiteSpace(name);
		SerialNumber = Guard.Against.NullOrWhiteSpace(serialNumber);
		DeviceType = deviceType;
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

	public DeviceType DeviceType { get; private set; }

	public string? Brand { get; private set; }
}