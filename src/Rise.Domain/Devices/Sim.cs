namespace Rise.Domain.Devices;

public class Sim : MaintenanceDevice, IMaintenanceDevice {
	private Sim() : base(Guid.Empty, "", MaintenanceDeviceType.Sim) { }

	public Sim(
		Guid measurementStationId,
		string name,
		string imsi,
		string iccid,
		string? operatorName = null,
		int signalStrength = 0,
		string? serialNumber = null
	) : base(measurementStationId, name, MaintenanceDeviceType.Sim, serialNumber) {
		Imsi = imsi ?? throw new ArgumentNullException(nameof(imsi));
		Iccid = iccid ?? throw new ArgumentNullException(nameof(iccid));
		OperatorName = operatorName;
		SignalStrength = signalStrength;
	}

	public string Imsi { get; private set; } = null!;
	public string Iccid { get; private set; } = null!;
	public string? OperatorName { get; private set; }
	public int SignalStrength { get; private set; }

	public void UpdateSignalStrength(int value) {
		SignalStrength = value;
	}

	public void UpdateOperator(string? name) {
		OperatorName = name;
	}
}