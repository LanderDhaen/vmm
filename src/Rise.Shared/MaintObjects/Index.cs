namespace Rise.Shared.MaintObjects;

/// <summary>
///     Represents the response structure for maintenance-object-related operations.
/// </summary>
public static class MaintObjResponse {
	public class Index {
		public IEnumerable<MaintObjDto.Index> MaintObjects { get; set; } = [];
		public int TotalCount { get; set; }
	}

	public class Detail {
		public required MaintObjDto.Detail MaintObj { get; set; }
	}
}