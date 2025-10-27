using System;
using Rise.Shared.Locations;

namespace Rise.Shared.Stations;

public static class MeasurementStationDto
{
	public class Index
	{
		public Guid Id { get; set; }
		public string Code { get; set; } = default!;
		public string Status { get; set; } = default!;
		public DateTime? InstalledAt { get; set; }
		public DateTime? RemovedAt { get; set; }

		// Match your Projects style: embed a minimal location DTO
		public required LocationDto.Index Location { get; init; }
	}
}