using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class Index {
	private static readonly (string Value, string Label)[] SortColumns = [
		("Code", "Code"),
		("InstalledAt", "Geïnstalleerd"),
		("RemovedAt", "Verwijderd"),
		("Location", "Locatie"),
		("City", "Gemeente"),
		("ProjectsCount", "Projecten")
	];

	private string _activeSorting = "Code";
	private bool _activeSortingDesc = true;

	private CancellationTokenSource _cancellationTokenSource = new();

	private MudDataGrid<MeasurementStationDto.Index> _grid;

	private IEnumerable<MeasurementStationDto.Index>? _mobileItems;

	private bool _mobileSortOptionsOpen;
	private string _searchTerm;


	[Inject] public required IMeasurementStationService StationService { get; init; }
	[Inject] public required NavigationManager NavigationManager { get; init; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; init; }
	[Inject] public required HeaderService HeaderService { get; init; }
	[Inject] public required IBrowserViewportService ViewportService { get; init; }


	protected override async Task OnInitializedAsync() {
		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Meetstations", null, true)
		);

		HeaderService.Set("Meetstations");

		Breakpoint breakpoint = await ViewportService.GetCurrentBreakpointAsync();

		if (breakpoint == Breakpoint.Xs) {
			await GetStationsMobileAsync();
		}
	}

	// Desktop

	private async Task<GridData<MeasurementStationDto.Index>> GetStationsAsync(
		GridState<MeasurementStationDto.Index> state) {
		SortDefinition<MeasurementStationDto.Index>? sorting = state.SortDefinitions.FirstOrDefault(
			new SortDefinition<MeasurementStationDto.Index>(_activeSorting, _activeSortingDesc, 0,
				null)); // Can't seem to have no sort function here

		QueryRequest.SkipTake request = new() {
			Skip = state.Page * state.PageSize,
			Take = state.PageSize,
			OrderBy = sorting.SortBy,
			OrderDescending = sorting.Descending,
			SearchTerm = _searchTerm
		};

		Result<MeasurementStationResponse.Index>
			result = await StationService
				.GetIndexAsync(
					request); // No support for cancellation, see https://github.com/HOGENT-RISE/dotnet-2526-vc2/pull/42

		return new GridData<MeasurementStationDto.Index> {
			TotalItems = result.Value.TotalCount,
			Items = result.Value.Stations
		};
	}

	// Mobile

	private async Task GetStationsMobileAsync() {
		_cancellationTokenSource.Cancel();
		_cancellationTokenSource.Dispose();
		_cancellationTokenSource = new CancellationTokenSource();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 10,
			OrderBy = _activeSorting,
			OrderDescending = _activeSortingDesc,
			SearchTerm = _searchTerm
		};

		Result<MeasurementStationResponse.Index> result =
			await StationService.GetIndexAsync(request, _cancellationTokenSource.Token);

		_mobileItems = result.Value.Stations;
	}

	// Mobile Sorting

	private void ToggleSortingDrawer() {
		_mobileSortOptionsOpen = !_mobileSortOptionsOpen;
	}

	private async Task SortMobileAsync(string orderBy, bool descending) {
		_activeSorting = orderBy;
		_activeSortingDesc = descending;
		await GetStationsMobileAsync();
		ToggleSortingDrawer();
	}

	// Filtering

	private async Task HandleDesktopIntervalElapsed() {
		await _grid.ReloadServerData();
	}

	private async Task HandleMobileIntervalElapsed() {
		await GetStationsMobileAsync();
	}

	// TODO: Implement filtering (updating the state for desktop or calling SortFilterAsync for mobile)

	// Navigation

	private void NavigateToStation(Guid stationId) {
		NavigationManager.NavigateTo($"/stations/{stationId}");
	}

	private void NavigateToCreate() {
		NavigationManager.NavigateTo("/stations/create");
	}
}