using Rise.Shared.Stations;

namespace Rise.Server.Endpoints.Stations;

public class Detail(IMeasurementStationService stationService)
	: Endpoint<EmptyRequest, Result<MeasurementStationResponse.Detail>> {
	public override void Configure() {
		Get("/api/stations/{stationId}");
		AllowAnonymous();
	}

	public override Task<Result<MeasurementStationResponse.Detail>> ExecuteAsync(EmptyRequest req, CancellationToken ct) {
		var stationId = Route<Guid>("stationId");
		return stationService.GetDetailAsync(stationId, ct);
	}
}