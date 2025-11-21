namespace Rise.Shared.Devices;

/// <summary>
///     Contains data transfer objects (DTOs) used for device-related operations.
/// </summary>
public static class DevicesDto {
	/// <summary>
	///     Represents a device index containing minimalistic project details.
	/// </summary>
	public class Index {
		public required Guid Id { get; set; }
		public required string Name { get; set; }
		public required string SerialNumber { get; set; }
		public required string DeviceType { get; set; }
		public required string Status { get; set; }
	}
}