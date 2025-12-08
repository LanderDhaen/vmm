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
	private readonly List<string> _tabs = new() {
		"Informatie",
		"Historiek"
	};

	private int _selectedTab;

	// ===== Main =====
	private MeasurementStationDto.Detail? _station;
	private IEnumerable<ProjectDto.Index>? _projectsFromStation;
	private IEnumerable<MaintObjDto.Index>? _maintObjectsFromStation;

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
			new BreadcrumbItem($"{_station.Code}", null, true)
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
		var parameters = new DialogParameters<UpdateProjectListDialog> {
			{ x => x.StationId, StationId },
			{ x => x.Projects, _projectsFromStation }
		};
		var options = new DialogOptions { CloseOnEscapeKey = true, CloseButton = true };

		var dialog = await DialogService.ShowAsync<UpdateProjectListDialog>("Simple Dialog", parameters, options);
		var result = await dialog.Result;

		if (!result.Canceled) {
			await RefreshProjects();
		}
	}
	
	private async Task OpenMaintObjectsListDialogAsync() {
		var parameters = new DialogParameters<UpdateMaintObjectsListDialog> {
			{ x => x.StationId, StationId },
			{ x => x.MaintObjects, _maintObjectsFromStation }
		};
		var options = new DialogOptions { CloseOnEscapeKey = true, CloseButton = true };

		var dialog = await DialogService.ShowAsync<UpdateMaintObjectsListDialog>("Simple Dialog", parameters, options);
		var result = await dialog.Result;

		if (!result.Canceled) {
			await RefreshMaintObjects();
		}
	}

	private async Task OpenUpdateLocationDialogAsync() {
		var parameters = new DialogParameters<UpdateLocationDialog> {
			{ x => x.StationId, StationId },
			{ x => x.LocationId, _station.Location.Id }
		};
		var options = new DialogOptions { CloseOnEscapeKey = true, CloseButton = true, MaxWidth = MaxWidth.ExtraLarge };

		var dialog = await DialogService.ShowAsync<UpdateLocationDialog>("Locatie wijzigen", parameters, options);
		var result = await dialog.Result;

		if (!result.Canceled) {
			await RefreshStationDetails();
		}
	}

	private async Task OpenEditStationDialogAsync() {
		var parameters = new DialogParameters<EditFormDialog> {
			{ x => x.StationId, StationId },
			{ x => x.Station, _station }
		};
		var options = new DialogOptions { CloseOnEscapeKey = true, CloseButton = true, MaxWidth = MaxWidth.ExtraLarge };

		var dialog = await DialogService.ShowAsync<EditFormDialog>("Station bewerken", parameters, options);
		var result = await dialog.Result;

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