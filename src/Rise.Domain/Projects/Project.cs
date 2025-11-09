using Rise.Domain.Locations;

namespace Rise.Domain.Projects;

using Networks;

/// <summary>
///     A project is something that owns multiple measuring stations.
/// </summary>
public class Project : Entity {
	private string _code = string.Empty;
	private string _name = string.Empty;
	private DateRange _dateRange;
	private MeasurementNetwork _measurementNetwork;

	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private Project() {
	}

	public Project(string name, string code, DateRange dateRange, MeasurementNetwork measurementNetwork) {
		Name = name;
		Code = code;
		DateRange = dateRange;
		MeasurementNetwork = measurementNetwork;
	}

	public string Name {
		get => _name;
		set => _name = Guard.Against.NullOrWhiteSpace(value);
	}

	public string Code {
		get => _code;
		set => _code = Guard.Against.NullOrWhiteSpace(value);
	}

	public DateRange DateRange {
		get => _dateRange; 
		set => _dateRange = Guard.Against.Null(value);
	}

	public ICollection<Stations.MeasurementStation> Stations { get; private set; } = new List<Stations.MeasurementStation>();

	public MeasurementNetwork MeasurementNetwork {
		get => _measurementNetwork; 
		set => _measurementNetwork = Guard.Against.Null(value);
	}

	public void Edit(string name, string code, DateRange dateRange, MeasurementNetwork measurementNetwork) {
		Name = name;
		Code = code;
		DateRange = dateRange;
		MeasurementNetwork = measurementNetwork;
	}
}