using Microsoft.AspNetCore.Components;

using Rise.Shared.Common;
using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Index {
	private IEnumerable<ProjectDto.Index>? projects;

	[Inject] public required IProjectService ProjectService { get; set; }

	protected override async Task OnInitializedAsync() {
		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 50,
			OrderBy = "Id"
		};

		Result<ProjectResponse.Index> result = await ProjectService.GetIndexAsync(request);
		projects = result.Value.Projects;
	}
}