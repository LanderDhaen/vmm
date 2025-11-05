using System.Security.Claims;

using Ardalis.Result;

using Microsoft.EntityFrameworkCore;

using Rise.Domain.Projects;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Services.Projects;
using Rise.Shared.Projects;

namespace Rise.Services.Tests.Projects;

public class ProjectServiceInMemoryTests {
	private ApplicationDbContext CreateDbContext(string dbName) {
		DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(dbName)
			.Options;

		return new ApplicationDbContext(options);
	}

	private ISessionContextProvider CreateFakeSession(string userId) {
		ClaimsPrincipal claims = new(new ClaimsIdentity(new[] {
			new Claim(ClaimTypes.NameIdentifier, userId)
		}, "TestAuthentication"));

		return new FakeSessionContextProvider(claims);
	}

	[Fact]
	public async Task GetDetailAsync_ShouldReturnNotFound_WhenProjectDoesNotExist() {
		// Arrange
		using ApplicationDbContext context = CreateDbContext("Db_NotFound");
		ISessionContextProvider session = CreateFakeSession("technician1");

		ProjectService service = new(context, session);

		// Act
		Result<ProjectResponse.Detail> result = await service.GetDetailAsync(Guid.NewGuid());

		// Assert
		result.IsSuccess.ShouldBeFalse();
		result.Errors.ShouldContain(e => e.Contains("was not found"));
	}

	[Fact]
	public async Task GetDetailAsync_ShouldReturnSuccess_WhenProjectExists() {
		// Arrange
		using ApplicationDbContext context = CreateDbContext("Db_ProjectExists");

		// Maak de domain entities
		Zone zone = new("Gent", "Z001");
		context.Zones.Add(zone);

		MeasurementNetwork network = new("Meetnet West", zone);
		context.MeasurementNetworks.Add(network);

		DateRange dateRange = new(DateOnly.FromDateTime(DateTime.UtcNow),
			DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)));
		Project project = new("Watermetingen", "WTR-01", dateRange, network);
		context.Projects.Add(project);

		await context.SaveChangesAsync();

		ISessionContextProvider session = CreateFakeSession("technician1");

		ProjectService service = new(context, session);

		// Act
		Result<ProjectResponse.Detail> result = await service.GetDetailAsync(project.Id);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.ShouldNotBeNull();
		result.Value!.Project.Name.ShouldBe("Watermetingen");
		result.Value.Project.Code.ShouldBe("WTR-01");
		result.Value.Project.MeasurementNetwork.ShouldBe("Meetnet West");
		result.Value.Project.Zone.ShouldBe("Gent");
	}
}

// Fake SessionContextProvider zoals jij eerder had
public class FakeSessionContextProvider : ISessionContextProvider {
	public FakeSessionContextProvider(ClaimsPrincipal user) {
		User = user;
	}

	public ClaimsPrincipal? User { get; }
}