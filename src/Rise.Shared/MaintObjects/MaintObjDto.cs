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

		// Location name of the maintenance object (mirrors assigned station's location when assigned)
		public required string Location { get; init; }
	}

	/// <summary>
	///     Represents detailed information about a maintenance object.
	/// </summary>
	public class Detail {
		public required Guid Id { get; set; }
		public required string Name { get; set; }
		public required string SerialNumber { get; set; }
		public required MaintObjType MaintObjType { get; set; }
		public required string? Brand { get; set; }
		public required MaintObjStatus MaintObjStatus { get; set; }
		public string? MeasurementStationCode { get; set; }

		// Location name (nullable when the object is in stock / unassigned)
		public Guid? LocationId { get; set; }
		public string? Location { get; set; }
		public DateTime? InstalledAt { get; set; }
		public DateTime? RemovedAt { get; set; }

		public DateTime? NextMaintenanceDate { get; set; }

		public DateTime? NextCalibrationDate { get; set; }
	}
}