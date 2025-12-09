using Rise.Shared.Identity;
using Rise.Shared.Stations;

namespace Rise.Server.Endpoints.Stations;

public class UpdateMaintObjects(IMeasurementStationService stationService)
	: Endpoint<MeasurementStationRequest.UpdateMaintObjects, Result> {
	public override void Configure() {
		Put("/api/stations/{stationId:guid}/maintobjects");
		Roles(AppRoles.Administrator);
	}

	public override async Task<Result> ExecuteAsync(MeasurementStationRequest.UpdateMaintObjects req,
		CancellationToken ct) {
		Guid stationId = Route<Guid>("stationId");
		req.MeasurementStationId = stationId;

		return await stationService.UpdateMaintObjsAsync(req, ct);
	}
}