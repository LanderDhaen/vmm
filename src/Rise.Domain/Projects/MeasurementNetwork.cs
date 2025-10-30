namespace Rise.Domain.Projects;

/// <summary>
///     A measurement network is something that owns multiple projects.
/// </summary>
public class MeasurementNetwork : Entity {
	private string _name = String.Empty;
	private readonly List<Project> _projects = [];

	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private MeasurementNetwork() {
	}

	public MeasurementNetwork(string name, Zone zone) {
		Name = name;
		Zone = Guard.Against.Null(zone);
	}

	public string Name {
		get => _name;
		set => _name = Guard.Against.NullOrEmpty(value);
	}

	public Zone Zone { get; init; } = default!;
	
	public IReadOnlyList<Project> Projects => _projects.AsReadOnly();
}