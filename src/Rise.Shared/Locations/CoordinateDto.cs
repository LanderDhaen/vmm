namespace Rise.Shared.Locations;

/// <summary>
///     Contains data transfer objects (DTOs) used for coordinate-related operations.
/// </summary>
public static class CoordinateDto {
	/// <summary>
	///     Represents a coordinate index containing minimalistic coordinate details.
	/// </summary>
	public class Index {
		public required double X { get; init; }
		public required double Y { get; init; }
	}
}