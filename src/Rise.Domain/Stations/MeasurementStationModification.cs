namespace Rise.Domain.Stations;
/// <summary>
///     A measurement station modification is something that holds a change made to a measurement station.
/// </summary>
public class MeasurementStationModification {
	public MeasurementStationModification(MeasurementStationModificationType changeType, DateTime modifiedAt, string previousValue, string currentValue) {
		MeasurementStationModificationType = changeType;
		ModifiedAt = modifiedAt;
		PreviousValue = previousValue;
		CurrentValue = currentValue;
	}
	
	public MeasurementStationModificationType MeasurementStationModificationType { get; init; }
	public DateTime ModifiedAt { get; init; }
	public string PreviousValue { get; init; }
	public string CurrentValue { get; init; }
}