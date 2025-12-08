namespace Rise.Shared.WorkOrders;

public static partial class WorkOrderRequest {
	public class UpdateStatus {
		public Guid WorkOrderId { get; set; }
		public WorkOrderStatus NewStatus { get; set; }
	}
}

public enum WorkOrderStatus {
	Pending,
	InProgress,
	Completed,
	Cancelled
}