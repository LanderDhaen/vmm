namespace Rise.Shared.Projects;

/// <summary>
///     Contains data transfer objects (DTOs) used for daterange-related operations.
/// </summary>
public static class DateRangeDto {
	/// <summary>
	///     Represents a daterange index containing minimalistic daterange details.
	/// </summary>
	public class Index {
		public required DateOnly StartDate { get; init; }
		public required DateOnly? EndDate { get; init; }
	}
}