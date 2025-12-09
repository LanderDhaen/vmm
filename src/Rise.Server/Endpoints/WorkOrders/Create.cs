using Ardalis.Result;

using FastEndpoints;

using Rise.Domain.WorkOrders;
using Rise.Shared.Identity;
using Rise.Shared.WorkOrders;

namespace Rise.Server.Endpoints.WorkOrders;

public class Create(IWorkOrderService workOrderService)
	: Endpoint<WorkOrderRequest.Create, Result<WorkOrderResponse.Create>> {
	public override void Configure() {
		Post("/api/workorders");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result<WorkOrderResponse.Create>> ExecuteAsync(
		WorkOrderRequest.Create req,
		CancellationToken ct
	) {
		return workOrderService.CreateAsync(req, ct);
	}
}