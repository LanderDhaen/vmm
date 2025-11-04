namespace Rise.Shared.Locations;

/// <summary>
///     Contains data transfer objects (DTOs) used for address-related operations.
/// </summary>
public static class AddressDto {
	/// <summary>
	///     Represents an address index containing minimalistic daterange details.
	/// </summary>
	public class Index {
		public required string AddressLine1 { get; init; }
		public required string AddressLine2 { get; init; }
		public required string City { get; init; }
		public required string PostalCode { get; init; }
		public required string CountryIso { get; init; }
	}

}