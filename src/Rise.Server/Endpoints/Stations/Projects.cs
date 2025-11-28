using Rise.Shared.Identity;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Server.Endpoints.Stations;

public sealed class Projects(IMeasurementStationService stationService)
	: EndpointWithoutRequest<Result<MeasurementStationResponse.ProjectList>> {
	public override void Configure() {
		Get("/api/stations/{stationId:guid}/projects");
		Roles(AppRoles.Administrator);
	}

	public override async Task<Result<MeasurementStationResponse.ProjectList>> ExecuteAsync(CancellationToken ct) {
		Guid stationId = Route<Guid>("stationId");
		return await stationService.GetProjectsAsync(stationId, ct);
	}
}