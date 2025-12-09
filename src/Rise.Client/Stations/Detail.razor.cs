using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Components.Stations;
using Rise.Client.Services;
using Rise.Shared.MaintObjects;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class Detail {
	// ===== Tabs =====
	private readonly List<string> _tabs = [
		"Informatie",
		"Historiek"
	];

	private IEnumerable<MaintObjDto.Index>? _maintObjectsFromStation;
	private IEnumerable<ProjectDto.Index>? _projectsFromStation;

	private int _selectedTab;

	// ===== Main =====
	private MeasurementStationDto.Detail? _station;

	[Parameter] public Guid StationId { get; set; }
	[Inject] public required IMeasurementStationService StationService { get; set; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; } = default!;
	[Inject] private IDialogService DialogService { get; set; } = default!;

	private void SetTabAsync(int index) {
		_selectedTab = index;
	}

	protected override async Task OnInitializedAsync() {
		await RefreshStationDetails();
		await RefreshProjects();
		await RefreshMaintObjects();

		// set breadcrumbs
		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Meetstations", "/stations"),
			new BreadcrumbItem($"{_station?.Code}", null, true)
		);

		// set header
		HeaderService.Set(
			"Detail",
			Icons.Material.Filled.ArrowBack,
			() => {
				Navigation.NavigateTo("/stations/");
				return Task.CompletedTask;
			}
		);
	}

	private async Task OpenDialogAsync() {
		DialogParameters<UpdateProjectListDialog> parameters = new() {
			{ x => x.StationId, StationId },
			{ x => x.Projects, _projectsFromStation }
		};
		DialogOptions options = new() { CloseOnEscapeKey = true, CloseButton = true };

		IDialogReference dialog =
			await DialogService.ShowAsync<UpdateProjectListDialog>("Simple Dialog", parameters, options);
		DialogResult? result = await dialog.Result;

		if (result is { Canceled: false }) {
			await RefreshProjects();
		}
	}

	private async Task OpenMaintObjectsListDialogAsync() {
		DialogParameters<UpdateMaintObjectsListDialog> parameters = new() {
			{ x => x.StationId, StationId },
			{ x => x.MaintObjects, _maintObjectsFromStation }
		};
		DialogOptions options = new() { CloseOnEscapeKey = true, CloseButton = true };

		IDialogReference dialog =
			await DialogService.ShowAsync<UpdateMaintObjectsListDialog>("Simple Dialog", parameters, options);
		DialogResult? result = await dialog.Result;

		if (result is { Canceled: false }) {
			await RefreshMaintObjects();
		}
	}

	private async Task OpenUpdateLocationDialogAsync() {
		DialogParameters<UpdateLocationDialog> parameters = new() {
			{ x => x.StationId, StationId },
			{ x => x.LocationId, _station!.Location.Id }
		};
		DialogOptions options = new() { CloseOnEscapeKey = true, CloseButton = true, MaxWidth = MaxWidth.ExtraLarge };

		IDialogReference dialog =
			await DialogService.ShowAsync<UpdateLocationDialog>("Locatie wijzigen", parameters, options);
		DialogResult? result = await dialog.Result;

		if (result is { Canceled: false }) {
			await RefreshStationDetails();
		}
	}

	private async Task OpenEditStationDialogAsync() {
		DialogParameters<EditFormDialog> parameters = new() {
			{ x => x.StationId, StationId },
			{ x => x.Station, _station }
		};
		DialogOptions options = new() { CloseOnEscapeKey = true, CloseButton = true, MaxWidth = MaxWidth.ExtraLarge };

		IDialogReference dialog =
			await DialogService.ShowAsync<EditFormDialog>("Station bewerken", parameters, options);
		DialogResult? result = await dialog.Result;

		if (!result.Canceled) {
			await RefreshStationDetails();
		}
	}

	private async Task RefreshProjects() {
		Result<MeasurementStationResponse.ProjectList> projectsResult =
			await StationService.GetProjectsAsync(StationId);

		if (projectsResult.IsSuccess) {
			_projectsFromStation = projectsResult.Value.Projects;
		}

		StateHasChanged();
	}

	private async Task RefreshMaintObjects() {
		Result<MeasurementStationResponse.MaintObjList> maintObjectsResult =
			await StationService.GetMaintObjsAsync(StationId);

		if (maintObjectsResult.IsSuccess) {
			_maintObjectsFromStation = maintObjectsResult.Value.MaintObjects;
		}

		StateHasChanged();
	}

	private async Task RefreshStationDetails() {
		Result<MeasurementStationResponse.Detail> result = await StationService.GetDetailAsync(StationId);

		if (result.IsSuccess) {
			_station = result.Value.Station;
		} else {
			string message = result.Errors != null
				? string.Join(Environment.NewLine, result.Errors)
				: "Er is een fout opgetreden.";
			Snackbar.Add(message, Severity.Error);
		}
	}
}