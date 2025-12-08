namespace Rise.Domain.Projects;
/// <summary>
///     A project modification is something that holds a change made to a project.
/// </summary>
public class ProjectModification {
	public ProjectModification(ProjectModificationType changeType, DateTime modifiedAt, string previousValue, string currentValue) {
		MeasurementStationModificationType = Guard.Against.Null(changeType);
		ModifiedAt = Guard.Against.Null(modifiedAt);
		PreviousValue = Guard.Against.NullOrWhiteSpace(previousValue);
		CurrentValue = Guard.Against.NullOrWhiteSpace(currentValue);
	}

	public ProjectModificationType MeasurementStationModificationType { get; init; }
	public DateTime ModifiedAt { get; init; }
	public string PreviousValue { get; init; }
	public string CurrentValue { get; init; }
}