using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class Projects {

	private string _searchTerm = string.Empty;
	
	private IEnumerable<ProjectDto.Index>? _projectsFromStation;

	[Parameter] public Guid StationId { get; init; }
	[Inject] public required IMeasurementStationService StationService { get; init; }
	[Inject] public required NavigationManager NavigationManager { get; init; }

	protected override async Task OnInitializedAsync() {
		await GetProjectsAsync();
	}

	private async Task GetProjectsAsync() {

		QueryRequest.Search request = new() {
			SearchTerm = _searchTerm
		};
		
		Result<MeasurementStationResponse.ProjectList> result = await StationService.GetProjectsAsync(StationId, request);

		if (result.IsSuccess) {
			_projectsFromStation = result.Value.Projects;
		}
	}

	private async Task HandleIntervalElapsed() {
		await GetProjectsAsync();
	}
	
	private void NavigateToProject(Guid projectId) {
		NavigationManager.NavigateTo($"/projects/{projectId}");
	}
}