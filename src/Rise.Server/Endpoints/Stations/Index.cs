using Rise.Services.Stations;          // <- import the interface
using Rise.Shared.Common;
using Rise.Shared.Stations;

namespace Rise.Server.Endpoints.Stations;

/// <summary>
///     List all measurement stations.
///     See https://fast-endpoints.com/
/// </summary>
/// <param name="stationService"></param>
public class Index(IMeasurementStationService stationService)
	: Endpoint<QueryRequest.SkipTake, Result<MeasurementStationResponse.Index>>
{
	public override void Configure()
	{
		Get("/api/stations");

		// Allow for now until Auth is correctly implemented in frontend
		AllowAnonymous();
	}

	public override Task<Result<MeasurementStationResponse.Index>> ExecuteAsync(
		QueryRequest.SkipTake req, CancellationToken ct)
		=> stationService.GetIndexAsync(req, ct);
}