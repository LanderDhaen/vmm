namespace Rise.Domain.Devices;

using System;

public interface IMaintenanceDevice {
	Guid Id { get; }
	Guid MeasurementStationId { get; }
	string Name { get; }
	MaintenanceDeviceType DeviceType { get; }
	string? SerialNumber { get; }
}
