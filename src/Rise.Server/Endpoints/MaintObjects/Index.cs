using Microsoft.Extensions.Primitives;

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
		// Manually parse filters from query string
		foreach ((string key, StringValues value) in HttpContext.Request.Query) {
			if (key == "skip" || key == "take" || key == "orderBy" || key == "orderDescending") {
				continue;
			}

			if (bool.TryParse(value, out bool boolValue)) {
				req.Filters[key] = boolValue;
			} else {
				req.Filters[key] = value.ToString();
			}
		}

		return maintObjService.GetIndexAsync(req, ct);
	}
}