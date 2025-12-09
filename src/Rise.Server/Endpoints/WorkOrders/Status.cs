using Rise.Shared.Identity;
using Rise.Shared.Projects;
using Rise.Shared.WorkOrders;

namespace Rise.Server.Endpoints.WorkOrders;

public class UpdateStatus(IWorkOrderService workOrderService) : Endpoint<WorkOrderRequest.UpdateStatus, Result> {
	public override void Configure() {
		Put("/api/workorders/{WorkOrderId}/status");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result> ExecuteAsync(WorkOrderRequest.UpdateStatus req, CancellationToken ct) {
		return workOrderService.UpdateStatusAsync(req, ct);
	}
}