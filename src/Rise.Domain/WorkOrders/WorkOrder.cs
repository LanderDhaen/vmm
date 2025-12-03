using Rise.Domain.MaintObjects;
using Rise.Domain.Projects;

namespace Rise.Domain.WorkOrders;

public class WorkOrder : Entity {
	// Constructors
	private WorkOrder() { }

	public WorkOrder(string name, string description, Priority priority, MaintObj maintObj, Technician? technician = null) {
		Name = Guard.Against.NullOrEmpty(name);
		Description = Guard.Against.NullOrEmpty(description);
		Priority = priority;
		MaintObj = Guard.Against.Null(maintObj);
		if (technician is not null) {
			AssignTechnician(technician);
		}
	}

	// Properties

	public string Name { get; private set; }
	public string Description { get; private set; }
	public WorkOrderStatus Status { get; private set; } = WorkOrderStatus.Onvoltooid;
	public Priority Priority { get; private set; } = Priority.Normaal;
	public DateRange? DateRange { get; private set; }

	// Relations
	
	public MaintObj MaintObj { get; private set; }
	public Technician? Technician { get; private set; }
	
	// Functions
	public void AssignTechnician(Technician technician) {
		Technician = technician;
		Status = WorkOrderStatus.InUitvoering;
		DateRange = new DateRange(DateOnly.FromDateTime(DateTime.UtcNow));
	}

	public void MarkAsCompleted() {
		Status = WorkOrderStatus.Voltooid;
		DateRange = new DateRange(DateRange!.StartDate, DateOnly.FromDateTime(DateTime.UtcNow));
	}
}