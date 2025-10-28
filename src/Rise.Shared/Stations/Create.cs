namespace Rise.Shared.Stations;

public static partial class MeasurementStationRequest {
	public class Create
	{
		public string Code { get; set; } = default!;
		public string Status { get; set; } = default!;
		public Guid LocationId { get; set; }
		public Guid ProjectId { get; set; }
		public DateTime? InstalledAt { get; set; }
	}
}