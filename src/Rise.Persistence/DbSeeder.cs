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

		List<Zone> zones = [
			new("West", "ASTA"),
			new("Oost", "HSTA")
		];

		dbContext.Zones.AddRange(zones);
		await dbContext.SaveChangesAsync();
	}

	private async Task MeasurementNetworksAsync() {
		if (dbContext.MeasurementNetworks.Any()) {
			return;
		}

		List<Zone> zones = await dbContext.Zones.ToListAsync();
		if (zones.Count == 0) {
			return;
		}

		List<MeasurementNetwork> measurementNetworks = [
			new("Conductiviteitmeetnet West", zones[0]),
			new("Multiparameter West", zones[0]),
			new("Nitraatmeetnet West", zones[0]),
			new("Overstortmeetnet West", zones[0]),
			new("Conductiviteitmeetnet Oost", zones[1]),
			new("Multiparametermeetnet Oost", zones[1]),
			new("Nitraatmeetnet Oost", zones[1]),
			new("Overstortmeetnet Oost", zones[1])
		];

		dbContext.MeasurementNetworks.AddRange(measurementNetworks);
		await dbContext.SaveChangesAsync();
	}


	private async Task LocationAsync() {
		if (dbContext.Locations.Any()) {
			return;
		}

		List<Address> addresses = [
			new(null, null, "Brussel", "1020"),
			new("Meersstraat 5", "Bus 2", "Harlbeke", "8530"),
			new("Duiklaan", null, "Gent", "9000"),
			new("Vaartstraat", null, "Aalst", "9300"),
			new(null, null, "Breda", "4811", "NL"),
			new("N9", null, "Aalst", "9300"),

			// Stock

			new("Industrielaan 15", null, "Aalst", "9320"),
			new("Industrieweg 124", null, "Gent", "9040")
		];

		List<Coordinate> coordinates = [
			new(50.88486, 4.36695),
			new(50.84326, 3.32974),
			new(51.04535, 3.68276),
			new(50.94093, 4.04293),
			new(51.60512, 4.75128),
			new(50.93499, 4.04801),

			// Stock

			new(50.91215, 4.03795),
			new(51.09013, 3.68663)
		];

		List<Location> locations = [
			new("Koninklijk Domein van Laken", addresses[0], coordinates[0]),
			new("Provinciedomein De Gavers", addresses[1], coordinates[1]),
			new("Duikplaats Blaarmeersen", addresses[2], coordinates[2]),
			new("Sint-Annabrug", addresses[3], coordinates[3]),
			new("Emerput", addresses[4], coordinates[4]),
			new("Zeebergbrug", addresses[5], coordinates[5]),

			// Stock

			new("Stock Aalst", addresses[6], coordinates[6], true),
			new("Stock Gent", addresses[7], coordinates[7], true)
		];

		dbContext.Locations.AddRange(locations);
		await dbContext.SaveChangesAsync();
	}

	private async Task ProjectsAsync() {
		if (dbContext.Projects.Any()) {
			return;
		}

		List<MeasurementNetwork> networks = await dbContext.MeasurementNetworks.ToListAsync();
		if (networks.Count == 0) {
			return;
		}

		// Initialise as projects without endDate to allow easier history seeding

		List<DateRange> dateRanges = [
			new(new DateOnly(2025, 1, 15)),
			new(new DateOnly(2025, 2, 1)),
			new(new DateOnly(2025, 3, 10)),
			new(new DateOnly(2025, 4, 5)),
			new(new DateOnly(2025, 5, 12)),
			new(new DateOnly(2025, 5, 12)),
			new(new DateOnly(2025, 4, 6))
		];

		List<Project> projects = [
			new("Conductiviteitsmonitoring Koninklijk Domein", "2025-001", dateRanges[0], networks[0]),
			new("Multiparameteranalyse Gavers", "2025-002", dateRanges[1], networks[1]),
			new("Nitraatobservatie Blaarmeersen", "2025-003", dateRanges[2], networks[2]),
			new("Overstortdetectie Aalst", "2025-004", dateRanges[3], networks[3]),
			new("Conductiviteitsmonitoring Aalst", "2025-005", dateRanges[4], networks[4]),
			new("Multiparameteranalyse", "2025-006", dateRanges[5], networks[5]),
			new("Conductivitetiesmonitoring Aalst", "2025-007", dateRanges[6], networks[0])
		];


		dbContext.Projects.AddRange(projects);
		await dbContext.SaveChangesAsync();
	}

	private async Task ProjectHistoriesAsync() {
		if (!dbContext.Projects.Any()) {
			return;
		}

		List<Project> projects = await dbContext.Projects.Include(p => p.DateRange).Include(p => p.MeasurementNetwork)
			.ToListAsync();

		List<DateOnly?> endDates = [
			new DateOnly(2025, 6, 30),
			new DateOnly(2025, 8, 15),
			new DateOnly(2025, 9, 25),
			null,
			new DateOnly(2025, 12, 31),
			null,
			null
		];

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

		List<Installation> installations = [
			new(new DateTime(2025, 1, 15, 12, 20, 25)),
			new(new DateTime(2025, 2, 1, 14, 3, 16)),
			new(new DateTime(2025, 3, 10, 9, 24, 1)),
			new(new DateTime(2025, 4, 5, 3, 24, 59)),
			new(new DateTime(2025, 5, 12, 23, 50, 40)),
			new(new DateTime(2025, 5, 12, 1, 1, 1))
		];

		List<MeasurementStation> stations = [
			new("STN-001", locations[0], installations[0]),
			new("STN-002", locations[1], installations[1]),
			new("STN-003", locations[2], installations[2]),
			new("STN-004", locations[3], installations[3]),
			new("STN-005", locations[4], installations[4]),
			new("STN-006", locations[5], installations[5])
		];

		// Add projects to stations

		stations[0].Projects.Add(projects[0]);
		stations[1].Projects.Add(projects[1]);
		stations[2].Projects.Add(projects[2]);
		stations[3].Projects.Add(projects[3]);
		stations[4].Projects.Add(projects[4]);
		stations[5].Projects.Add(projects[3]);

		dbContext.MeasurementStations.AddRange(stations);
		await dbContext.SaveChangesAsync();
	}

	private async Task MeasurementStationsHistoriesAsync() {
		if (!dbContext.MeasurementStations.Any()) {
			return;
		}

		List<MeasurementStation> stations =
			await dbContext.MeasurementStations.Include(s => s.Installation).ToListAsync();
		List<Location> locations = await dbContext.Locations.ToListAsync();

		List<DateTime?> removedAts = [
			new DateTime(2025, 6, 30, 5, 12, 30),
			new DateTime(2025, 8, 15, 12, 40, 19),
			new DateTime(2025, 9, 25, 16, 1, 21),
			null,
			new DateTime(2025, 12, 31, 9, 1, 1),
			null
		];

		Location dummyLocation =
			new("Zwartehoekbrug", new Address("", "", "Aalst", "9300"), new Coordinate(50.95, 4.03));

		int i = 0;

		foreach (MeasurementStation station in stations) {
			Installation installation = station.Installation;
			Location location = station.Location;

			// Move the location

			station.MoveTo(dummyLocation);

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

		List<MeasurementStation> stations = dbContext.MeasurementStations.Local.Count != 0
			? dbContext.MeasurementStations.Local.ToList()
			: await dbContext.MeasurementStations.ToListAsync();

		List<Location> stockLocations = dbContext.Locations.Local.Any(l => l.IsStockLocation)
			? dbContext.Locations.Local.Where(l => l.IsStockLocation).ToList()
			: await dbContext.Locations.Where(l => l.IsStockLocation).ToListAsync();

		List<Installation> installations = [
			new(new DateTime(2025, 1, 15, 12, 20, 25)),
			new(new DateTime(2025, 2, 1, 14, 3, 16)),
			new(new DateTime(2025, 3, 10, 9, 24, 1)),
			new(new DateTime(2025, 4, 5, 3, 24, 59)),
			new(new DateTime(2025, 5, 12, 23, 50, 40)),
			new(new DateTime(2025, 5, 12, 1, 1, 1))
		];

		// Fetch an existing technician or create a seed technician
		Technician? technician = await dbContext.Technicians.FirstOrDefaultAsync();
		if (technician == null) {
			technician = new Technician("Seed", "Tech", "seed-tech-account");
			dbContext.Technicians.Add(technician);
			await dbContext.SaveChangesAsync();
		}


		List<MaintObjType> types = dbContext.MaintObjTypes.Local.Count != 0
			? dbContext.MaintObjTypes.Local.ToList()
			: await dbContext.MaintObjTypes.ToListAsync();

		List<MaintObj> maintObjects = [];

		// Create a random generator to get consistent seed data across runs
		Random rnd = new(12345);

		if (stations.Count != 0) {
			for (int i = 0; i < stations.Count; i++) {
				MeasurementStation station = stations[i];
				Installation installation = installations[i % installations.Count];

				List<MaintObj> objsForStation = [
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
				];

				objsForStation.ForEach(mo => {
					mo.AssignToStation(station);
					mo.RecordTechnician(technician);
					mo.SetStatus(RandomStatus(rnd));
					maintObjects.Add(mo);
				});
			}
		}

		maintObjects.AddRange(CreateStockItems(types, stockLocations));
		if (stations.Count == 0 && maintObjects.Count == 0) {
			maintObjects.AddRange(CreateLegacyItems(types, stockLocations));
		}

		// Ensure stock/legacy items also get a random status (make stock/legacy items inactive by default)
		foreach (MaintObj mo in maintObjects) {
			mo.SetStatus(mo.IsInStock ? MaintObjStatus.Inactief : RandomStatus(rnd));
		}

		dbContext.MaintObjects.AddRange(maintObjects);
		await dbContext.SaveChangesAsync();
	}

	private static MaintObj CreateMaintObj(MeasurementStation station, string name, string prefix,
		MaintObjType maintObjType,
		string brand, Installation installation) {
		return new MaintObj(
			$"{station.Code}-{name}",
			$"{prefix}-{Guid.NewGuid().ToString()[..8]}",
			maintObjType,
			brand,
			new Installation(installation.InstalledAt, installation.RemovedAt),
			false,
			null
		);
	}

	private static List<MaintObj> CreateStockItems(List<MaintObjType> types, List<Location> stockLocations) {
		return [
			new MaintObj("Stock-Sonde", "567927", types.First(t => t.Code == "Sonde"), "Aquatroll", null, true,
				stockLocations[0]),

			new MaintObj("Stock-Logger", "31045", types.First(t => t.Code == "Logger"), "Campbell", null, true,
				stockLocations[0]),

			new MaintObj("Stock-Modem", "354990101102816", types.First(t => t.Code == "Modem"), "Centerion", null, true,
				stockLocations[1]),

			new MaintObj("Stock-SIM", "2100267739177", types.First(t => t.Code == "Sim"), "002.APN.VMM.BE", null, true,
				stockLocations[1]),

			new MaintObj("Stock-Voeding", "03Y90D0134", types.First(t => t.Code == "Voeding"), "Kyocera", null, true,
				stockLocations[1])
		];
	}

	private static List<MaintObj> CreateLegacyItems(List<MaintObjType> types, List<Location> stockLocations) {
		return [
			new MaintObj("W-Sonde", "567927", types.First(t => t.Code == "Sonde"), "Aquatroll", null, true,
				stockLocations[0]),
			new MaintObj("W-Logger", "31045", types.First(t => t.Code == "Logger"), "Campbell", null, true,
				stockLocations[0]),
			new MaintObj("W-Modem", "354990101102816", types.First(t => t.Code == "Modem"), "Centerion", null, true,
				stockLocations[1]),

			new MaintObj("W-SIM", "2100267739177", types.First(t => t.Code == "Sim"), "002.APN.VMM.BE", null, true,
				stockLocations[1]),

			new MaintObj("W-Voeding", "03Y90D0134", types.First(t => t.Code == "Voeding"), "Kyocera", null, true,
				stockLocations[2])
		];
	}

	// Helper to pick a random status with weighted probabilities
	private static MaintObjStatus RandomStatus(Random rnd) {
		int roll = rnd.Next(100);
		return roll < 40 ? MaintObjStatus.Actief
			: roll < 80 ? MaintObjStatus.Inactief
			: MaintObjStatus.Defect;
	}
}