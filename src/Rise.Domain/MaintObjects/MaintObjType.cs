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

	public string Code { get; private set; } 
	public string Name { get; private set; }
	public int MaintenanceIntervalDays { get; private set; }
	public int CalibrationIntervalDays { get; private set; }
}