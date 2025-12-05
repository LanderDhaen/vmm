using System.Net.Http.Json;

using Rise.Shared.Common;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public class StationService(HttpClient httpClient) : IMeasurementStationService {
	public async Task<Result> EditAsync(MeasurementStationRequest.Edit request, CancellationToken ctx = default) {
		HttpResponseMessage response = await httpClient.PutAsJsonAsync("/api/stations", request, ctx);
		return (await response.Content.ReadFromJsonAsync<Result>(ctx))!;
	}

	public async Task<Result<MeasurementStationResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request,
		CancellationToken ctx = default) {
		return (await httpClient.GetFromJsonAsync<Result<MeasurementStationResponse.Index>>(
			$"/api/stations?{request.AsQuery()}", ctx))!;
	}

	public async Task<Result<MeasurementStationResponse.Detail>> GetDetailAsync(Guid stationId,
		CancellationToken ctx = default) {
		return (await httpClient.GetFromJsonAsync<Result<MeasurementStationResponse.Detail>>(
			$"/api/stations/{stationId}", ctx))!;
	}

	public async Task<Result<MeasurementStationResponse.History>> GetHistoryAsync(Guid stationId,
		CancellationToken ctx = default) {
		Result<MeasurementStationResponse.History>? result =
			await httpClient.GetFromJsonAsync<Result<MeasurementStationResponse.History>>(
				$"/api/stations/{stationId}/history", ctx);
		return result!;
	}

	public async Task<Result<MeasurementStationResponse.ProjectList>> GetProjectsAsync(
		Guid stationId,
		QueryRequest.Search request,
		CancellationToken ctx = default) {
		return (await httpClient.GetFromJsonAsync<Result<MeasurementStationResponse.ProjectList>>(
			$"/api/stations/{stationId}/projects?{request.AsQuery()}", ctx))!;
	}

	public async Task<Result<MeasurementStationResponse.Create>> CreateAsync(MeasurementStationRequest.Create request,
		CancellationToken ctx = default) {
		HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/stations", request, ctx);
		Result<MeasurementStationResponse.Create>? result =
			await response.Content.ReadFromJsonAsync<Result<MeasurementStationResponse.Create>>(ctx);
		return result!;
	}

	public async Task<Result> UpdateProjectsAsync(MeasurementStationRequest.UpdateProjects req,
		CancellationToken ctx = default) {
		HttpResponseMessage response =
			await httpClient.PutAsJsonAsync($"/api/stations/{req.MeasurementStationId}/projects", req, ctx);

		Result? result = await response.Content.ReadFromJsonAsync<Result>(ctx);
		return result!;
	}
}