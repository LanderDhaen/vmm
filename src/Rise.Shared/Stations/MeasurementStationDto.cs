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
		public string? Location { get; set; }
		public string? City { get; set; }
		public int ProjectsCount { get; set; }
		public int MaintObjectsCount { get; set; }
	}

	public class History {
		public required Guid Id { get; init; }
		public required string Code { get; init; }
		public required DateTime InstalledAt { get; init; }
		public required DateTime? RemovedAt { get; init; }
		public required string Location { get; init; }
		public required string City { get; init; }
		public required IReadOnlyList<MeasurementStationHistoryDto.Index> MeasurementStationHistories { get; init; }
	}
}

public static class MeasurementStationHistoryDto {
	public class Index {
		public required Guid Id { get; init; }
		public required DateTime InstalledAt { get; init; }
		public required DateTime? RemovedAt { get; init; }
		public required string Location { get; init; }
		public required string City { get; init; }
		public required DateTime ValidFrom { get; init; }
		public required DateTime ValidTo { get; init; }
	}

	public class Modification {
		public required ModificationType ModificationType { get; init; }
		public DateTime ModifiedAt { get; init; }
		public string PreviousValue { get; init; }
		public string CurrentValue { get; init; }
	}
}