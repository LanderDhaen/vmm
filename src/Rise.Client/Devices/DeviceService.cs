using System.Net.Http.Json;

using Rise.Shared.Common;
using Rise.Shared.Devices;

namespace Rise.Client.Devices;

public class DeviceService(HttpClient httpClient) : IDeviceService {
	public async Task<Result<DeviceResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request,
		CancellationToken ctx = default) {
		return (await httpClient.GetFromJsonAsync<Result<DeviceResponse.Index>>(
			$"/api/maintenancedevices?{request.AsQuery()}", ctx))!;
	}
}