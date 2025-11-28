using System.Net.Http.Json;

using Rise.Shared.Common;
using Rise.Shared.MaintObjects;
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

	public async Task<Result<List<ProjectDto.Index>>> GetProjectsAsync(
		Guid stationId,
		CancellationToken ctx = default) {
		return (await httpClient.GetFromJsonAsync<Result<List<ProjectDto.Index>>>(
			$"/api/stations/{stationId}/projects", ctx))!;
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

	public async Task<Result<MeasurementStationResponse.MaintObjList>> GetMaintObjsAsync(
		Guid stationId,
		CancellationToken ctx = default)
	{
		return (await httpClient.GetFromJsonAsync<Result<MeasurementStationResponse.MaintObjList>>(
			$"/api/stations/{stationId}/maintobjs", ctx))!;
	}

	public async Task<Result> UpdateMaintObjsAsync(
		MeasurementStationRequest.UpdateMaintObjects req,
		CancellationToken ctx = default) {
		HttpResponseMessage response =
			await httpClient.PutAsJsonAsync(
				$"/api/stations/{req.MeasurementStationId}/maintobjs", req, ctx);

		Result? result = await response.Content.ReadFromJsonAsync<Result>(ctx);
		return result!;
	}
}