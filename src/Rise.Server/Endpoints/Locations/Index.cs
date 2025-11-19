using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.Locations;

namespace Rise.Server.Endpoints.Locations;

/// <summary>
///     List all locations.
///     See https://fast-endpoints.com/
/// </summary>
/// <param name="locationService"></param>
public class Index(ILocationService locationService) : Endpoint<QueryRequest.SkipTake, Result<LocationResponse.Index>> {
	public override void Configure() {
		Get("/api/locations");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result<LocationResponse.Index>> ExecuteAsync(QueryRequest.SkipTake req, CancellationToken ct) {
		Task<Result<LocationResponse.Index>> locaties = locationService.GetIndexAsync(req, ct);
		return locaties;
	}
}