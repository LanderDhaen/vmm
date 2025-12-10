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
	
	private string _searchTerm = string.Empty;

	private CancellationTokenSource _cancellationTokenSource = new();


	private IEnumerable<MaintObjDto.Index>? _mobileItems;
	private MudDataGrid<MaintObjDto.Index> _grid;


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
			await GetMaintObjectsMobileAsync();
		}
	}

	// Desktop

	private async Task<GridData<MaintObjDto.Index>> GetMaintObjectsAsync(GridState<MaintObjDto.Index> state) {
		SortDefinition<MaintObjDto.Index> sorting =
			state.SortDefinitions.FirstOrDefault(
				new SortDefinition<MaintObjDto.Index>(_activeSorting, _activeSortingDesc, 0, null));

		QueryRequest.SkipTake request = new() {
			Skip = state.Page * state.PageSize,
			Take = state.PageSize,
			OrderBy = sorting.SortBy,
			OrderDescending = sorting.Descending,
			SearchTerm = _searchTerm
		};

		Result<MaintObjResponse.Index> result =
			await MaintObjService.GetIndexAsync(request, _cancellationTokenSource.Token);

		return new GridData<MaintObjDto.Index> {
			TotalItems = result.Value.TotalCount,
			Items = result.Value.MaintObjects
		};
	}

	// Mobile

	private async Task GetMaintObjectsMobileAsync() {
		await _cancellationTokenSource.CancelAsync();
		_cancellationTokenSource.Dispose();
		_cancellationTokenSource = new CancellationTokenSource();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 10,
			OrderBy = _activeSorting,
			OrderDescending = _activeSortingDesc,
			SearchTerm = _searchTerm
		};

		Result<MaintObjResponse.Index> result = await MaintObjService.GetIndexAsync(request, _cancellationTokenSource.Token);

		_mobileItems = result.Value.MaintObjects;
	}

	// Mobile Sorting

	private void ToggleSortingDrawer() {
		_mobileSortOptionsOpen = !_mobileSortOptionsOpen;
	}

	private async Task SortMobileAsync(string orderBy, bool descending) {
		_activeSorting = orderBy;
		_activeSortingDesc = descending;
		await GetMaintObjectsMobileAsync();
		ToggleSortingDrawer();
	}
	
	// Searching

	private async Task HandleDesktopIntervalElapsed() {
		await _grid.ReloadServerData();
	}

	private async Task HandleMobileIntervalElapsed() {
		await GetMaintObjectsMobileAsync();
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