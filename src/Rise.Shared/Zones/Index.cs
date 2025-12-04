namespace Rise.Shared.Zones;

/// <summary>
///     Represents the response structure for zone-related operations.
/// </summary>
public static class ZoneResponse {
	public class Index {
		public IEnumerable<ZoneDto.Index> zones { get; set; } = [];
	}
}