namespace Rise.Shared.MaintObjects;

public static partial class MaintObjRequest {
	public class UpdateStatus {
		public Guid MaintObjId { get; set; }
		public MaintObjStatus NewStatus { get; set; }
	}
}

public enum MaintObjStatus {
	Actief,
	Inactief,
	Defect
}