using System.Collections.Generic;

namespace Rise.Domain.Locations;

/// <summary>
///     A physical address using the <see cref="ValueObject" /> design pattern.
///     <see cref="https://enterprisecraftsmanship.com/posts/value-objects-explained/" />
/// </summary>
public class Address : ValueObject {
	/// <summary>
	///     Entity Framework requires a default constructor.
	/// </summary>
	private Address() {
		Addressline1 = null;
		Addressline2 = null;
		City = string.Empty;
		PostalCode = string.Empty;
		CountryIso = string.Empty;
	}

	public Address(string? addressline1, string? addressline2, string city, string zipCode, string countryIso = "BE") {
		Addressline1 = addressline1;
		Addressline2 = addressline2;
		City = city;
		PostalCode = zipCode;
		CountryIso = countryIso;
	}

	public string? Addressline1 { get; }
	public string? Addressline2 { get; }
	public string City { get; }
	public string PostalCode { get; }
	public string CountryIso { get; }


	protected override IEnumerable<object> GetEqualityComponents() {
		yield return PostalCode;
		yield return City;
		yield return CountryIso;
		yield return Addressline1 ?? string.Empty;
		yield return Addressline2 ?? string.Empty;
	}
}