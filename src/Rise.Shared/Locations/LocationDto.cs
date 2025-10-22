using Rise.Shared.Locations;

namespace Rise.Shared.Projects;

/// <summary>
///     Contains data transfer objects (DTOs) used for location-related operations.
/// </summary>
public static class LocationDto {
	/// <summary>
	///     Represents a location index containing minimalistic location details.
	/// </summary>
	public class Index {
		public required int Id { get; set; }
		public required string Name { get; set; }
		public required AddressDto.Index Address { get; init; }
		public required CoordinateDto.Index Coordinate { get; init; }
	}
}