using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Detail {
	//===== Breadcrumbs =====
	private List<BreadcrumbItem> _items = [];

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
	[Parameter] public Guid ProjectId { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }

	private void SetTab(int index) {
		_selectedTab = index;
	}


	protected override async Task OnInitializedAsync() {
		Result<ProjectResponse.Detail> result = await ProjectService.GetDetailAsync(ProjectId);

		if (result.IsSuccess) {
			_project = result.Value.Project;

			_items = [
				new("Home", "#"),
				new("Projecten", "/projects"),
				new($"{_project.Code} - {_project.Name}", null, true)
			];
		}
	}

	//NAV
	private void NavigateToProjects() {
		Navigation.NavigateTo("/projects/");
	}
}