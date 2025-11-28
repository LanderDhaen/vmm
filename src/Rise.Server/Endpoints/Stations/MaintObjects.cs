using Rise.Shared.Identity;
using Rise.Shared.MaintObjects;
using Rise.Shared.Stations;

namespace Rise.Server.Endpoints.Stations;

public sealed class MaintObjs(IMeasurementStationService stationService)
	: EndpointWithoutRequest<Result<List<MaintObjDto.Index>>> {
	public override void Configure() {
		Get("/api/stations/{stationId:guid}/maintobjs");
		Roles(AppRoles.Administrator);
	}

	public override async Task<Result<List<MaintObjDto.Index>>> ExecuteAsync(CancellationToken ct) {
		Guid stationId = Route<Guid>("stationId");
		return await stationService.GetMaintObjsAsync(stationId, ct);
	}
}