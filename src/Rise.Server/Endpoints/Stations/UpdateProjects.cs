using Rise.Domain.Projects;
using Rise.Shared.Identity;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Server.Endpoints.Stations;

/// <summary>
///     Update list of projects from a <see cref="MeasurementStation" />.
///     See https://fast-endpoints.com/
/// </summary>
/// <param name="stationService"></param>
public class UpdateStations(IMeasurementStationService stationService) : Endpoint<MeasurementStationRequest.UpdateProjects, Result> {
	public override void Configure() {
		Put("/api/stations/{StationId}/projects");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result> ExecuteAsync(MeasurementStationRequest.UpdateProjects req, CancellationToken ctx) {
		return stationService.UpdateProjectsAsync(req, ctx);
	}
}