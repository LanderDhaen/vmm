using Microsoft.AspNetCore.Components;

using MudBlazor;


using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Index {

	private IEnumerable<ProjectDto.Index>? _mobileItems;

	private bool _mobileSortOptionsOpen;

	private static readonly (string Value, string Label)[] SortColumns =
	[
		("Code", "Code"),
		("Name", "Naam"),
		("StartDate", "Startdatum"),
		("EndDate", "Einddatum"),
		("MeasurementNetwork", "Meetnet"),
		("Zone", "Zone"),
		("Status", "Status")
	];

	private string _activeSorting = "Name";
	private bool _activeSortingDesc;


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

		var breakpoint = await ViewportService.GetCurrentBreakpointAsync();

		if (breakpoint == Breakpoint.Xs) {
			await GetProjectsMobileAsync(_activeSorting, _activeSortingDesc);
		}
	}

	// Desktop

	private async Task<GridData<ProjectDto.Index>> GetProjectsAsync(GridState<ProjectDto.Index> state) {
		
		var sorting = state.SortDefinitions.FirstOrDefault();

		QueryRequest.SkipTake request = new() {
			Skip = state.Page * state.PageSize,
			Take = state.PageSize,
			OrderBy = sorting?.SortBy ?? "Name",
			OrderDescending = sorting?.Descending ?? false,
		};

		Result<ProjectResponse.Index> result = await ProjectService.GetIndexAsync(request);

		return new GridData<ProjectDto.Index>() {
			TotalItems = result.Value.TotalCount,
			Items = result.Value.Projects,
		};
	}

	// Mobile

	private async Task GetProjectsMobileAsync(string orderBy, bool orderDescending) {
		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 10,
			OrderBy = orderBy,
			OrderDescending = orderDescending
		};

		Result<ProjectResponse.Index> result = await ProjectService.GetIndexAsync(request);

		_mobileItems = result.Value.Projects;
	}

	// Mobile Sorting

	private void ToggleSortingDrawer() {
		_mobileSortOptionsOpen = !_mobileSortOptionsOpen;
	}

	private async Task SortMobileAsync(string orderBy, bool descending) {
		await GetProjectsMobileAsync(orderBy, descending);
		ToggleSortingDrawer();
		_activeSorting = orderBy;
		_activeSortingDesc = descending;
	}

	// TODO: Implement filtering (updating the state for desktop or calling SortFilterAsync for mobile)

	// Navigation

	private void NavigateToProject(Guid projectId) {
		NavigationManager.NavigateTo($"/projects/{projectId}");
	}

	private void NavigateToCreate() {
		NavigationManager.NavigateTo($"/projects/create");
	}
}