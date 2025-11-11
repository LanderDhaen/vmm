using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class Detail {
	// ===== Breadcrumbs =====
	private readonly List<BreadcrumbItem> _items = new();

	// ===== Tabs =====
	private readonly List<string> _tabs = new() {
		"Info",
		"Apparaten",
		"Projecten",
		"Historiek"
	};

	private bool _loaded;
	private List<ProjectDto.Index>? _projects;
	private int _selectedTab;

	// ===== Main =====
	private MeasurementStationDto.Detail? _station;

	[Parameter] public Guid StationId { get; set; }

	[Inject] public required StationService StationService { get; set; }

	private async Task SetTabAsync(int index) {
		_selectedTab = index;

		if (index == 2 && _projects is null) {
			Result<List<ProjectDto.Index>> res = await StationService.GetProjectsAsync(StationId);
			_projects = res.IsSuccess ? res.Value : new List<ProjectDto.Index>();
			StateHasChanged();
		}
	}

	protected override async Task OnInitializedAsync() {
		Result<MeasurementStationResponse.Detail> result = await StationService.GetDetailAsync(StationId);
		_loaded = true;

		if (result.IsSuccess) {
			_station = result.Value.Station;

			_items.Clear();
			_items.Add(new BreadcrumbItem("", "/", icon: Icons.Material.Filled.Home));
			_items.Add(new BreadcrumbItem("Meetstations", "/stations"));
			_items.Add(new BreadcrumbItem(_station.Code, null, true));

			_tabs[2] = $"Projecten ({_station.ProjectsCount})";
		}
	}

	private void NavigateToStations() {
		Navigation.NavigateTo("/stations/");
	}

	private Task<GridData<ProjectDto.Index>> GetProjectsGridAsync(GridState<ProjectDto.Index> state) {
		if (_projects is null) {
			return LoadAndSliceAsync(state);
		}

		List<ProjectDto.Index> items = _projects
			.Skip(state.Page * state.PageSize)
			.Take(state.PageSize)
			.ToList();

		return Task.FromResult(new GridData<ProjectDto.Index> {
			TotalItems = _projects.Count,
			Items = items
		});
	}

	private async Task<GridData<ProjectDto.Index>> LoadAndSliceAsync(GridState<ProjectDto.Index> state) {
		Result<List<ProjectDto.Index>> res = await StationService.GetProjectsAsync(StationId);
		_projects = res.IsSuccess ? res.Value : new List<ProjectDto.Index>();

		List<ProjectDto.Index> items = _projects
			.Skip(state.Page * state.PageSize)
			.Take(state.PageSize)
			.ToList();

		return new GridData<ProjectDto.Index> {
			TotalItems = _projects.Count,
			Items = items
		};
	}

	private void OnProjectRowClick(DataGridRowClickEventArgs<ProjectDto.Index> args) {
		Navigation.NavigateTo($"/projects/{args.Item.Id}");
	}

	private static string FormatDate(DateTime? dt) {
		return dt is null ? "-" : dt.Value.ToShortDateString();
	}

	private static string FormatStatus(string? raw) {
		return raw switch {
			"Active" or "Actief" => "Actief",
			"Inactive" or "Inactief" => "Inactief",
			_ => raw ?? "-"
		};
	}
}