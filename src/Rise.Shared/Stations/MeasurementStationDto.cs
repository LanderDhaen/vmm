using Rise.Shared.Locations;

namespace Rise.Shared.Stations;

public static class MeasurementStationDto {
	public sealed class Index {
		public Guid Id { get; set; }
		public string Code { get; set; } = default!;
		public DateTime InstalledAt { get; set; }
		public DateTime? RemovedAt { get; set; }
		public string? Location { get; set; }
		public string? City { get; set; }
		public int ProjectsCount { get; set; }
		public int MaintObjectsCount { get; set; }
	}

	public sealed class Detail {
		public Guid Id { get; set; }
		public string Code { get; set; } = default!;
		public DateTime InstalledAt { get; set; }
		public DateTime? RemovedAt { get; set; }
		public required LocationDto.Detail Location { get; set; }
		public int ProjectsCount { get; set; }
	}

	public class Modification {
		public required MeasurementStationModificationType MeasurementStationModificationType { get; init; }
		public required DateTime ModifiedAt { get; init; }
		public required string PreviousValue { get; init; }
		public required string CurrentValue { get; init; }
	}
}