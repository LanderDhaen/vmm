namespace Rise.Shared.MaintObjects;

public static partial class MaintObjRequest {
	// public class UpdateStatus {
	// 	public MaintObjStatus NewStatus { get; set; }
	// }
}

public enum MaintObjType {
	Sonde,
	Logger,
	Modem,
	Sim,
	Voeding
}