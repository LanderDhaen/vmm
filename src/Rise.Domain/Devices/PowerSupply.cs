namespace Rise.Domain.Devices;

public class PowerSupply : MaintenanceDevice, IMaintenanceDevice {
	private PowerSupply() : base(Guid.Empty, "", MaintenanceDeviceType.PowerSupply) { }

	public PowerSupply(Guid measurementStationId, string name, decimal nominalVoltage, decimal maxCurrent,
		bool hasBattery = false, string? serialNumber = null) : base(measurementStationId, name,
		MaintenanceDeviceType.PowerSupply, serialNumber) {
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