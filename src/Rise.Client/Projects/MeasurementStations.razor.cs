using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Client.Projects;

public partial class MeasurementStations {
	private IEnumerable<MeasurementStationDto.Index>? _allStations;
	private Result? _result;
	private IEnumerable<MeasurementStationDto.Index>? _stationsFromProject;

	[Parameter] public Guid ProjectId { get; set; }
	[Parameter] public bool IsActive { get; set; }
	[Parameter] public EventCallback<string> TabChanged { get; set; }


	private ProjectRequest.UpdateMeasurementStations Model { get; } = new();
	[Inject] private ISnackbar Snackbar { get; set; } = null!;
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required IMeasurementStationService StationService { get; set; }
	[Inject] public required NavigationManager Navigation { get; set; }

	protected override async Task OnInitializedAsync() {
		Result<List<MeasurementStationDto.Index>>
			stationsFromProject = await ProjectService.GetStationsAsync(ProjectId);
		Result<MeasurementStationResponse.Index> allStations =
			await StationService.GetIndexAsync(new QueryRequest.SkipTake());

		// --- Stations for this project ---
		if (stationsFromProject is { IsSuccess: true, Value: not null }) {
			_stationsFromProject = stationsFromProject.Value;
			Model.MeasurementStationIds = _stationsFromProject.Select(s => s.Id).ToList();
		} else {
			_stationsFromProject = new List<MeasurementStationDto.Index>();

			if (!stationsFromProject.IsSuccess) {
				string message = stationsFromProject.Errors != null
					? string.Join(Environment.NewLine, stationsFromProject.Errors)
					: "Kon meetstations niet laden.";
				Snackbar.Add(message, Severity.Warning);
			}
		}

		// --- All stations that can be added to the project ---
		if (allStations is { IsSuccess: true, Value: not null }) {
			_allStations = allStations.Value.Stations;
		} else {
			if (!allStations.IsSuccess) {
				string message = allStations.Errors != null
					? string.Join(Environment.NewLine, allStations.Errors)
					: "Kon meetstations niet laden.";
				Snackbar.Add(message, Severity.Warning);
			}
		}

		Model.ProjectId = ProjectId;
	}

	private void OnStationRowClick(TableRowClickEventArgs<MeasurementStationDto.Index> args) {
		Navigation.NavigateTo($"/stations/{args.Item.Id}");
	}

	private void OnStationCardClick(MeasurementStationDto.Index station) {
		Navigation.NavigateTo($"/stations/{station.Id}");
	}

	private static string FormatDate(DateTime? dt) {
		return dt is null ? "-" : dt.Value.ToShortDateString();
	}

	private async Task UpdateMeasurementStations() {
		bool selectionChanged = !Model.MeasurementStationIds.SequenceEqual(
			_stationsFromProject?.Select(s => s.Id) ?? Enumerable.Empty<Guid>());

		if (!selectionChanged) {
			Snackbar.Add("Je moet minstens 1 meetstation selecteren.", Severity.Error);
		} else {
			_result = await ProjectService.UpdateMeasurementStationsAsync(Model);
			if (_result.IsSuccess) {
				Snackbar.Add("Meetstations succesvol bijgewerkt!", Severity.Success);
				Result<List<MeasurementStationDto.Index>> stationsResult =
					await ProjectService.GetStationsAsync(ProjectId);
				if (stationsResult is { IsSuccess: true, Value: not null }) {
					_stationsFromProject = stationsResult.Value;
					await TabChanged.InvokeAsync($"Meetstations ({_stationsFromProject.Count()})");
				}
			} else {
				Snackbar.Add(string.Join("\n", _result.Errors), Severity.Error);
			}
		}
	}
}