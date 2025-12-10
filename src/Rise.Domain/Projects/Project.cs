using Rise.Domain.Networks;
using Rise.Domain.Stations;

namespace Rise.Domain.Projects;

/// <summary>
///     A project is something that owns multiple measuring stations.
/// </summary>
public class Project : Entity {
	// History

	private readonly List<ProjectHistory> _projectHistories = [];

	// Constructors

	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private Project() { }

	public Project(string name, string code, DateRange dateRange, MeasurementNetwork measurementNetwork) {
		Name = Guard.Against.NullOrWhiteSpace(name);
		Code = Guard.Against.NullOrWhiteSpace(code);
		DateRange = Guard.Against.Null(dateRange);
		MeasurementNetwork = Guard.Against.Null(measurementNetwork);
		UpdateStatusBasedOnEndDate();
	}

	// Properties

	public string Name { get; private set; }
	public string Code { get; private set; }
	public DateRange DateRange { get; private set; } = default!;
	public Status Status { get; private set; } = Status.Actief;

	// Relations

	public MeasurementNetwork MeasurementNetwork { get; private set; }

	public ICollection<MeasurementStation> Stations { get; } =
		new List<MeasurementStation>();

	public IReadOnlyList<ProjectHistory> ProjectHistories => _projectHistories.AsReadOnly();

	// Functions

	public void UpdateMeasurementStationList(ICollection<MeasurementStation> measurementStations) {
		if (Status == Status.Afgesloten) {
			throw new InvalidOperationException("Afgesloten projecten kunnen niet worden bewerkt.");
		}

		// Get all the measurement stations that are not already in the project
		List<MeasurementStation> measurementStationsToAdd = measurementStations
			.Where(s => Stations.All(st => st.Id != s.Id))
			.ToList();

		measurementStationsToAdd.ForEach(m => Stations.Add(m));

		// Get all the measurement stations that are already in the project but need to be deleted
		List<MeasurementStation> measurementStationsToRemove = Stations
			.Where(s => measurementStations.All(st => st.Id != s.Id))
			.ToList();

		measurementStationsToRemove.ForEach(m => Stations.Remove(m));
	}

	public void Edit(string name, string code, DateRange dateRange) {
		if (Status == Status.Afgesloten) {
			throw new InvalidOperationException("Afgesloten projecten kunnen niet worden bewerkt.");
		}

		Name = Guard.Against.NullOrWhiteSpace(name);
		Code = Guard.Against.NullOrWhiteSpace(code);

		UpdateDateRange(dateRange);
	}

	public void UpdateMeasurementNetwork(MeasurementNetwork measurementNetwork) {
		// Check if the project is closed
		if (Status == Status.Afgesloten) {
			throw new InvalidOperationException("Afgesloten projecten kunnen niet worden bewerkt.");
		}

		// Check if the values are the same as the old values
		if (measurementNetwork.Equals(MeasurementNetwork)) {
			return;
		}

		// Create history object using old values

		AddHistoryEntry();

		// Assign the new values

		MeasurementNetwork = Guard.Against.Null(measurementNetwork);
	}

	private void UpdateDateRange(DateRange dateRange) {
		// Check if the values are the same as the old values

		if (dateRange.Equals(DateRange)) {
			return;
		}

		// Create history object using the old values

		AddHistoryEntry();

		// Assign the new values

		UpdatedAt = DateTime.UtcNow; // Update this here since trigger doesn't fire for owned entities (https://github.com/koenbeuk/EntityFrameworkCore.Triggered/issues/171)
		DateRange = dateRange;
		UpdateStatusBasedOnEndDate();
	}

	public void UpdateStatus(Status newStatus) {
		// Check if the values are the same as the old values

		if (Status == newStatus) {
			return;
		}

		// Create history object using the old values

		AddHistoryEntry();

		// Assign the new values

		DateRange = newStatus switch {
			Status.Actief => new DateRange(DateRange.StartDate),
			Status.Afgesloten when !DateRange.EndDate.HasValue => new DateRange(DateRange.StartDate,
				DateOnly.FromDateTime(DateTime.UtcNow)),
			_ => DateRange
		};

		Status = newStatus;
	}

	private void UpdateStatusBasedOnEndDate() {
		Status = DateRange.EndDate.HasValue
			? DateRange.EndDate.Value <= DateOnly.FromDateTime(DateTime.UtcNow) ? Status.Afgesloten : Status.Actief
			: Status.Actief;
	}

	// History

	private void AddHistoryEntry() {
		ValidityRange validityRange = new(UpdatedAt, DateTime.UtcNow);
		ProjectHistory projectHistory = new(new DateRange(DateRange.StartDate, DateRange.EndDate), Status, MeasurementNetwork, validityRange);
		_projectHistories.Add(projectHistory);
	}

	public List<ProjectModification> GetModifications() {
		List<ProjectModification> modifications = [];

		// Add current state as the first history entry

		_projectHistories.Insert(0, new ProjectHistory(DateRange, Status, MeasurementNetwork, new ValidityRange(UpdatedAt, DateTime.UtcNow)));

		for (int i = 0; i < _projectHistories.Count - 1; i++) {

			var current = _projectHistories[i];
			var previous = _projectHistories[i + 1];

			List<ProjectModification> modification = ProjectModificationFactory.Create(current, previous);
			modifications.AddRange(modification);
		}

		return modifications;
	}
}

/// <summary>
///     A project history is something that holds a past state of a project.
/// </summary>
public class ProjectHistory : HistoryEntity {
	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private ProjectHistory() {
	}

	public ProjectHistory(DateRange dateRange, Status status, MeasurementNetwork measurementNetwork, ValidityRange validityRange) {
		DateRange = Guard.Against.Null(dateRange);
		Status = Guard.Against.Null(status);
		MeasurementNetwork = Guard.Against.Null(measurementNetwork);
		ValidityRange = Guard.Against.Null(validityRange);
	}

	public DateRange DateRange { get; init; }
	public Status Status { get; init; }
	public MeasurementNetwork MeasurementNetwork { get; init; }
}