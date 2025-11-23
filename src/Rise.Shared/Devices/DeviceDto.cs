using Rise.Shared.Projects;

namespace Rise.Shared.Devices;

/// <summary>
///     Contains data transfer objects (DTOs) used for device-related operations.
/// </summary>
public static class DeviceDto {
	//TODO: what is index and what is details?

	/// <summary>
	///     Represents a device index containing minimalistic device details.
	/// </summary>
	public class Index {
		public required Guid Id { get; set; }
		public required string Name { get; set; }
		public required string SerialNumber { get; set; }
		public required DeviceType DeviceType { get; set; }
		public required Status Status { get; set; }
	}

	/// <summary>
	///     Represents detailed information about a device.
	/// </summary>
	public class Details {
		public required Guid Id { get; set; }
		public required string Name { get; set; }
		public required string SerialNumber { get; set; }
		public required DeviceType DeviceType { get; set; }
		public required string? Brand { get; set; }
		public required Status Status { get; set; }
		public Guid? MeasurementStationId { get; set; }
	}
}