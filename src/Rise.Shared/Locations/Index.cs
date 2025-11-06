namespace Rise.Shared.Locations;

/// <summary>
///     Represents the response structure for location-related operations.
/// </summary>
public static class LocationResponse {
	public class Index {
		public IEnumerable<LocationDto.Index> Locations { get; set; } = [];
	}
}