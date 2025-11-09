using Rise.Domain.Networks;
using Rise.Domain.Stations;

namespace Rise.Domain.Projects;

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

	public Project(string name, string code, DateRange dateRange, MeasurementNetwork measurementNetwork,
		Status status = Status.Active) {
		Name = name;
		Code = code;
		DateRange = Guard.Against.Null(dateRange);
		MeasurementNetwork = Guard.Against.Null(measurementNetwork);
		Status = status;
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

	public ICollection<MeasurementStation> Stations { get; private set; } =
		new List<MeasurementStation>();

	public MeasurementNetwork MeasurementNetwork { get; init; } = default!;

	public Status Status { get; private set; } = Status.Active;

	public void Edit(string name) {
		Name = Guard.Against.NullOrEmpty(name);
	}
}