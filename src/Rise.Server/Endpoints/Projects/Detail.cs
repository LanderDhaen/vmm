using Rise.Shared.Identity;
using Rise.Shared.Projects;

namespace Rise.Server.Endpoints.Projects;

public class Detail(IProjectService projectService) : Endpoint<EmptyRequest, Result<ProjectResponse.Detail>> {
	public override void Configure() {
		Get("/api/projects/{projectId}");
		Roles(AppRoles.Administrator);
	}

	public override async Task<Result<ProjectResponse.Detail>> ExecuteAsync(EmptyRequest req, CancellationToken ct) {
		Guid projectId = Route<Guid>("projectId");
		return await projectService.GetDetailAsync(projectId, ct);
	}
}