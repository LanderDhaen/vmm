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

	public async Task<Result<MaintObjResponse.Create>> CreateAsync(MaintObjRequest.Create request,
		CancellationToken ctx = default) {
		HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/maintobjects", request, ctx);
		Result<MaintObjResponse.Create>? result =
			await response.Content.ReadFromJsonAsync<Result<MaintObjResponse.Create>>(ctx);
		return result!;
	}

	public async Task<Result> EditAsync(MaintObjRequest.Edit request, CancellationToken ctx = default) {
		HttpResponseMessage response =
			await httpClient.PutAsJsonAsync($"/api/maintobjects/{request.MaintObjId}", request, ctx);
		Result? result = await response.Content.ReadFromJsonAsync<Result>(ctx);
		return result!;
	}

	public async Task<Result<MaintObjResponse.Detail>>
		GetDetailAsync(Guid maintObjId, CancellationToken ctx = default) {
		Result<MaintObjResponse.Detail>? result =
			await httpClient.GetFromJsonAsync<Result<MaintObjResponse.Detail>>($"/api/maintObjects/{maintObjId}", ctx);
		return result!;
	}
}