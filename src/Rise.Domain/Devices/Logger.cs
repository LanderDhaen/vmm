namespace Rise.Domain.Devices;

public class Logger : MaintenanceDevice, IMaintenanceDevice {
	private Logger() : base(Guid.Empty, "", MaintenanceDeviceType.Logger) { }

	// Use long constructor parameters instead of DeviceSpecification
	public Logger(Guid measurementStationId, string name, string? serialNumber = null) : base(
		measurementStationId, name, MaintenanceDeviceType.Logger, serialNumber) {
		// Initialize logger-specific defaults here
	}

	// Example of a logger-specific property
	public int MaxEntries { get; private set; } = 1000;

	public void SetMaxEntries(int max) {
		if (max <= 0) {
			throw new ArgumentOutOfRangeException(nameof(max));
		}

		MaxEntries = max;
	}
}