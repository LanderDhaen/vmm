using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Client.Projects;

public partial class Detail {
	// ====== Tabs ======
	private readonly List<string> _tabs = new() {
		"Info",
		"Meetstations",
		"Historiek"
	};

	// Drawer
	private bool _open;

	// ====== Main ======
	private ProjectDto.Detail? _project;

	private string? _remarks;
	private int _selectedTab;
	private List<MeasurementStationDto.Index>? _stations;

	[Parameter] public Guid ProjectId { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; } = default!;
	[Inject] private IDialogService DialogService { get; set; } = default!;
	private Color BadgeColor => _project?.Status == Status.Actief ? Color.Success : Color.Error;
	private bool IsActive => _project?.Status == Status.Actief;

	private void SetTab(int index) {
		_selectedTab = index;
	}

	protected override async Task OnInitializedAsync() {
		// Load project details and stations in parallel
		Task<Result<ProjectResponse.Detail>> detailTask = ProjectService.GetDetailAsync(ProjectId);
		Task<Result<List<MeasurementStationDto.Index>>> stationsTask = ProjectService.GetStationsAsync(ProjectId);

		await Task.WhenAll(detailTask, stationsTask);

		Result<ProjectResponse.Detail> detailResult = detailTask.Result;
		Result<List<MeasurementStationDto.Index>> stationsResult = stationsTask.Result;

		// --- Project detail ---
		if (detailResult.IsSuccess) {
			_project = detailResult.Value.Project;

			// set breadcrumbs
			BreadcrumbService.SetBreadcrumbs(
				new BreadcrumbItem("Home", "/"),
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
			string message = detailResult.Errors != null
				? string.Join(Environment.NewLine, detailResult.Errors)
				: "Er is een fout opgetreden.";
			Snackbar.Add(message, Severity.Error);
			return;
		}

		// --- Stations for this project (for tab count & grid) ---
		if (stationsResult.IsSuccess && stationsResult.Value is not null) {
			_stations = stationsResult.Value;
			_tabs[1] = $"Meetstations ({_stations.Count})";
		} else {
			_stations = new List<MeasurementStationDto.Index>();
			_tabs[1] = "Meetstations (0)";

			if (!stationsResult.IsSuccess) {
				string message = stationsResult.Errors != null
					? string.Join(Environment.NewLine, stationsResult.Errors)
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

	private Task<GridData<MeasurementStationDto.Index>> GetStationsGridAsync(
		GridState<MeasurementStationDto.Index> state) {
		if (_stations is null) {
			return LoadAndSliceStationsAsync(state);
		}

		List<MeasurementStationDto.Index> items = _stations
			.Skip(state.Page * state.PageSize)
			.Take(state.PageSize)
			.ToList();

		return Task.FromResult(new GridData<MeasurementStationDto.Index> {
			TotalItems = _stations.Count,
			Items = items
		});
	}

	private async Task<GridData<MeasurementStationDto.Index>> LoadAndSliceStationsAsync(
		GridState<MeasurementStationDto.Index> state) {
		if (_stations is null) {
			Result<List<MeasurementStationDto.Index>> res =
				await ProjectService.GetStationsAsync(ProjectId);

			_stations = res.IsSuccess && res.Value is not null
				? res.Value
				: new List<MeasurementStationDto.Index>();

			_tabs[1] = $"Meetstations ({_stations.Count})";
		}

		List<MeasurementStationDto.Index> items = _stations
			.Skip(state.Page * state.PageSize)
			.Take(state.PageSize)
			.ToList();

		return new GridData<MeasurementStationDto.Index> {
			TotalItems = _stations.Count,
			Items = items
		};
	}

	private void OnStationRowClick(DataGridRowClickEventArgs<MeasurementStationDto.Index> args) {
		Navigation.NavigateTo($"/stations/{args.Item.Id}");
	}

	private static string FormatDate(DateTime? dt) {
		return dt is null ? "-" : dt.Value.ToShortDateString();
	}

	//NAV
	private void NavigateToProjects() {
		Navigation.NavigateTo("/projects/");
	}
}