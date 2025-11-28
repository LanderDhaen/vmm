using Rise.Shared.Identity;
using Rise.Shared.Common;
using Rise.Shared.Stations;

namespace Rise.Server.Endpoints.Stations;

public class Projects(IMeasurementStationService stationService)
	: Endpoint<QueryRequest.Search, Result<MeasurementStationResponse.ProjectList>> {
	public override void Configure() {
		Get("/api/stations/{stationId:guid}/projects");
		Roles(AppRoles.Administrator);
	}

	public override async Task<Result<MeasurementStationResponse.ProjectList>> ExecuteAsync(QueryRequest.Search request, CancellationToken ct) {
		Guid stationId = Route<Guid>("stationId");
		return await stationService.GetProjectsAsync(stationId, request, ct);
	}
}