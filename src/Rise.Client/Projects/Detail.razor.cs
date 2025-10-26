using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Detail {
	[Parameter] public Guid ProjectId { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }

	private ProjectDto.Detail? _project;

	protected override async Task OnInitializedAsync() {
		var result = await ProjectService.GetDetailAsync(ProjectId);
		if (result.IsSuccess) {
			_project = result.Value.Project;
		}
	}
}