using Rise.Shared.Identity;
using Rise.Shared.WorkOrders;

namespace Rise.Server.Endpoints.WorkOrders;

public class Detail(IWorkOrderService workOrderService)
	: Endpoint<EmptyRequest, Result<WorkOrderResponse.Detail>> {

	public override void Configure() {
		Get("/api/workorders/{workOrderId}");
		Roles(AppRoles.Administrator);
	}

	public override async Task<Result<WorkOrderResponse.Detail>> ExecuteAsync(
		EmptyRequest req,
		CancellationToken ct
	) {
		Guid workOrderId = Route<Guid>("workOrderId");
		return await workOrderService.GetDetailAsync(workOrderId, ct);
	}
}