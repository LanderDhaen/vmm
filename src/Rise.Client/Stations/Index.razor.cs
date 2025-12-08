using System.Globalization;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Locations;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class Index {
	private static readonly (string Value, string Label)[] SortColumns = new[]
	{
		("Code", "Code"),
		("InstalledAt", "Geïnstalleerd"),
		("RemovedAt", "Verwijderd"),
		("Location", "Locatie"),
		("City", "Gemeente"),
		("ProjectsCount", "Projecten")
	};

	private string _activeSorting = "Code";
	private bool _activeSortingDesc;
	private string _searchTerm = string.Empty;

	private MudDataGrid<MeasurementStationDto.Index>? _grid;

	private IEnumerable<MeasurementStationDto.Index>? _mobileItems;

	private bool _mobileSortOptionsOpen;
	private DateTime? _filterInstalledAt;
	private DateTime? _filterRemovedAt;
	private string? _filterLocation;
	private string? _filterCity;
	private List<string>? _locations;
	private List<string>? _cities;

	private CancellationTokenSource _selectCancellationTokenSource = new();

	private bool _filterOptionsOpen;
	private Anchor _filterDrawerAnchor = Anchor.Right;

	[Inject] public required IMeasurementStationService StationService { get; init; }
	[Inject] public required ILocationService LocationService { get; init; }
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
	private QueryRequest.SkipTake CreateRequestFromGridState(GridState<MeasurementStationDto.Index> state) {
		var sorting = state.SortDefinitions.FirstOrDefault();
		var request = new QueryRequest.SkipTake {
			Skip = state.Page * state.PageSize,
			Take = state.PageSize,
			OrderBy = sorting?.SortBy ?? _activeSorting,
			OrderDescending = sorting?.Descending ?? _activeSortingDesc,
			SearchTerm = _searchTerm.Trim()
		};
		request.Filters = [];
		return request;
	}
	private async Task<GridData<MeasurementStationDto.Index>> FetchStationsAsync(QueryRequest.SkipTake request, bool useContainsForText) {
		ApplyDrawerFilters(request, useContainsForText);

		Result<MeasurementStationResponse.Index> result = await StationService.GetIndexAsync(request);

		var stations = result.Value.Stations ?? [];


		var total = result?.Value?.TotalCount ?? 0;

		return new GridData<MeasurementStationDto.Index> {
			TotalItems = total,
			Items = stations
		};
	}

	// Desktop 
	private async Task<GridData<MeasurementStationDto.Index>> GetStationsAsync(
		GridState<MeasurementStationDto.Index> state) {
		_ = state.SortDefinitions.FirstOrDefault();

		var request = CreateRequestFromGridState(state);

		return await FetchStationsAsync(request, useContainsForText: false);
	}

	// Mobile fetch
	private async Task GetStationsMobileAsync() {
		var request = new QueryRequest.SkipTake {
			Skip = 0,
			Take = 10,
			OrderBy = _activeSorting,
			OrderDescending = _activeSortingDesc,
			SearchTerm = _searchTerm.Trim()
		};
		request.Filters = [];
		var gridData = await FetchStationsAsync(request, useContainsForText: true);

		_mobileItems = gridData.Items;
	}

	// Mobile Sorting drawer
	private void ToggleSortingDrawer() {
		_mobileSortOptionsOpen = !_mobileSortOptionsOpen;
	}

	private async Task SortMobileAsync(string orderBy, bool descending) {
		_activeSorting = orderBy;
		_activeSortingDesc = descending;
		await GetStationsMobileAsync();
		ToggleSortingDrawer();
	}

	private async Task ToggleFiltersDrawer() {
		var bp = await ViewportService.GetCurrentBreakpointAsync();
		_filterDrawerAnchor = bp == Breakpoint.Xs ? Anchor.Bottom : Anchor.Right;
		_filterOptionsOpen = !_filterOptionsOpen;
		StateHasChanged();
	}

	private async Task HandleDesktopIntervalElapsed() {
		if (_grid is not null)
			await _grid.ReloadServerData();
	}

	private async Task HandleMobileIntervalElapsed() {
		await GetStationsMobileAsync();
	}

	// Navigation
	private void NavigateToStation(Guid stationId) {
		NavigationManager.NavigateTo($"/stations/{stationId}");
	}

	private void NavigateToCreate() {
		NavigationManager.NavigateTo("/stations/create");
	}

	// Filtering
	private void ApplyDrawerFilters(QueryRequest.SkipTake request, bool useContainsForText = false) {
		request.Filters = [];

		if (_filterInstalledAt.HasValue)
			request.Filters["InstalledAt"] = new { Operator = "GreaterThanOrEqual", Value = _filterInstalledAt.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) };

		if (_filterRemovedAt.HasValue)
			request.Filters["RemovedAt"] = new { Operator = "LessThanOrEqual", Value = _filterRemovedAt.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) };

		if (!string.IsNullOrWhiteSpace(_filterLocation))
			request.Filters["Location"] = new { Operator = useContainsForText ? "Contains" : "Equals", Value = _filterLocation!.Trim() };

		if (!string.IsNullOrWhiteSpace(_filterCity))
			request.Filters["City"] = new { Operator = useContainsForText ? "Contains" : "Equals", Value = _filterCity!.Trim() };
	}

	private async Task HandleLocationSelectOpen() {
		try {
			_selectCancellationTokenSource.Cancel();
		} catch { }
		_selectCancellationTokenSource.Dispose();
		_selectCancellationTokenSource = new CancellationTokenSource();

		_locations = null;
		StateHasChanged();

		try {
			var reqAll = new QueryRequest.SkipTake { Skip = 0, Take = int.MaxValue };
			var ct = _selectCancellationTokenSource.Token;
			var result = await LocationService.GetIndexAsync(reqAll, ct);
			var locations = result?.Value?.Locations ?? Enumerable.Empty<LocationDto.Index>();

			_locations = locations
				.Select(l => l.Name ?? string.Empty)
				.Where(n => !string.IsNullOrWhiteSpace(n))
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
				.ToList();
		} catch (OperationCanceledException) {
		} finally {
			StateHasChanged();
		}
	}

	private async Task HandleCitySelectOpen() {
		try {
			_selectCancellationTokenSource.Cancel();
		} catch { }
		_selectCancellationTokenSource.Dispose();
		_selectCancellationTokenSource = new CancellationTokenSource();

		_cities = null;
		StateHasChanged();

		try {
			var reqAll = new QueryRequest.SkipTake { Skip = 0, Take = int.MaxValue };
			var ct = _selectCancellationTokenSource.Token;
			var result = await LocationService.GetIndexAsync(reqAll, ct);
			var locations = result.Value.Locations;

			_cities = locations
				.Select(l => l.Address.City)
				.Where(c => !string.IsNullOrWhiteSpace(c))
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
				.ToList();
		} catch (OperationCanceledException) {
		} finally {
			StateHasChanged();
		}
	}

	private async Task ClearFilters() {
		_filterInstalledAt = null;
		_filterRemovedAt = null;
		_filterLocation = null;
		_filterCity = null;

		var bp = await ViewportService.GetCurrentBreakpointAsync();

		if (bp == Breakpoint.Xs)
			await GetStationsMobileAsync();
		else if (_grid != null)
			await _grid.ReloadServerData();

		_filterOptionsOpen = false;
		StateHasChanged();
	}

	private async Task ApplyOrReloadOnChangeAsync() {
		var bp = await ViewportService.GetCurrentBreakpointAsync();
		if (bp == Breakpoint.Xs) {
			await GetStationsMobileAsync();
		} else {
			if (_grid != null)
				await _grid.ReloadServerData();
		}
		StateHasChanged();
	}
	private string GetFilterDrawerClass() =>
		_filterDrawerAnchor == Anchor.Right ? "p-4 rounded-l-lg" : "p-4 rounded-t-lg";
}