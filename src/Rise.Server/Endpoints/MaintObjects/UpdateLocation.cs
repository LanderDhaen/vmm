using Rise.Shared.Identity;
using Rise.Shared.MaintObjects;

namespace Rise.Server.Endpoints.MaintObjects;

/// <summary>
///     Update location from a <see cref="MaintObject" />.
///     See https://fast-endpoints.com/
/// </summary>
/// <param name="maintObjService"></param>
public class UpdateLocation(IMaintObjService maintObjService) : Endpoint<MaintObjRequest.UpdateLocation, Result> {
	public override void Configure() {
		Put("/api/maintobjects/{maintObjId}/location");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result> ExecuteAsync(MaintObjRequest.UpdateLocation req, CancellationToken ctx) {
		return maintObjService.UpdateLocationAsync(req, ctx);
	}
}