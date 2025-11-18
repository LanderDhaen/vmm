namespace Rise.Domain.Devices;

public class Probe : MaintenanceDevice, IMaintenanceDevice {
	private Probe() : base(Guid.Empty, "", MaintenanceDeviceType.Probe) { }

	public Probe(DeviceSpecification spec, IEnumerable<string>? measurementTypes = null) : base(
		spec.MeasurementStationId, spec.Name, MaintenanceDeviceType.Probe, spec.SerialNumber) {
		MeasurementTypes = measurementTypes is null ? new List<string>() : new List<string>(measurementTypes);
	}

	public IReadOnlyList<string> MeasurementTypes { get; private set; } = Array.Empty<string>();

	public TimeSpan SamplingInterval { get; private set; } = TimeSpan.FromMinutes(1);

	public void SetSamplingInterval(TimeSpan interval) {
		if (interval <= TimeSpan.Zero) {
			throw new ArgumentOutOfRangeException(nameof(interval));
		}

		SamplingInterval = interval;
	}
}