namespace Rise.Client.Utilities;

/// <summary>
/// Provides helper methods to format and display information about a location.
/// </summary>
public static class LocationUtility
{
	/// <summary>
	/// Formats an address from individual components.
	/// </summary>
	/// <param name="addresLine1">The first line of the address, or <c>null</c> if not provided.</param>
	/// <param name="addresLine2">The second line of the address, or <c>null</c> if not provided.</param>
	/// <param name="city">The city of the address.</param>
	/// <param name="postalCode">The postal code of the address.</param>
	/// <param name="country">The iso code of the country of the address.</param>
	/// <returns>
	/// A formatted address string where non-null lines are combined with commas.
	/// </returns>
	/// <example>
	/// Burchtstraat 1, Bus 3, 9300 Aalst, BE<br/>
	/// </example>
	public static string FormatAddress(
		string? addresLine1,
		string? addresLine2,
		string city,
		string postalCode,
		string country) {

		string address = "";

		if (!string.IsNullOrWhiteSpace(addresLine1)) {
			address += $"{addresLine1} ";
		}

		if (!string.IsNullOrWhiteSpace(addresLine2))
			address += $"{addresLine2}, ";

		address += $"{postalCode} {city}, {country}";

		return address;
	}
}