using System;

namespace Rise.Shared.Stations
{
	public class CreateMeasurementStationRequest
	{
		public string Code { get; set; } = default!;
		public string Status { get; set; } = default!;
		public DateTime? InstalledAt { get; set; }
		public Guid LocationId { get; set; }
		
		public Guid ProjectId { get; set; }
	}
	
	public class UpdateMeasurementStationRequest
	{
		public string Code { get; set; } = default!;
		public string Status { get; set; } = default!;
		public DateTime? InstalledAt { get; set; }
		public DateTime? RemovedAt { get; set; }
		public Guid LocationId { get; set; }
		
		public Guid ProjectId { get; set; }
	}
	
	public class MeasurementStationListRequest
	{
		public int Page { get; set; } = 1;         
		public int PageSize { get; set; } = 10;
		public string? Search { get; set; }       
	}
}