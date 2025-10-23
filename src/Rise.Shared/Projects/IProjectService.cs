using Rise.Shared.Common;

namespace Rise.Shared.Projects;

/// <summary>
///     Provides methods for managing project-related operations.
/// </summary>
public interface IProjectService {
	Task<Result> EditAsync(ProjectRequest.Edit req, CancellationToken ctx);
	Task<Result<ProjectResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default);
}