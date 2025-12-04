using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.Zones;

namespace Rise.Server.Endpoints.Zones;

/// <summary>
///     List all zones.
///     See https://fast-endpoints.com/
/// </summary>
/// <param name="zoneService"></param>
public class Index(IZoneService zoneService) : Endpoint<QueryRequest.SkipTake, Result<ZoneResponse.Index>> {
	public override void Configure() {
		Get("/api/zones");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result<ZoneResponse.Index>> ExecuteAsync(QueryRequest.SkipTake req, CancellationToken ct) {
		return zoneService.GetIndexAsync(req, ct);
	}
}