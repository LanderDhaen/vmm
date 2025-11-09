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
		// Allow for now until Auth is correctly implemented in frontend

		AllowAnonymous();
		//Roles(AppRoles.Administrator); // Only Administrators can create projects.
	}

	public override Task<Result<ProjectResponse.Create>>
		ExecuteAsync(ProjectRequest.Create req, CancellationToken ctx) {
		return projectService.CreateAsync(req, ctx);
	}
}