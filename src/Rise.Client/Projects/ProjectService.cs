using System.Net.Http.Json;

using Rise.Shared.Common;
using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public class ProjectService(HttpClient httpClient) : IProjectService {
	public async Task<Result> EditAsync(ProjectRequest.Edit request, CancellationToken ctx = default) {
		HttpResponseMessage response =
			await httpClient.PutAsJsonAsync($"/api/projects/{request.ProjectId}", request, ctx);
		Result? result = await response.Content.ReadFromJsonAsync<Result>(ctx);
		return result!;
	}

	public async Task<Result<ProjectResponse.Create>> CreateAsync(ProjectRequest.Create request,
		CancellationToken ctx = default) {
		HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/projects", request, ctx);
		Result<ProjectResponse.Create>? result =
			await response.Content.ReadFromJsonAsync<Result<ProjectResponse.Create>>(ctx);
		return result!;
	}

	public async Task<Result<ProjectResponse.Index>> GetIndexAsync(QueryRequest.SkipTake req,
		CancellationToken ctx = default) {
		Result<ProjectResponse.Index>? result =
			await httpClient.GetFromJsonAsync<Result<ProjectResponse.Index>>($"/api/projects?{req.AsQuery()}", ctx);
		return result!;
	}

	public async Task<Result<ProjectResponse.Detail>> GetDetailAsync(Guid projectId, CancellationToken ctx = default) {
		Result<ProjectResponse.Detail>? result =
			await httpClient.GetFromJsonAsync<Result<ProjectResponse.Detail>>($"/api/projects/{projectId}", ctx);
		return result!;
	}

	public async Task<Result<ProjectResponse.History>>
		GetHistoryAsync(Guid projectId, CancellationToken ctx = default) {
		Result<ProjectResponse.History>? result =
			await httpClient.GetFromJsonAsync<Result<ProjectResponse.History>>($"/api/projects/{projectId}/history",
				ctx);
		return result!;
	}

	public async Task<Result> UpdateStatusAsync(ProjectRequest.UpdateStatus req, CancellationToken ctx = default) {
		HttpResponseMessage response =
			await httpClient.PutAsJsonAsync($"/api/projects/{req.ProjectId}/status", req, ctx);

		Result? result = await response.Content.ReadFromJsonAsync<Result>(ctx);
		return result!;
	}

	public async Task<Result<ProjectResponse.MeasurementStations>> GetStationsAsync(
		Guid projectId,
		CancellationToken ctx = default) {
		Result<ProjectResponse.MeasurementStations>? result =
			await httpClient.GetFromJsonAsync<Result<ProjectResponse.MeasurementStations>>(
				$"/api/projects/{projectId}/stations",
				ctx);
		return result!;
	}

	public async Task<Result> UpdateMeasurementStationsAsync(ProjectRequest.UpdateMeasurementStations req,
		CancellationToken ctx = default) {
		HttpResponseMessage response =
			await httpClient.PutAsJsonAsync($"/api/projects/{req.ProjectId}/stations", req, ctx);

		Result? result = await response.Content.ReadFromJsonAsync<Result>(ctx);
		return result!;
	}

	public async Task<Result> UpdateMeasurementNetworkAsync(ProjectRequest.UpdateMeasurementNetwork req,
		CancellationToken ctx = default) {
		HttpResponseMessage response =
			await httpClient.PutAsJsonAsync($"/api/projects/{req.ProjectId}/measurementnetwork", req, ctx);

		Result? result = await response.Content.ReadFromJsonAsync<Result>(ctx);
		return result!;
	}
}