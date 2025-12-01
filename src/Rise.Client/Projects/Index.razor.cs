using System.Globalization;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Projects;

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

	[Inject] public required IProjectService ProjectService { get; init; }
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

		await LoadAllSelectOptionsAsync();

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
	private void UpdateSelectOptions(IEnumerable<ProjectDto.Index> projects) {
		_zones = projects
			.Select(p => p.Zone ?? string.Empty)
			.Where(s => !string.IsNullOrWhiteSpace(s))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
			.ToList();

		_measurementNetworks = projects
			.Select(p => p.MeasurementNetwork ?? string.Empty)
			.Where(s => !string.IsNullOrWhiteSpace(s))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
			.ToList();
	}

	private async Task LoadAllSelectOptionsAsync() {
		try {
			var request = new QueryRequest.SkipTake {
				Skip = 0,
				Take = 10000,
				OrderBy = "Name",
				OrderDescending = false,
				SearchTerm = string.Empty
			};
			var result = await ProjectService.GetIndexAsync(request);
			var projects = result?.Value?.Projects ?? Enumerable.Empty<ProjectDto.Index>();
			UpdateSelectOptions(projects);
		}
		catch {
			_zones = new List<string>();
			_measurementNetworks = new List<string>();
		}
	}

	private async Task<GridData<ProjectDto.Index>> GetProjectsAsync(GridState<ProjectDto.Index> state) {
		var request = CreateRequestFromGridState(state);

		ApplyDrawerFilters(request, useContainsForText: false);

		var result = await ProjectService.GetIndexAsync(request);

		var projects = result?.Value?.Projects ?? Enumerable.Empty<ProjectDto.Index>();

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
		_filterOptionsOpen = !_filterOptionsOpen;
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
	private Color DefineColor(Status status) => status == Status.Actief ? Color.Success : Color.Error;

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