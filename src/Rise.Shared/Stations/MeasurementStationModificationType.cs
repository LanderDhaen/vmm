namespace Rise.Shared.Stations;

/// <summary>
///     A measurement station modification type is something that defines the type of change made to a measurement station.
/// </summary>
public enum MeasurementStationModificationType {
	LocationChanged,
	InstalledAtChanged,
	RemovedAtChanged
}