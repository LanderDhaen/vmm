using Rise.Domain.Common;
using Rise.Domain.Locations;
using Rise.Domain.Stations.ValueObjects;
using Rise.Domain.Projects;

namespace Rise.Domain.Stations;

public class MeasurementStation : Entity
{
	public string Code { get; private set; } = default!;
	public string Status { get; private set; } = default!;

	public Location Location { get; private set; } = default!;
	public Project Project { get; private set; } = default!;
	public Installation Installation { get; private set; } = default!;

	private MeasurementStation() { }

	public MeasurementStation(
		string code,
		string status,
		Location location,
		Installation installation,
		Project project
	)
	{
		Code = code;
		Status = status;

		Location = location ?? throw new ArgumentNullException(nameof(location));
		Installation = installation ?? throw new ArgumentNullException(nameof(installation));
		Project = project ?? throw new ArgumentNullException(nameof(project));
	}

	public void UpdateStatus(string status) => Status = status;

	public void MoveTo(Location newLocation)
		=> Location = newLocation ?? throw new ArgumentNullException(nameof(newLocation));

	public void UpdateInstallation(Installation installation)
		=> Installation = installation ?? throw new ArgumentNullException(nameof(installation));
}