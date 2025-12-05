using Rise.Domain.MaintObjects;
using Rise.Domain.Projects;

namespace Rise.Domain.WorkOrders;

/// <summary>
///     A work order is something that holds a task for a technician on a maintenance object
/// </summary>
public class WorkOrder : Entity {

	// Constructors

	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
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
	public WorkOrderStatus Status { get; private set; } = WorkOrderStatus.Pending;
	public Priority Priority { get; private set; } = Priority.Normal;
	public DateRange? DateRange { get; private set; }

	// Relations

	public MaintObj MaintObj { get; private set; }
	public Technician? Technician { get; private set; }

	// Functions
	public void AssignTechnician(Technician technician) {
		Technician = technician;
		Status = WorkOrderStatus.InProgress;
		DateRange = new DateRange(DateOnly.FromDateTime(DateTime.UtcNow));
	}

	public void MarkAsCompleted() {
		Status = WorkOrderStatus.Completed;
		DateRange = new DateRange(DateRange!.StartDate, DateOnly.FromDateTime(DateTime.UtcNow));
	}
}