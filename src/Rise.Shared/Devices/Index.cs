namespace Rise.Shared.Devices;

/// <summary>
///     Represents the response structure for device-related operations.
/// </summary>
public static class DeviceResponse {
	public class Index {
		public IEnumerable<DeviceDto.Index> Devices { get; set; } = [];
		public int TotalCount { get; set; }
	}
}