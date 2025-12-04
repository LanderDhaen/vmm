using System.Data;
using System.Reflection;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Rise.Persistence;

namespace Rise.Services.Tests.Infrastructure;

public class SqlServerTestDatabaseFixture : IAsyncLifetime {
	private readonly string _connectionTemplate;

	public SqlServerTestDatabaseFixture() {
		// Build configuration from user-secrets only (tests rely exclusively on user-secrets)
		IConfigurationRoot configuration = new ConfigurationBuilder()
			.AddUserSecrets(Assembly.GetExecutingAssembly(), true)
			.Build();

		// Require ConnectionStrings:TestDatabaseConnectionDocker in user-secrets.
		string? csConn = configuration["ConnectionStrings:TestDatabaseConnectionDocker"];
		if (!string.IsNullOrWhiteSpace(csConn)) {
			_connectionTemplate = csConn;
			return;
		}

		throw new InvalidOperationException(
			"Geen ConnectionStrings:TestDatabaseConnectionDocker gevonden in user-secrets. Maak user-secrets aan voor tests/Rise.Services.Tests zoals beschreven in tests/Rise.Services.Tests/README.md en zet 'ConnectionStrings:TestDatabaseConnectionDocker'. Test draait alleen met user-secrets.");
	}

	public string ConnectionString { get; private set; } = null!;
	public string DatabaseName { get; private set; } = null!;

	public async Task InitializeAsync() {
		DatabaseName = "RiseTests_" + Guid.NewGuid().ToString("N");

		// Build master connection to create/drop DB
		SqlConnectionStringBuilder masterBuilder = new(_connectionTemplate);
		masterBuilder.InitialCatalog = "master";
		masterBuilder.TrustServerCertificate = true;

		string masterConnectionString = masterBuilder.ConnectionString;

		await using (SqlConnection conn = new(masterConnectionString)) {
			await conn.OpenAsync();
			using SqlCommand? cmd = conn.CreateCommand();
			cmd.CommandText = $@"IF DB_ID(N'{DatabaseName}') IS NOT NULL
				DROP DATABASE [{DatabaseName}];
			CREATE DATABASE [{DatabaseName}];";
			cmd.CommandType = CommandType.Text;
			await cmd.ExecuteNonQueryAsync();
		}

		// Build final connection string for tests
		SqlConnectionStringBuilder testBuilder = new(_connectionTemplate);
		testBuilder.InitialCatalog = DatabaseName;
		testBuilder.TrustServerCertificate = true;
		ConnectionString = testBuilder.ConnectionString;

		// Apply migrations from the Rise.Persistence assembly
		DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder =
			new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseSqlServer(ConnectionString,
					sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

		await using (ApplicationDbContext context = new(optionsBuilder.Options)) {
			await context.Database.MigrateAsync();
		}
	}

	public async Task DisposeAsync() {
		try {
			SqlConnectionStringBuilder masterBuilder = new(_connectionTemplate);
			masterBuilder.InitialCatalog = "master";
			masterBuilder.TrustServerCertificate = true;
			string masterConnectionString = masterBuilder.ConnectionString;

			await using (SqlConnection conn = new(masterConnectionString)) {
				await conn.OpenAsync();
				using SqlCommand? cmd = conn.CreateCommand();
				cmd.CommandText =
					$@"ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE IF EXISTS [{DatabaseName}];";
				cmd.CommandType = CommandType.Text;
				await cmd.ExecuteNonQueryAsync();
			}
		} catch (Exception ex) {
			// Swallow exceptions during cleanup but log to console so CI users can see the issue
			Console.WriteLine($"[SqlServerTestDatabaseFixture] cleanup failed: {ex}");
		}
	}

	public DbContextOptions<TContext> CreateDbContextOptions<TContext>() where TContext : DbContext {
		DbContextOptionsBuilder<TContext> builder = new();
		builder.UseSqlServer(ConnectionString,
			sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
		return builder.Options;
	}
}