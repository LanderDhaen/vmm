using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.Stations;

// <- import the interface

// <- import the interface

namespace Rise.Server.Endpoints.Stations;

/// <summary>
///     List all measurement stations.
///     See https://fast-endpoints.com/
/// </summary>
/// <param name="stationService"></param>
public class Index(IMeasurementStationService stationService)
	: Endpoint<QueryRequest.SkipTake, Result<MeasurementStationResponse.Index>> {
	public override void Configure() {
		Get("/api/stations");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result<MeasurementStationResponse.Index>> ExecuteAsync(
		QueryRequest.SkipTake req, CancellationToken ct) {
		return stationService.GetIndexAsync(req, ct);
	}
}