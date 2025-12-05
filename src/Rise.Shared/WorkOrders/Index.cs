
namespace Rise.Shared.WorkOrders;

/// <summary>
///     Represents the response structure for work-order-related operations.
/// </summary>
public static partial class WorkOrderResponse {
	public class Index {
		public IEnumerable<WorkOrderDto.Index> WorkOrders { get; set; } = [];
		public int TotalCount { get; set; }
	}
}