namespace Rise.Domain.Locations;

/// <summary>
///     A project is something that owns multiple measuring stations.
/// </summary>
public class Location : Entity {
	private string _name = string.Empty;

	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private Location() {
		// kept empty because inline initializers satisfy the compiler's nullability checks
	}

	public Location(string name, Address address, Coordinate coordinate, bool isStockLocation = false) {
		Name = name;
		Address = Guard.Against.Null(address);
		Coordinate = Guard.Against.Null(coordinate);
		IsStockLocation = isStockLocation;
	}

	public string Name {
		get => _name;
		set => _name = Guard.Against.NullOrWhiteSpace(value);
	}

	/// <summary>
	///     The address is immutable and owned by the <see cref="Location" />. If you want to change the address, create a new
	///     Address and link it to the location.
	///     <see cref="LocationConfiguration" />
	/// </summary>
	public Address Address { get; init; } = null!;

	/// <summary>
	///     The coordinate is immutable and owned by the <see cref="Location" />. If you want to change the coordinate, create
	///     a new
	///     Coordinate and link it to the location.
	///     <see cref="LocationConfiguration" />
	/// </summary>
	public Coordinate Coordinate { get; init; } = null!;

	public bool IsStockLocation { get; set; }

	public void Edit(string name) {
		Name = Guard.Against.NullOrEmpty(name);
	}
}