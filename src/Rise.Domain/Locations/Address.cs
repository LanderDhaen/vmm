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
	}

	public Address(string addressline1, string addressline2, string city, string zipCode, string countryIso = "BE") {
		Addressline1 = Guard.Against.NullOrWhiteSpace(addressline1).Trim();
		Addressline2 = addressline2;
		City = Guard.Against.NullOrWhiteSpace(city).Trim();
		PostalCode = Guard.Against.NullOrWhiteSpace(zipCode).Trim();
		CountryIso = Guard.Against.NullOrWhiteSpace(countryIso).Trim();
	}

	public string Addressline1 { get; } = default!;
	public string Addressline2 { get; } = default!;
	public string City { get; } = default!;
	public string PostalCode { get; } = default!;
	public string CountryIso { get; } = default!;
	

	protected override IEnumerable<object> GetEqualityComponents() {
		yield return PostalCode;
		yield return City;
		yield return CountryIso;
		yield return Addressline1;
		yield return Addressline2;
	}
}