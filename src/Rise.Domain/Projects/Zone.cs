namespace Rise.Domain.Projects;

/// <summary>
///     A zone is something that owns multiple measurement networks.
/// </summary>
public class Zone : Entity {
	private string _name = String.Empty;
	private readonly List<MeasurementNetwork> _measurementnetworks = [];

	private Zone() {
	}

	public Zone(string name) {
		Name = name;
	}
	
	public string Name {
		get => _name;
		set => _name = Guard.Against.NullOrWhiteSpace(value);
	}
	
	public IReadOnlyList<MeasurementNetwork> MeasurementNetworks => _measurementnetworks.AsReadOnly();
}