using LeafletForBlazor;

using Microsoft.AspNetCore.Components;

using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Detail {
	[Parameter] public Guid ProjectId { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }

	private ProjectDto.Detail? _project;
	private string? _remarks;

	private RealTimeMap.LoadParameters? parameters;

	protected override async Task OnInitializedAsync() {
		var result = await ProjectService.GetDetailAsync(ProjectId);

		if (result.IsSuccess) {
			_project = result.Value.Project;

			// Pas hier stel je de parameters in op basis van je projectlocatie
			if (_project?.Location?.Coordinate is not null) {
				parameters = new RealTimeMap.LoadParameters {
					location = new RealTimeMap.Location {
						latitude = _project.Location.Coordinate.Y, // meestal Y = lat
						longitude = _project.Location.Coordinate.X // meestal X = lon
					},
					zoomLevel = 15
				};
			}
		}
	}

	private void NavigateToHistory() {
		Navigation.NavigateTo("/projects/history");
	}
}