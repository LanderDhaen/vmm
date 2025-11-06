using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Rise.Persistence;

namespace Rise.Services.Tests.Common;

public static class DbContextFactory {
	public static ApplicationDbContext CreateContext(out SqliteConnection connection) {
		connection = new SqliteConnection("DataSource=:memory:");
		connection.Open(); // Belangrijk: open houden zolang de context leeft!

		DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseSqlite(connection)
			.Options;

		ApplicationDbContext context = new(options);
		context.Database.EnsureCreated(); // maakt tabellen aan
		return context;
	}
}