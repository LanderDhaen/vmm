using Rise.Domain.Locations;                    
using Rise.Domain.Stations.ValueObjects;

namespace Rise.Domain.Stations;

public class MeasurementStation
{
	public Guid Id { get; private set; }

	public string Code { get; private set; } = default!;
	public string Status { get; private set; } = default!;
	
	public Guid? ProjectId { get; private set; }
	public string? ProjectName { get; private set; }
	public string? ProjectCode { get; private set; }
	
	public Location Location { get; private set; } = default!;
	
	public Installation Installation { get; private set; } = default!;

	private MeasurementStation() { } 

	public MeasurementStation(
		string code,
		string status,
		Location location,
		Installation installation,
		Guid? projectId = null,
		string? projectName = null,
		string? projectCode = null)
	{
		Code = code;
		Status = status;

		Location = location ?? throw new ArgumentNullException(nameof(location));
		Installation = installation ?? throw new ArgumentNullException(nameof(installation));

		ProjectId = projectId;
		ProjectName = projectName;
		ProjectCode = projectCode;
	}

	public void UpdateStatus(string status) => Status = status;

	public void MoveTo(Location newLocation)
		=> Location = newLocation ?? throw new ArgumentNullException(nameof(newLocation));

	public void UpdateInstallation(Installation installation)
		=> Installation = installation ?? throw new ArgumentNullException(nameof(installation));
}