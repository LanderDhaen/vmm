using Rise.Shared.Common;

namespace Rise.Shared.Projects;

/// <summary>
///     Provides methods for managing project-related operations.
/// </summary>
public interface IProjectService {
	Task<Result> EditAsync(ProjectRequest.Edit req, CancellationToken ctx);
	Task<Result<ProjectResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default);
	Task<Result<ProjectResponse.Create>> CreateAsync(ProjectRequest.Create request, CancellationToken ctx = default);
	Task<Result<ProjectResponse.Detail>> GetDetailAsync(Guid projectId, CancellationToken ctx = default);
}