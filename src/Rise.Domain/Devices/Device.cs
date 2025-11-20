using Rise.Domain.Projects;

namespace Rise.Domain.Devices;

public class Device : Entity {
	// Constructors

	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private Device() { }

	public Device(string name, string serialNumber) {
		Name = Guard.Against.NullOrWhiteSpace(name);
		SerialNumber = Guard.Against.NullOrWhiteSpace(serialNumber);
	}

	// Properties
	public string Name { get; private set; }
	public string SerialNumber { get; private set; }
	public Status Status { get; } = Status.Actief;


	// Functions
	public void Edit(string name, string serialNumber) {
		if (Status == Status.Afgesloten) {
			throw new InvalidOperationException("Inactieve apparaten kunnen niet worden bewerkt.");
		}

		Name = Guard.Against.NullOrWhiteSpace(name);
		SerialNumber = Guard.Against.NullOrWhiteSpace(serialNumber);
	}
}