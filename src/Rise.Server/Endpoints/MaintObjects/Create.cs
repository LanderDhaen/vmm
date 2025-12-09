using Rise.Shared.Identity;
using Rise.Shared.MaintObjects;

namespace Rise.Server.Endpoints.MaintObjects;

/// <summary>
///     Creation of a <see cref="MaintObjects" />
///     See https://fast-endpoints.com/
/// </summary>
/// <param name="maintObjService"></param>
public class Create(IMaintObjService maintObjService)
	: Endpoint<MaintObjRequest.Create, Result<MaintObjResponse.Create>> {
	public override void Configure() {
		Post("/api/maintobjects");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result<MaintObjResponse.Create>>
		ExecuteAsync(MaintObjRequest.Create req, CancellationToken ctx) {
		return maintObjService.CreateAsync(req, ctx);
	}
}