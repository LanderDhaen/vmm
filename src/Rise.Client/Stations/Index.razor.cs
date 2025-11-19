using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class Index {

	private IEnumerable<MeasurementStationDto.Index>? _mobileItems;

	private bool _mobileSortOptionsOpen;

	// Id is intentionally NOT included here so it won't be visible in the drawer,
	// but _activeSorting remains "Id" as default.
	private static readonly (string Value, string Label)[] SortColumns =
	[
		("Code", "Code"),
		("Status", "Status"),
		("InstalledAt", "Geïnstalleerd"),
		("RemovedAt", "Verwijderd"),
		("Location", "Locatie"),
		("City", "Gemeente"),
		("ProjectsCount", "Projecten")
	];

	// Default sorting set to Id (used for initial load), but Id is hidden in the drawer.
	private string _activeSorting = "Id";
	private bool _activeSortingDesc;


	[Inject] public required StationService StationService { get; init; } // <- changed from IProjectService
	[Inject] public required NavigationManager NavigationManager { get; init; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; init; }
	[Inject] public required HeaderService HeaderService { get; init; }
	[Inject] public required IBrowserViewportService ViewportService { get; init; }


	protected override async Task OnInitializedAsync() {

		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Home", "/"),
			new BreadcrumbItem("Meetstations", null, true)
		);

		HeaderService.Set("Meetstations");

		var breakpoint = await ViewportService.GetCurrentBreakpointAsync();

		if (breakpoint == Breakpoint.Xs) {
			await GetStationsMobileAsync(_activeSorting, _activeSortingDesc);
		}
	}

	// Desktop

	private async Task<GridData<MeasurementStationDto.Index>> GetStationsAsync(GridState<MeasurementStationDto.Index> state) {

		var sorting = state.SortDefinitions.FirstOrDefault();

		QueryRequest.SkipTake request = new() {
			Skip = state.Page * state.PageSize,
			Take = state.PageSize,
			OrderBy = sorting?.SortBy ?? "Id",
			OrderDescending = sorting?.Descending ?? false,
		};

		Result<MeasurementStationResponse.Index> result = await StationService.GetIndexAsync(request);

		return new GridData<MeasurementStationDto.Index>() {
			TotalItems = result.Value.TotalCount,
			Items = result.Value.Stations,
		};
	}
	private static string TranslateStatus(string raw) {
		if (string.IsNullOrWhiteSpace(raw)) {
			return string.Empty;
		}

		return raw switch {
			"Active" or "Actief" => "Actief",
			"Inactive" or "Inactief" => "Inactief",
			_ => raw
		};
	}
	// Mobile

	private async Task GetStationsMobileAsync(string orderBy, bool orderDescending) {
		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 10,
			OrderBy = orderBy,
			OrderDescending = orderDescending
		};

		Result<MeasurementStationResponse.Index> result = await StationService.GetIndexAsync(request);

		_mobileItems = result.Value.Stations;
	}

	// Mobile Sorting

	private void ToggleSortingDrawer() {
		_mobileSortOptionsOpen = !_mobileSortOptionsOpen;
	}

	private async Task SortMobileAsync(string orderBy, bool descending) {
		await GetStationsMobileAsync(orderBy, descending);
		ToggleSortingDrawer();
		_activeSorting = orderBy;
		_activeSortingDesc = descending;
	}

	// TODO: Implement filtering (updating the state for desktop or calling SortFilterAsync for mobile)

	// Navigation

	private void NavigateToStation(Guid stationId) {
		NavigationManager.NavigateTo($"/stations/{stationId}/projects");
	}


	// Other

	private Color DefineColor(string statusRaw) {
		if (string.IsNullOrWhiteSpace(statusRaw))
			return Color.Default;

		return statusRaw switch {
			"Active" or "Actief" => Color.Success,
			"Inactive" or "Inactief" => Color.Default,
			_ => Color.Default
		};
	}
}