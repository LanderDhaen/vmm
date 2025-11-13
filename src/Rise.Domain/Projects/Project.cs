using Rise.Domain.Networks;
using Rise.Domain.Stations;

namespace Rise.Domain.Projects;

public class Project : Entity {
	
	// Properties

	public string Name { get; private set; }
	public string Code { get; private set; }
	public DateRange DateRange { get; private set; } = default!;
	public Status Status { get; private set; } = Status.Actief;
	
	// Relations
	
	public MeasurementNetwork MeasurementNetwork { get; private set; }
	public ICollection<MeasurementStation> Stations { get; private set; } =
		new List<MeasurementStation>();
	
	// History

	private readonly List<ProjectHistory> _projectHistories = [];
	public IReadOnlyList<ProjectHistory> ProjectHistories => _projectHistories.AsReadOnly();
	
	// Constructors
	
	/// <summary>
	/// Entity Framework Core Constructor
	/// </summary>
	private Project() { }
	
	public Project(string name, string code, DateRange dateRange, MeasurementNetwork measurementNetwork) {
		Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
		Code = Guard.Against.NullOrWhiteSpace(code, nameof(code));
		DateRange = Guard.Against.Null(dateRange);
		MeasurementNetwork = Guard.Against.Null(measurementNetwork);
		UpdateStatusBasedOnEndDate();
	}
	
	// Functions

	public void Edit(string name, string code, DateRange dateRange, MeasurementNetwork measurementNetwork) {
		
		
		if (Status == Status.Afgesloten) {
			throw new InvalidOperationException("Afgesloten projecten kunnen niet worden bewerkt.");
		}

		Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
		Code = Guard.Against.NullOrWhiteSpace(code, nameof(code));
		MeasurementNetwork = Guard.Against.Null(measurementNetwork);
		UpdateStatusBasedOnEndDate();
		
		CreateProjectHistory(dateRange);
		
	}

	private void CreateProjectHistory(DateRange dateRange) {
		
		// Check if the values are the same as the old values

		if (dateRange.Equals(DateRange)) {
			return;
		}
		
		// Create history object using the old values
		
		ValidityRange validityRange = new(UpdatedAt, DateTime.Now);
		ProjectHistory projectHistory = new(new DateRange(DateRange.StartDate, DateRange.EndDate), validityRange);
		_projectHistories.Add(projectHistory);

		// Assign the new values

		DateRange = dateRange;
	}

	public void UpdateStatus(Status newStatus) {
		if (Status == newStatus) {
			return;
		}

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
	public DateRange DateRange { get; init; }

	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private ProjectHistory() {
	}
	public ProjectHistory(DateRange dateRange, ValidityRange validityRange) {
		DateRange = Guard.Against.Null(dateRange, nameof(dateRange));
		ValidityRange = Guard.Against.Null(validityRange, nameof(validityRange));
	}
}