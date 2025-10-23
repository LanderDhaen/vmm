using System.Net.Http.Json;

using Rise.Shared.Common;
using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public class ProjectService(HttpClient httpClient) : IProjectService {
	public async Task<Result> EditAsync(ProjectRequest.Edit request, CancellationToken ctx = default) {
		HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/projects", request, ctx);
		Result? result = await response.Content.ReadFromJsonAsync<Result>(ctx);
		return result!;
	}

	public async Task<Result<ProjectResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request,
		CancellationToken ctx = default) {
		
		Result<ProjectResponse.Index>? result =
			await httpClient.GetFromJsonAsync<Result<ProjectResponse.Index>>($"/api/projects?{request.AsQuery()}", ctx);
		return result!;
	}
}