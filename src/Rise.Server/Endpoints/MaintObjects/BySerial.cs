using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.MaintObjects;

namespace Rise.Server.Endpoints.MaintObjects;

public class BySerial(IMaintObjService maintObjService)
	: Endpoint<MaintObjRequest.BySerial, Result<MaintObjResponse.BySerial>> {
	public override void Configure() {
		Get("/api/maintobjects/by-serial/{serialNumber}");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result<MaintObjResponse.BySerial>> ExecuteAsync(MaintObjRequest.BySerial req, CancellationToken ct) {
		var serial = Route<string>("serialNumber")!;
		return maintObjService.GetBySerialAsync(serial, ct);
	}
}