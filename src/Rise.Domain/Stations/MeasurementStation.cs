using Rise.Domain.Locations;
using Rise.Domain.Projects;

namespace Rise.Domain.Stations;

public class MeasurementStation : Entity {
	private string _code = string.Empty;
	private Installation _installation = default!;
	private Location _location = default!;

	private MeasurementStation() { }

	public MeasurementStation(string code, Location location, Installation installation) {
		Code = code;
		Location = location;
		Installation = installation;
	}

	public string Code {
		get => _code;
		private set => _code = Guard.Against.NullOrEmpty(value);
	}

	public Location Location {
		get => _location;
		private set => _location = Guard.Against.Null(value);
	}

	public Installation Installation {
		get => _installation;
		private set => _installation = Guard.Against.Null(value);
	}

	public ICollection<Project> Projects { get; } = new List<Project>();

	public void MoveTo(Location newLocation) {
		Location = newLocation;
	}

	public void UpdateInstallation(Installation installation) {
		Installation = installation;
	}
}