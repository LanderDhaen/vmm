namespace Rise.Domain.Devices;

public record DeviceSpecification(
	Guid MeasurementStationId,
	string Name,
	MaintenanceDeviceType DeviceType,
	string? SerialNumber = null,
	IReadOnlyDictionary<string, string>? Metadata = null
);