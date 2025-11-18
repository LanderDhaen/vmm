namespace Rise.Domain.Devices;

using System;
using System.Collections.Generic;

public class MaintenanceDeviceFactory : IMaintenanceDeviceFactory {
	public MaintenanceDeviceFactory() {
		// nothing to register anymore; constructors are explicit
	}

	public IMaintenanceDevice CreateLogger(Guid measurementStationId, string name, string? serialNumber = null) {
		return new Logger(measurementStationId, name, serialNumber);
	}

	public IMaintenanceDevice CreateProbe(Guid measurementStationId, string name, IEnumerable<string>? measurementTypes = null, string? serialNumber = null) {
		return new Probe(measurementStationId, name, measurementTypes, serialNumber);
	}

	public IMaintenanceDevice CreateSim(Guid measurementStationId, string name, string imsi, string iccid, string? operatorName = null, int signalStrength = 0, string? serialNumber = null) {
		return new Sim(measurementStationId, name, imsi, iccid, operatorName, signalStrength, serialNumber);
	}

	public IMaintenanceDevice CreatePowerSupply(Guid measurementStationId, string name, decimal nominalVoltage, decimal maxCurrent, bool hasBattery = false, string? serialNumber = null) {
		return new PowerSupply(measurementStationId, name, nominalVoltage, maxCurrent, hasBattery, serialNumber);
	}
}