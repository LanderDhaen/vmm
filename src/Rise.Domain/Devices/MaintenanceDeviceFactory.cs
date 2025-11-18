namespace Rise.Domain.Devices;

using System;
using System.Collections.Generic;

public class MaintenanceDeviceFactory : IMaintenanceDeviceFactory {
	private readonly Dictionary<MaintenanceDeviceType, Func<DeviceSpecification, IMaintenanceDevice>> _creators = new();

	public MaintenanceDeviceFactory() {
		// register known types
		Register(MaintenanceDeviceType.Logger, spec => new LoggerDevice(spec));
		Register(MaintenanceDeviceType.Probe, spec => new ProbeDevice(spec));
		Register(MaintenanceDeviceType.Sim, spec => new SimDevice(spec, imsi: "", iccid: ""));
		Register(MaintenanceDeviceType.PowerSupply, spec => new PowerSupplyDevice(spec, nominalVoltage: 0m, maxCurrent: 0m));
		// other types can be registered by calling Register
	}

	public void Register(MaintenanceDeviceType type, Func<DeviceSpecification, IMaintenanceDevice> creator) {
		_creators[type] = creator ?? throw new ArgumentNullException(nameof(creator));
	}

	public IMaintenanceDevice Create(DeviceSpecification spec) {
		if (spec is null) throw new ArgumentNullException(nameof(spec));

		if (!_creators.TryGetValue(spec.DeviceType, out var creator)) {
			throw new ArgumentException($"No creator registered for device type {spec.DeviceType}", nameof(spec));
		}

		return creator(spec);
	}
}