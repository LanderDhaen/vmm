using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.WorkOrders;

namespace Rise.Client.WorkOrders;

public partial class Index {
	private static readonly (string Value, string Label)[] SortColumns = new[] {
		("Priority", "Prioriteit"),
		("MaintObjType", "Type"),
		("MaintObj", "Onderhoudsobject"),
		("Name", "Titel"),
		("Technician", "Technieker"),
		("Status", "Status"),
		("StartDate", "Startdatum"),
		("EndDate", "Einddatum")
	};

	private string _activeSorting = "Priority";
	private bool _activeSortingDesc = true;
	private bool _mobileSortOptionsOpen;

	private IEnumerable<WorkOrderDto.Index>? _mobilityWorkOrders;

	[Inject] public required IWorkOrderService WorkOrderService { get; init; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; init; }
	[Inject] public required HeaderService HeaderService { get; init; }
	[Inject] public required IBrowserViewportService ViewportService { get; init; }
	[Inject] public required NavigationManager Navigation { get; init; }

	protected override async Task OnInitializedAsync() {
		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Werkbonnen", null, true));

		HeaderService.Set("Werkbonnen");

		Breakpoint breakpoint = await ViewportService.GetCurrentBreakpointAsync();

		if (breakpoint == Breakpoint.Xs) {
			await GetWorkOrdersMobileAsync();
		}
	}

	private async Task<GridData<WorkOrderDto.Index>> GetWorkOrdersAsync(GridState<WorkOrderDto.Index> state) {
		SortDefinition<WorkOrderDto.Index>? sorting = state.SortDefinitions.FirstOrDefault();

		QueryRequest.SkipTake request = new() {
			Skip = state.Page * state.PageSize,
			Take = state.PageSize,
			OrderBy = sorting?.SortBy ?? _activeSorting,
			OrderDescending = sorting?.Descending ?? _activeSortingDesc
		};

		Result<WorkOrderResponse.Index> result =
			await WorkOrderService.GetIndexAsync(request);

		return new GridData<WorkOrderDto.Index> {
			TotalItems = result.Value.TotalCount,
			Items = result.Value.WorkOrders
		};
	}

	private void OnRowClick(DataGridRowClickEventArgs<WorkOrderDto.Index> args) {
		NavigateToDetail(args.Item.Id);
	}

	private void NavigateToDetail(Guid id) {
		Navigation.NavigateTo($"/workorders/{id}");
	}

	private void NavigateToCreate() {
		Navigation.NavigateTo("/workorders/create");
	}

	private async Task GetWorkOrdersMobileAsync() {
		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 10,
			OrderBy = _activeSorting,
			OrderDescending = _activeSortingDesc
		};

		Result<WorkOrderResponse.Index> result =
			await WorkOrderService.GetIndexAsync(request);

		if (result.IsSuccess) {
			_mobilityWorkOrders = result.Value.WorkOrders;
		}

		StateHasChanged();
	}

	private void ToggleSortingDrawer() {
		_mobileSortOptionsOpen = !_mobileSortOptionsOpen;
	}

	private async Task SortMobileAsync(string orderBy, bool descending) {
		_activeSorting = orderBy;
		_activeSortingDesc = descending;
		await GetWorkOrdersMobileAsync();
		ToggleSortingDrawer();
	}
}