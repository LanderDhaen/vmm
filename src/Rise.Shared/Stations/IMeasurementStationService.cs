using Rise.Shared.Common;   

namespace Rise.Shared.Stations;

public interface IMeasurementStationService
{
	Task<Result<MeasurementStationResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default);
}