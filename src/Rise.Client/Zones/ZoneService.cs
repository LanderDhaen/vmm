using System.Net.Http.Json;

using Rise.Shared.Common;
using Rise.Shared.Zones;

namespace Rise.Client.Zones;

public class ZoneService(HttpClient httpClient) : IZoneService {
	public async Task<Result<ZoneResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request,
		CancellationToken ctx = default) {
		Result<ZoneResponse.Index>? result =
			await httpClient.GetFromJsonAsync<Result<ZoneResponse.Index>>("/api/zones", ctx);
		return result!;
	}
}