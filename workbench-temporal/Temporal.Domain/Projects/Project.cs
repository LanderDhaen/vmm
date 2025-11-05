using Temporal.Domain.Locations;

namespace Temporal.Domain.Projects;

/// <summary>
///     A project is something that owns multiple measuring stations.
/// </summary>
public class Project : Entity {
	private string _code = string.Empty;

	private string _name = string.Empty;

	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private Project() {
	}

	public Project(string name, string code, DateRange dateRange) {
		Name = name;
		Code = code;
		DateRange = Guard.Against.Null(dateRange);
	}

	public string Name {
		get => _name;
		set => _name = Guard.Against.NullOrWhiteSpace(value);
	}

	public string Code {
		get => _code;
		set => _code = Guard.Against.NullOrWhiteSpace(value);
	}

	public DateRange DateRange { get; init; } = default!;

	public ICollection<Stations.MeasurementStation> Stations { get; private set; } = new List<Stations.MeasurementStation>();
}