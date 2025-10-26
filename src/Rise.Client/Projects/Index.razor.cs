using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Index {
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; set; }
	
	private async Task<GridData<ProjectDto.Index>> GetProjectsAsync(GridState<ProjectDto.Index> state) {
		QueryRequest.SkipTake request = new() {
			Skip = state.Page * state.PageSize,
			Take = state.PageSize,
			OrderBy = "Id"
		};

		Result<ProjectResponse.Index> result = await ProjectService.GetIndexAsync(request);

		return new GridData<ProjectDto.Index> {
			TotalItems = result.Value.TotalCount,
			Items = result.Value.Projects
		};
	}
	
	private void OnRowClick(DataGridRowClickEventArgs<ProjectDto.Index> args) {
		NavigationManager.NavigateTo($"/projects/{args.Item.Id}");
	}
}