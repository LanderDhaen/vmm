namespace Rise.Domain.MaintObjects;

public class MaintObjType : Entity {
	private MaintObjType() { }

	public MaintObjType(Guid id, string code, string name, int? defaultMaintenanceIntervalDays = null) {
		Id = id;
		Code = Guard.Against.NullOrWhiteSpace(code);
		Name = Guard.Against.NullOrWhiteSpace(name);
		DefaultMaintenanceIntervalDays = defaultMaintenanceIntervalDays;
	}

	public string Code { get; private set; } = null!; // unique code, e.g. "Sonde"
	public string Name { get; private set; } = null!;
	public int? DefaultMaintenanceIntervalDays { get; private set; }

	// Navigation
	public ICollection<MaintObj> MaintObjects { get; } = new List<MaintObj>();
}