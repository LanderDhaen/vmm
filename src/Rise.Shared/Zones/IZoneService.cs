using Rise.Shared.Common;

namespace Rise.Shared.Zones;

/// <summary>
///     Provides methods for managing zone-related operations.
/// </summary>
public interface IZoneService {
	Task<Result<ZoneResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default);
}