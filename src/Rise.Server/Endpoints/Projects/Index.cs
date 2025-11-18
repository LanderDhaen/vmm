using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.Projects;

namespace Rise.Server.Endpoints.Projects;

/// <summary>
///     List all projects.
///     See https://fast-endpoints.com/
/// </summary>
/// <param name="projectService"></param>
public class Index(IProjectService projectService) : Endpoint<QueryRequest.SkipTake, Result<ProjectResponse.Index>> {
	public override void Configure() {
		Get("/api/projects");

		// Allow for now until Auth is correctly implemented in frontend

		Roles(AppRoles.Administrator);
	}

	public override Task<Result<ProjectResponse.Index>> ExecuteAsync(QueryRequest.SkipTake req, CancellationToken ct) {
		return projectService.GetIndexAsync(req, ct);
	}
}