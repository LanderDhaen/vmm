namespace Rise.Shared.Stations;

public class CreateMeasurementStationRequest
{
	public string Code { get; set; } = default!;
	public string Status { get; set; } = default!;
	public Guid LocationId { get; set; }
	public Guid ProjectId { get; set; }
	public DateTime? InstalledAt { get; set; }
}