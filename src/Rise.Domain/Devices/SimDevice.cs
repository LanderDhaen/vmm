namespace Rise.Domain.Devices;

using System;

public class SimDevice : MaintenanceDevice, IMaintenanceDevice {
	private SimDevice() : base(Guid.Empty, "", MaintenanceDeviceType.Sim) { }

	public SimDevice(DeviceSpecification spec, string imsi, string iccid, string? operatorName = null, int signalStrength = 0) : base(spec.MeasurementStationId, spec.Name, MaintenanceDeviceType.Sim, spec.SerialNumber) {
		Imsi = imsi ?? throw new ArgumentNullException(nameof(imsi));
		Iccid = iccid ?? throw new ArgumentNullException(nameof(iccid));
		OperatorName = operatorName;
		SignalStrength = signalStrength;
	}

	public string Imsi { get; private set; } = default!;
	public string Iccid { get; private set; } = default!;
	public string? OperatorName { get; private set; }
	public int SignalStrength { get; private set; }

	public void UpdateSignalStrength(int value) {
		SignalStrength = value;
	}

	public void UpdateOperator(string? name) {
		OperatorName = name;
	}
}