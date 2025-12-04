namespace Rise.Shared.Zones;

/// <summary>
///     Contains data transfer objects (DTOs) used for zone-related operations.
/// </summary>
public static class ZoneDto {
	/// <summary>
	///     Represents a zone index containing minimalistic zone details.
	/// </summary>
	public class Index {
		public required Guid Id { get; set; }
		public required string Name { get; set; }
	}
}