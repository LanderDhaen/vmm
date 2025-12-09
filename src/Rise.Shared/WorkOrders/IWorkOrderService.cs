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

	Task<Result<WorkOrderResponse.Detail>> GetDetailAsync(
		Guid workOrderId,
		CancellationToken ctx = default
	);

	Task<Result> UpdateStatusAsync(
		WorkOrderRequest.UpdateStatus req,
		CancellationToken ctx = default
	);
	Task<IEnumerable<string>> GetTechnicianNamesAsync(string? query, CancellationToken ctx = default);

}