using Microsoft.EntityFrameworkCore;

namespace Temporal.Persistence;

public class Program
{
	public static async Task Main()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseSqlServer("Server=localhost;Database=TemporalDb;Trusted_Connection=True;TrustServerCertificate=True;")
			.Options;

		var db = new ApplicationDbContext(options);
		
		Console.WriteLine("Deleting database...");
		await db.Database.EnsureDeletedAsync();

		Console.WriteLine("Creating database...");
		await db.Database.EnsureCreatedAsync();

		Console.WriteLine("Seeding data...");
		var seeder = new DbSeeder(db);
		await seeder.SeedAsync();

		Console.WriteLine("Database created and seeded successfully.");
	}
}