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
		// Manually parse filters from query string
		foreach (var (key, value) in HttpContext.Request.Query)
		{
			if (key == "skip" || key == "take" || key == "orderBy" || key == "orderDescending")
				continue;

			if (bool.TryParse(value, out var boolValue))
				req.Filters[key] = boolValue;
			else
				req.Filters[key] = value.ToString();
		}
		
		Task<Result<LocationResponse.Index>> locaties = locationService.GetIndexAsync(req, ct);
		return locaties;
	}
}