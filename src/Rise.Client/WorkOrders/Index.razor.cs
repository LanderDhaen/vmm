using System.Globalization;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.MaintObjects;
using Rise.Shared.WorkOrders;

namespace Rise.Client.WorkOrders;

public partial class Index{
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
	private MudDataGrid<WorkOrderDto.Index>? _grid;

	private string _searchTerm = string.Empty;

	private bool _filterOptionsOpen;
	private Anchor _filterDrawerAnchor = Anchor.Right;
	private WorkOrderStatus? _filterStatus;
	private MaintObjType? _filterMaintObjType;
	private string? _filterTechnician;
	private DateTime? _filterStartDate;
	private DateTime? _filterEndDate;
	private Priority? _filterPriority;
	
  private IEnumerable<Priority>? _priorityOptions;

	private IEnumerable<WorkOrderStatus>? _statusOptions;
	private IEnumerable<MaintObjType>? _maintObjTypeOptions;
	private IEnumerable<string>? _filterTechnicianOptions;
	private CancellationTokenSource _selectCancellationTokenSource = new();

	[Inject] public required IWorkOrderService WorkOrderService { get; init; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; init; }
	[Inject] public required HeaderService HeaderService { get; init; }
	[Inject] public required IBrowserViewportService ViewportService { get; init; }
	[Inject] public required NavigationManager Navigation { get; init; }

	protected override async Task OnInitializedAsync() {
		BreadcrumbService.SetBreadcrumbs(new BreadcrumbItem("Werkbonnen", null, true));
		HeaderService.Set("Werkbonnen");

		var bp = await ViewportService.GetCurrentBreakpointAsync();
		if (bp == Breakpoint.Xs)
			await GetWorkOrdersMobileAsync();
	}

	private void ApplyDrawerFilters(QueryRequest.SkipTake request) {
		if (_filterPriority.HasValue) request.Filters["Priority"] = new { Operator = "Equals", Value = _filterPriority.Value.ToString() }; // toegevoegd
		if (_filterStatus.HasValue) request.Filters["Status"] = new { Operator = "Equals", Value = _filterStatus.Value.ToString() };
		if (_filterMaintObjType.HasValue) request.Filters["MaintObjType"] = new { Operator = "Equals", Value = _filterMaintObjType.Value.ToString() };
		if (!string.IsNullOrWhiteSpace(_filterTechnician)) request.Filters["Technician"] = new { Operator = "Contains", Value = _filterTechnician!.Trim() };
		if (_filterStartDate.HasValue) request.Filters["StartDate"] = new { Operator = "GreaterThanOrEqual", Value = _filterStartDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) };
		if (_filterEndDate.HasValue) request.Filters["EndDate"] = new { Operator = "LessThanOrEqual", Value = _filterEndDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) };
	}

	private async Task<GridData<WorkOrderDto.Index>> GetWorkOrdersAsync(GridState<WorkOrderDto.Index> state) {
		var sorting = state.SortDefinitions.FirstOrDefault();
		var request = new QueryRequest.SkipTake {
			Skip = state.Page * state.PageSize,
			Take = state.PageSize,
			OrderBy = sorting?.SortBy ?? _activeSorting,
			OrderDescending = sorting?.Descending ?? _activeSortingDesc,
			SearchTerm = _searchTerm.Trim()
		};

		ApplyDrawerFilters(request);

		var result = await WorkOrderService.GetIndexAsync(request);
		return new GridData<WorkOrderDto.Index> {
			TotalItems = result.Value.TotalCount,
			Items = result.Value.WorkOrders
		};
	}

	private async Task GetWorkOrdersMobileAsync() {
		var request = new QueryRequest.SkipTake {
			Skip = 0,
			Take = 10,
			OrderBy = _activeSorting,
			OrderDescending = _activeSortingDesc,
			SearchTerm = _searchTerm.Trim()
		};

		ApplyDrawerFilters(request);

		var result = await WorkOrderService.GetIndexAsync(request);
		_mobilityWorkOrders = result.IsSuccess ? result.Value.WorkOrders : Enumerable.Empty<WorkOrderDto.Index>();
		StateHasChanged();
	}

	private async Task ToggleSortingDrawer() {
		_mobileSortOptionsOpen = !_mobileSortOptionsOpen;
		await InvokeAsync(StateHasChanged);
	}
	private async Task SortMobileAsync(string orderBy, bool descending) {
		_activeSorting = orderBy;
		_activeSortingDesc = descending;

		await GetWorkOrdersMobileAsync();
		_mobileSortOptionsOpen = false;
		await InvokeAsync(StateHasChanged);
	}

	private async Task HandleDesktopIntervalElapsed() {
		if (_grid != null) await _grid.ReloadServerData();
	}
	private async Task HandleMobileIntervalElapsed() => await GetWorkOrdersMobileAsync();

	private void OnRowClick(DataGridRowClickEventArgs<WorkOrderDto.Index> args) => NavigateToDetail(args.Item.Id);
	private void NavigateToDetail(Guid id) => Navigation.NavigateTo($"/workorders/{id}");

	private void ToggleFiltersDrawer() => _ = SetFilterDrawerAnchorAndToggleAsync();

	private async Task SetFilterDrawerAnchorAndToggleAsync() {
		var bp = await ViewportService.GetCurrentBreakpointAsync();
		_filterDrawerAnchor = bp == Breakpoint.Xs ? Anchor.Bottom : Anchor.Right;
		_filterOptionsOpen = !_filterOptionsOpen;
		StateHasChanged();
	}

	private async Task ClearFilters() {
		_filterStatus = null;
		_filterMaintObjType = null;
		_filterPriority = null;
		_filterTechnician = null;
		_filterStartDate = null;
		_filterEndDate = null;

		try { _selectCancellationTokenSource.Cancel(); } catch { }

		_filterOptionsOpen = false;
		var bp = await ViewportService.GetCurrentBreakpointAsync();
		if (bp == Breakpoint.Xs)
			await GetWorkOrdersMobileAsync();
		else if (_grid != null)
			await _grid.ReloadServerData();

		StateHasChanged();
	}

	private async Task ApplyOrReloadOnChangeAsync() {
		var bp = await ViewportService.GetCurrentBreakpointAsync();
		if (bp == Breakpoint.Xs) await GetWorkOrdersMobileAsync();
		else if (_grid != null) await _grid.ReloadServerData();
		StateHasChanged();
	}

	private string GetFilterDrawerClass() => _filterDrawerAnchor == Anchor.Right ? "pa-4 rounded-l-lg" : "pa-4 rounded-t-lg";

	private Task HandleStatusSelectOpen() {
		_statusOptions = Enum.GetValues(typeof(WorkOrderStatus)).Cast<WorkOrderStatus>().ToList();
		StateHasChanged();
		return Task.CompletedTask;
	}

	private Task HandleMaintObjTypeSelectOpen() {
		_maintObjTypeOptions = Enum.GetValues(typeof(MaintObjType)).Cast<MaintObjType>().ToList();
		StateHasChanged();
		return Task.CompletedTask;
	}
	private async Task HandleTechnicianSelectOpen() {
		try { _selectCancellationTokenSource.Cancel(); } catch { }
		_selectCancellationTokenSource = new CancellationTokenSource();

		_filterTechnicianOptions = null;
		StateHasChanged();

		try {
			var ct = _selectCancellationTokenSource.Token;
			var list = await WorkOrderService.GetTechnicianNamesAsync(null, ct);
			_filterTechnicianOptions = list ?? Enumerable.Empty<string>();
		} catch (OperationCanceledException) { } catch { _filterTechnicianOptions = Enumerable.Empty<string>(); } finally { StateHasChanged(); }
	}
private Task HandlePrioritySelectOpen() {
	_priorityOptions = Enum.GetValues(typeof(Priority)).Cast<Priority>().ToList();
	StateHasChanged();
	return Task.CompletedTask;
}
	public ValueTask DisposeAsync() {
		try { _selectCancellationTokenSource.Cancel(); } catch { }
		_selectCancellationTokenSource.Dispose();
		return ValueTask.CompletedTask;
	}
}