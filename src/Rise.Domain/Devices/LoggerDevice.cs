namespace Rise.Domain.Devices;

using System;

public class LoggerDevice : MaintenanceDevice, IMaintenanceDevice {
	private LoggerDevice() : base(Guid.Empty, "", MaintenanceDeviceType.Logger) { }

	public LoggerDevice(DeviceSpecification spec) : base(spec.MeasurementStationId, spec.Name, MaintenanceDeviceType.Logger, spec.SerialNumber) {
		// Initialize logger-specific defaults here
	}

	// Example of a logger-specific property
	public int MaxEntries { get; private set; } = 1000;

	public void SetMaxEntries(int max) {
		if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max));
		MaxEntries = max;
	}
}
