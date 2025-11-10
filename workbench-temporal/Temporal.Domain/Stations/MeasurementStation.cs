using Temporal.Domain.Common;
using Temporal.Domain.Locations;
using Temporal.Domain.Projects;

namespace Temporal.Domain.Stations;

public class MeasurementStation : Entity {
	public string Code { get; private set; } = default!;
	public string Status { get; private set; } = default!;

	public Location Location { get; private set; } = default!;
	public Installation Installation { get; private set; } = default!;

	private MeasurementStation() { }

	public MeasurementStation(
		string code,
		string status,
		Location location,
		Installation installation
	) {
		Code = code;
		Status = status;

		Location = location ?? throw new ArgumentNullException(nameof(location));
		Installation = installation ?? throw new ArgumentNullException(nameof(installation));
	}

	public void UpdateStatus(string status) => Status = status;

	public void MoveTo(Location newLocation)
		=> Location = newLocation ?? throw new ArgumentNullException(nameof(newLocation));

	public void UpdateInstallation(Installation installation)
		=> Installation = installation ?? throw new ArgumentNullException(nameof(installation));
}