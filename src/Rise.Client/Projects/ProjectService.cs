using Rise.Shared.Projects;
using System.Net.Http.Json;
using Rise.Shared.Common;

namespace Rise.Client.Projects;

public class ProjectService(HttpClient httpClient) : IProjectService
{

    public async Task<Result> EditAsync(ProjectRequest.Edit request, CancellationToken ctx = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/projects", request, ctx);
        var result = await response.Content.ReadFromJsonAsync<Result>(cancellationToken: ctx);
        return result!;
    }

    public async Task<Result<ProjectResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default)
    {
        var result = await httpClient.GetFromJsonAsync<Result<ProjectResponse.Index>>("/api/projects", cancellationToken: ctx);
        return result!;
    }
}