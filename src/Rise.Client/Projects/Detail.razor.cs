using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Client.Projects;

public partial class Detail {
	// ====== Tabs ======
	private readonly List<string> _tabs = new() {
		"Info",
		"Historiek"
	};

	// Drawer
	private bool _open;

	// Overlay
	private bool _visible;

	// Radio button
	private string _selectedOption = "Bestaand";

	// ====== Main ======
	private ProjectDto.Detail? _project;
	private IEnumerable<MeasurementStationDto.Index>? _stationsFromProject;
	private IEnumerable<MeasurementStationDto.Index>? _allStations = new List<MeasurementStationDto.Index>();
	private Result? _result;

	private string? _remarks;
	private int _selectedTab;

	[Parameter] public Guid ProjectId { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required IMeasurementStationService StationService { get; set; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; } = default!;
	[Inject] private IDialogService DialogService { get; set; } = default!;
	private ProjectRequest.UpdateMeasurementStations Model { get; } = new();
	private Color BadgeColor => _project?.Status == Status.Actief ? Color.Success : Color.Error;
	private bool IsActive => _project?.Status == Status.Actief;

	private void SetTab(int index) {
		_selectedTab = index;
	}

	protected override async Task OnInitializedAsync() {
		Result<ProjectResponse.Detail> detail = await ProjectService.GetDetailAsync(ProjectId);
		Result<ProjectResponse.MeasurementStations> stationsFromProject = await ProjectService.GetStationsAsync(ProjectId);

		// --- Project detail ---
		if (detail.IsSuccess) {
			_project = detail.Value.Project;

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
		} else {
			string message = detail.Errors != null
				? string.Join(Environment.NewLine, detail.Errors)
				: "Er is een fout opgetreden.";
			Snackbar.Add(message, Severity.Error);
		}
		
		// --- Stations for this project ---
		if (stationsFromProject.IsSuccess && stationsFromProject.Value is not null) {
			_stationsFromProject = stationsFromProject.Value.MeasurementStationsFromProject;
			Model.MeasurementStationIds = _stationsFromProject.Select(s => s.Id).ToList();
		} else {
			_stationsFromProject = new List<MeasurementStationDto.Index>();

			string message = stationsFromProject.Errors != null
				? string.Join(Environment.NewLine, stationsFromProject.Errors)
				: "Kon meetstations niet laden.";
			Snackbar.Add(message, Severity.Warning);
		}
		Model.ProjectId = ProjectId;
	}
	
	private async Task OpenOverlay() {
		_visible = true;
		
		if (_allStations is null || !_allStations.Any()) {
			Result<MeasurementStationResponse.Index> allStations = await StationService.GetIndexAsync(new QueryRequest.SkipTake());
		
			// --- Load in all stations for select ---
			if (allStations.IsSuccess && allStations.Value is not null) {
				_allStations = allStations.Value.Stations;
			} else {
				string message = allStations.Errors != null
					? string.Join(Environment.NewLine, allStations.Errors)
					: "Kon meetstations niet laden.";
				Snackbar.Add(message, Severity.Warning);
			}
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

	private async Task UpdateMeasurementStations() {
		bool selectionChanged = !Model.MeasurementStationIds.SequenceEqual(
			_stationsFromProject?.Select(s => s.Id) ?? Enumerable.Empty<Guid>());

		if (!selectionChanged) {
			Snackbar.Add("Je moet minstens 1 meetstation selecteren.", Severity.Error);
			return;
		} 
		
		_result = await ProjectService.UpdateMeasurementStationsAsync(Model);
		if (_result.IsSuccess) {
			Snackbar.Add("Meetstations succesvol bijgewerkt!", Severity.Success);
			Result<ProjectResponse.MeasurementStations> stationsResult =
				await ProjectService.GetStationsAsync(ProjectId);
			if (stationsResult.IsSuccess && stationsResult.Value is not null) {
				_stationsFromProject = stationsResult.Value.MeasurementStationsFromProject;
			}

			_visible = false;
		} else {
			Snackbar.Add(string.Join("\n", _result.Errors), Severity.Error);
		}
	}
}