using System.Security.Claims;

using Ardalis.Result;

using Microsoft.EntityFrameworkCore;

using Rise.Domain.Projects;
using Rise.Domain.Locations;
using Rise.Persistence;
using Rise.Services.Projects;
using Rise.Services.Tests.Fakers;
using Rise.Shared.Projects;

namespace Rise.Services.Tests.Projects;

public class ProjectServiceShould {

	private static readonly DateRange DummyDateRange = new(
		new DateOnly(2025, 1, 5),
		new DateOnly(2025, 5, 10)
	);
	
	[Fact]
	public async Task EditProjectWhenTechnicianIsTheSame() {
		// Arrange
		DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(
				nameof(
					EditProjectWhenTechnicianIsTheSame)) // Do NOT use InMemoryDatabase... it's not reliable. Use a real database and come up with a strategy to clean up the database between tests.
			.Options;

		using ApplicationDbContext dbContext = new(options);

		string technicianAccountId = Guid.NewGuid().ToString();
		Technician technician = new("FName", "LName", technicianAccountId);
		Project project = new("Old Project", "OLD123", DummyDateRange);

		dbContext.Technicians.Add(technician);
		dbContext.Projects.Add(project);
		await dbContext.SaveChangesAsync();

		ClaimsPrincipal claimsPrincipal = new(new ClaimsIdentity([
			new Claim(ClaimTypes.NameIdentifier, technicianAccountId)
		], "mock"));

		FakeSessionContextProvider sessionProvider = new(claimsPrincipal);

		ProjectService service = new(dbContext, sessionProvider);

		ProjectRequest.Edit request = new() {
			ProjectId = project.Id,
			Name = "New Project"
		};

		// Act
		Result result = await service.EditAsync(request);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		Project updatedProject = await dbContext.Projects.FirstAsync();
		updatedProject.Name.ShouldBe("New Project");
	}

	[Fact]
	public async Task NotEditProjectWhenTechnicianIsDifferent() {
		// Arrange
		DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(
				nameof(
					NotEditProjectWhenTechnicianIsDifferent)) // Do NOT use InMemoryDatabase... it's not reliable. Use a real database and come up with a strategy to clean up the database between tests.
			.Options;

		using ApplicationDbContext dbContext = new(options);

		string correctTechAccountId = Guid.NewGuid().ToString();
		string otherTechAccountId = Guid.NewGuid().ToString();

		Technician projectTechnician = new("FName", "LName", correctTechAccountId);
		Technician loggedInTechnician = new("Other", "Tech", otherTechAccountId);

		Project project = new("Old Project", "OLD123", DummyDateRange);

		dbContext.Technicians.AddRange(projectTechnician, loggedInTechnician);
		dbContext.Projects.Add(project);
		await dbContext.SaveChangesAsync();

		ClaimsPrincipal claimsPrincipal = new(new ClaimsIdentity([
			new Claim(ClaimTypes.NameIdentifier, otherTechAccountId)
		], "mock"));

		FakeSessionContextProvider sessionProvider = new(claimsPrincipal);
		ProjectService service = new(dbContext, sessionProvider);

		ProjectRequest.Edit request = new() {
			ProjectId = project.Id,
			Name = "New Project"
		};

		// Act
		Result result = await service.EditAsync(request);

		// Assert
		result.IsSuccess.ShouldBeFalse();
		result.Status.ShouldBe(ResultStatus.Unauthorized);
		Project unchangedProject = await dbContext.Projects.FirstAsync();
		unchangedProject.Name.ShouldBe("Old Project");
	}
}