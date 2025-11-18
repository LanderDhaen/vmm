using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.Networks;

namespace Rise.Server.Endpoints.Networks;

/// <summary>
///     List all measurement networks.
///     See https://fast-endpoints.com/
/// </summary>
/// <param name="networkService"></param>
public class Index(INetworkService networkService) : Endpoint<QueryRequest.SkipTake, Result<NetworkResponse.Index>> {
	public override void Configure() {
		Get("/api/measurementnetworks");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result<NetworkResponse.Index>> ExecuteAsync(QueryRequest.SkipTake req, CancellationToken ct) {
		return networkService.GetIndexAsync(req, ct);
	}
}