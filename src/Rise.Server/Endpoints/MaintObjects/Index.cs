using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.MaintObjects;

namespace Rise.Server.Endpoints.MaintObjects;

/// <summary>
///     List all maintenance objects.
///     See https://fast-endpoints.com/
/// </summary>
/// <param name="maintObjService"></param>
public class Index(IMaintObjService maintObjService) : Endpoint<QueryRequest.SkipTake, Result<MaintObjResponse.Index>> {
	public override void Configure() {
		Get("/api/maintobjects");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result<MaintObjResponse.Index>> ExecuteAsync(QueryRequest.SkipTake req, CancellationToken ct) {
		return maintObjService.GetIndexAsync(req, ct);
	}
}