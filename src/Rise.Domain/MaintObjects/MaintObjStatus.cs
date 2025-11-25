namespace Rise.Domain.MaintObjects;

/// <summary>
///     Status is used to indicate the current state of a maintobject.
///     Different statuses can be used to filter projects in various ways.
///     Statuses can include Active, Inactive, Defect ... possibly more in the future.
/// </summary>
public enum MaintObjStatus {
	Actief,
	Inactief,
	Defect
}