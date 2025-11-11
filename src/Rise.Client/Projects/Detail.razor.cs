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

	// Backing fields for MudTextField bindings (read-only in UI)
	private string _code = string.Empty;
	private string? _endDate = string.Empty;
	private string _measurementNetwork = string.Empty;
	private string _name = string.Empty;

	// Drawer
	private bool _open;

	// ====== Main ======
	private ProjectDto.Detail? _project;

	private string? _remarks;
	private int _selectedTab;
	private string _startDate = string.Empty;
	private string _zone = string.Empty;
	[Parameter] public Guid ProjectId { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; } = default!;
	[Inject] private IDialogService DialogService { get; set; } = default!;
	private Color BadgeColor => _project?.Status == Status.Actief ? Color.Success : Color.Error;
	private bool IsActive => _project?.Status == Status.Actief;

	private void UpdateBoundFields() {
		if (_project is null) {
			_code = string.Empty;
			_name = string.Empty;
			_startDate = string.Empty;
			_endDate = string.Empty;
			_measurementNetwork = string.Empty;
			_zone = string.Empty;
			_remarks = string.Empty;
			return;
		}

		_code = _project.Code ?? string.Empty;
		_name = _project.Name ?? string.Empty;
		// DateOnly formatting - fall back to empty when null
		_startDate = _project.StartDate.ToString();
		_endDate = _project.EndDate?.ToString() ?? string.Empty;
		_measurementNetwork = _project.MeasurementNetwork ?? string.Empty;
		_zone = _project.Zone ?? string.Empty;
		// ProjectDto.Detail does not include remarks; keep remarks empty by default; could be extended later
		_remarks = string.Empty;
	}

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
			// Initialize bound fields
			UpdateBoundFields();
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
				_project.EndDate = DateOnly.FromDateTime(DateTime.UtcNow);
				// refresh the bound end date
				_endDate = _project.EndDate?.ToString() ?? string.Empty;
			}

			Snackbar.Add("Project succesvol afgesloten!", Severity.Success);
			_open = false;

			StateHasChanged();
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
				_project.EndDate = null;
				// refresh the bound end date
				_endDate = string.Empty;
			}

			Snackbar.Add("Project succesvol opgestart!", Severity.Success);
			_open = false;

			StateHasChanged();
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


	// Show confirmation dialogs
	private async Task ShowCloseConfirmationAsync() {
		bool? result = await DialogService.ShowMessageBox(
			"Bevestiging",
			"Weet u zeker dat u het project wilt afsluiten?",
			"Bevestig",
			"Annuleren",
			options: new DialogOptions { CloseOnEscapeKey = true }
		);

		if (result == true) {
			await Close();
		}
	}

	private async Task ShowStartConfirmationAsync() {
		bool? result = await DialogService.ShowMessageBox(
			"Bevestiging",
			"Weet u zeker dat u het project wilt opstarten?",
			"Bevestig",
			"Annuleren",
			options: new DialogOptions { CloseOnEscapeKey = true }
		);

		if (result == true) {
			await Start();
		}
	}


	//NAV
	private void NavigateToProjects() {
		Navigation.NavigateTo("/projects/");
	}
}