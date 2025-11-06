using Rise.Shared.Common;

namespace Rise.Shared.Networks;

/// <summary>
///     Provides methods for managing project-related operations.
/// </summary>
public interface INetworkService {
	Task<Result<NetworkResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default);
}