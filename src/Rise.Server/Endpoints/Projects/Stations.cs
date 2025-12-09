using Rise.Shared.Identity;
using Rise.Shared.Projects;

namespace Rise.Server.Endpoints.Projects;

public class Stations(IProjectService projectService)
	: EndpointWithoutRequest<Result<ProjectResponse.MeasurementStations>> {
	public override void Configure() {
		Get("/api/projects/{projectId:guid}/stations");
		Roles(AppRoles.Administrator);
	}

	public override async Task<Result<ProjectResponse.MeasurementStations>> ExecuteAsync(CancellationToken ct) {
		Guid projectId = Route<Guid>("projectId");
		return await projectService.GetStationsAsync(projectId, ct);
	}
}