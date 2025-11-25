namespace Rise.Shared.Projects;

/// <summary>
///     Represents a static utility class containing request-related structures for projects.
/// </summary>
public static partial class ProjectRequest {
	public class UpdateMeasurementStations {
		public Guid ProjectId { get; set; }
		public IEnumerable<Guid> MeasurementStationIds { get; set; } = new List<Guid>();
	}
}