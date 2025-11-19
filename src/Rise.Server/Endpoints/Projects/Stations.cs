using Rise.Shared.Identity;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Server.Endpoints.Projects;

public sealed class Stations(IProjectService projectService)
	: EndpointWithoutRequest<Result<List<MeasurementStationDto.Index>>> {
	public override void Configure() {
		Get("/api/projects/{projectId:guid}/stations");
		Roles(AppRoles.Administrator);
	}

	public override async Task<Result<List<MeasurementStationDto.Index>>> ExecuteAsync(CancellationToken ct) {
		Guid projectId = Route<Guid>("projectId");
		return await projectService.GetStationsAsync(projectId, ct);
	}
}