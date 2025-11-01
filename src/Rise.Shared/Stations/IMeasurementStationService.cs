using Rise.Shared.Common;

namespace Rise.Shared.Stations;

/// <summary>
///     Provides methods for managing measurementstation-related operations.
/// </summary>
public interface IMeasurementStationService {
	Task<Result<MeasurementStationResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request,
		CancellationToken ctx = default);
}