using Rise.Domain.Devices;
using Rise.Domain.Locations;
using Rise.Domain.Projects;
using Rise.Domain.Stations.ValueObjects;

namespace Rise.Domain.Stations;

public class MeasurementStation : Entity {
	private MeasurementStation() { }

	public MeasurementStation(
		string code,
		string status,
		Location location,
		Installation installation,
		IEnumerable<Project> projects
	) {
		Code = code;
		Status = status;

		Location = location ?? throw new ArgumentNullException(nameof(location));
		Installation = installation ?? throw new ArgumentNullException(nameof(installation));
		if (projects is null) {
			throw new ArgumentNullException(nameof(projects));
		}

		foreach (Project p in projects) {
			Projects.Add(p);
		}
	}

	public string Code { get; private set; } = default!;
	public string Status { get; private set; } = default!;

	public Location Location { get; private set; } = default!;

	public ICollection<Project> Projects { get; } = new List<Project>();
	public Installation Installation { get; private set; } = default!;

	/// <summary>
	///     Navigation collection of maintenance devices belonging to this station.
	/// </summary>
	public ICollection<MaintenanceDevice> MaintenanceDevices { get; } = new List<MaintenanceDevice>();

	public void UpdateStatus(string status) {
		Status = status;
	}

	public void MoveTo(Location newLocation) {
		Location = newLocation ?? throw new ArgumentNullException(nameof(newLocation));
	}

	public void UpdateInstallation(Installation installation) {
		Installation = installation ?? throw new ArgumentNullException(nameof(installation));
	}
}