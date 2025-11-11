namespace Rise.Shared.Networks;

/// <summary>
///     Contains data transfer objects (DTOs) used for measurement network-related operations.
/// </summary>
public static class MeasurementNetworkDto {
	/// <summary>
	///     Represents a measurement network index containing minimalistic measurement network details.
	/// </summary>
	public class Index {
		public required Guid Id { get; set; }
		public required string Name { get; set; }
		public required string Zone { get; set; }
	}
}