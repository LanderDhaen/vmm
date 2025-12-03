namespace Rise.Shared.Stations;

/// <summary>
///     Represents the response structure for measurement station list operations.
/// </summary>
public static partial class MeasurementStationResponse {
	public class Index {
		public IEnumerable<MeasurementStationDto.Index> Stations { get; set; } = [];
		public int TotalCount { get; set; }
	}

	public class Detail {
		public required MeasurementStationDto.Detail Station { get; set; }
	}

	public class History {
		public List<MeasurementStationHistoryDto.Modification> Modifications { get; set; } = [];
	}
}