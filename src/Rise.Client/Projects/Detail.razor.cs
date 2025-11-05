using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Detail {
	//BREADCRUMB
	private readonly List<BreadcrumbItem> _items = new();

	private readonly List<string> _tabs = new() {
		"Info",
		"Meetstations",
		"Historiek"
	};

	// ====== Main ======
	private ProjectDto.Detail? _project;

	private string? _remarks;
	// private RealTimeMap.LoadParameters? _parameters;

	// ====== Tabs ======
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

			_items.Clear();
			_items.Add(new BreadcrumbItem("", "/", icon: Icons.Material.Filled.Home));
			_items.Add(new BreadcrumbItem("Projecten", "/projects"));
			_items.Add(new BreadcrumbItem(_project?.Code ?? "Detail", null, true));
		}
	}

	//NAV
	private void NavigateToProjects() {
		Navigation.NavigateTo("/projects/");
	}
}