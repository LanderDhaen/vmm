using Rise.Shared.Identity;
using Rise.Shared.Projects;

namespace Rise.Server.Endpoints.Projects;

public class UpdateStatus(IProjectService projectService) : Endpoint<ProjectRequest.UpdateStatus, Result> {
	public override void Configure() {
		Put("/api/projects/{ProjectId}/status");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result> ExecuteAsync(ProjectRequest.UpdateStatus req, CancellationToken ct) {
		return projectService.UpdateStatusAsync(req, ct);
	}
}