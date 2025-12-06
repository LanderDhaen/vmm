namespace Rise.Domain.MaintObjects;

public class MaintObjPlan : Entity {
	private MaintObjPlan() { }

	public MaintObjPlan(string name, int maintenanceIntervalDays, int calibrationIntervalDays) {
		Name = Guard.Against.NullOrWhiteSpace(name);
		MaintenanceIntervalDays = Guard.Against.NegativeOrZero(maintenanceIntervalDays);
		CalibrationIntervalDays = Guard.Against.NegativeOrZero(calibrationIntervalDays);
	}

	public string Name { get; private set; }
	public int MaintenanceIntervalDays { get; private set; }
	public int CalibrationIntervalDays { get; private set; }
}