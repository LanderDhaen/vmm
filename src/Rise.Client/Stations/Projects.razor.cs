using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class Projects {
	private IEnumerable<ProjectDto.Index>? _projectsFromStation;

	[Parameter] public Guid StationId { get; init; }
	[Inject] public required IMeasurementStationService StationService { get; init; }
	[Inject] public required NavigationManager NavigationManager { get; init; }

	protected override async Task OnInitializedAsync() {
		Result<MeasurementStationResponse.ProjectList> result = await StationService.GetProjectsAsync(StationId);

		if (result.IsSuccess) {
			_projectsFromStation = result.Value.Projects;
		}
	}
	
	private void NavigateToProject(Guid projectId) {
		NavigationManager.NavigateTo($"/projects/{projectId}");
	}
	private static Color DefineColor(Status status) {
		return status == Status.Actief ? Color.Success : Color.Error;
	}
}