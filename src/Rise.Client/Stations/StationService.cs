using System.Net.Http.Json;
using Rise.Shared.Common;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public class StationService(HttpClient httpClient)
{
	public async Task<Result<MeasurementStationResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request,
		CancellationToken ctx = default)
	{
		Result<MeasurementStationResponse.Index>? result =
			await httpClient.GetFromJsonAsync<Result<MeasurementStationResponse.Index>>(
				$"/api/stations?{request.AsQuery()}", ctx);

		return result!;
	}
}