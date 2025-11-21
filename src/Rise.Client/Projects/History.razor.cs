using Microsoft.AspNetCore.Components;

using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class History {
	private ProjectDto.History? _project;

	[Parameter] public Guid ProjectId { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }
	private bool DisplayTable { get; set; }

	protected override async Task OnInitializedAsync() {
		Result<ProjectResponse.History> result = await ProjectService.GetHistoryAsync(ProjectId);

		if (result.IsSuccess) {
			_project = result.Value.Project;
		}
	}
}