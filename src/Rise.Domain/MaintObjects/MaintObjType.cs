namespace Rise.Domain.MaintObjects;

public class MaintObjType : Entity {
	private MaintObjType() { }

	public MaintObjType(Guid id, string code, string name, int maintenanceIntervalDays, int calibrationIntervalDays) {
		Id = id;
		Code = Guard.Against.NullOrWhiteSpace(code);
		Name = Guard.Against.NullOrWhiteSpace(name);
		MaintenanceIntervalDays = Guard.Against.NegativeOrZero(maintenanceIntervalDays);
		CalibrationIntervalDays = Guard.Against.NegativeOrZero(calibrationIntervalDays);
	}

	public string Code { get; private set; } = null!; // unique code, e.g. "Sonde"
	public string Name { get; private set; } = null!;
	public int MaintenanceIntervalDays { get; private set; }
	public int CalibrationIntervalDays { get; private set; }

	// Navigation
	public ICollection<MaintObj> MaintObjects { get; } = new List<MaintObj>();
}