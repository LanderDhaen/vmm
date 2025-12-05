using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Rise.Domain.Locations;
using Rise.Domain.MaintObjects;
using Rise.Domain.Networks;
using Rise.Domain.Projects;
using Rise.Domain.Stations;
using Rise.Domain.WorkOrders;

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
		await MaintObjectPlansAsync();
		await MaintObjectsAsync();
		await WorkOrdersAsync();
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
			new Technician("William", "Willems", technicianAccount1.Id),
			new Technician("Geert", "Gertens", technicianAccount2.Id));

		await dbContext.SaveChangesAsync();
	}

	private async Task ZonesAsync() {
		if (dbContext.Zones.Any()) {
			return;
		}

		List<Zone> zones = new() {
			new Zone("West", "ASTA"),
			new Zone("Oost", "HSTA")
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
			new Address(null, null, "Brussel", "1020"),
			new Address("Meersstraat 5", "Bus 2", "Harelbeke", "8530"),
			new Address("Duiklaan", null, "Gent", "9000"),
			new Address("Vaartstraat", null, "Aalst", "9300"),
			new Address(null, null, "Breda", "4811", "NL"),
			new Address("N9", null, "Aalst", "9300"),

			// Stock

			new Address("Industrielaan 15", null, "Aalst", "9320"),
			new Address("Industrieweg 124", null, "Gent", "9040"),
			new Address("Olieweg 70", null, "Antwerpen", "2020")
		};

		List<Coordinate> coordinates = new() {
			new Coordinate(50.88486, 4.36695),
			new Coordinate(50.84326, 3.32974),
			new Coordinate(51.04535, 3.68276),
			new Coordinate(50.94093, 4.04293),
			new Coordinate(51.60512, 4.75128),
			new Coordinate(50.93499, 4.04801),

			// Stock

			new Coordinate(50.91215, 4.03795),
			new Coordinate(51.09013, 3.68663),
			new Coordinate(51.19292, 4.36380)
		};

		List<Location> locations = new() {
			new Location("Koninklijk Domein van Laken", addresses[0], coordinates[0]),
			new Location("Provinciedomein De Gavers", addresses[1], coordinates[1]),
			new Location("Duikplaats Blaarmeersen", addresses[2], coordinates[2]),
			new Location("Sint-Annabrug", addresses[3], coordinates[3]),
			new Location("Emerput", addresses[4], coordinates[4]),
			new Location("Zeebergbrug", addresses[5], coordinates[5]),

			// Stock

			new Location("Stock Aalst", addresses[6], coordinates[6], true),
			new Location("Stock Gent", addresses[7], coordinates[7], true),
			new Location("Stock Antwerpen", addresses[8], coordinates[8], true),
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
			new DateRange(new DateOnly(2025, 5, 12)),
			new DateRange(new DateOnly(2025, 4, 6))
		};

		List<Project> projects = new() {
			new Project("Conductiviteitsmonitoring Koninklijk Domein", "2025-001", dateRanges[0], networks[0]),
			new Project("Multiparameteranalyse Gavers", "2025-002", dateRanges[1], networks[1]),
			new Project("Nitraatobservatie Blaarmeersen", "2025-003", dateRanges[2], networks[2]),
			new Project("Overstortdetectie Aalst", "2025-004", dateRanges[3], networks[3]),
			new Project("Conductiviteitsmonitoring Aalst", "2025-005", dateRanges[4], networks[4]),
			new Project("Multiparameteranalyse", "2025-006", dateRanges[5], networks[5]),
			new Project("Conductiviteitsmonitoring Aalst", "2025-007", dateRanges[6], networks[0])
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
			null,
			null
		};

		int i = 0;

		foreach (Project project in projects) {
			DateRange dateRange = project.DateRange;

			// Move the start date

			project.Edit(project.Name, project.Code, new DateRange(dateRange.StartDate.AddDays(1), dateRange.EndDate));
			await dbContext.SaveChangesAsync();

			// Revert change

			project.Edit(project.Name, project.Code, dateRange);
			await dbContext.SaveChangesAsync();

			// Add the end date

			project.Edit(project.Name, project.Code, new DateRange(dateRange.StartDate, endDates[i]));
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
			new Installation(new DateTime(2025, 4, 5, 3, 24, 59)),
			new Installation(new DateTime(2025, 5, 12, 23, 50, 40)),
			new Installation(new DateTime(2025, 5, 12, 1, 1, 1))
		};

		List<MeasurementStation> stations = new() {
			new MeasurementStation("STN-001", locations[0], installations[0]),
			new MeasurementStation("STN-002", locations[1], installations[1]),
			new MeasurementStation("STN-003", locations[2], installations[2]),
			new MeasurementStation("STN-004", locations[3], installations[3]),
			new MeasurementStation("STN-005", locations[4], installations[4]),
			new MeasurementStation("STN-006", locations[5], installations[5])
		};

		// Add one corresponding project to each stations 

		stations[0].Projects.Add(projects[0]);
		stations[1].Projects.Add(projects[1]);
		stations[2].Projects.Add(projects[2]);
		stations[3].Projects.Add(projects[3]);
		stations[4].Projects.Add(projects[4]);
		stations[5].Projects.Add(projects[5]);

		// Add second project to stations in Aalst

		stations[3].Projects.Add(projects[5]);
		stations[5].Projects.Add(projects[3]);

		dbContext.MeasurementStations.AddRange(stations);
		await dbContext.SaveChangesAsync();
	}

	private async Task MeasurementStationsHistoriesAsync() {
		if (!dbContext.MeasurementStations.Any()) {
			return;
		}

		List<MeasurementStation> stations =
			await dbContext.MeasurementStations
				.Include(s => s.Installation)
				.Include(s => s.Location).ThenInclude(l => l.Address)
				.Include(s => s.Location).ThenInclude(l => l.Coordinate)
				.ToListAsync();

		List<DateTime?> removedAts = new() {
			new DateTime(2025, 6, 30, 5, 12, 30),
			new DateTime(2025, 8, 15, 12, 40, 19),
			new DateTime(2025, 9, 25, 16, 1, 21),
			null,
			new DateTime(2025, 12, 31, 9, 1, 1),
			null
		};

		Location DummyLocation =
			new("Zwartehoekbrug", new Address("", "", "Aalst", "9300"), new Coordinate(50.95, 4.03));

		int i = 0;

		foreach (MeasurementStation station in stations) {
			Installation installation = station.Installation;
			Location location = station.Location;

			// Move the location

			station.MoveTo(DummyLocation);
			await dbContext.SaveChangesAsync();

			// Move the installed at

			station.UpdateInstallation(new Installation(installation.InstalledAt.AddYears(-1).AddMonths(-2).AddDays(-5), installation.RemovedAt));
			await dbContext.SaveChangesAsync();

			// Revert the installed at change

			station.UpdateInstallation(installation);
			await dbContext.SaveChangesAsync();

			// Add removed at

			station.UpdateInstallation(new Installation(installation.InstalledAt, removedAts[i]));
			await dbContext.SaveChangesAsync();

			// Revert the location change

			station.MoveTo(location);
			await dbContext.SaveChangesAsync();

			// Update the counter

			i++;
		}
	}

	private async Task MaintObjectPlansAsync() {
		if (dbContext.MaintObjPlans.Any()) {
			return;
		}

		List<MaintObjPlan> maintObjPlans = new() {
			new MaintObjPlan("Korter", 7, 28),
			new MaintObjPlan("Standaard", 14, 56),
			new MaintObjPlan("Langer", 21, 84)
		};
		
		dbContext.MaintObjPlans.AddRange(maintObjPlans);
		await dbContext.SaveChangesAsync();
	}

	private async Task MaintObjectsAsync() {
		if (dbContext.MaintObjects.Any()) {
			return;
		}

		List<MaintObjPlan> plans = await dbContext.MaintObjPlans.ToListAsync();
		List<Location> stockLocations = await dbContext.Locations.Where(x => x.IsStockLocation).ToListAsync();
		
		// Adding devices to one of the stations in Aalst
		
		List<MaintObj> maintObjsAalst = new()
		{
			new MaintObj("BT1000", "48291", MaintObjType.Battery, plans[0], null, stockLocations[0]),
			new MaintObj("CR3000", "24365", MaintObjType.Logger, plans[1], "Campbell Scientific", stockLocations[0]), // Matches one of the provided barcodes
			new MaintObj("SA5000", "16308", MaintObjType.Sensor, plans[2], "In-Situ", stockLocations[0]),
			new MaintObj("SM1200", "72459", MaintObjType.Sim, plans[0], null, stockLocations[0]),
			new MaintObj("CT9300", "59142", MaintObjType.Modem, plans[1], "Cinterion", stockLocations[0]),
		};

		MeasurementStation stationAalst = await dbContext.MeasurementStations.Where(x => x.Location.Name == "Zeebergbrug").FirstAsync();
		
		foreach (MaintObj maintObj in maintObjsAalst) {
			maintObj.AssignToStation(stationAalst);
			if (maintObj.MaintObjType == MaintObjType.Battery ||  maintObj.MaintObjType == MaintObjType.Logger) {
				maintObj.SetStatus(MaintObjStatus.Broken);
			}
		}
		
		dbContext.MaintObjects.AddRange(maintObjsAalst);
		
		// Adding devices to one of the stations in Gent

		List<MaintObj> maintObjsGent = new() {
			new MaintObj("BT1000", "80613", MaintObjType.Battery, plans[0], null, stockLocations[1]),
			new MaintObj("CR3000", "27486", MaintObjType.Logger, plans[1], "Campbell Scientific", stockLocations[1]),
			new MaintObj("SA5000", "91837", MaintObjType.Sensor, plans[2], "In-Situ", stockLocations[1]),
			new MaintObj("SM1200", "34527", MaintObjType.Sim, plans[0], null, stockLocations[1]),
			new MaintObj("CT9300", "76094", MaintObjType.Modem, plans[1], "Cinterion", stockLocations[1]),
		};
		
		MeasurementStation stationGent = await dbContext.MeasurementStations.Where(x => x.Location.Name == "Duikplaats Blaarmeersen").FirstAsync();
		
		foreach (MaintObj maintObj in maintObjsGent) {
			maintObj.AssignToStation(stationGent);
			if (maintObj.MaintObjType == MaintObjType.Modem) {
				maintObj.SetStatus(MaintObjStatus.Broken);
			}
		}
		
		dbContext.MaintObjects.AddRange(maintObjsGent);
		
		// Adding devices to the stock in Antwerpen

		List<MaintObj> maintObjsAntwerpen = new() {
			new MaintObj("BT1000", "15248", MaintObjType.Battery, plans[0], null, stockLocations[2]),
			new MaintObj("CR3000", "68973", MaintObjType.Logger, plans[1], "Campbell Scientific", stockLocations[2]),
			new MaintObj("SA5000", "43105", MaintObjType.Sensor, plans[2], "In-Situ", stockLocations[2]),
			new MaintObj("SM1200", "87962", MaintObjType.Sim, plans[0], null, stockLocations[2]),
			new MaintObj("CT9300", "50736", MaintObjType.Modem, plans[1], "Cinterion", stockLocations[2]),
		};
		
		dbContext.MaintObjects.AddRange(maintObjsAntwerpen);
		
		// Saving changes
		
		await dbContext.SaveChangesAsync();
		
	}

	private async Task WorkOrdersAsync() {
		if (dbContext.WorkOrders.Any()) {
			return;
		}

		// 

		List<MaintObj> maintObjs = await dbContext.MaintObjects.Include(x => x.MaintObjPlan).ToListAsync();
		List<WorkOrder> workOrders = [];

		// Creating workorders

		foreach (MaintObj maintObj in maintObjs) {
			if (maintObj.MaintObjStatus == MaintObjStatus.Broken) {
				workOrders.Add(new WorkOrder($"{maintObj.MaintObjPlan.Name} vervangen",
					$"{maintObj.Name} met serienummer {maintObj.SerialNumber} is defect door waterschade",
					Priority.High, maintObj));
			}

			if (maintObj.MaintObjStatus == MaintObjStatus.Active) {

				workOrders.Add(new WorkOrder($"{maintObj.MaintObjPlan.Name} onderhouden",
					$"{maintObj.Name} met serienummer {maintObj.SerialNumber} moet onderhouden worden volgens onderhoudsplan",
					Priority.Normal, maintObj));

				workOrders.Add(new WorkOrder($"{maintObj.MaintObjPlan.Name} kalibreren",
					$"{maintObj.Name} met serienummer {maintObj.SerialNumber} moet gekalibreerd worden volgens onderhoudsplan",
					Priority.Normal, maintObj));
			}

			if (maintObj.MaintObjStatus == MaintObjStatus.Inactive) {
				workOrders.Add(new WorkOrder($"{maintObj.MaintObjPlan.Name} plaatsen",
					$"{maintObj.Name} met serienummer {maintObj.SerialNumber} is geleverd en kan geplaatst worden",
					Priority.Low, maintObj));
			}
		}

		// Assigning every second and third work order, finishing every third work order

		int i = 0;

		List<Technician> technicians = await dbContext.Technicians.ToListAsync();

		foreach (WorkOrder workOrder in workOrders) {
			if (i % 2 == 0) {
				workOrder.AssignTechnician(technicians[0]);
			}

			if (i % 3 == 0) {
				workOrder.AssignTechnician(technicians[1]);
				workOrder.MarkAsCompleted();
			}
			i++;
		}

		dbContext.WorkOrders.AddRange(workOrders);
		await dbContext.SaveChangesAsync();
	}
}