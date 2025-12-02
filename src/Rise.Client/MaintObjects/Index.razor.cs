using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.MaintObjects;

namespace Rise.Client.MaintObjects;

public partial class Index {
	private static readonly (string Value, string Label)[] SortColumns = [
		("SerialNumber", "Serienummer"),
		("Name", "Naam"),
		("MaintObjType", "Type"),
		("MaintObjStatus", "Status"),
		("Location", "Locatie")
	];

	private string _activeSorting = "SerialNumber";
	private bool _activeSortingDesc = true;

	private IEnumerable<MaintObjDto.Index>? _mobileItems;

	private bool _mobileSortOptionsOpen;


	[Inject] public required IMaintObjService MaintObjService { get; init; }
	[Inject] public required NavigationManager NavigationManager { get; init; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; init; }
	[Inject] public required HeaderService HeaderService { get; init; }
	[Inject] public required IBrowserViewportService ViewportService { get; init; }


	protected override async Task OnInitializedAsync() {
		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Onderhoudsobjecten", null, true)
		);

		HeaderService.Set("Onderhouds-objecten");

		Breakpoint breakpoint = await ViewportService.GetCurrentBreakpointAsync();

		if (breakpoint == Breakpoint.Xs) {
			await GetMaintObjectsMobileAsync(_activeSorting, _activeSortingDesc);
		}
	}

	// Desktop

	private async Task<GridData<MaintObjDto.Index>> GetMaintObjectsAsync(GridState<MaintObjDto.Index> state) {
		SortDefinition<MaintObjDto.Index>? sorting = state.SortDefinitions.FirstOrDefault();

		QueryRequest.SkipTake request = new() {
			Skip = state.Page * state.PageSize,
			Take = state.PageSize,
			OrderBy = sorting?.SortBy ?? "Name",
			OrderDescending = sorting?.Descending ?? false
		};

		Result<MaintObjResponse.Index> result = await MaintObjService.GetIndexAsync(request);

		return new GridData<MaintObjDto.Index> {
			TotalItems = result.Value.TotalCount,
			Items = result.Value.MaintObjects
		};
	}

	// Mobile

	private async Task GetMaintObjectsMobileAsync(string orderBy, bool orderDescending) {
		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 10,
			OrderBy = orderBy,
			OrderDescending = orderDescending
		};

		Result<MaintObjResponse.Index> result = await MaintObjService.GetIndexAsync(request);

		_mobileItems = result.Value.MaintObjects;
	}

	// Mobile Sorting

	private void ToggleSortingDrawer() {
		_mobileSortOptionsOpen = !_mobileSortOptionsOpen;
	}

	private async Task SortMobileAsync(string orderBy, bool descending) {
		await GetMaintObjectsMobileAsync(orderBy, descending);
		ToggleSortingDrawer();
		_activeSorting = orderBy;
		_activeSortingDesc = descending;
	}

	// TODO: Implement filtering (updating the state for desktop or calling SortFilterAsync for mobile)

	// Navigation

	private void NavigateToMaintObj(Guid maintObjId) {
		NavigationManager.NavigateTo($"/maintobjects/{maintObjId}");
	}

	// Create
	private void NavigateToCreate() {
		NavigationManager.NavigateTo("/maintobjects/create");
	}
}