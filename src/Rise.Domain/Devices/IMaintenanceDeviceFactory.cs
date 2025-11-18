using System;

namespace Rise.Domain.Devices;

public interface IMaintenanceDeviceFactory {
	IMaintenanceDevice Create(DeviceSpecification spec);
}
