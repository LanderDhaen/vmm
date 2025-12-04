using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Components.Projects;
using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Client.Projects;

public partial class Detail {
	// ====== Tabs ======
	private readonly List<string> _tabs = ["Informatie", "Historiek"];

	// Drawer
	private bool _open;

	// Overlay
	private readonly bool _visible;

	// ====== Main ======
	private ProjectDto.Detail? _project;
	private IEnumerable<MeasurementStationDto.Index>? _stationsFromProject;

	private int _selectedTab;

	[Parameter] public Guid ProjectId { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; } = default!;
	[Inject] private IDialogService DialogService { get; set; } = default!;
	private bool IsActive => _project?.Status == Status.Actief;

	private async Task SetTabAsync(int index) {
		_selectedTab = index;
	}

	protected override async Task OnInitializedAsync() {
		await GetProjectDetail();
		await GetStationsAsync();

		if (_project is not null) {
			// set breadcrumbs
			BreadcrumbService.SetBreadcrumbs(
				new BreadcrumbItem("Projecten", "/projects"),
				new BreadcrumbItem($"{_project.Code} - {_project.Name}", null, true)
			);

			// set header
			HeaderService.Set(
				"Detail",
				Icons.Material.Filled.ArrowBack,
				() => {
					Navigation.NavigateTo("/projects/");
					return Task.CompletedTask;
				}
			);
		}
	}

	private async Task GetStationsAsync() {
		Result<ProjectResponse.MeasurementStations> stationsFromProject =
			await ProjectService.GetStationsAsync(ProjectId, new QueryRequest.Search());

		// --- Stations for this project ---
		if (stationsFromProject is { IsSuccess: true, Value: not null }) {
			_stationsFromProject = stationsFromProject.Value.MeasurementStationsFromProject;
		} else {
			_stationsFromProject = new List<MeasurementStationDto.Index>();

			string message = stationsFromProject.Errors != null
				? string.Join(Environment.NewLine, stationsFromProject.Errors)
				: "Kon meetstations niet laden.";
			Snackbar.Add(message, Severity.Warning);
		}
	}

	private async Task GetProjectDetail() {
		Result<ProjectResponse.Detail> detail = await ProjectService.GetDetailAsync(ProjectId);

		// --- Project detail ---
		if (detail.IsSuccess) {
			_project = detail.Value.Project;
		} else {
			string message = detail.Errors != null
				? string.Join(Environment.NewLine, detail.Errors)
				: "Er is een fout opgetreden.";
			Snackbar.Add(message, Severity.Error);
		}
	}

	private Task OpenDialogAsync() {
		var options = new DialogOptions { CloseOnEscapeKey = true };

		return DialogService.ShowAsync<UpdateStationListDialog>("Simple Dialog", options);
	}

	private async Task OpenUpdateMeasurementDialogAsync() {
		var parameters = new DialogParameters<UpdateMeasurementNetworkDialog> {
			{ x => x.ProjectId, ProjectId },
			{ x => x.MeasurementNetworkId, _project!.MeasurementNetwork.Id }
		};
		var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.ExtraLarge, CloseButton = true };

		var dialog = await DialogService.ShowAsync<UpdateMeasurementNetworkDialog>("Meetnet wijzigen", parameters, options);
		var result = await dialog.Result;

		if (!result.Canceled) {
			await RefreshProjectDetail();
		}
	}

	private async Task OpenEditFormDialogAsync() {
		var parameters = new DialogParameters<EditFormDialog> {
			{ x => x.ProjectId, ProjectId },
			{ x => x.Project, _project }
		};
		var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.ExtraLarge, CloseButton = true };

		var dialog = await DialogService.ShowAsync<EditFormDialog>("Project bewerken", parameters, options);
		var result = await dialog.Result;

		if (!result.Canceled) {
			await RefreshProjectDetail();
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
			"Bevestigen",
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
			"Bevestigen",
			"Annuleren",
			options: new DialogOptions { CloseOnEscapeKey = true }
		);

		if (result == true) {
			await Start();
		}
	}

	private async Task RefreshStations() {
		await GetStationsAsync();
		StateHasChanged();
	}

	private async Task RefreshProjectDetail() {
		await GetProjectDetail();
		StateHasChanged();
	}
}