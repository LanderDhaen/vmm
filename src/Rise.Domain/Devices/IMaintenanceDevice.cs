namespace Rise.Domain.Devices;

public interface IMaintenanceDevice {
	Guid Id { get; }
	Guid MeasurementStationId { get; }
	string Name { get; }
	MaintenanceDeviceType DeviceType { get; }
	string? SerialNumber { get; }
}