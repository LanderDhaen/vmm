using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class Index {
	[Inject] public required StationService StationService { get; set; }

	private List<MeasurementStationDto.Index>? _mobileStations;
	private int _mobileTotal;
	protected override async Task OnInitializedAsync() {
		await base.OnInitializedAsync();
		await FetchMobileItemsAsync(); // uses default page = 0, pageSize = 10
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
	private static QueryRequest.SkipTake BuildRequest(int skip, int take, string? orderBy)
	=> new() {
		Skip = skip,
		Take = take,
		OrderBy = orderBy,
	};
	// --- Mobile data (cards) --------------------------------------------

	// Default parameter values make fields unnecessary; call without parameters for page 0 / size 10
	private async Task FetchMobileItemsAsync(int page = 0, int pageSize = 10, string? orderBy = null) {
		try {
			var req = BuildRequest(page * pageSize, pageSize, orderBy);
			var result = await StationService.GetIndexAsync(req);

			if (result?.Value is not null) {
				_mobileStations = result.Value.Stations?.ToList() ?? new List<MeasurementStationDto.Index>();
				_mobileTotal = result.Value.TotalCount;
			} else {
				Log.Warning("FetchMobileItemsAsync: service returned null or empty result");
				_mobileStations = new List<MeasurementStationDto.Index>();
				_mobileTotal = 0;
			}
		} catch (Exception ex) {
			Log.Error(ex, "Failed to fetch mobile project items");
			_mobileStations = new List<MeasurementStationDto.Index>();
			_mobileTotal = 0;
		}

		await InvokeAsync(StateHasChanged);
	}
}