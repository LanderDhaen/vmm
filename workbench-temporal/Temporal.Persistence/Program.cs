using Microsoft.EntityFrameworkCore;

using Temporal.Domain.Projects;
using Temporal.Domain.Stations;
using Temporal.Persistence.Triggers;

namespace Temporal.Persistence;

public class Program
{
	public static async Task Main()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseSqlServer("Server=localhost;Database=TemporalDb;Trusted_Connection=True;TrustServerCertificate=True;")
			.EnableDetailedErrors()
			.UseTriggers(options => options.AddTrigger<EntityBeforeSaveTrigger>())
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
		
		List<Project> projects = await db.Projects.Include(p => p.ProjectHistories).ToListAsync();
		
		Console.WriteLine("--- Projects ---");
		foreach (Project p in projects) {
			Console.WriteLine($"Project: {p.Name}, {p.Code} ");
			foreach (ProjectHistory h in p.ProjectHistories) {
				Console.WriteLine(
					$"  - History: {h.Name}, {h.Code} ({h.ValidityRange.ValidFrom} - {h.ValidityRange.ValidTo})");
			}
		}

		List<MeasurementStation> measurementStations =
			await db.MeasurementStations.Include(m => m.MeasurementStationHistories).ToListAsync();
			
		Console.WriteLine("--- Measurement Stations ---");
		foreach (MeasurementStation m in measurementStations) {
			Console.WriteLine($"Measurement Station: {m.Code}");
			foreach (MeasurementStationHistory h in m.MeasurementStationHistories) {
				Console.WriteLine($"  - History: {h.Code} ({h.ValidityRange.ValidFrom} - {h.ValidityRange.ValidTo})");
			}
		}
		
	}
}