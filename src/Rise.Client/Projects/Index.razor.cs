using System.Globalization;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Networks;
using Rise.Shared.Projects;
using Rise.Shared.Zones;

namespace Rise.Client.Projects {
	public partial class Index {
		// Sort options 
		private static readonly (string Value, string Label)[] SortColumns = new[]
		{
			("Code", "Code"),
			("Name", "Naam"),
			("StartDate", "Startdatum"),
			("EndDate", "Einddatum"),
			("MeasurementNetwork", "Meetnet"),
			("Zone", "Zone"),
			("Status", "Status")
		};

		private string _activeSorting = "Name";
		private bool _activeSortingDesc;
		private string _searchTerm = string.Empty;

		private IEnumerable<ProjectDto.Index>? _mobileItems;

		private bool _mobileSortOptionsOpen;
		private bool _filterOptionsOpen;
		private Anchor _filterDrawerAnchor = Anchor.Right;

		private CancellationTokenSource _cancellationTokenSource = new();

		private MudDataGrid<ProjectDto.Index>? _grid;


		private Status? _filterStatus;
		private string? _filterZone;
		private string? _filterMeasurementNetwork;
		private DateTime? _filterStartDate;
		private DateTime? _filterEndDate;
		private IEnumerable<ZoneDto.Index>? _zones;
		private IEnumerable<MeasurementNetworkDto.Index>? _measurementNetworks;

		[Inject] public required IProjectService ProjectService { get; init; }
		[Inject] public required INetworkService NetworkService { get; init; }
		[Inject] public required IZoneService ZoneService { get; init; }
		[Inject] public required NavigationManager NavigationManager { get; init; }
		[Inject] public required BreadcrumbService BreadcrumbService { get; init; }
		[Inject] public required HeaderService HeaderService { get; init; }
		[Inject] public required IBrowserViewportService ViewportService { get; init; }

		protected override async Task OnInitializedAsync() {
			BreadcrumbService.SetBreadcrumbs(
				new BreadcrumbItem("Home", "/"),
				new BreadcrumbItem("Projecten", null, true)
			);

			HeaderService.Set("Projecten");

			Breakpoint breakpoint = await ViewportService.GetCurrentBreakpointAsync();
			if (breakpoint == Breakpoint.Xs) {
				var bp = await ViewportService.GetCurrentBreakpointAsync();
				if (bp == Breakpoint.Xs)
					await GetProjectsMobileAsync();
			}
		}
		
		private void ApplyDrawerFilters(QueryRequest.SkipTake request, bool useContainsForText = false) {
			if (_filterStatus.HasValue)
				request.Filters["Status"] = new { Operator = "Equals", Value = _filterStatus.Value.ToString() };

			if (!string.IsNullOrWhiteSpace(_filterZone))
				request.Filters["Zone"] = new { Operator = useContainsForText ? "Contains" : "Equals", Value = _filterZone!.Trim() };

			if (!string.IsNullOrWhiteSpace(_filterMeasurementNetwork))
				request.Filters["MeasurementNetwork"] = new { Operator = useContainsForText ? "Contains" : "Equals", Value = _filterMeasurementNetwork!.Trim() };

			if (_filterStartDate.HasValue)
				request.Filters["StartDate"] = new { Operator = "GreaterThanOrEqual", Value = _filterStartDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) };

			if (_filterEndDate.HasValue)
				request.Filters["EndDate"] = new { Operator = "LessThanOrEqual", Value = _filterEndDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) };
		}
		
		private async Task LoadAllSelectOptionsAsync(CancellationToken ct = default) {
			var reqAll = new QueryRequest.SkipTake { Skip = 0, Take = int.MaxValue };
				
			Result<ZoneResponse.Index> zoneResult = await ZoneService.GetIndexAsync(reqAll, ct);
			_zones = zoneResult.Value.zones;
				
			Result<NetworkResponse.Index> measurementNetworkResult = await NetworkService.GetIndexAsync(reqAll, ct);
			_measurementNetworks = measurementNetworkResult.Value.measurementsNetworks;
		}

		private async Task<GridData<ProjectDto.Index>> GetProjectsAsync(GridState<ProjectDto.Index> state) {
			var sorting = state.SortDefinitions.FirstOrDefault();
			QueryRequest.SkipTake request = new() {
				Skip = state.Page * state.PageSize,
				Take = state.PageSize,
				OrderBy = sorting?.SortBy ?? _activeSorting,
				OrderDescending = sorting?.Descending ?? _activeSortingDesc,
				SearchTerm = _searchTerm.Trim()
			};

			ApplyDrawerFilters(request, useContainsForText: false);

			Result<ProjectResponse.Index> result = await ProjectService.GetIndexAsync(request);
			
			return new GridData<ProjectDto.Index> {
				TotalItems = result.Value.TotalCount,
				Items = result.Value.Projects
			};
		}

		private async Task GetProjectsMobileAsync() {
			await _cancellationTokenSource.CancelAsync();
			_cancellationTokenSource.Dispose();
			_cancellationTokenSource = new CancellationTokenSource();

			QueryRequest.SkipTake request = new() {
				Skip = 0,
				Take = 10,
				OrderBy = _activeSorting,
				OrderDescending = _activeSortingDesc,
				SearchTerm =  _searchTerm.Trim()
			};

			ApplyDrawerFilters(request, useContainsForText: true);

			Result<ProjectResponse.Index> result = await ProjectService.GetIndexAsync(request);
			
			_mobileItems = result.Value.Projects;
		}

		private void ToggleSortingDrawer() => _mobileSortOptionsOpen = !_mobileSortOptionsOpen;

		private async Task ToggleFiltersDrawer() {
			var bp = await ViewportService.GetCurrentBreakpointAsync();
			_filterDrawerAnchor = bp == Breakpoint.Xs ? Anchor.Bottom : Anchor.Right;
			bool willOpen = !_filterOptionsOpen;
			if (willOpen) {
				await LoadAllSelectOptionsAsync();
			}
			_filterOptionsOpen = willOpen;
		}

		private async Task SortMobileAsync(string orderBy, bool descending) {
			_activeSorting = orderBy;
			_activeSortingDesc = descending;
			await GetProjectsMobileAsync();
			ToggleSortingDrawer();
		}

		private async Task HandleDesktopIntervalElapsed() {
			if (_grid != null)
				await _grid.ReloadServerData();
		}

		private async Task HandleMobileIntervalElapsed() => await GetProjectsMobileAsync();

		private void NavigateToProject(Guid projectId) => NavigationManager.NavigateTo($"/projects/{projectId}");
		private void NavigateToCreate() => NavigationManager.NavigateTo("/projects/create");
		private async Task UpdateFilterAsync<T>(Action<T?> setter, T? value, Func<Task>? after = null) {
			setter(value);
			if (after != null)
				await after();
			else
				await ApplyOrReloadOnChangeAsync();
			_filterOptionsOpen = false;
			StateHasChanged();
		}

		private async Task ClearFilters() {
			_filterStatus = null;
			_filterZone = null;
			_filterMeasurementNetwork = null;
			_filterStartDate = null;
			_filterEndDate = null;

			var bp = await ViewportService.GetCurrentBreakpointAsync();

			if (bp == Breakpoint.Xs)
				await GetProjectsMobileAsync();
			else if (_grid != null)
				await _grid.ReloadServerData();

			_filterOptionsOpen = false;
			StateHasChanged();
		}
		private async Task ApplyOrReloadOnChangeAsync() {
			var bp = await ViewportService.GetCurrentBreakpointAsync();
			if (bp == Breakpoint.Xs) {
				await GetProjectsMobileAsync();
			} else {
				if (_grid != null)
					await _grid.ReloadServerData();
			}
			StateHasChanged();
		}


		private string GetFilterDrawerClass() =>
			_filterDrawerAnchor == Anchor.Right ? "p-4 rounded-l-lg" : "p-4 rounded-t-lg";
	}
}