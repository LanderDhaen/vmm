using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Rise.Domain.Locations;
using Rise.Domain.Products;
using Rise.Domain.Projects;

namespace Rise.Persistence;

/// <summary>
///     Seeds the database
/// </summary>
/// <param name="dbContext"></param>
/// <param name="roleManager"></param>
/// <param name="userManager"></param>
public class DbSeeder(
	ApplicationDbContext dbContext,
	RoleManager<IdentityRole> roleManager,
	UserManager<IdentityUser> userManager) {
	private const string PasswordDefault = "A1b2C3!";

	public async Task SeedAsync() {
		await RolesAsync();
		await UsersAsync();
		await ProductsAsync();
		await LocationAsync();
		await ProjectsAsync();
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


	private async Task ProductsAsync() {
		if (dbContext.Products.Any()) {
			return;
		}

		dbContext.Products.AddRange(
			new Product { Name = "Laptop", Description = "15-inch display, 16GB RAM" },
			new Product { Name = "Smartphone", Description = "6.5-inch screen, 128GB storage" },
			new Product { Name = "Headphones", Description = "Wireless noise-cancelling" },
			new Product { Name = "Keyboard", Description = "Mechanical RGB backlit" },
			new Product { Name = "Mouse", Description = "Ergonomic wireless mouse" },
			new Product { Name = "Monitor", Description = "27-inch 4K UHD display" },
			new Product { Name = "Printer", Description = "All-in-one inkjet printer" },
			new Product { Name = "Camera", Description = "Mirrorless 24MP with 4K video" },
			new Product { Name = "Smartwatch", Description = "Heart rate monitor, GPS" },
			new Product { Name = "Speaker", Description = "Bluetooth portable speaker" }
		);

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
			new Address(null,null, "Breda", "4811", "NL"),
			};
		
		List<Coordinate> coordinates = new() {
			new Coordinate(4.35, 50.85),   // Brussel (Koningstraat 12)
			new Coordinate(4.40, 51.22),   // Antwerpen
			new Coordinate(3.73, 51.05),   // Gent
			new Coordinate(4.37, 50.84),   // Bruxelles (Rue de la Loi)
			new Coordinate(4.78, 51.59),   // Breda
			new Coordinate(2.92, 51.22)    // Oostende
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

		List<Technician> technicians = await dbContext.Technicians.ToListAsync();

		if (!technicians.Any()) {
			return;
		}

		List<Location> locations = await dbContext.Locations.ToListAsync();
		
		List<DateRange> dateRanges = new() {
			new DateRange(new DateOnly(2025, 1, 15), new DateOnly(2025, 6, 30)),
			new DateRange(new DateOnly(2025, 2, 1), new DateOnly(2025, 8, 15)),
			new DateRange(new DateOnly(2025, 3, 10), new DateOnly(2025, 9, 25)),
			new DateRange(new DateOnly(2025, 4, 5), null),
			new DateRange(new DateOnly(2025, 5, 12), new DateOnly(2025, 12, 31)),
			new DateRange(new DateOnly(2025, 5, 12), null)
		};

		Random rnd = new(123); // deterministic seed

		List<Project> projects = new() {
			new Project("Canal Cleanup", "CC2025", dateRanges[0], locations[0]),         
			new Project("River Monitoring", "RM2025", dateRanges[1], locations[1]),      
			new Project("Boating Safety Upgrade", "BSU2025", dateRanges[2], locations[2]), 
			new Project("Pond Restoration", "PR2025", dateRanges[3], locations[3]),      
			new Project("Waterway CRM Analytics", "WCA2025", dateRanges[4], locations[4]), 
			new Project("Coastal Dashboard", "CD2025", dateRanges[5], locations[5]) 
		};



		dbContext.Projects.AddRange(projects);
		await dbContext.SaveChangesAsync();
	}
}