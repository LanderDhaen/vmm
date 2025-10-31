namespace Rise.Domain.Projects;

/// <summary>
///     A zone is something that owns multiple measurement networks.
/// </summary>
public class Zone : Entity {
	private string _name = String.Empty;

	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private Zone() {
	}

	public Zone(string name) {
		Name = name;
	}
	
	public string Name {
		get => _name;
		set => _name = Guard.Against.NullOrWhiteSpace(value);
	}
}