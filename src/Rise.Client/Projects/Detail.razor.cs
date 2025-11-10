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
	[Inject] private IDialogService DialogService { get; set; } = default!;
	private Color BadgeColor => _project?.Status == Status.Actief ? Color.Success : Color.Error;
	private bool IsActive => _project?.Status == Status.Actief;


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
			if (_project is not null) {
				_project.Status = Status.Afgesloten;
				_project.EndDate = DateOnly.FromDateTime(DateTime.UtcNow); // optioneel: meteen tonen
			}

			Snackbar.Add("Project succesvol afgesloten!", Severity.Success);
			_open = false;

			StateHasChanged(); // 🔥 UI updaten
		} else {
			string message = result.Errors != null
				? string.Join(Environment.NewLine, result.Errors)
				: "Er is een fout opgetreden.";
			Snackbar.Add(message, Severity.Error);
		}
	}

	private async Task Start() {
		Result result = await ProjectService.UpdateStatusAsync(new ProjectRequest.UpdateStatus {
			ProjectId = ProjectId,
			NewStatus = Status.Actief
		});

		if (result.IsSuccess) {
			if (_project is not null) {
				_project.Status = Status.Actief;
				_project.EndDate = null; // 🔥 maakt actief, dus einddatum wissen
			}

			Snackbar.Add("Project succesvol opgestart!", Severity.Success);
			_open = false;

			StateHasChanged(); // 🔥 hertekenen
		} else {
			string message = result.Errors != null
				? string.Join(Environment.NewLine, result.Errors)
				: "Er is een fout opgetreden.";
			Snackbar.Add(message, Severity.Error);
		}
	}

	private void OpenDrawer() {
		_open = !_open;
	}


	// Confirmation wrappers that show a dialog first
	private async Task ShowCloseConfirmationAsync()
	{
		bool? result = await DialogService.ShowMessageBox(
			"Bevestiging",
			"Weet u zeker dat u het project wilt afsluiten?",
			yesText: "Bevestig",
			noText: "Annuleren",
			options: new DialogOptions { CloseOnEscapeKey = true }
		);

		if (result == true)
		{
			await Close();
		}
	}

	private async Task ShowStartConfirmationAsync()
	{
		bool? result = await DialogService.ShowMessageBox(
			"Bevestiging",
			"Weet u zeker dat u het project wilt opstarten?",
			yesText: "Bevestig",
			noText: "Annuleren",
			options: new DialogOptions { CloseOnEscapeKey = true }
		);

		if (result == true)
		{
			await Start();
		}
	}


	//NAV
	private void NavigateToProjects() {
		Navigation.NavigateTo("/projects/");
	}
}