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

		public required DateRangeDto.Index DateRange { get; init; }

		public required LocationDto.Index Location { get; init; }
	}
	
	public class Detail {
	    public required Guid Id { get; set; }
	    public required string Name { get; set; }
	    public required string Code { get; set; }
	    public required DateRangeDto.Index DateRange { get; init; }
	    public required LocationDto.Index Location { get; init; }
	}
}