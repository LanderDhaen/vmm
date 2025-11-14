using Rise.Shared.Common;

namespace Rise.Shared.Projects;

/// <summary>
///     Provides methods for managing project-related operations.
/// </summary>
public interface IProjectService {
	Task<Result> EditAsync(ProjectRequest.Edit req, CancellationToken ctx);
	Task<Result<ProjectResponse.Index>> GetIndexAsync(QueryRequest.SkipTake req, CancellationToken ctx = default);
	Task<Result<ProjectResponse.Create>> CreateAsync(ProjectRequest.Create req, CancellationToken ctx = default);
	Task<Result<ProjectResponse.Detail>> GetDetailAsync(Guid projectId, CancellationToken ctx = default);
	Task<Result> UpdateStatusAsync(ProjectRequest.UpdateStatus req, CancellationToken ctx = default);
}