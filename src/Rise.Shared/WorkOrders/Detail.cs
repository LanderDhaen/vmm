namespace Rise.Shared.WorkOrders;

public static partial class WorkOrderResponse {

	public class Detail {
		public required WorkOrderDto.Detail WorkOrder { get; set; }
	}
}