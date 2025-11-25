using Rise.Domain.Locations;
using Rise.Domain.MaintObjects;
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
	public ICollection<MaintObj> MaintObjects { get; } = new List<MaintObj>();


	public IReadOnlyCollection<MeasurementStationHistory> MeasurementStationHistories => _measurementStationHistories.AsReadOnly();

	// Functions

	public void MoveTo(Location location) {

		// Check if the values are the same as the old values

		if (location.Equals(Location)) {
			return;
		}

		// Create history object using the old values

		ValidityRange validityRange = new(UpdatedAt, DateTime.UtcNow);
		MeasurementStationHistory measurementStationHistory = new(new Installation(Installation.InstalledAt, Installation.RemovedAt), Location, validityRange);
		_measurementStationHistories.Add(measurementStationHistory);

		// Assign the new values

		Location = location;
	}

	public void UpdateInstallation(Installation installation) {

		// Check if the values are the same as the old values

		if (installation.Equals(Installation)) {
			return;
		}

		// Create history object using the old values

		ValidityRange validityRange = new(UpdatedAt, DateTime.UtcNow);
		MeasurementStationHistory measurementStationHistory = new(new Installation(Installation.InstalledAt, Installation.RemovedAt), Location, validityRange);
		_measurementStationHistories.Add(measurementStationHistory);

		// Assign the new values

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

	public MeasurementStationHistory(Installation installation, Location location, ValidityRange validityRange) {
		Location = Guard.Against.Null(location);
		Installation = Guard.Against.Null(installation);
		ValidityRange = Guard.Against.Null(validityRange);
	}

	public Installation Installation { get; init; }

	public Location Location { get; init; }
}