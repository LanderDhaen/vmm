using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class Index {
	[Inject] public required StationService StationService { get; set; }

	private readonly List<BreadcrumbItem> _items =
	[
		new("Home", href: "#"),
		new("Meetstations", null, true),
	];


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