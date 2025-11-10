using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Detail {
	//===== Breadcrumbs =====
	private readonly List<BreadcrumbItem> _items = new();

	// ====== Tabs ======
	private readonly List<string> _tabs = new() {
		"Info",
		"Meetstations",
		"Historiek"
	};

	//DRAWER
	private bool _open;

	// ====== Main ======
	private ProjectDto.Detail? _project;

	private string? _remarks;
	private int _selectedTab;
	[Parameter] public Guid ProjectId { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; } = default!;


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
			_items.Add(new BreadcrumbItem(_project.Code + " - " + _project.Name, null, true));
		}
	}


	private async Task Close() {
		Result result = await ProjectService.UpdateStatusAsync(new ProjectRequest.UpdateStatus {
			ProjectId = ProjectId,
			NewStatus = Status.Afgesloten
		});

		if (result.IsSuccess) {
			Snackbar.Add("Project succesvol afgesloten!", Severity.Success);
		}
		// else {
		// 	Snackbar.Add(result.Message, Severity.Error);
		// }
	}

	private async Task Start() {
		Result result = await ProjectService.UpdateStatusAsync(new ProjectRequest.UpdateStatus {
			ProjectId = ProjectId,
			NewStatus = Status.Actief
		});

		if (result.IsSuccess) {
			Snackbar.Add("Project succesvol opgestart!", Severity.Success);
		}
		// else {
		// 	Snackbar.Add(result.Message, Severity.Error);
		// }
	}

	private void OpenDrawer() {
		_open = !_open;
	}


	//NAV
	private void NavigateToProjects() {
		Navigation.NavigateTo("/projects/");
	}
}