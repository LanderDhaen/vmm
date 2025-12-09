using Rise.Shared.Identity;
using Rise.Shared.Stations;

namespace Rise.Server.Endpoints.Stations;

public class MaintObjs(IMeasurementStationService stationService)
	: EndpointWithoutRequest<Result<MeasurementStationResponse.MaintObjList>> {
	public override void Configure() {
		Get("/api/stations/{stationId:guid}/maintobjects");
		Roles(AppRoles.Administrator);
	}

	public override async Task<Result<MeasurementStationResponse.MaintObjList>> ExecuteAsync(CancellationToken ct) {
		Guid stationId = Route<Guid>("stationId");
		return await stationService.GetMaintObjsAsync(stationId, ct);
	}
}