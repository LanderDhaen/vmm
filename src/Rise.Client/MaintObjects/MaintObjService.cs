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

	public async Task<Result<MaintObjResponse.Detail>>
		GetDetailAsync(Guid maintObjId, CancellationToken ctx = default) {
		Result<MaintObjResponse.Detail>? result =
			await httpClient.GetFromJsonAsync<Result<MaintObjResponse.Detail>>($"/api/maintObjects/{maintObjId}", ctx);
		return result!;
	}
}