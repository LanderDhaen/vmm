using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class Index {
	[Inject] public required StationService StationService { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; set; }

	private async Task<GridData<MeasurementStationDto.Index>> GetStationsAsync(
		GridState<MeasurementStationDto.Index> state) {
		QueryRequest.SkipTake request = new() {
			Skip = state.Page * state.PageSize,
			Take = state.PageSize,
			OrderBy = "Id"
		};

		Result<MeasurementStationResponse.Index> result = await StationService.GetIndexAsync(request);

		return new GridData<MeasurementStationDto.Index> {
			TotalItems = result.Value.TotalCount,
			Items = result.Value.Stations
		};
	}
	private void OnRowClick(DataGridRowClickEventArgs<MeasurementStationDto.Index> e) {
		NavigationManager.NavigateTo($"/stations/{e.Item.Id}");
	}
}