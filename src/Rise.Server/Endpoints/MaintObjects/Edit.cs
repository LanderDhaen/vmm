using Rise.Shared.Identity;
using Rise.Shared.MaintObjects;

namespace Rise.Server.Endpoints.MaintObjects;

/// <summary>
///     Edit a <see cref="MaintObjects" />
///     See https://fast-endpoints.com/
/// </summary>
/// <param name="maintObjService"></param>
public class Edit(IMaintObjService maintObjService) : Endpoint<MaintObjRequest.Edit, Result> {
	public override void Configure() {
		Put("/api/maintobjects/{MaintObjId}");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result> ExecuteAsync(MaintObjRequest.Edit req, CancellationToken ctx) {
		return maintObjService.EditAsync(req, ctx);
	}
}