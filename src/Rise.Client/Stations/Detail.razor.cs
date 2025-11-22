using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class Detail {
	// ===== Tabs =====
	private readonly List<string> _tabs = new() {
		"Info",
		"Apparaten",
		"Projecten",
		"Historiek"
	};

	private bool _loaded;
	private List<ProjectDto.Index>? _projectsFromMeasurementStation;
	private int _selectedTab;
	private IEnumerable<ProjectDto.Index>? _allProjects;
	private Result? _result;
	private MudDataGrid<ProjectDto.Index>? _projectsGrid;

	// ===== Main =====
	private MeasurementStationDto.Detail? _station;
	[Parameter] public Guid StationId { get; set; }
	[Inject] public required IMeasurementStationService StationService { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; } = default!;
	private MeasurementStationRequest.UpdateProjects Model { get; } = new();

	private async Task SetTabAsync(int index) {
		_selectedTab = index;

		if (index == 2 && _projectsFromMeasurementStation is null) {
			Result<List<ProjectDto.Index>> res = await StationService.GetProjectsAsync(StationId);
			_projectsFromMeasurementStation = res.IsSuccess ? res.Value : new List<ProjectDto.Index>();
			StateHasChanged();
		}
	}

	protected override async Task OnInitializedAsync() {
		Result<MeasurementStationResponse.Detail> result = await StationService.GetDetailAsync(StationId);
		Result<ProjectResponse.Index> allProjects = await ProjectService.GetIndexAsync(new QueryRequest.SkipTake());
		_loaded = true;

		if (result.IsSuccess) {
			_station = result.Value.Station;

			_tabs[2] = $"Projecten ({_station.ProjectsCount})";
		}

		// set breadcrumbs
		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Home", "/"),
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
		
		// --- All projects that can be added to the measurement station ---
		if (allProjects.IsSuccess && allProjects.Value is not null) {
			_allProjects = allProjects.Value.Projects;
		} else {
			if (!allProjects.IsSuccess) {
				string message = allProjects.Errors != null
					? string.Join(Environment.NewLine, allProjects.Errors)
					: "Kon projecten niet laden.";
				Snackbar.Add(message, Severity.Warning);
			}
		}

		Model.MeasurementStationId = StationId;
	}

	private Task<GridData<ProjectDto.Index>> GetProjectsGridAsync(GridState<ProjectDto.Index> state) {
		if (_projectsFromMeasurementStation is null) {
			return LoadAndSliceAsync(state);
		}

		List<ProjectDto.Index> items = _projectsFromMeasurementStation
			.Skip(state.Page * state.PageSize)
			.Take(state.PageSize)
			.ToList();

		return Task.FromResult(new GridData<ProjectDto.Index> {
			TotalItems = _projectsFromMeasurementStation.Count,
			Items = items
		});
	}

	private async Task<GridData<ProjectDto.Index>> LoadAndSliceAsync(GridState<ProjectDto.Index> state) {
		Result<List<ProjectDto.Index>> res = await StationService.GetProjectsAsync(StationId);
		_projectsFromMeasurementStation = res.IsSuccess ? res.Value : new List<ProjectDto.Index>();
		Model.ProjectIds = _projectsFromMeasurementStation.Select(s => s.Id).ToList();

		List<ProjectDto.Index> items = _projectsFromMeasurementStation
			.Skip(state.Page * state.PageSize)
			.Take(state.PageSize)
			.ToList();

		return new GridData<ProjectDto.Index> {
			TotalItems = _projectsFromMeasurementStation.Count,
			Items = items
		};
	}
	
	private async Task UpdateProjects() {
		bool selectionChanged = !Model.ProjectIds.SequenceEqual(
			_projectsFromMeasurementStation?.Select(s => s.Id) ?? Enumerable.Empty<Guid>());
		
		if (!selectionChanged) {
			Snackbar.Add("Je moet minstens 1 project selecteren.", Severity.Error);
		} else {
			_result = await StationService.UpdateProjectsAsync(Model);
			if (_result.IsSuccess) {
				Snackbar.Add("Projecten succesvol toegevoegd!", Severity.Success);
				Result<List<ProjectDto.Index>> projectsResult = await StationService.GetProjectsAsync(StationId);
				if (projectsResult.IsSuccess && projectsResult.Value is not null)
				{
					_projectsFromMeasurementStation = projectsResult.Value;
					_tabs[2] = $"Projecten ({_projectsFromMeasurementStation.Count})";
					StateHasChanged();
				}
				
				if (_projectsGrid is not null)
					await _projectsGrid.ReloadServerData();
			} else {
				Snackbar.Add(string.Join("\n", _result.Errors), Severity.Error);
			}
		}
	}

	private void OnProjectRowClick(DataGridRowClickEventArgs<ProjectDto.Index> args) {
		Navigation.NavigateTo($"/projects/{args.Item.Id}");
	}

	private static string FormatDate(DateTime? dt) {
		return dt is null ? "-" : dt.Value.ToShortDateString();
	}
	
	private void NavigateToCreateProject() {
		Navigation.NavigateTo($"/projects/create/{StationId}");
	}
}