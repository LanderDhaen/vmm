using Rise.Domain.Projects;
using Rise.Shared.Identity;
using Rise.Shared.Projects;

namespace Rise.Server.Endpoints.Projects;

/// <summary>
///     Update the measurement network from a <see cref="Project" />.
///     See https://fast-endpoints.com/
/// </summary>
/// <param name="projectService"></param>
public class UpdateMeasurementNetwork(IProjectService projectService)
	: Endpoint<ProjectRequest.UpdateMeasurementNetwork, Result> {
	public override void Configure() {
		Put("/api/projects/{ProjectId}/measurementnetwork");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result> ExecuteAsync(ProjectRequest.UpdateMeasurementNetwork req, CancellationToken ctx) {
		return projectService.UpdateMeasurementNetworkAsync(req, ctx);
	}
}