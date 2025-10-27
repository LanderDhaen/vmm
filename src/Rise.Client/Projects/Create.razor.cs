using Microsoft.AspNetCore.Components;

using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Create {
	private Result<ProjectResponse.Create>? _result;
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; set; }
	private ProjectRequest.Create Model { get; } = new();

	private async Task CreateProjectAsync() {
		_result = await ProjectService.CreateAsync(Model);
		if (_result.IsSuccess) {
			NavigationManager.NavigateTo("/projects");
		}
	}
}