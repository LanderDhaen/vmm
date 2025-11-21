using Rise.Domain.Locations;
using Rise.Domain.Projects;

namespace Rise.Domain.Stations;

/// <summary>
///     A measurement station is something that owns multiple devices.
/// </summary>
public class MeasurementStation : Entity {
	
	private readonly List<MeasurementStationHistory> _measurementStationHistories = [];
	
	private MeasurementStation() { }

	public MeasurementStation(string code, Location location, Installation installation) {
		Code = code;
		Location = location;
		Installation = installation;
	}
	
	// Properties

	public string Code { get; private set; }
	
	public Installation Installation { get; private set; }
	
	// Relations
	
	public Location Location { get; private set; }

	public ICollection<Project> Projects { get; } = new List<Project>();
	
	public IReadOnlyCollection<MeasurementStationHistory> MeasurementStationHistories => _measurementStationHistories.AsReadOnly();
	
	// Functions

	public void MoveTo(Location newLocation) {
		Location = newLocation;
	}

	public void UpdateInstallation(Installation installation) {
		Installation = installation;
	}
}

/// <summary>
///     A measurement station history is something that holds changes made to a measurement station.
/// </summary>
public class MeasurementStationHistory : HistoryEntity {
	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private MeasurementStationHistory() { }

	public MeasurementStationHistory(Installation installation, ValidityRange validityRange) {
		Installation = Guard.Against.Null(installation);
		ValidityRange = Guard.Against.Null(validityRange);
	}
	
	public Installation Installation { get; init; }
}
