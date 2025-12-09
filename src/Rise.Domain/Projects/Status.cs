namespace Rise.Domain.Projects;

/// <summary>
///     Status is used to indicate the current state of a project.
///     Different statuses can be used to filter projects in various ways.
///     Statuses can include Active, Inactive, Planned ... possibly more in the future.
/// </summary>
public enum Status {
	//no end date -> null in db
	Actief,

	//end date in the past -> not active anymore
	Afgesloten
}