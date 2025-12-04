using System.Security.Claims;

using Ardalis.Result;

using Microsoft.EntityFrameworkCore;

using Rise.Persistence;
using Rise.Services.Projects;
using Rise.Services.Tests.Fakers;
using Rise.Shared.Projects;

using Testcontainers.MsSql;

namespace Rise.Services.Tests.Projects;

public class ProjectServiceShould {
	[Fact]
	public async Task EditAsync_Should_Return_NotFound_When_Project_Does_Not_Exist() {
		// 1. Start MSSQL Testcontainer
		MsSqlContainer? container = new MsSqlBuilder()
			.WithPassword("YourStrong(!)Password")
			.Build();

		await container.StartAsync();

		try {
			// 2. Maak DbContext met de container connection string
			DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseSqlServer(container.GetConnectionString())
				.Options;

			await using ApplicationDbContext db = new(options);

			// 3. Zorg dat database schema klaar is
			await db.Database.MigrateAsync();

			// 4. Fake sessionprovider (simpel)
			ClaimsPrincipal fakeUser = new(new ClaimsIdentity([
				new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
			], "TestAuth"));

			FakeSessionContextProvider session = new(fakeUser);

			// 5. Maak ProjectService
			ProjectService service = new(db, session);

			// 6. Test: EditAsync met niet-bestaand project
			ProjectRequest.Edit request = new() {
				ProjectId = Guid.NewGuid(),
				Name = "Test",
				Code = "TEST",
				StartDate = DateTime.UtcNow,
				MeasurementNetworkId = Guid.NewGuid()
			};

			Result result = await service.EditAsync(request);

			// 7. Assert
			Assert.False(result.IsSuccess);
			Assert.True(result.IsNotFound());
		} finally {
			// 8. Stop container
			await container.DisposeAsync();
		}
	}
}