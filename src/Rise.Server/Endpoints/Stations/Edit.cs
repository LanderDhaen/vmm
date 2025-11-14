using Rise.Shared.Identity;
using Rise.Shared.Stations;

namespace Rise.Server.Endpoints.Stations;

public sealed class Edit(IMeasurementStationService stationService)
	: Endpoint<MeasurementStationRequest.Edit, Result> {
	public override void Configure() {
		Put("/api/stations");
		Roles(AppRoles.Technician, AppRoles.Administrator);
	}

	public override Task<Result> ExecuteAsync(MeasurementStationRequest.Edit req, CancellationToken ct) {
		return stationService.EditAsync(req, ct);
	}
}