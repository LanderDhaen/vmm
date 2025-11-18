namespace Rise.Domain.Devices;

using System;

public class PowerSupplyDevice : MaintenanceDevice, IMaintenanceDevice {
	private PowerSupplyDevice() : base(Guid.Empty, "", MaintenanceDeviceType.PowerSupply) { }

	public PowerSupplyDevice(DeviceSpecification spec, decimal nominalVoltage, decimal maxCurrent, bool hasBattery = false) : base(spec.MeasurementStationId, spec.Name, MaintenanceDeviceType.PowerSupply, spec.SerialNumber) {
		NominalVoltage = nominalVoltage;
		MaxCurrent = maxCurrent;
		HasBattery = hasBattery;
	}

	public decimal NominalVoltage { get; private set; }
	public decimal MaxCurrent { get; private set; }
	public bool HasBattery { get; private set; }

	public void UpdateBatteryPresence(bool hasBattery) {
		HasBattery = hasBattery;
	}
}
