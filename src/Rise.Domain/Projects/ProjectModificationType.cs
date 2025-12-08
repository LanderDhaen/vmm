namespace Rise.Domain.Projects;

/// <summary>
///     A project modification type is something that defines the type of change made to a project.
/// </summary>
public enum ProjectModificationType {
	MeasurementNetworkChanged,
	StatusChanged,
	StartDateChanged,
	EndDateChanged,
}