namespace Rise.Domain.MaintObjects;

public class MaintObjModification {
	public MaintObjModification(MaintObjModificationType changetype, DateTime modificationDate, string previousValue, string currentValue) {
		MaintObjModificationType = Guard.Against.Null(changetype);
		ModifiedAt = Guard.Against.Null(modificationDate);
		PreviousValue = Guard.Against.Null(previousValue);
		CurrentValue = Guard.Against.Null(currentValue);
	}

	public MaintObjModificationType MaintObjModificationType { get; init; }
	public DateTime ModifiedAt { get; init; }
	public string PreviousValue { get; init; }
	public string CurrentValue { get; init; }
}