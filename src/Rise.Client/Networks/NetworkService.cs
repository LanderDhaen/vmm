using System.Net.Http.Json;

using Rise.Shared.Common;
using Rise.Shared.Networks;

namespace Rise.Client.Networks;

public class NetworkService(HttpClient httpClient) : INetworkService {
	public async Task<Result<NetworkResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request,
		CancellationToken ctx = default) {
		Result<NetworkResponse.Index>? result =
			await httpClient.GetFromJsonAsync<Result<NetworkResponse.Index>>(
				$"/api/measurementnetworks?{request.AsQuery()}", ctx);
		return result!;
	}
}