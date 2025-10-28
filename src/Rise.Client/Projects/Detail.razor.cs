using LeafletForBlazor;

using Microsoft.AspNetCore.Components;

using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Detail {
	[Parameter] public Guid ProjectId { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }

	private ProjectDto.Detail? _project;
	private string? _remarks;


	protected override async Task OnInitializedAsync() {
		var result = await ProjectService.GetDetailAsync(ProjectId);
		if (result.IsSuccess) {
			_project = result.Value.Project;
		}
	}

	RealTimeMap.LoadParameters parameters = new RealTimeMap.LoadParameters() {
		location = new RealTimeMap.Location() {
			latitude = 50,
			longitude = 4,
		},
		zoomLevel = 10
	};


	private void NavigateToHistory() {
		Navigation.NavigateTo("/projects/history");
	}
}