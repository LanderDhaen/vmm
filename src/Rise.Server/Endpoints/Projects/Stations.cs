using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.Projects;

namespace Rise.Server.Endpoints.Projects;

public sealed class Stations(IProjectService projectService)
	: Endpoint<QueryRequest.Search, Result<ProjectResponse.MeasurementStations>> {
	public override void Configure() {
		Get("/api/projects/{projectId:guid}/stations");
		Roles(AppRoles.Administrator);
	}

	public override async Task<Result<ProjectResponse.MeasurementStations>> ExecuteAsync(QueryRequest.Search req, CancellationToken ct) {
		Guid projectId = Route<Guid>("projectId");
		return await projectService.GetStationsAsync(projectId, req, ct);
	}
}