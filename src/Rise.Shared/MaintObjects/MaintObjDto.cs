using Rise.Shared.Locations;
using Rise.Shared.Stations;

namespace Rise.Shared.MaintObjects;

/// <summary>
///     Contains data transfer objects (DTOs) used for maintenance-objects-related operations.
/// </summary>
public static class MaintObjDto {
	/// <summary>
	///     Represents a maintenance object index containing minimalistic maintenance object details.
	/// </summary>
	public class Index {
		public required Guid Id { get; init; }
		public required string Name { get; init; }
		public required string SerialNumber { get; init; }
		public required string? Brand { get; init; }
		public required MaintObjType MaintObjType { get; init; }
		public required MaintObjStatus MaintObjStatus { get; init; }
		public required string Location { get; init; }
		public required string? StationCode { get; init; }
	}

	/// <summary>
	///     Represents detailed information about a maintenance object.
	/// </summary>
	public class Detail {
		public required Guid Id { get; init; }
		public required string Name { get; init; }
		public required string SerialNumber { get; init; }
		public required string? Brand { get; init; }
		public required MaintObjType MaintObjType { get; init; }
		public required MaintObjStatus MaintObjStatus { get; init; }

		public required LocationDto.Detail Location { get; init; }
		public required string? StationCode { get; init; }

		public required MeasurementStationDto.Detail Station { get; init; }

		public required DateTime? InstalledAt { get; init; }
		public required DateTime? RemovedAt { get; init; }
		public required int MaintenanceIntervalDays { get; init; }
		public required int CalibrationIntervalDays { get; init; }
	}
}