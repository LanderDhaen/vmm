using Rise.Domain.Locations;
using Rise.Domain.Projects;

namespace Rise.Domain.Stations;

public class MeasurementStation : Entity {
	private string _code = String.Empty;
	private Location _location = default!;
	private Installation _installation = default!;
	private ICollection<Project> _projects = new List<Project>();
	
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
	
	public void UpdateProjectList(ICollection<Project> projects) {
		// Get all the projects that are not already in the measurement station
		List<Project> projectsToAdd = projects
			.Where(s => Projects.All(pr => pr.Id != s.Id))
			.ToList();

		projectsToAdd.ForEach(p => Projects.Add(p));
		
		// Get all the projects that are already in the measurement station but need to be deleted
		List<Project> projectsToRemove = Projects
			.Where(p => projects.All(pr => pr.Id != p.Id))
			.ToList();
		
		projectsToRemove.ForEach(p => Projects.Remove(p));
	}

	public void MoveTo(Location newLocation) {
		Location = newLocation;
	}

	public void UpdateInstallation(Installation installation) {
		Installation = installation;
	}
}