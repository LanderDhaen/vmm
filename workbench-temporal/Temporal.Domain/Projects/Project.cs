namespace Temporal.Domain.Projects;

/// <summary>
///     A project is something that owns multiple measuring stations.
/// </summary>
public class Project : Entity {
	private readonly List<ProjectHistory> _projectHistories = [];

	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private Project() {
	}

	public Project(string name, string code, DateRange dateRange) {
		Name = Guard.Against.NullOrWhiteSpace(name);
		Code = Guard.Against.NullOrWhiteSpace(code);
		DateRange = Guard.Against.Null(dateRange);
	}

	public string Name { get; private set; }
	public string Code { get; private set; }
	public DateRange DateRange { get; private set; } = default!;
	public IReadOnlyList<ProjectHistory> ProjectHistories => _projectHistories.AsReadOnly();

	public void EditDateRange(DateRange dateRange) {
		// Check if the values are the same as the old values
		// Also split it up into functions and only track what's really important like changing the enddate or location or status

		if (dateRange.Equals(DateRange)) {
			Console.WriteLine("I've returned");
			return;
		}

		// Create history object using the old values

		ValidityRange validityRange = new(UpdatedAt, DateTime.Now);
		ProjectHistory projectHistory = new(new DateRange(DateRange.StartDate, DateRange.EndDate), validityRange);
		_projectHistories.Add(projectHistory);

		// Assign the new values

		DateRange = dateRange;
	}
}

public class ProjectHistory : HistoryEntity {
	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private ProjectHistory() {
	}

	public ProjectHistory(DateRange dateRange, ValidityRange validityRange) {
		DateRange = Guard.Against.Null(dateRange);
		ValidityRange = Guard.Against.Null(validityRange);
	}

	public DateRange DateRange { get; init; }

	public override string ToString() {
		return $"Start: {DateRange.StartDate}, Einde: {DateRange.EndDate} " + base.ToString();
	}
}