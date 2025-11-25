using Rise.Domain.Projects;
using Rise.Shared.Identity;
using Rise.Shared.Projects;

namespace Rise.Server.Endpoints.Projects;

/// <summary>
///     Update list of measurementstation from a <see cref="Project" />.
///     See https://fast-endpoints.com/
/// </summary>
/// <param name="projectService"></param>
public class UpdateStations(IProjectService projectService) : Endpoint<ProjectRequest.UpdateMeasurementStations, Result> {
	public override void Configure() {
		Put("/api/projects/{ProjectId}/stations");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result> ExecuteAsync(ProjectRequest.UpdateMeasurementStations req, CancellationToken ctx) {
		return projectService.UpdateMeasurementStationsAsync(req, ctx);
	}
}