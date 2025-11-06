using System.Net.Http.Json;

using Rise.Shared.Common;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public class StationService(HttpClient httpClient) {
	public async Task<Result<MeasurementStationResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default)
		=> (await httpClient.GetFromJsonAsync<Result<MeasurementStationResponse.Index>>($"/api/stations?{request.AsQuery()}", ctx))!;
	public async Task<Result<MeasurementStationResponse.Detail>> GetDetailAsync(Guid stationId, CancellationToken ctx = default)
		=> (await httpClient.GetFromJsonAsync<Result<MeasurementStationResponse.Detail>>($"/api/stations/{stationId}", ctx))!;
	
	public async Task<Result<List<ProjectDto.Index>>> GetProjectsAsync(
		Guid stationId,
		CancellationToken ctx = default)
		=> (await httpClient.GetFromJsonAsync<Result<List<ProjectDto.Index>>>(
			$"/api/stations/{stationId}/projects", ctx))!;
}