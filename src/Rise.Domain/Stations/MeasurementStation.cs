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

	public List<MeasurementStationModification> GetModifications() {

		var modifications = new List<MeasurementStationModification>();
		
		// TODO: Add current value

		for (var i = 0; i < _measurementStationHistories.Count - 1; i++) {
			
			var curr = _measurementStationHistories[i];
			var prev = _measurementStationHistories[i + 1];

			if (!prev.Location.Equals(curr.Location)) {
				modifications.Add(new MeasurementStationModification(
					ModificationType.LocationChanged,
					curr.ValidityRange.ValidFrom,
					prev.Location.Name,
					curr.Location.Name));
			}

			if (!prev.Installation.InstalledAt.Equals(curr.Installation.InstalledAt)) {
				modifications.Add(new MeasurementStationModification(
					ModificationType.InstalledAtChanged,
					curr.ValidityRange.ValidFrom,
					prev.Installation.InstalledAt.ToString(),
					curr.Installation.InstalledAt.ToString()));
			}

			if (!prev.Installation.RemovedAt.Equals(curr.Installation.RemovedAt)) {
				modifications.Add(new MeasurementStationModification(
					ModificationType.RemovedAtChanged,
					curr.ValidityRange.ValidFrom,
					prev.Installation.RemovedAt?.ToString() ?? "-",
					curr.Installation.RemovedAt?.ToString() ?? "-"));
			}
		}
		
		return modifications;
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

public enum ModificationType {
	LocationChanged,
	InstalledAtChanged,
	RemovedAtChanged
}

public class MeasurementStationModification {
	public MeasurementStationModification(ModificationType changeType, DateTime modifiedAt, string previousValue, string currentValue) {
		ModificationType = changeType;
		ModifiedAt = modifiedAt;
		PreviousValue = previousValue;
		CurrentValue = currentValue;
	}
	
	public ModificationType ModificationType { get; init; }
	public DateTime ModifiedAt { get; init; }
	public string PreviousValue { get; init; }
	public string CurrentValue { get; init; }
}