using Microsoft.EntityFrameworkCore;

using Temporal.Domain.Locations;
using Temporal.Domain.Projects;
using Temporal.Domain.Stations;

namespace Temporal.Persistence;

/// <summary>
///     Seeds the database
/// </summary>
public class DbSeeder(
	ApplicationDbContext dbContext) {

	public async Task SeedAsync() {
		await LocationAsync();
		await ProjectsAsync();
		await StationsAsync();
	}

	private async Task LocationAsync() {
		if (dbContext.Locations.Any()) {
			return;
		}

		List<Address> addresses = new() {
			new Address("Koningstraat 12", "Bus 3A", "Brussel", "1000"),
			new Address("", "", "Antwerpen", "2000"),
			new Address("Veldstraat 78", null, "Gent", "9000"),
			new Address("Rue de la Loi 175", "", "Bruxelles", "1040"),
			new Address(null, null, "Breda", "4811", "NL")
		};

		List<Coordinate> coordinates = new() {
			new Coordinate(4.35, 50.85), // Brussel (Koningstraat 12)
			new Coordinate(4.40, 51.22), // Antwerpen
			new Coordinate(3.73, 51.05), // Gent
			new Coordinate(4.37, 50.84), // Bruxelles (Rue de la Loi)
			new Coordinate(4.78, 51.59), // Breda
			new Coordinate(2.92, 51.22) // Oostende
		};

		List<Location> locations = new() {
			new Location("Paleisgracht", addresses[0], coordinates[0]),
			new Location("Schelde", addresses[1], coordinates[1]),
			new Location("Watersportbaan", addresses[2], coordinates[2]),
			new Location("Paleisvijver", addresses[3], coordinates[3]),
			new Location("Stadsgracht", addresses[4], coordinates[4]),
			new Location("Zee", null, coordinates[5])
		};

		dbContext.Locations.AddRange(locations);
		await dbContext.SaveChangesAsync();
	}

	private async Task ProjectsAsync() {
		if (dbContext.Projects.Any()) {
			return;
		}

		List<DateRange> dateRanges = new() {
			new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 6, 30)),
			new DateRange(new DateOnly(2025, 2, 1), new DateOnly(2025, 8, 15)),
			new DateRange(new DateOnly(2025, 3, 10), new DateOnly(2025, 9, 25)),
			new DateRange(new DateOnly(2025, 4, 5), null),
			new DateRange(new DateOnly(2025, 5, 12), new DateOnly(2025, 12, 31)),
			new DateRange(new DateOnly(2025, 5, 12), null)
		};

		List<Project> projects = new() {
			new Project("Canal Cleanup", "CC2025", dateRanges[0]),
			new Project("River Monitoring", "RM2025", dateRanges[1]),
			new Project("Boating Safety Upgrade", "BSU2025", dateRanges[2]),
			new Project("Pond Restoration", "PR2025", dateRanges[3]),
			new Project("Waterway CRM Analytics", "WCA2025", dateRanges[4]),
			new Project("Coastal Dashboard", "CD2025", dateRanges[5])
		};

		dbContext.Projects.AddRange(projects);
		await dbContext.SaveChangesAsync();
	}

	private async Task StationsAsync() {
		if (dbContext.MeasurementStations.Any())
			return;

		var locations = dbContext.Locations.Local.Any()
			? dbContext.Locations.Local.ToList()
			: await dbContext.Locations.ToListAsync();

		var projects = dbContext.Projects.Local.Any()
			? dbContext.Projects.Local.ToList()
			: await dbContext.Projects.ToListAsync();

		if (!locations.Any() || !projects.Any())
			return;

		int l0 = 0;
		int l1 = Math.Min(1, locations.Count - 1);
		int p0 = 0;
		int p1 = Math.Min(1, projects.Count - 1);

		var station1 = new MeasurementStation(
			code: "STN-001",
			status: "Active",
			location: locations[l0],
			installation: new Installation(DateTime.UtcNow.AddMonths(-6)),
			projects: new[] { projects[p0] }
		);

		var station2 = new MeasurementStation(
			code: "STN-002",
			status: "Inactive",
			location: locations[l1],
			installation: new Installation(
				installedAt: DateTime.UtcNow.AddYears(-1),
				removedAt: DateTime.UtcNow.AddDays(-30)
			),
			projects: new[] { projects[p1], projects[p0] }
		);

		dbContext.MeasurementStations.AddRange(station1, station2);
		await dbContext.SaveChangesAsync();
	}
}