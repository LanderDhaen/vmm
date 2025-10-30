using System;
using Rise.Shared.Common;     // DateRangeDto
using Rise.Shared.Locations;  // LocationDto

namespace Rise.Shared.Projects;

/// <summary>
///     Contains data transfer objects (DTOs) used for project-related operations.
/// </summary>
public static class ProjectDto
{
	/// <summary>
	///     Represents a project index containing minimalistic project details.
	/// </summary>
	public class Index
	{
		public required Guid Id { get; set; }
		public required string Name { get; set; }
		public required string Code { get; set; }
		public required DateOnly StartDate {get; set; }
		public required DateOnly? EndDate {get; set; }

	}
}