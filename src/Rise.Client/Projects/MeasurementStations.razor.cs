using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Client.Projects;

public partial class MeasurementStations {
	private CancellationTokenSource _cancellationTokenSource = new();
	private string _searchTerm = string.Empty;
	
	[Parameter] public Guid ProjectId { get; set; }
	[Parameter] public IEnumerable<MeasurementStationDto.Index>? Stations { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required NavigationManager Navigation { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; } = default!;
	
	private async Task GetStationsAsync() {
		_cancellationTokenSource.Cancel();
		_cancellationTokenSource.Dispose();
		_cancellationTokenSource = new CancellationTokenSource();

		QueryRequest.Search request = new() {
			SearchTerm = _searchTerm
		};

		Result<ProjectResponse.MeasurementStations> stationsFromProject =
			await ProjectService.GetStationsAsync(ProjectId, request, _cancellationTokenSource.Token);

		// --- Stations for this project ---
		if (stationsFromProject.IsSuccess && stationsFromProject.Value is not null) {
			Stations = stationsFromProject.Value.MeasurementStationsFromProject;
		} else {
			Stations = new HashSet<MeasurementStationDto.Index>();

			string message = stationsFromProject.Errors != null
				? string.Join(Environment.NewLine, stationsFromProject.Errors)
				: "Kon meetstations niet laden.";
			Snackbar.Add(message, Severity.Warning);
		}
	}
	
	private async Task UpdateSearch(string searchTerm) {
		_searchTerm = searchTerm;
		await GetStationsAsync();
	}

	private void OnStationRowClick(TableRowClickEventArgs<MeasurementStationDto.Index> args) {
		Navigation.NavigateTo($"/stations/{args.Item.Id}");
	}

	private void OnStationCardClick(MeasurementStationDto.Index station) {
		Navigation.NavigateTo($"/stations/{station.Id}");
	}
}