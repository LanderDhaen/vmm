using Microsoft.AspNetCore.Components;

using Rise.Shared.Common;
using Rise.Shared.Projects;

using MudBlazor;

namespace Rise.Client.Projects;

public partial class Index {
	
	[Inject] public required IProjectService ProjectService { get; set; }

	private async Task<GridData<ProjectDto.Index>> GetProjectsAsync(GridState<ProjectDto.Index> state) {
		
		var sortDefinition = state.SortDefinitions.FirstOrDefault();
		
		QueryRequest.SkipTake request = new() {
			Skip = state.Page * state.PageSize,
			Take = state.PageSize,
			OrderBy = sortDefinition.SortBy,
			OrderDescending = sortDefinition.Descending
		};
		
		Result<ProjectResponse.Index> result = await ProjectService.GetIndexAsync(request);

		return new GridData<ProjectDto.Index>() {
			TotalItems = result.Value.TotalCount,
			Items = result.Value.Projects,
		};

	}
}