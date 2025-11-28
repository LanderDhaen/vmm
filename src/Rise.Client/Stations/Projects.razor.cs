using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class Projects {
	private IEnumerable<ProjectDto.Index> _projectsFromStation;

	[Parameter] public Guid StationId { get; set; }
	[Inject] public required IMeasurementStationService StationService { get; set; }
	[Inject] public required NavigationManager Navigation { get; set; }

	protected override async Task OnInitializedAsync() {
		Result<MeasurementStationResponse.ProjectList> result = await StationService.GetProjectsAsync(StationId);

		if (result.IsSuccess) {
			_projectsFromStation = result.Value.Projects;
		}
	}

	private void OnProjectRowClick(TableRowClickEventArgs<ProjectDto.Index> args) {
		Navigation.NavigateTo($"/projects/{args.Item.Id}");
	}

	private void OnProjectCardClick(ProjectDto.Index project) {
		Navigation.NavigateTo($"/projects/{project.Id}");
	}

	private static Color DefineColor(Status status) {
		return status == Status.Actief ? Color.Success : Color.Error;
	}
}