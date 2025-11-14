using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class Index {
	[Inject] public required StationService StationService { get; set; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }

	protected override void OnInitialized() {
		// update breadcrumbs in de service
		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Home", "/"),
			new BreadcrumbItem("Meetstations", null, true)
		);
	}

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
}