namespace Rise.Shared.Stations;

/// <summary>
///     Represents a static utility class containing request-related structures for projects.
/// </summary>
public static partial class MeasurementStationRequest {
	public class UpdateProjects {
		public Guid MeasurementStationId { get; set; }
		public IEnumerable<Guid> ProjectIds { get; set; } = new List<Guid>();
	}
}