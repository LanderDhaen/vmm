using Microsoft.AspNetCore.Components;

using Rise.Shared.Projects;

namespace Rise.Client.Components.Projects;

public partial class ProjectCard {
	[Parameter] public required ProjectDto.Index Project { get; init; }

	[Inject] public required NavigationManager NavigationManager { get; init; }

	private void NavigateToProject(Guid id) {
		NavigationManager.NavigateTo($"projects/{id}");
	}
}