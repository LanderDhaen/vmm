using Rise.Shared.Identity;
using Rise.Shared.Stations;

namespace Rise.Server.Endpoints.Stations;

public class Detail(IMeasurementStationService stationService)
	: Endpoint<EmptyRequest, Result<MeasurementStationResponse.Detail>> {
	public override void Configure() {
		Get("/api/stations/{stationId}");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result<MeasurementStationResponse.Detail>>
		ExecuteAsync(EmptyRequest req, CancellationToken ct) {
		Guid stationId = Route<Guid>("stationId");
		return stationService.GetDetailAsync(stationId, ct);
	}
}