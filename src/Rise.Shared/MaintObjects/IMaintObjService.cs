using Rise.Shared.Common;

namespace Rise.Shared.MaintObjects;

/// <summary>
///     Provides methods for managing maintenance-object-related operations.
/// </summary>
public interface IMaintObjService {
	Task<Result<MaintObjResponse.Index>> GetIndexAsync(
		QueryRequest.SkipTake req,
		CancellationToken ctx = default
	);

	Task<Result<MaintObjResponse.Create>> CreateAsync(
		MaintObjRequest.Create req,
		CancellationToken ctx = default
	);

	Task<Result<MaintObjResponse.Detail>> GetDetailAsync(
		Guid maintObjId,
		CancellationToken ctx = default
	);

	Task<Result> EditAsync(MaintObjRequest.Edit req, CancellationToken ctx);
	
	Task<Result> UpdateLocationAsync(MaintObjRequest.UpdateLocation req, CancellationToken ctx = default);

	Task<Result<MaintObjResponse.BySerial>> GetBySerialAsync(string serialNumber, CancellationToken ct = default);
}