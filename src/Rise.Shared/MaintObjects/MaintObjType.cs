namespace Rise.Shared.MaintObjects;

public static partial class MaintObjRequest {
	// public class UpdateStatus {
	// 	public Guid ProjectId { get; set; }
	// 	public Status NewStatus { get; set; }
	// }
}

public enum MaintObjType {
	Sonde,
	Logger,
	Modem,
	Sim,
	Voeding
}