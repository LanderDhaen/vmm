using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Rise.Domain.Devices;
using Rise.Domain.Locations;
using Rise.Domain.Networks;
using Rise.Domain.Projects;
using Rise.Domain.Stations;

namespace Rise.Persistence;

/// <summary>
///     Seeds the database
/// </summary>
public class DbSeeder(
	ApplicationDbContext dbContext,
	RoleManager<IdentityRole> roleManager,
	UserManager<IdentityUser> userManager) {
	private const string PasswordDefault = "A1b2C3!";

	public async Task SeedAsync() {
		await RolesAsync();
		await UsersAsync();
		await ZonesAsync();
		await MeasurementNetworksAsync();
		await LocationAsync();
		await ProjectsAsync();
		await ProjectHistoriesAsync();
		await StationsAsync();
		await DevicesAsync();
	}

	private async Task RolesAsync() {
		if (dbContext.Roles.Any()) {
			return;
		}

		await roleManager.CreateAsync(new IdentityRole("Administrator"));
		await roleManager.CreateAsync(new IdentityRole("Secretary"));
		await roleManager.CreateAsync(new IdentityRole("Technician"));
	}

	private async Task UsersAsync() {
		if (dbContext.Users.Any()) {
			return;
		}

		await dbContext.Roles.ToListAsync();

		IdentityUser admin = new() {
			UserName = "admin@example.com",
			Email = "admin@example.com",
			EmailConfirmed = true
		};
		await userManager.CreateAsync(admin, PasswordDefault);

		IdentityUser secretary = new() {
			UserName = "secretary@example.com",
			Email = "secretary@example.com",
			EmailConfirmed = true
		};
		await userManager.CreateAsync(secretary, PasswordDefault);

		IdentityUser technicianAccount1 = new() {
			UserName = "technician1@example.com",
			Email = "technician1@example.com",
			EmailConfirmed = true
		};
		await userManager.CreateAsync(technicianAccount1, PasswordDefault);

		IdentityUser technicianAccount2 = new() {
			UserName = "technician2@example.com",
			Email = "technician2@example.com",
			EmailConfirmed = true
		};
		await userManager.CreateAsync(technicianAccount2, PasswordDefault);

		IdentityUser user = new() {
			UserName = "user@example.com",
			Email = "user@example.com",
			EmailConfirmed = true
		};
		await userManager.CreateAsync(user, PasswordDefault);

		await userManager.AddToRoleAsync(admin, "Administrator");
		await userManager.AddToRoleAsync(secretary, "Secretary");
		await userManager.AddToRoleAsync(technicianAccount1, "Technician");
		await userManager.AddToRoleAsync(technicianAccount2, "Technician");

		dbContext.Technicians.AddRange(
			new Technician("Tech 1", "Awesome", technicianAccount1.Id),
			new Technician("Tech 2", "Less Awesome", technicianAccount2.Id));

		await dbContext.SaveChangesAsync();
	}

	private async Task ZonesAsync() {
		if (dbContext.Zones.Any()) {
			return;
		}

		List<Zone> zones = new() {
			new Zone("Zone West", "ASTA"),
			new Zone("Zone Oost", "HSTA")
		};

		dbContext.Zones.AddRange(zones);
		await dbContext.SaveChangesAsync();
	}

	private async Task MeasurementNetworksAsync() {
		if (dbContext.MeasurementNetworks.Any()) {
			return;
		}

		List<Zone> zones = await dbContext.Zones.ToListAsync();
		if (!zones.Any()) {
			return;
		}

		List<MeasurementNetwork> measurementNetworks = new() {
			new MeasurementNetwork("Conductiviteitmeetnet West", zones[0]),
			new MeasurementNetwork("Multiparameter West", zones[0]),
			new MeasurementNetwork("Nitraatmeetnet West", zones[0]),
			new MeasurementNetwork("Overstortmeetnet West", zones[0]),
			new MeasurementNetwork("Conductiviteitmeetnet Oost", zones[1]),
			new MeasurementNetwork("Multiparametermeetnet Oost", zones[1]),
			new MeasurementNetwork("Nitraatmeetnet Oost", zones[1]),
			new MeasurementNetwork("Overstortmeetnet Oost", zones[1])
		};

		dbContext.MeasurementNetworks.AddRange(measurementNetworks);
		await dbContext.SaveChangesAsync();
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
			new Address(null, null, "Breda", "4811", "NL"),
			new Address("Kustlaan 45", null, "Oostende", "8400")
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
			new Location("Zee", addresses[5], coordinates[5])
		};

		dbContext.Locations.AddRange(locations);
		await dbContext.SaveChangesAsync();
	}

	private async Task ProjectsAsync() {
		if (dbContext.Projects.Any()) {
			return;
		}

		List<MeasurementNetwork> networks = await dbContext.MeasurementNetworks.ToListAsync();
		if (!networks.Any()) {
			return;
		}

		// Initialise as projects without endDate to allow easier history seeding

		List<DateRange> dateRanges = new() {
			new DateRange(new DateOnly(2025, 1, 15)),
			new DateRange(new DateOnly(2025, 2, 1)),
			new DateRange(new DateOnly(2025, 3, 10)),
			new DateRange(new DateOnly(2025, 4, 5)),
			new DateRange(new DateOnly(2025, 5, 12)),
			new DateRange(new DateOnly(2025, 5, 12))
		};

		List<Project> projects = new() {
			new Project("Canal Cleanup", "CC2025", dateRanges[0], networks[0]),
			new Project("River Monitoring", "RM2025", dateRanges[1], networks[1]),
			new Project("Boating Safety Upgrade", "BSU2025", dateRanges[2], networks[2]),
			new Project("Pond Restoration", "PR2025", dateRanges[3], networks[3]),
			new Project("Waterway CRM Analytics", "WCA2025", dateRanges[4], networks[4]),
			new Project("Coastal Dashboard", "CD2025", dateRanges[5], networks[5])
		};

		dbContext.Projects.AddRange(projects);
		await dbContext.SaveChangesAsync();
	}

	private async Task ProjectHistoriesAsync() {
		if (!dbContext.Projects.Any()) {
			return;
		}

		List<Project> projects = await dbContext.Projects.Include(p => p.DateRange).Include(p => p.MeasurementNetwork)
			.ToListAsync();

		List<DateOnly?> endDates = new() {
			new DateOnly(2025, 6, 30),
			new DateOnly(2025, 8, 15),
			new DateOnly(2025, 9, 25),
			null,
			new DateOnly(2025, 12, 31),
			null
		};

		int i = 0;

		foreach (Project project in projects) {
			DateRange dateRange = project.DateRange;

			// Move the start date

			project.Edit(project.Name, project.Code, new DateRange(dateRange.StartDate.AddDays(1), dateRange.EndDate),
				project.MeasurementNetwork);
			await dbContext.SaveChangesAsync();

			// Revert change

			project.Edit(project.Name, project.Code, dateRange, project.MeasurementNetwork);
			await dbContext.SaveChangesAsync();

			// Move the end date

			project.Edit(project.Name, project.Code, new DateRange(dateRange.StartDate, endDates[i]),
				project.MeasurementNetwork);
			await dbContext.SaveChangesAsync();

			// Update the counter

			i++;
		}
	}

	private async Task StationsAsync() {
		if (dbContext.MeasurementStations.Any()) {
			return;
		}

		List<Location> locations = dbContext.Locations.Local.Any()
			? dbContext.Locations.Local.ToList()
			: await dbContext.Locations.ToListAsync();

		List<Project> projects = dbContext.Projects.Local.Any()
			? dbContext.Projects.Local.ToList()
			: await dbContext.Projects.ToListAsync();

		if (!locations.Any() || !projects.Any()) {
			return;
		}

		int l0 = 0;
		int l1 = Math.Min(1, locations.Count - 1);

		MeasurementStation station1 = new(
			"STN-001",
			locations[l0],
			new Installation(DateTime.UtcNow.AddMonths(-6))
		);

		MeasurementStation station2 = new(
			"STN-002",
			locations[l1],
			new Installation(
				DateTime.UtcNow.AddYears(-1),
				DateTime.UtcNow.AddDays(-30)
			)
		);

		dbContext.MeasurementStations.AddRange(station1, station2);
		await dbContext.SaveChangesAsync();
	}

	private async Task DevicesAsync() {
		if (dbContext.Devices.Any()) {
			return;
		}

		List<MeasurementStation> stations = dbContext.MeasurementStations.Local.Any()
			? dbContext.MeasurementStations.Local.ToList()
			: await dbContext.MeasurementStations.ToListAsync();

		// If there are no stations, still create some unassigned devices
		Device d1 = new("W-Sonde", "567927", DeviceType.Sonde, "Aquatroll");
		Device d2 = new("W-Logger", "31045", DeviceType.Logger, "Campbell");
		Device d3 = new("W-Modem", "354990101102816", DeviceType.Modem, "Centerion");
		Device d4 = new("W-SIM", "2100267739177", DeviceType.Sim, "002.APN.VMM.BE");
		Device d5 = new("W-Voeding", "03Y90D0134", DeviceType.Voeding, "Kyocera");

		// Attach some devices to the first station if available
		if (stations.Any()) {
			d1.AssignToMeasurementStation(stations[0]);
			if (stations.Count > 1) {
				d2.AssignToMeasurementStation(stations[1]);
			}
		}

		dbContext.Devices.AddRange(d1, d2, d3, d4, d5);
		await dbContext.SaveChangesAsync();
	}
}