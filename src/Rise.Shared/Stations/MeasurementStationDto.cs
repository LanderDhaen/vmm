using System;
using Rise.Shared.Locations;

namespace Rise.Shared.Stations
{
	public static class MeasurementStationDto
	{
		public class Index
		{
			public Guid Id { get; set; }
			
			public string Code { get; set; } = default!;
			public string Status { get; set; } = default!;
			
			public DateTime? InstalledAt { get; set; }
			public DateTime? RemovedAt { get; set; }
			
			public required LocationDto.Index Location { get; init; }
			
			public Guid ProjectId { get; set; }
			public string ProjectCode { get; set; } = default!;
			public string ProjectName { get; set; } = default!;
		}
	}
}