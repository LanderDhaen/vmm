using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Projects;
using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Projects;
using Rise.Shared.WorkOrders;

namespace Rise.Client.WorkOrders;

public partial class Index {
	
	[Inject] public required IWorkOrderService WorkOrderService { get; init; }
	[Inject] public NavigationManager NavigationManager { get; init; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; init; }
	[Inject] public required HeaderService HeaderService { get; init; }

	protected override async Task OnInitializedAsync() {
		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Werkbonnen", null, true));
		
		HeaderService.Set("Werkbonnen");
	}
	
	// Desktop

	private async Task<GridData<WorkOrderDto.Index>> GetWorkOrdersAsync(GridState<WorkOrderDto.Index> state) {

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 100,
			OrderBy = "Id"
		};

		Result<WorkOrderResponse.Index> result =
			await WorkOrderService.GetIndexAsync(request);

		return new GridData<WorkOrderDto.Index> {
			TotalItems = result.Value.TotalCount,
			Items = result.Value.WorkOrders
		};
	}
}