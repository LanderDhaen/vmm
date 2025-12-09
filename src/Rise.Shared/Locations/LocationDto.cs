namespace Rise.Shared.Locations;

/// <summary>
///     Contains data transfer objects (DTOs) used for location-related operations.
/// </summary>
public static class LocationDto {
	/// <summary>
	///     Represents a location index containing minimalistic location details.
	/// </summary>
	public class Index {
		public required Guid Id { get; set; }
		public required string Name { get; set; }
		public required AddressDto.Index Address { get; init; }
		public required CoordinateDto.Index Coordinate { get; init; }
	}

	/// <summary>
	///     Represents a location index containing minimalistic location details.
	/// </summary>
	public class Detail {
		public required Guid Id { get; init; }
		public required string Name { get; init; }
		public string? AddressLine1 { get; init; }
		public string? AddressLine2 { get; init; }
		public required string City { get; init; }
		public required string PostalCode { get; init; }
		public required string Country { get; init; }
		public required double X { get; init; }
		public required double Y { get; init; }
	}
}