using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Create {
	private Result<ProjectResponse.Create>? _result;
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; set; }
	private ProjectRequest.Create Model { get; } = new();
	private ProjectRequest.Create.Validator validator;
	MudForm form;

	private async Task CreateProjectAsync() {
		await form.Validate();

		if (form.IsValid) {
			_result = await ProjectService.CreateAsync(Model);
			if (_result.IsSuccess) {
				NavigationManager.NavigateTo("/projects");
			}
		}
	}
}