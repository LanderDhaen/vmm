using System;
using System.Collections.Generic;

namespace Rise.Domain.Stations
{
	public class MeasurementStation : Common.Entity
	{
		public string Code { get; set; } = default!;
		public string Status { get; set; } = default!;

		public DateTime? InstalledAt { get; set; }
		public DateTime? RemovedAt { get; set; }

		public Guid LocationId { get; set; }
		public Locations.Location? Location { get; set; }
		
	}
}