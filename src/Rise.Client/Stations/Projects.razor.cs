using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class Projects {
	private IEnumerable<ProjectDto.Index>? _allProjects;
	private IEnumerable<ProjectDto.Index>? _projectsFromStation;
	private Result? _result;

	[Parameter] public Guid StationId { get; set; }
	[Parameter] public EventCallback<string> TabChanged { get; set; }


	private MeasurementStationRequest.UpdateProjects Model { get; } = new();
	[Inject] private ISnackbar Snackbar { get; set; } = default!;
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required IMeasurementStationService StationService { get; set; }
	[Inject] public required NavigationManager Navigation { get; set; }

	protected override async Task OnInitializedAsync() {
		Result<List<ProjectDto.Index>> projectsFromStation = await StationService.GetProjectsAsync(StationId);
		Result<ProjectResponse.Index> allProjects = await ProjectService.GetIndexAsync(new QueryRequest.SkipTake());

		// --- Projects from this station ---
		if (projectsFromStation.IsSuccess && projectsFromStation.Value is not null) {
			_projectsFromStation = projectsFromStation.Value;
			Model.ProjectIds = _projectsFromStation.Select(s => s.Id).ToList();
		} else {
			_projectsFromStation = new List<ProjectDto.Index>();

			if (!projectsFromStation.IsSuccess) {
				string message = projectsFromStation.Errors != null
					? string.Join(Environment.NewLine, projectsFromStation.Errors)
					: "Kon projecten niet laden.";
				Snackbar.Add(message, Severity.Warning);
			}
		}

		// --- All projects that can be added to the station ---
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

	private void OnProjectRowClick(TableRowClickEventArgs<ProjectDto.Index> args) {
		Navigation.NavigateTo($"/projects/{args.Item.Id}");
	}

	private void OnProjectCardClick(ProjectDto.Index project) {
		Navigation.NavigateTo($"/projects/{project.Id}");
	}

	private async Task UpdateProjects() {
		bool selectionChanged = !Model.ProjectIds.SequenceEqual(
			_projectsFromStation?.Select(s => s.Id) ?? Enumerable.Empty<Guid>());

		if (!selectionChanged) {
			Snackbar.Add("Je moet minstens 1 project selecteren.", Severity.Error);
		} else {
			_result = await StationService.UpdateProjectsAsync(Model);
			if (_result.IsSuccess) {
				Snackbar.Add("Projecten succesvol bijgewerkt!", Severity.Success);
				Result<List<ProjectDto.Index>> projectsResult = await StationService.GetProjectsAsync(StationId);
				if (projectsResult.IsSuccess && projectsResult.Value is not null) {
					_projectsFromStation = projectsResult.Value;
					await TabChanged.InvokeAsync($"Projecten ({_projectsFromStation.Count()})");
				}
			} else {
				Snackbar.Add(string.Join("\n", _result.Errors), Severity.Error);
			}
		}
	}

	private static Color DefineColor(Status status) {
		return status == Status.Actief ? Color.Success : Color.Error;
	}
}