using Rise.Shared.Common;   
using Rise.Shared.Stations;

namespace Rise.Services.Stations;

public interface IMeasurementStationService
{
	Task<Result<MeasurementStationResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default);
}