namespace Rise.Domain.Devices;

using System;
using Rise.Domain.Stations;

public class MaintenanceDevice : Entity {
	// Parameterless ctor for EF / serializers
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
	/// The MeasurementStation this device belongs to (FK).
	/// </summary>
	public Guid MeasurementStationId { get; private set; }

	/// <summary>
	/// Navigation property to the owning MeasurementStation.
	/// </summary>
	public MeasurementStation MeasurementStation { get; private set; } = default!;

	public string Name { get; private set; } = default!;

	public MaintenanceDeviceType DeviceType { get; private set; }

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