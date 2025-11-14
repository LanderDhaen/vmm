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
		await ProjectHistoryAsync();
		await MeasurementStationAsync();
		await MeasurementStationHistoryAsync();
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

	private async Task ProjectHistoryAsync() {
		if (!dbContext.Projects.Any()) {
			return;
		}

		List<Project> projects = await dbContext.Projects.ToListAsync();

		foreach (Project project in projects) {
			DateRange dateRange = project.DateRange;

			project.EditDateRange(new DateRange(dateRange.StartDate, DateOnly.FromDateTime(DateTime.Now)));
			await dbContext.SaveChangesAsync();

			await Task.Delay(TimeSpan.FromSeconds(1));

			project.EditDateRange(dateRange);
			await dbContext.SaveChangesAsync();
		}
	}

	private async Task MeasurementStationAsync() {
		if (dbContext.MeasurementStations.Any()) {
			return;
		}

		List<Location> locations = await dbContext.Locations.ToListAsync();

		List<Installation> installations = new() {
			new Installation(new DateTime(2025, 1, 15), new DateTime(2025, 6, 30)),
			new Installation(new DateTime(2025, 2, 1), new DateTime(2025, 8, 15)),
			new Installation(new DateTime(2025, 3, 10), new DateTime(2025, 9, 25)),
			new Installation(new DateTime(2025, 4, 5)),
			new Installation(new DateTime(2025, 5, 12), new DateTime(2025, 12, 31)),
			new Installation(new DateTime(2025, 5, 12))
		};

		List<MeasurementStation> measurementStations = new() {
			new MeasurementStation("STN-001", locations[0], installations[0]),
			new MeasurementStation("STN-002", locations[1], installations[1]),
			new MeasurementStation("STN-003", locations[2], installations[2]),
			new MeasurementStation("STN-004", locations[3], installations[3]),
			new MeasurementStation("STN-005", locations[4], installations[4]),
			new MeasurementStation("STN-006", locations[5], installations[5])
		};

		dbContext.AddRange(measurementStations);
		await dbContext.SaveChangesAsync();
	}

	private async Task MeasurementStationHistoryAsync() {
		if (!dbContext.MeasurementStations.Any()) {
			return;
		}

		List<MeasurementStation> measurementStations = await dbContext.MeasurementStations.ToListAsync();

		foreach (MeasurementStation measurementStation in measurementStations) {
			string code = measurementStation.Code;

			measurementStation.Edit("I've changed this");
			await dbContext.SaveChangesAsync();

			await Task.Delay(TimeSpan.FromSeconds(1));

			measurementStation.Edit(code + " v2");
			await dbContext.SaveChangesAsync();
		}
	}
}