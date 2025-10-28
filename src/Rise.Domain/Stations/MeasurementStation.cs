using System;
using Rise.Domain.Locations;
using Rise.Domain.Projects;

namespace Rise.Domain.Stations
{
	public class MeasurementStation : Common.Entity
	{
		public string Code { get; set; } = default!;
		public string Status { get; set; } = default!;
		
		public DateTime? InstalledAt { get; set; }
		public DateTime? RemovedAt { get; set; }
		
		public Guid LocationId { get; set; }
		public Location? Location { get; set; }
		
		public Guid ProjectId { get; set; }
		public Project? Project { get; set; }
	}
}