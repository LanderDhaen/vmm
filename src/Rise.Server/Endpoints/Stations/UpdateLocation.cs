using Rise.Shared.Identity;
using Rise.Shared.Stations;

namespace Rise.Server.Endpoints.Stations;

/// <summary>
///     Update location from a <see cref="MeasurementStation" />.
///     See https://fast-endpoints.com/
/// </summary>
/// <param name="stationService"></param>
public class UpdateLocation(IMeasurementStationService stationService)
	: Endpoint<MeasurementStationRequest.UpdateLocation, Result> {
	public override void Configure() {
		Put("/api/stations/{StationId}/location");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result> ExecuteAsync(MeasurementStationRequest.UpdateLocation req, CancellationToken ctx) {
		return stationService.UpdateLocationAsync(req, ctx);
	}
}