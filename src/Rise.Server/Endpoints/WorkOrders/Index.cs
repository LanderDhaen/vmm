using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.WorkOrders;

namespace Rise.Server.Endpoints.WorkOrders;


/// <summary>
///     List all work orders.
///     See https://fast-endpoints.com/
/// </summary>
/// <param name="workOrderService"></param>
public class Index(IWorkOrderService workOrderService) : Endpoint<QueryRequest.SkipTake, Result<WorkOrderResponse.Index>> {
	public override void Configure() {
		Get("/api/workorders");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result<WorkOrderResponse.Index>> ExecuteAsync(QueryRequest.SkipTake req, CancellationToken ctx) {
		return workOrderService.GetIndexAsync(req, ctx);
	}
}