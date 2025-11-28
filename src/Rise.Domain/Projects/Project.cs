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

	public string Name { get; private set; } = string.Empty;
	public string Code { get; private set; } = string.Empty;
	public DateRange DateRange { get; private set; } = new DateRange(DateOnly.FromDateTime(DateTime.UtcNow));
	public Status Status { get; private set; } = Status.Actief;

	// Relations

	public MeasurementNetwork MeasurementNetwork { get; private set; } = default!;

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

	public void Edit(string name, string code, DateRange dateRange, MeasurementNetwork measurementNetwork) {
		if (Status == Status.Afgesloten) {
			throw new InvalidOperationException("Afgesloten projecten kunnen niet worden bewerkt.");
		}

		Name = Guard.Against.NullOrWhiteSpace(name);
		Code = Guard.Against.NullOrWhiteSpace(code);
		MeasurementNetwork = Guard.Against.Null(measurementNetwork);

		UpdateDateRange(dateRange);
	}

	private void UpdateDateRange(DateRange dateRange) {
		// Check if the values are the same as the old values

		if (dateRange.Equals(DateRange)) {
			return;
		}

		// Create history object using the old values

		ValidityRange validityRange = new(UpdatedAt, DateTime.UtcNow);
		ProjectHistory projectHistory = new(new DateRange(DateRange.StartDate, DateRange.EndDate), validityRange);
		_projectHistories.Add(projectHistory);

		// Assign the new values

		DateRange = dateRange;
		UpdateStatusBasedOnEndDate();
	}

	public void UpdateStatus(Status newStatus) {
		// Check if the values are the same as the old values

		if (Status == newStatus) {
			return;
		}

		// Create history object using the old values

		ValidityRange validityRange = new(UpdatedAt, DateTime.UtcNow);
		ProjectHistory projectHistory = new(new DateRange(DateRange.StartDate, DateRange.EndDate), validityRange);
		_projectHistories.Add(projectHistory);

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
}

public class ProjectHistory : HistoryEntity {
	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private ProjectHistory() {
		DateRange = new DateRange(DateOnly.FromDateTime(DateTime.UtcNow));
	}

	public ProjectHistory(DateRange dateRange, ValidityRange validityRange) {
		DateRange = Guard.Against.Null(dateRange);
		ValidityRange = Guard.Against.Null(validityRange);
	}

	public DateRange DateRange { get; init; }
}