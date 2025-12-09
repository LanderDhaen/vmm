using Microsoft.AspNetCore.Components;

using Rise.Shared.Projects;

namespace Rise.Client.Stations;

public partial class Projects {
	[Parameter] public Guid StationId { get; init; }
	[Parameter] public IEnumerable<ProjectDto.Index>? ProjectsFromStation { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; init; }

	private void NavigateToProject(Guid projectId) {
		NavigationManager.NavigateTo($"/projects/{projectId}");
	}
}