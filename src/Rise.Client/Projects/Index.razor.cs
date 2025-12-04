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
		private List<string> _zones = new();
		private List<string> _measurementNetworks = new();
		private bool _loadingSelectOptions;

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

		private QueryRequest.SkipTake CreateRequestFromGridState(GridState<ProjectDto.Index> state) {
			var sorting = state.SortDefinitions.FirstOrDefault();
			var request = new QueryRequest.SkipTake {
				Skip = state.Page * state.PageSize,
				Take = state.PageSize,
				OrderBy = sorting?.SortBy ?? _activeSorting,
				OrderDescending = sorting?.Descending ?? _activeSortingDesc,
				SearchTerm = string.IsNullOrWhiteSpace(_searchTerm) ? string.Empty : _searchTerm.Trim()
			};
			request.Filters ??= new Dictionary<string, object?>();
			return request;
		}
		private void ApplyDrawerFilters(QueryRequest.SkipTake request, bool useContainsForText = false) {
			request.Filters ??= new Dictionary<string, object?>();

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
			_loadingSelectOptions = true;
			try {
				var reqAll = new QueryRequest.SkipTake { Skip = 0, Take = int.MaxValue };

				var zoneTask = ZoneService.GetIndexAsync(reqAll, ct);
				var networkTask = NetworkService.GetIndexAsync(reqAll, ct);

				await Task.WhenAll(zoneTask, networkTask);

				var zoneResult = await zoneTask;
				var networkResult = await networkTask;

				_zones = (zoneResult?.IsSuccess == true ? zoneResult.Value?.zones ?? Enumerable.Empty<ZoneDto.Index>() : Enumerable.Empty<ZoneDto.Index>())
					.Select(z => z.Name ?? string.Empty)
					.Where(s => !string.IsNullOrWhiteSpace(s))
					.Distinct(StringComparer.OrdinalIgnoreCase)
					.OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
					.ToList();

				_measurementNetworks = (networkResult?.IsSuccess == true ? networkResult.Value?.measurementsNetworks ?? Enumerable.Empty<MeasurementNetworkDto.Index>() : Enumerable.Empty<MeasurementNetworkDto.Index>())
					.Select(n => n.Name ?? string.Empty)
					.Where(s => !string.IsNullOrWhiteSpace(s))
					.Distinct(StringComparer.OrdinalIgnoreCase)
					.OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
					.ToList();
			} catch (OperationCanceledException) {

			} catch {
				_zones = new List<string>();
				_measurementNetworks = new List<string>();
			} finally {
				_loadingSelectOptions = false;
				StateHasChanged();
			}
		}

		private async Task<GridData<ProjectDto.Index>> GetProjectsAsync(GridState<ProjectDto.Index> state) {
			var request = CreateRequestFromGridState(state);

			ApplyDrawerFilters(request, useContainsForText: false);

			var result = await ProjectService.GetIndexAsync(request);
			_ = result?.Value?.Projects ?? Enumerable.Empty<ProjectDto.Index>();

			var total = result?.Value?.TotalCount ?? 0;

			return new GridData<ProjectDto.Index> {
				TotalItems = total,
				Items = result?.Value?.Projects ?? Enumerable.Empty<ProjectDto.Index>()
			};
		}

		private async Task GetProjectsMobileAsync() {
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
			_cancellationTokenSource = new CancellationTokenSource();

			var request = new QueryRequest.SkipTake {
				Skip = 0,
				Take = 10,
				OrderBy = _activeSorting,
				OrderDescending = _activeSortingDesc,
				SearchTerm = string.IsNullOrWhiteSpace(_searchTerm) ? string.Empty : _searchTerm.Trim()
			};
			request.Filters ??= new Dictionary<string, object?>();

			ApplyDrawerFilters(request, useContainsForText: true);

			var result = await ProjectService.GetIndexAsync(request);

			var projects = result?.Value?.Projects ?? Enumerable.Empty<ProjectDto.Index>();
			_mobileItems = projects;
		}

		private void ToggleSortingDrawer() => _mobileSortOptionsOpen = !_mobileSortOptionsOpen;

		private async Task ToggleFiltersDrawer() {
			var bp = await ViewportService.GetCurrentBreakpointAsync();
			_filterDrawerAnchor = bp == Breakpoint.Xs ? Anchor.Bottom : Anchor.Right;
			bool willOpen = !_filterOptionsOpen;
			if (willOpen) {
				try {
					await LoadAllSelectOptionsAsync();
				} catch {
					_zones = new List<string>();
					_measurementNetworks = new List<string>();
				}
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