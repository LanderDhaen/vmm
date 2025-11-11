using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Detail {
	// ====== Tabs ======
	private readonly List<string> _tabs = new() {
		"Info",
		"Meetstations",
		"Historiek"
	};

	// ====== Main ======
	private ProjectDto.Detail? _project;

	private string? _remarks;
	private int _selectedTab;
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }
	[Parameter] public Guid ProjectId { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }

	private void SetTab(int index) {
		_selectedTab = index;
	}


	protected override async Task OnInitializedAsync() {
		Result<ProjectResponse.Detail> result = await ProjectService.GetDetailAsync(ProjectId);

		if (result.IsSuccess) {
			_project = result.Value.Project;

			BreadcrumbService.Set(
				new BreadcrumbItem("Home", "/", icon: Icons.Material.Filled.Home),
				new BreadcrumbItem("Projecten", "/projects"),
				new BreadcrumbItem(_project.Code + " - " + _project.Name, "", true)
			);
		}
	}

	//NAV
	private void NavigateToProjects() {
		Navigation.NavigateTo("/projects/");
	}
}