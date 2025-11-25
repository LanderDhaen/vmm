using System.Net.Http.Json;

using Rise.Shared.Common;
using Rise.Shared.Locations;

namespace Rise.Client.Locations;

public class LocationService(HttpClient httpClient) : ILocationService {
	public async Task<Result<LocationResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request,
		CancellationToken ctx = default) {
		Result<LocationResponse.Index>? result =
			await httpClient.GetFromJsonAsync<Result<LocationResponse.Index>>($"/api/locations?{request.AsQuery()}", ctx);
		return result!;
	}
}