namespace Rise.Shared.Locations;

public interface ILocationRequest {
	public Guid? LocationId { get; set; }
	public bool IsNewLocation { get; set; }

	// In case of a new Location
	public LocationRequest.Create? Location { get; set; }
}