using Rise.Domain.Stations;

namespace Rise.Domain.Devices;

public class MaintenanceDevice : Entity {
	private MaintenanceDevice() { }

	public MaintenanceDevice(
		Guid measurementStationId,
		string name,
		MaintenanceDeviceType deviceType,
		string? serialNumber = null
	) {
		MeasurementStationId = measurementStationId;
		Name = name ?? throw new ArgumentNullException(nameof(name));
		DeviceType = deviceType;
		SerialNumber = serialNumber;
	}

	/// <summary>
	///     The MeasurementStation this device belongs to (FK).
	/// </summary>
	public Guid MeasurementStationId { get; }

	/// <summary>
	///     Navigation property to the owning MeasurementStation.
	/// </summary>
	public MeasurementStation MeasurementStation { get; private set; } = default!;

	public string Name { get; private set; } = default!;

	public MaintenanceDeviceType DeviceType { get; }

	public string? SerialNumber { get; private set; }

	public void UpdateName(string name) {
		Name = name ?? throw new ArgumentNullException(nameof(name));
	}

	public void UpdateSerialNumber(string? serial) {
		SerialNumber = serial;
	}
}

public enum MaintenanceDeviceType {
	Logger,
	Probe,
	Sim,
	PowerSupply
}