using Microsoft.AspNetCore.Components;

using Rise.Shared.WorkOrders;

namespace Rise.Client.Components.WorkOrders;

public partial class WorkOrderCard {
	[Parameter] public required WorkOrderDto.Index WorkOrder { get; init; }

	[Inject] public required NavigationManager NavigationManager { get; init; }

	private void NavigateToWorkOrder(Guid id) {
		NavigationManager.NavigateTo($"workorders/{id}");
	}
}