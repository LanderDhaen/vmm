using Rise.Shared.Stations;
using Rise.Shared.Identity;

namespace Rise.Server.Endpoints.Stations;

/// <summary>
///     Creation of a <see cref="Station" />
///     See https://fast-endpoints.com/
/// </summary>
/// <param name="stationService"></param>
public class Create(IMeasurementStationService stationService)
	: Endpoint<MeasurementStationRequest.Create, Result<MeasurementStationResponse.Create>> {
	public override void Configure() {
		Post("/api/stations");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result<MeasurementStationResponse.Create>>
		ExecuteAsync(MeasurementStationRequest.Create req, CancellationToken ctx) {
		return stationService.CreateAsync(req, ctx);
	}
}