using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Rise.Shared.Common;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class Index
{
	[Inject] public required StationService StationService { get; set; }

	private async Task<GridData<MeasurementStationDto.Index>> GetStationsAsync(
		GridState<MeasurementStationDto.Index> state)
	{
		var orderBy = state.SortDefinitions?.FirstOrDefault()?.SortBy ?? "Code";

		var req = new QueryRequest.SkipTake
		{
			Skip = state.Page * state.PageSize,
			Take = state.PageSize,
			OrderBy = orderBy
		};

		var result = await StationService.GetIndexAsync(req);
		var value = result.Value ?? new MeasurementStationResponse.Index();

		return new GridData<MeasurementStationDto.Index>
		{
			Items = value.Stations,
			TotalItems = value.TotalCount
		};
	}
}