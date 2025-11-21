using Rise.Shared.Common;

namespace Rise.Shared.Devices;

/// <summary>
///     Provides methods for managing device-related operations.
/// </summary>
public interface IDeviceService {
	Task<Result<Projects.ProjectResponse.Index>> GetIndexAsync(
		QueryRequest.SkipTake req,
		CancellationToken ctx = default
	);
}