using System.Security.Claims;

using Ardalis.Result;

using Microsoft.EntityFrameworkCore;

using Rise.Domain.Networks;
using Rise.Domain.Projects;
using Rise.Persistence;
using Rise.Services.Projects;
using Rise.Services.Tests.Fakers;
using Rise.Shared.Projects;

using Testcontainers.MsSql;

namespace Rise.Services.Tests.Projects;

public class ProjectServiceShould {
	// One container for the entire test class
	private static readonly MsSqlContainer? _container;
	private static readonly ApplicationDbContext? _dbContext;
	private readonly ProjectService _service;

	static ProjectServiceShould() {
		_container = new MsSqlBuilder()
			.WithImage("mcr.microsoft.com/mssql/server:2022-latest")
			.WithPassword("YourStrong(!)Password")
			.Build();

		_container.StartAsync().GetAwaiter().GetResult();
		// WaitForSqlReadyAsync(_container.GetConnectionString()).GetAwaiter().GetResult();

		DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseSqlServer(_container.GetConnectionString())
			.Options;

		_dbContext = new ApplicationDbContext(options);
		_dbContext.Database.Migrate();
	}

	public ProjectServiceShould() {
		ClaimsPrincipal fakeUser = new(
			new ClaimsIdentity(
				[new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())],
				"TestAuth"));

		FakeSessionContextProvider session = new(fakeUser);

		_service = new ProjectService(_dbContext!, session);
	}

	[Fact]
	public async Task GetDetailAsync_ShouldReturnNotFound_WhenProjectDoesNotExist() {
		Result<ProjectResponse.Detail> result = await _service.GetDetailAsync(Guid.NewGuid());
		Assert.True(result.IsNotFound());
	}

	[Fact]
	public async Task GetDetailAsync_ShouldReturnProjectDetails_WhenProjectExists() {
		MeasurementNetwork network = new("MN1", new Zone("Zone1", "Z1"));
		_dbContext!.MeasurementNetworks.Add(network);
		Project project = new(
			"Test Project", "CODE1",
			new DateRange(DateOnly.FromDateTime(DateTime.Today)), network);
		_dbContext.Projects.Add(project);
		await _dbContext.SaveChangesAsync();

		Result<ProjectResponse.Detail> result = await _service.GetDetailAsync(project.Id);

		Assert.True(result.IsSuccess);
		Assert.Equal("Test Project", result.Value!.Project!.Name);
	}

	//Helper to wait until SQL is ready
	// Optional
	// private static async Task WaitForSqlReadyAsync(string connectionString, int timeoutSeconds = 30) {
	// 	DateTime start = DateTime.UtcNow;
	// 	while (true) {
	// 		try {
	// 			using SqlConnection conn = new(connectionString);
	// 			await conn.OpenAsync();
	// 			return;
	// 		} catch {
	// 			if ((DateTime.UtcNow - start).TotalSeconds > timeoutSeconds) {
	// 				throw new TimeoutException("SQL Server did not become ready in time.");
	// 			}
	//
	// 			await Task.Delay(500);
	// 		}
	// 	}
	// }
}