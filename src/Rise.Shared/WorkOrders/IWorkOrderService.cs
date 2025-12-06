using Rise.Shared.Common;

namespace Rise.Shared.WorkOrders;

/// <summary>
///     Provides methods for managing work-order-related operations.
/// </summary>
public interface IWorkOrderService {
	Task<Result<WorkOrderResponse.Index>> GetIndexAsync(
		QueryRequest.SkipTake req,
		CancellationToken ctx = default
	);

}