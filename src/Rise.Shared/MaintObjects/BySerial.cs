namespace Rise.Shared.MaintObjects;

public static partial class MaintObjRequest {
	public class BySerial {
		public string SerialNumber { get; set; } = default!;
	}
}

public static partial class MaintObjResponse {
	public class BySerial {
		public Guid Id { get; set; }
		public string SerialNumber { get; set; } = default!;
		public string Name { get; set; } = default!;
	}
}