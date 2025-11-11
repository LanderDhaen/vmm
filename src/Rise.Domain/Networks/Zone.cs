namespace Rise.Domain.Networks;

/// <summary>
///     A zone is something that owns multiple measurement networks.
/// </summary>
public class Zone : Entity {
	private string _code = string.Empty;
	private string _name = string.Empty;

	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private Zone() {
	}

	public Zone(string name, string code) {
		Name = name;
		Code = code;
	}

	public string Name {
		get => _name;
		set => _name = Guard.Against.NullOrWhiteSpace(value);
	}

	public string Code {
		get => _code;
		set => _code = Guard.Against.NullOrWhiteSpace(value);
	}
}