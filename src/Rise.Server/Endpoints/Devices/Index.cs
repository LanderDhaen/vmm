using Rise.Shared.Common;
using Rise.Shared.Devices;
using Rise.Shared.Identity;

namespace Rise.Server.Endpoints.Devices;

/// <summary>
///     List all devices.
///     See https://fast-endpoints.com/
/// </summary>
/// <param name="deviceService"></param>
public class Index(IDeviceService deviceService) : Endpoint<QueryRequest.SkipTake, Result<DeviceResponse.Index>> {
	public override void Configure() {
		Get("/api/maintenanceobjects");
		Roles(AppRoles.Administrator);
	}

	public override Task<Result<DeviceResponse.Index>> ExecuteAsync(QueryRequest.SkipTake req, CancellationToken ct) {
		return deviceService.GetIndexAsync(req, ct);
	}
}