using System.Net.Http.Json;

using Rise.Shared.Common;
using Rise.Shared.Networks;
using Rise.Shared.Products;

namespace Rise.Client.Networks;

public class NetworkService(HttpClient httpClient) : INetworkService {
	public async Task<Result<NetworkResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request,
		CancellationToken ctx = default) {
		Result<NetworkResponse.Index>? result = await httpClient.GetFromJsonAsync<Result<NetworkResponse.Index>>("/api/measurementnetworks", ctx);
		return result!;
	}
}