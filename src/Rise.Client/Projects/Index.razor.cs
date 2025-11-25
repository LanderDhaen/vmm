using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Index {
	private static readonly (string Value, string Label)[] SortColumns = [
		("Code", "Code"),
		("Name", "Naam"),
		("StartDate", "Startdatum"),
		("EndDate", "Einddatum"),
		("MeasurementNetwork", "Meetnet"),
		("Zone", "Zone"),
		("Status", "Status")
	];
	
	private string _activeSorting = "Code";
	private bool _activeSortingDesc;
	private string _searchTerm;

	private IEnumerable<ProjectDto.Index>? _mobileItems;

	private bool _mobileSortOptionsOpen;
	
	private CancellationTokenSource _cancellationTokenSource = new();
	
	private MudDataGrid<ProjectDto.Index> _grid;

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

		Breakpoint breakpoint = await ViewportService.GetCurrentBreakpointAsync();

		if (breakpoint == Breakpoint.Xs) {
			await GetProjectsMobileAsync();
		}
	}

	// Desktop

	private async Task<GridData<ProjectDto.Index>> GetProjectsAsync(GridState<ProjectDto.Index> state) {
		
		SortDefinition<ProjectDto.Index> sorting = state.SortDefinitions.FirstOrDefault(
			new SortDefinition<ProjectDto.Index>(_activeSorting, _activeSortingDesc, 0, null)); // Can't seem to have no sort function here
		
		QueryRequest.SkipTake request = new() {
			Skip = state.Page * state.PageSize,
			Take = state.PageSize,
			OrderBy = sorting.SortBy,
			OrderDescending = sorting.Descending,
			SearchTerm = _searchTerm
		};

		Result<ProjectResponse.Index> result = await ProjectService.GetIndexAsync(request, _cancellationTokenSource.Token);

		return new GridData<ProjectDto.Index> {
			TotalItems = result.Value.TotalCount,
			Items = result.Value.Projects
		};
	}

	// Mobile

	private async Task GetProjectsMobileAsync() {
		
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

		Result<ProjectResponse.Index> result = await ProjectService.GetIndexAsync(request, _cancellationTokenSource.Token);

		_mobileItems = result.Value.Projects;
	}

	// Mobile Sorting

	private void ToggleSortingDrawer() {
		_mobileSortOptionsOpen = !_mobileSortOptionsOpen;
	}

	private async Task SortMobileAsync(string orderBy, bool descending) {
		_activeSorting = orderBy;
		_activeSortingDesc = descending;
		await GetProjectsMobileAsync();
		ToggleSortingDrawer();
	}
	
	// Searching

	private async Task HandleDesktopIntervalElapsed() {
		await _grid.ReloadServerData();
	}
	
	private async Task HandleMobileIntervalElapsed() {
		await GetProjectsMobileAsync();
	}

	// TODO: Implement filtering (updating the state for desktop or calling SortFilterAsync for mobile)

	// Navigation

	private void NavigateToProject(Guid projectId) {
		NavigationManager.NavigateTo($"/projects/{projectId}");
	}

	private void NavigateToCreate() {
		NavigationManager.NavigateTo("/projects/create");
	}

	// Other

	private Color DefineColor(Status status) {
		return status == Status.Actief ? Color.Success : Color.Error;
	}
}