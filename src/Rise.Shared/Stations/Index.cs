namespace Rise.Shared.Stations;

/// <summary>
///     Represents the response structure for measurement station list operations.
/// </summary>
public static class MeasurementStationResponse
{
	public class Index
	{
		public IEnumerable<MeasurementStationDto.Index> Stations { get; set; } = [];
		public int TotalCount { get; set; }
	}
}