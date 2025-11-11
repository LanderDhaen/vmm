using Temporal.Domain.Locations;

namespace Temporal.Domain.Projects;

/// <summary>
///     A project is something that owns multiple measuring stations.
/// </summary>
public class Project : Entity {
	public string Name { get; private set; }
	public string Code { get; private set; }
	public DateRange DateRange { get; init; } = default!;
	
	private readonly List<ProjectHistory> _projectHistories = [];
	public IReadOnlyList<ProjectHistory> ProjectHistories => _projectHistories.AsReadOnly();
	
	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private Project() {
	}

	public Project(string name, string code, DateRange dateRange) {
		Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
		Code = Guard.Against.NullOrWhiteSpace(code, nameof(code));
		DateRange = Guard.Against.Null(dateRange, nameof(dateRange));
	}

	public void Edit(string name, string code) {
		
		// Validate the input before creating the history & assigning the values
		
		Guard.Against.NullOrWhiteSpace(name, nameof(name));
		Guard.Against.NullOrWhiteSpace(code, nameof(code));
		
		// Create history object using the old values
		
		ValidityRange validityRange = new(UpdatedAt, DateTime.Now);
		ProjectHistory projectHistory = new(Name, Code, new DateRange(DateRange.StartDate, DateRange.EndDate),  validityRange);
		_projectHistories.Add(projectHistory);
		
		// Assign the new values

		Name = name;
		Code = code;
	}
}

public class ProjectHistory : HistoryEntity {
	public string Name { get; private set; }
	public string Code { get; private set; }
	public DateRange DateRange { get; init; }
	
	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private ProjectHistory() {
	}
	public ProjectHistory(string name, string code, DateRange dateRange, ValidityRange validityRange) {
		Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
		Code = Guard.Against.NullOrWhiteSpace(code, nameof(code));
		DateRange = Guard.Against.Null(dateRange,  nameof(dateRange));
		ValidityRange = Guard.Against.Null(validityRange, nameof(validityRange));
	}
}