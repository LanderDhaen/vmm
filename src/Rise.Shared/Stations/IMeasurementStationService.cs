using Rise.Shared.Common;
using Rise.Shared.Projects;

namespace Rise.Shared.Stations;

/// <summary>
///     Provides methods for managing measurementstation-related operations.
/// </summary>
public interface IMeasurementStationService {
	Task<Result> EditAsync(MeasurementStationRequest.Edit request, CancellationToken ctx = default);

	Task<Result<MeasurementStationResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request,
		CancellationToken ctx = default);

	Task<Result<MeasurementStationResponse.Detail>> GetDetailAsync(Guid stationId, CancellationToken ctx = default);

	Task<Result<MeasurementStationResponse.History>> GetHistoryAsync(Guid stationId, CancellationToken ctx = default);

	Task<Result<MeasurementStationResponse.ProjectList>> GetProjectsAsync(Guid stationId, CancellationToken ctx = default);

	Task<Result<MeasurementStationResponse.Create>> CreateAsync(MeasurementStationRequest.Create request,
		CancellationToken ctx = default);

	Task<Result> UpdateProjectsAsync(MeasurementStationRequest.UpdateProjects req, CancellationToken ctx = default);
}