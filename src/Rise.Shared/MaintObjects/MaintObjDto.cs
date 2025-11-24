using Rise.Shared.Projects;

namespace Rise.Shared.MaintObjects;

/// <summary>
///     Contains data transfer objects (DTOs) used for maintenance-objects-related operations.
/// </summary>
public static class MaintObjDto {

	/// <summary>
	///     Represents a maintenance object index containing minimalistic maintenance object details.
	/// </summary>
	public class Index {
		public required Guid Id { get; set; }
		public required string Name { get; set; }
		public required string SerialNumber { get; set; }
		public required MaintObjType MaintObjType { get; set; }
		public required Status Status { get; set; }
	}

	/// <summary>
	///     Represents detailed information about a maintenance object.
	/// </summary>
	public class Details {
		public required Guid Id { get; set; }
		public required string Name { get; set; }
		public required string SerialNumber { get; set; }
		public required MaintObjType MaintObjType { get; set; }
		public required string? Brand { get; set; }
		public required Status Status { get; set; }
		public Guid? MeasurementStationId { get; set; }
		public DateTime InstalledAt { get; set; }
		public DateTime? RemovedAt { get; set; }
	}
}