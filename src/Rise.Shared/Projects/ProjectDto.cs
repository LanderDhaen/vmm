using Rise.Shared.Networks;

namespace Rise.Shared.Projects;

/// <summary>
///     Contains data transfer objects (DTOs) used for project-related operations.
/// </summary>
public static class ProjectDto {
	/// <summary>
	///     Represents a project index containing minimalistic project details.
	/// </summary>
	public class Index {
		public required Guid Id { get; set; }
		public required string Name { get; set; }
		public required string Code { get; set; }
		public required DateOnly StartDate { get; set; }
		public required DateOnly? EndDate { get; set; }
		public required string MeasurementNetwork { get; set; }
		public required string Zone { get; set; }
		public required Status Status { get; set; }
	}

	public class Detail {
		public required Guid Id { get; set; }
		public required string Name { get; set; }
		public required string Code { get; set; }
		public required DateOnly StartDate { get; set; }
		public required DateOnly? EndDate { get; set; }
		public required MeasurementNetworkDto.Index MeasurementNetwork { get; set; }
		public required string Zone { get; set; }
		public required Status Status { get; set; }
	}

	public class History {
		public required Guid Id { get; set; }
		public required string Name { get; set; }
		public required string Code { get; set; }
		public required DateOnly StartDate { get; set; }
		public required DateOnly? EndDate { get; set; }
		public required IReadOnlyList<ProjectHistoryDto.Index> ProjectHistories { get; init; }
	}
}

public static class ProjectHistoryDto {
	public class Index {
		public required Guid Id { get; init; }
		public required DateOnly StartDate { get; init; }
		public required DateOnly? EndDate { get; init; }
		public required DateTime ValidFrom { get; init; }
		public required DateTime ValidTo { get; init; }
	}
}