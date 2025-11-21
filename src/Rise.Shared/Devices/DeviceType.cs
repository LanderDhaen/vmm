namespace Rise.Shared.Devices;

public static class DeviceRequest {
	// public class UpdateStatus {
	// 	public Guid ProjectId { get; set; }
	// 	public Status NewStatus { get; set; }
	// }
}

public enum DeviceType {
	Sonde,
	Logger,
	Modem,
	Sim,
	Voeding
}