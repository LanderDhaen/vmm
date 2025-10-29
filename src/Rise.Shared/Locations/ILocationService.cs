using Rise.Shared.Common;

namespace Rise.Shared.Locations;

/// <summary>
///     Provides methods for managing location-related operations.
/// </summary>
public interface ILocationService {
	Task<Result<LocationResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default);
}