using Rise.Shared.Identity;
using Rise.Shared.MaintObjects;

namespace Rise.Server.Endpoints.MaintObjects;

public class Detail(IMaintObjService maintObjService) : Endpoint<EmptyRequest, Result<MaintObjResponse.Detail>> {
	public override void Configure() {
		Get("/api/maintObjects/{maintObjId}");
		Roles(AppRoles.Administrator);
	}

	public override async Task<Result<MaintObjResponse.Detail>> ExecuteAsync(EmptyRequest req, CancellationToken ct) {
		Guid maintObjId = Route<Guid>("maintObjId");
		return await maintObjService.GetDetailAsync(maintObjId, ct);
	}
}