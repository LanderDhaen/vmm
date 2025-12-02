using Rise.Domain.MaintObjects;
using Rise.Domain.Projects;

namespace Rise.Domain.Workorders;

public class Workorder : Entity {
//  * Id
// 	* Registratiedatum (in mijn ogen is dit CreatedAt)
// 	* Voltooid (boolean)
// 	* Naam
// 	* Beschrijving
// 	* Meetlocatie
// 	* Onderhoudsobjecten
// 	* Technieker
// 	* Voltooingsdatum
// 	* Status (snap niet dat het dan ook een boolean voltooid heeft)
// 	* Ernst
// 	* Prioriteit
// 	* Soort
	// Constructors
	private Workorder() { }

	public Workorder(string name, string description, WorkorderStatus status, Prioriteit prioriteit, MaintObj maintObj, Technician? technician) {
		Name = Guard.Against.NullOrEmpty(name);
		Description = Guard.Against.NullOrEmpty(description);
		Status = status;
		Prioriteit = prioriteit;
		MaintObj = Guard.Against.Null(maintObj);
		Technician = technician;
	}

	// Properties

	public string Name { get; private set; }
	public string Description { get; private set; }
	public WorkorderStatus Status { get; private set; } = WorkorderStatus.Onvoltooid;
	public Prioriteit Prioriteit { get; private set; } = Prioriteit.Normaal;

	// Relations
	
	public MaintObj MaintObj { get; private set; }
	public Technician? Technician { get; private set; }
	
	// Functions
	
}