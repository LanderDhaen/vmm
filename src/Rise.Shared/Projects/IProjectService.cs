using Rise.Shared.Common;
using Rise.Shared.Stations;

namespace Rise.Shared.Projects;

/// <summary>
///     Provides methods for managing project-related operations.
/// </summary>
public interface IProjectService {
	Task<Result> EditAsync(ProjectRequest.Edit req, CancellationToken ctx);

	Task<Result<ProjectResponse.Index>> GetIndexAsync(
		QueryRequest.SkipTake req,
		CancellationToken ctx = default
	);

	Task<Result<ProjectResponse.Create>> CreateAsync(
		ProjectRequest.Create req,
		CancellationToken ctx = default
	);

	Task<Result<ProjectResponse.Detail>> GetDetailAsync(
		Guid projectId,
		CancellationToken ctx = default
	);

	Task<Result<ProjectResponse.History>> GetHistoryAsync(
		Guid projectId,
		CancellationToken ctx = default
	);

	Task<Result> UpdateStatusAsync(
		ProjectRequest.UpdateStatus req,
		CancellationToken ctx = default
	);

	Task<Result<List<MeasurementStationDto.Index>>> GetStationsAsync(
		Guid projectId,
		CancellationToken ctx = default
	);
	Task<Result<ProjectResponse.ZoneSelectOptions>> GetZoneSelectOptionsAsync(CancellationToken ctx = default);
	Task<Result<ProjectResponse.NetworkSelectOptions>> GetNetworkSelectOptionsAsync(CancellationToken ctx = default);

}