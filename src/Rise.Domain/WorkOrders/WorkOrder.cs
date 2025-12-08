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

	public WorkOrder(string name, string description, Priority priority, MaintObj maintObj) {
		Name = Guard.Against.NullOrEmpty(name);
		Description = Guard.Against.NullOrEmpty(description);
		Priority = priority;
		MaintObj = Guard.Against.Null(maintObj);
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
		Technician = Guard.Against.Null(technician);
		MarkAsInProgress();
	}

	public void UpdateStatus(WorkOrderStatus status) {
		if (Status == status) {
			return;
		}

		Guard.Against.InvalidInput(Status, nameof(Status),
			s => s != WorkOrderStatus.Cancelled,
			"Een geannuleerde werkbon kan niet meer aangepast worden.");

		switch (status) {
		case WorkOrderStatus.Pending:
			MarkAsPending();
			break;

		case WorkOrderStatus.InProgress:
			MarkAsInProgress();
			break;

		case WorkOrderStatus.Completed:
			MarkAsCompleted();
			break;

		case WorkOrderStatus.Cancelled:
			MarkAsCancelled();
			break;

		default:
			throw new ArgumentOutOfRangeException(nameof(status), status, null);
		}
	}

	private void MarkAsPending() {
		Guard.Against.InvalidInput(Status, nameof(Status),
			s => s != WorkOrderStatus.Completed && s != WorkOrderStatus.Cancelled,
			"Afgewerkte of geannuleerde werkbonnen kunnen niet terug op gepland worden gezet.");

		Status = WorkOrderStatus.Pending;
		DateRange = null;
	}

	private void MarkAsCompleted() {
		Guard.Against.Null(DateRange, nameof(DateRange),
			"Een werkbon moet eerst gestart worden voor ze kan worden afgesloten.");

		Status = WorkOrderStatus.Completed;

		DateRange = new DateRange(
			DateRange.StartDate,
			DateOnly.FromDateTime(DateTime.UtcNow)
		);
	}

	private void MarkAsInProgress() {
		Status = WorkOrderStatus.InProgress;

		DateRange = new DateRange(DateOnly.FromDateTime(DateTime.UtcNow));
	}

	private void MarkAsCancelled() {
		Guard.Against.InvalidInput(Status, nameof(Status),
			s => s == WorkOrderStatus.Pending,
			"Alleen werkbonnen met status gepland kunnen geannuleerd worden.");

		Status = WorkOrderStatus.Cancelled;
		DateRange = null;
		Technician = null;
	}
}