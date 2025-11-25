using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Rise.Domain.Locations;
using Rise.Domain.MaintObjects;
using Rise.Domain.Networks;
using Rise.Domain.Projects;
using Rise.Domain.Stations;

using Installation = Rise.Domain.Common.Installation;

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
		await MeasurementStationsHistoriesAsync();
		await MaintObjectsAsync();
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
			new Address("Kustlaan 45", null, "Oostende", "8400"),
			new Address("Industrielaan 15", null, "Aalst", "9320"),
			new Address("Havenlaan 12", null, "Roeselare", "8800"),
			new Address("Industrieweg 124", null, "Gent", "9040")
		};

		List<Coordinate> coordinates = new() {
			new Coordinate(4.35, 50.85), // Brussel (Koningstraat 12)
			new Coordinate(4.40, 51.22), // Antwerpen
			new Coordinate(3.73, 51.05), // Gent
			new Coordinate(4.37, 50.84), // Bruxelles (Rue de la Loi)
			new Coordinate(4.78, 51.59), // Breda
			new Coordinate(2.92, 51.22), // Oostende
			new Coordinate(4.06, 50.94), // Aalst
			new Coordinate(3.11, 50.94), // Roeselare
			new Coordinate(3.75, 51.03)  // Gentbrugge
		};

		List<Location> locations = new() {
			new Location("Paleisgracht", addresses[0], coordinates[0]),
			new Location("Schelde", addresses[1], coordinates[1]),
			new Location("Watersportbaan", addresses[2], coordinates[2]),
			new Location("Paleisvijver", addresses[3], coordinates[3]),
			new Location("Stadsgracht", addresses[4], coordinates[4]),
			new Location("Zee", addresses[5], coordinates[5]),
			new Location("Aalst stock", addresses[6], coordinates[6], true),
			new Location("Roeselare stock", addresses[7], coordinates[7], true),
			new Location("Gent stock", addresses[8], coordinates[8], true)
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

			// Add the end date

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

		List<Location> locations = await dbContext.Locations.ToListAsync();

		List<Project> projects = await dbContext.Projects.ToListAsync();

		// Initialise as stations without removedAt to allow easier history seeding

		List<Installation> installations = new() {
			new Installation(new DateTime(2025, 1, 15, 12, 20, 25)),
			new Installation(new DateTime(2025, 2, 1, 14, 3, 16)),
			new Installation(new DateTime(2025, 3, 10, 9, 24, 1)),
			new Installation(new DateTime(2025, 4, 5, 3 , 24, 59)),
			new Installation(new DateTime(2025, 5, 12, 23, 50, 40)),
			new Installation(new DateTime(2025, 5, 12, 1, 1, 1))
		};

		List<MeasurementStation> stations = new() {
			new MeasurementStation("STN-001", locations[0], installations[0]),
			new MeasurementStation("STN-002", locations[1], installations[1]),
			new MeasurementStation("STN-003", locations[2], installations[2]),
			new MeasurementStation("STN-004", locations[3], installations[3]),
			new MeasurementStation("STN-005", locations[4], installations[4]),
			new MeasurementStation("STN-006", locations[5], installations[5]),
		};

		dbContext.MeasurementStations.AddRange(stations);
		await dbContext.SaveChangesAsync();
	}

	private async Task MeasurementStationsHistoriesAsync() {
		if (!dbContext.MeasurementStations.Any()) {
			return;
		}

		List<MeasurementStation> stations = await dbContext.MeasurementStations.Include(s => s.Installation).ToListAsync();
		List<Location> locations = await dbContext.Locations.ToListAsync();

		List<DateTime?> removedAts = new() {
			new DateTime(2025, 6, 30, 5, 12, 30),
			new DateTime(2025, 8, 15, 12, 40, 19),
			new DateTime(2025, 9, 25, 16, 1, 21),
			null,
			new DateTime(2025, 12, 31, 9, 1, 1),
			null
		};

		Location DummyLocation = new("Zwartehoekbrug", new Address("", "", "Aalst", "9300"), new Coordinate(50.95, 4.03));

		int i = 0;

		foreach (MeasurementStation station in stations) {

			Installation installation = station.Installation;
			Location location = station.Location;

			// Move the location

			station.MoveTo(DummyLocation);

			// Move the installed at

			station.UpdateInstallation(new Installation(installation.InstalledAt.AddDays(1), installation.RemovedAt));
			await dbContext.SaveChangesAsync();

			// Revert the installed at change

			station.UpdateInstallation(installation);
			await dbContext.SaveChangesAsync();

			// Add removed at

			station.UpdateInstallation(new Installation(installation.InstalledAt, removedAts[i]));
			await dbContext.SaveChangesAsync();

			// Revert the location change

			station.MoveTo(location);

			// Update the counter

			i++;
		}
	}

	private async Task MaintObjectsAsync() {
		if (dbContext.MaintObjects.Any()) {
			return;
		}

		List<MeasurementStation> stations = dbContext.MeasurementStations.Local.Any()
			? dbContext.MeasurementStations.Local.ToList()
			: await dbContext.MeasurementStations.ToListAsync();

		List<Location> stockLocations = dbContext.Locations.Local.Any(l => l.IsStockLocation)
			? dbContext.Locations.Local.Where(l => l.IsStockLocation).ToList()
			: await dbContext.Locations.Where(l => l.IsStockLocation).ToListAsync();

		List<Installation> installations = new() {
			new Installation(new DateTime(2025, 1, 15, 12, 20, 25)),
			new Installation(new DateTime(2025, 2, 1, 14, 3, 16)),
			new Installation(new DateTime(2025, 3, 10, 9, 24, 1)),
			new Installation(new DateTime(2025, 4, 5, 3, 24, 59)),
			new Installation(new DateTime(2025, 5, 12, 23, 50, 40)),
			new Installation(new DateTime(2025, 5, 12, 1, 1, 1))
		};

		// Fetch an existing technician or create a seed technician
		Technician? technician = await dbContext.Technicians.FirstOrDefaultAsync();
		if (technician == null) {
			technician = new Technician("Seed", "Tech", "seed-tech-account");
			dbContext.Technicians.Add(technician);
			await dbContext.SaveChangesAsync();
		}


		List<MaintObjType> types = dbContext.MaintObjTypes.Local.Any()
			? dbContext.MaintObjTypes.Local.ToList()
			: await dbContext.MaintObjTypes.ToListAsync();

		List<MaintObj> maintObjects = new();

		// Create a random generator to get consistent seed data across runs
		Random rnd = new(12345);

		if (stations.Any()) {
			for (int i = 0; i < stations.Count; i++) {
				MeasurementStation station = stations[i];
				Installation installation = installations[i % installations.Count];

				List<MaintObj> objsForStation = new() {
					CreateMaintObj(station, "Sonde", "SN", types.First(t => t.Code == "Sonde"), "Aquatroll",
						installation),
					CreateMaintObj(station, "Logger", "LG", types.First(t => t.Code == "Logger"), "Campbell",
						installation),
					CreateMaintObj(station, "Modem", "MD", types.First(t => t.Code == "Modem"), "Centerion",
						installation),
					CreateMaintObj(station, "SIM", "SI", types.First(t => t.Code == "Sim"), "002.APN.VMM.BE",
						installation),
					CreateMaintObj(station, "Voeding", "VO", types.First(t => t.Code == "Voeding"), "Kyocera",
						installation)
				};

				objsForStation.ForEach(mo => {
					mo.AssignToStation(station);
					mo.RecordTechnician(technician);
					mo.SetStatus(RandomStatus(rnd));
					maintObjects.Add(mo);
				});
			}
		}

		maintObjects.AddRange(CreateStockItems(types, stockLocations));
		if (!stations.Any() && !maintObjects.Any()) {
			maintObjects.AddRange(CreateLegacyItems(types, stockLocations));
		}

		// Ensure stock/legacy items also get a random status (make stock/legacy items inactive by default)
		foreach (MaintObj mo in maintObjects) {
			mo.SetStatus(mo.IsInStock ? MaintObjStatus.Inactief : RandomStatus(rnd));
		}

		dbContext.MaintObjects.AddRange(maintObjects);
		await dbContext.SaveChangesAsync();
	}

	private MaintObj CreateMaintObj(MeasurementStation station, string name, string prefix, MaintObjType maintObjType,
		string brand, Installation installation) {
		return new MaintObj(
			$"{station.Code}-{name}",
			$"{prefix}-{Guid.NewGuid().ToString().Substring(0, 8)}",
			maintObjType,
			brand,
			new Installation(installation.InstalledAt, installation.RemovedAt),
			false,
			null
		);
	}

	private List<MaintObj> CreateStockItems(List<MaintObjType> types, List<Location> stockLocations) {
		return new List<MaintObj> {
			new("Stock-Sonde", "567927", types.First(t => t.Code == "Sonde"), "Aquatroll", null, true, stockLocations[0]),
			new("Stock-Logger", "31045", types.First(t => t.Code == "Logger"), "Campbell", null, true, stockLocations[0]),
			new("Stock-Modem", "354990101102816", types.First(t => t.Code == "Modem"), "Centerion", null, true, stockLocations[1]),
			new("Stock-SIM", "2100267739177", types.First(t => t.Code == "Sim"), "002.APN.VMM.BE", null, true, stockLocations[1]),
			new("Stock-Voeding", "03Y90D0134", types.First(t => t.Code == "Voeding"), "Kyocera", null, true, stockLocations[2])
		};
	}

	private List<MaintObj> CreateLegacyItems(List<MaintObjType> types, List<Location> stockLocations) {
		return new List<MaintObj> {
			new("W-Sonde", "567927", types.First(t => t.Code == "Sonde"), "Aquatroll", null, true, stockLocations[0]),
			new("W-Logger", "31045", types.First(t => t.Code == "Logger"), "Campbell", null, true, stockLocations[0]),
			new("W-Modem", "354990101102816", types.First(t => t.Code == "Modem"), "Centerion", null, true, stockLocations[1]),
			new("W-SIM", "2100267739177", types.First(t => t.Code == "Sim"), "002.APN.VMM.BE", null, true, stockLocations[1]),
			new("W-Voeding", "03Y90D0134", types.First(t => t.Code == "Voeding"), "Kyocera", null, true, stockLocations[2])
		};
	}

	// Helper to pick a random status with weighted probabilities
	private static MaintObjStatus RandomStatus(Random rnd) {
		int roll = rnd.Next(100);
		return roll < 40 ? MaintObjStatus.Actief
			: roll < 80 ? MaintObjStatus.Inactief
			: MaintObjStatus.Defect;
	}
}