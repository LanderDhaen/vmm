using Microsoft.Extensions.Primitives;

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
		Roles(AppRoles.Administrator);
	}

	public override Task<Result<ProjectResponse.Index>> ExecuteAsync(QueryRequest.SkipTake req, CancellationToken ct) {
		// Manually parse filters from query string
		foreach ((string key, StringValues value) in HttpContext.Request.Query) {
			if (key == "skip" || key == "take" || key == "orderBy" || key == "orderDescending") {
				continue;
			}

			if (bool.TryParse(value, out bool boolValue)) {
				req.Filters[key] = boolValue;
			} else {
				req.Filters[key] = value.ToString();
			}
		}
		return projectService.GetIndexAsync(req, ct);
	}
}