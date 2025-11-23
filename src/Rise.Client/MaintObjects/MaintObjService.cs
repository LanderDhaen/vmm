using System.Net.Http.Json;

using Rise.Shared.Common;
using Rise.Shared.MaintObjects;

namespace Rise.Client.MaintObjects;

public class MaintObjService(HttpClient httpClient) : IMaintObjService {
	public async Task<Result<MaintObjResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request,
		CancellationToken ctx = default) {
		return (await httpClient.GetFromJsonAsync<Result<MaintObjResponse.Index>>(
			$"/api/maintobjects?{request.AsQuery()}", ctx))!;
	}
}