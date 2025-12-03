namespace Rise.Domain.Stations;
/// <summary>
///     A measurement station modification is something that holds a change made to a measurement station.
/// </summary>
public class MeasurementStationModification {
	public MeasurementStationModification(MeasurementStationModificationType changeType, DateTime modifiedAt, string previousValue, string currentValue) {
		MeasurementStationModificationType = Guard.Against.Null(changeType);
		ModifiedAt = Guard.Against.Null(modifiedAt);
		PreviousValue = Guard.Against.NullOrWhiteSpace(previousValue);
		CurrentValue = Guard.Against.NullOrWhiteSpace(currentValue);
	}
	
	public MeasurementStationModificationType MeasurementStationModificationType { get; init; }
	public DateTime ModifiedAt { get; init; }
	public string PreviousValue { get; init; }
	public string CurrentValue { get; init; }
}