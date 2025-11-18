namespace Rise.Domain.Devices;

public class MaintenanceDeviceFactory : IMaintenanceDeviceFactory {
	private readonly Dictionary<MaintenanceDeviceType, Func<DeviceSpecification, IMaintenanceDevice>> _creators = new();

	public MaintenanceDeviceFactory() {
		// register known types
		Register(MaintenanceDeviceType.Logger, spec => new Logger(spec));
		Register(MaintenanceDeviceType.Probe, spec => new Probe(spec));
		Register(MaintenanceDeviceType.Sim, spec => new Sim(spec, "", ""));
		Register(MaintenanceDeviceType.PowerSupply,
			spec => new PowerSupply(spec, 0m, 0m));
		// other types can be registered by calling Register
	}

	public IMaintenanceDevice Create(DeviceSpecification spec) {
		if (spec is null) {
			throw new ArgumentNullException(nameof(spec));
		}

		if (!_creators.TryGetValue(spec.DeviceType, out Func<DeviceSpecification, IMaintenanceDevice>? creator)) {
			throw new ArgumentException($"No creator registered for device type {spec.DeviceType}", nameof(spec));
		}

		return creator(spec);
	}

	public void Register(MaintenanceDeviceType type, Func<DeviceSpecification, IMaintenanceDevice> creator) {
		_creators[type] = creator ?? throw new ArgumentNullException(nameof(creator));
	}
}