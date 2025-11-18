using Rise.Domain.Projects;
using Rise.Shared.Identity;
using Rise.Shared.Projects;

namespace Rise.Server.Endpoints.Projects;

/// <summary>
///     Creation of a <see cref="Project" />
///     See https://fast-endpoints.com/
/// </summary>
/// <param name="projectService"></param>
public class Create(IProjectService projectService) : Endpoint<ProjectRequest.Create, Result<ProjectResponse.Create>> {
	public override void Configure() {
		Post("/api/projects");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result<ProjectResponse.Create>>
		ExecuteAsync(ProjectRequest.Create req, CancellationToken ctx) {
		return projectService.CreateAsync(req, ctx);
	}
}