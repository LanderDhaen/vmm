namespace Rise.Shared.Projects;

/// <summary>
///     Represents the response structure for project-related operations.
/// </summary>
public static partial class ProjectResponse {
	public class Index {
		public IEnumerable<ProjectDto.Index> Projects { get; set; } = [];
		public int TotalCount { get; set; }
	}

	public class Detail {
		public required ProjectDto.Detail Project { get; set; }
	}

	public class History {
		public required ProjectDto.History Project { get; set; }
	}
	public class SelectOptions {
		public required IReadOnlyList<Networks.MeasurementNetworkDto.Index> MeasurementNetworks { get; set; }
		public required IReadOnlyList<string> Zones { get; set; }
	}
}