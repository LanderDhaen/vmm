using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Client.Projects;

public partial class MeasurementStations {
	private IEnumerable<MeasurementStationDto.Index>? _stationsFromProject;

	[Parameter] public Guid ProjectId { get; set; }
	[Parameter] public bool IsActive { get; set; }

	[Inject] private ISnackbar Snackbar { get; set; } = default!;
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required NavigationManager Navigation { get; set; }

	protected override async Task OnInitializedAsync() {
		Result<List<MeasurementStationDto.Index>>
			stationsFromProject = await ProjectService.GetStationsAsync(ProjectId);

		// --- Stations for this project ---
		if (stationsFromProject.IsSuccess && stationsFromProject.Value is not null) {
			_stationsFromProject = stationsFromProject.Value;
		} else {
			_stationsFromProject = new List<MeasurementStationDto.Index>();

			if (!stationsFromProject.IsSuccess) {
				string message = stationsFromProject.Errors != null
					? string.Join(Environment.NewLine, stationsFromProject.Errors)
					: "Kon meetstations niet laden.";
				Snackbar.Add(message, Severity.Warning);
			}
		}
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
}