using Rise.Domain.Locations;
using Rise.Domain.MaintObjects;
using Rise.Domain.Projects;

namespace Rise.Domain.Stations;

/// <summary>
///     A measurement station is something that owns multiple devices.
/// </summary>
public class MeasurementStation : Entity {
	// history list (single consistent name)
	private readonly List<MeasurementStationHistory> _measurementStationHistories = new();

	// EF Core requires a parameterless ctor. Keep it but properties are initialized inline.
	private MeasurementStation() { }

	public MeasurementStation(string code, Location location, Installation installation) {
		Code = Guard.Against.NullOrWhiteSpace(code);
		Location = Guard.Against.Null(location);
		Installation = Guard.Against.Null(installation);
	}

	// Properties - inline initializers satisfy the nullability checker for the parameterless ctor
	public string Code { get; private set; } = string.Empty;
	public Installation Installation { get; private set; } = default!;
	public Location Location { get; private set; } = default!;

	// Relations
	public ICollection<Project> Projects { get; } = new List<Project>();
	public ICollection<MaintObj> MaintObjects { get; } = new List<MaintObj>();

	public IReadOnlyCollection<MeasurementStationHistory> MeasurementStationHistories =>
		_measurementStationHistories.AsReadOnly();

	// Functions

	public void UpdateProjectList(List<Project> projects) {
		List<Project> closedProjectsNotAlreadyLinked = projects
			.Where(p => p.Status == Status.Afgesloten &&
						Projects.All(existing => existing.Id != p.Id))
			.ToList();

		if (closedProjectsNotAlreadyLinked.Any()) {
			throw new InvalidOperationException("Afgesloten projecten kunnen niet worden bewerkt of gekoppeld.");
		}

		// Get all the projects that are not already in the measurement station
		List<Project> projectsToAdd = projects
			.Where(s => Projects.All(pr => pr.Id != s.Id))
			.ToList();

		projectsToAdd.ForEach(p => Projects.Add(p));

		// Get all the projects that are already in the measurement station but need to be deleted
		List<Project> projectsToRemove = Projects
			.Where(p => projects.All(pr => pr.Id != p.Id))
			.ToList();

		projectsToRemove.ForEach(p => Projects.Remove(p));
	}

	public void MoveTo(Location location) {
		// Check if the values are the same as the old values
		if (location.Equals(Location)) {
			return;
		}

		// Create history object using the old values
		ValidityRange validityRange = new(UpdatedAt, DateTime.UtcNow);
		MeasurementStationHistory measurementStationHistory =
			new(new Installation(Installation.InstalledAt, Installation.RemovedAt), Location, validityRange);
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
		MeasurementStationHistory measurementStationHistory =
			new(new Installation(Installation.InstalledAt, Installation.RemovedAt), Location, validityRange);
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
	private MeasurementStationHistory() {
		Installation = default!;
		Location = default!;
	}

	public MeasurementStationHistory(Installation installation, Location location, ValidityRange validityRange) {
		Location = Guard.Against.Null(location);
		Installation = Guard.Against.Null(installation);
		ValidityRange = Guard.Against.Null(validityRange);
	}

	public Installation Installation { get; init; }
	public Location Location { get; init; }
}