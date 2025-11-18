using Microsoft.AspNetCore.Components;

using MudBlazor;


using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Index {

	private IEnumerable<ProjectDto.Index>? _mobileItems;

	private bool _mobileSortOptionsOpen;

	private enum MobileProjectSortColumn {
		Code,
		Name,
		StartDate,
		EndDate,
		MeasurementNetwork,
		Zone,
		Status
	}

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
			await GetProjectsMobileAsync();
		}
	}

	private async Task<GridData<ProjectDto.Index>> GetProjectsAsync(GridState<ProjectDto.Index> state) {

		QueryRequest.SkipTake request = new() {
			Skip = state.Page * state.PageSize,
			Take = state.PageSize,
			OrderBy = "Id"
		};

		Result<ProjectResponse.Index> result = await ProjectService.GetIndexAsync(request);

		return new GridData<ProjectDto.Index>() {
			TotalItems = result.Value.TotalCount,
			Items = result.Value.Projects,
		};
	}
	
	private async Task GetProjectsMobileAsync() {
		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 10,
			OrderBy = "Id"
		};

		Result<ProjectResponse.Index> result = await ProjectService.GetIndexAsync(request);

		_mobileItems = result.Value.Projects;
	}


	public Task OpenSortOptionsAsync() {
		_mobileSortOptionsOpen = true;
		StateHasChanged();
		return Task.CompletedTask;
	}

	private async Task SortMobileAsync(MobileProjectSortColumn column, bool descending) {
		await GetProjectsMobileAsync();
		_mobileSortOptionsOpen = false;
	}
	
	private void NavigateToProject(Guid projectId) {
		NavigationManager.NavigateTo($"/projects/{projectId}");
	}
}
