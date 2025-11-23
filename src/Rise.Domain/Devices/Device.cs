using Rise.Domain.Projects;
using Rise.Domain.Stations;

namespace Rise.Domain.Devices;

public class Device : Entity {
	private Device() { }

	public Device(string name, string serialNumber, DeviceType deviceType, string? brand = null) {
		Name = Guard.Against.NullOrWhiteSpace(name);
		SerialNumber = Guard.Against.NullOrWhiteSpace(serialNumber);
		DeviceType = deviceType;
		Brand = brand;
	}

	public string Name { get; private set; } = null!;
	public string SerialNumber { get; private set; } = null!;
	public Status Status { get; } = Status.Actief;

	public DeviceType DeviceType { get; private set; }

	public string? Brand { get; private set; }

	public Guid? MeasurementStationId { get; private set; }
	public MeasurementStation? MeasurementStation { get; private set; }

	public void Edit(string name, string serialNumber, DeviceType deviceType, string? brand = null) {
		if (Status == Status.Afgesloten) {
			throw new InvalidOperationException("Inactieve apparaten kunnen niet worden bewerkt.");
		}

		Name = Guard.Against.NullOrWhiteSpace(name);
		SerialNumber = Guard.Against.NullOrWhiteSpace(serialNumber);
		DeviceType = deviceType;
		Brand = brand;
	}

	/// <summary>
	///     Attach this device to a measurement station.
	/// </summary>
	public void AssignToMeasurementStation(MeasurementStation station) {
		MeasurementStation = Guard.Against.Null(station);
		MeasurementStationId = station.Id;
	}

	/// <summary>
	///     Detach this device from any measurement station.
	/// </summary>
	public void DetachFromMeasurementStation() {
		MeasurementStation = null;
		MeasurementStationId = null;
	}
}