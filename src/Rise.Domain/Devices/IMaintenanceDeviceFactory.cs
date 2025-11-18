namespace Rise.Domain.Devices;

public interface IMaintenanceDeviceFactory {
	IMaintenanceDevice CreateLogger(Guid measurementStationId, string name, string? serialNumber = null);
	IMaintenanceDevice CreateProbe(Guid measurementStationId, string name, IEnumerable<string>? measurementTypes = null, string? serialNumber = null);
	IMaintenanceDevice CreateSim(Guid measurementStationId, string name, string imsi, string iccid, string? operatorName = null, int signalStrength = 0, string? serialNumber = null);
	IMaintenanceDevice CreatePowerSupply(Guid measurementStationId, string name, decimal nominalVoltage, decimal maxCurrent, bool hasBattery = false, string? serialNumber = null);
}