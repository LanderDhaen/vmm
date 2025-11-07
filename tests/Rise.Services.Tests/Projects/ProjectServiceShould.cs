using Ardalis.Result;

using Microsoft.Data.Sqlite;

using Rise.Domain.Networks;
using Rise.Domain.Projects;
using Rise.Persistence;
using Rise.Services.Projects;
using Rise.Services.Tests.Common;
using Rise.Shared.Projects;

namespace Rise.Services.Tests.Projects;

public class ProjectServiceShould {
	[Fact]
	public async Task GetDetailAsync_ShouldReturnProjectDetails_WhenProjectExists() {
		// Arrange
		using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);

		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);
		DateRange dateRange = new(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));

		Project project = new("Test Project", "TP-001", dateRange, measurementNetwork);
		db.Zones.Add(zone);
		db.MeasurementNetworks.Add(measurementNetwork);
		db.Projects.Add(project);
		await db.SaveChangesAsync();

		ProjectService service = new(db, null!);

		// Act
		Result<ProjectResponse.Detail> result = await service.GetDetailAsync(project.Id);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
		Assert.Equal("Test Project", result.Value!.Project.Name);

		connection.Close();
	}

	[Fact]
	public async Task GetDetailAsync_ShouldReturnNotFound_WhenProjectDoesNotExist() {
		// Arrange
		using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);
		ProjectService service = new(db, null!);

		Guid nonExistentId = Guid.NewGuid();

		// Act
		Result<ProjectResponse.Detail> result = await service.GetDetailAsync(nonExistentId);

		// Assert
		Assert.Equal(ResultStatus.NotFound, result.Status);

		connection.Close();
	}
	
	[Fact]
	public async Task CreateAsync_ShouldReturnNewProjectId() {
		// Arrange
		using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);

		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);

		db.Zones.Add(zone);
		db.MeasurementNetworks.Add(measurementNetwork);
		await db.SaveChangesAsync();

		ProjectService service = new(db, null!);

		ProjectRequest.Create request = new() {
			Name = "Test Project",
			Code = "TP-001",
			MeasurementNetworkId = measurementNetwork.Id,
			StartDate = new DateTime(2025, 1, 1)
		};
		
		// Act
		Result<ProjectResponse.Create> result = await service.CreateAsync(request);

		// Assert
		Assert.True(result.IsCreated());
		Assert.NotNull(result.Value);
		Assert.NotEqual(Guid.Empty, result.Value!.ProjectId);
		
		Project? created = await db.Projects.FindAsync(result.Value.ProjectId);
		Assert.NotNull(created);
		Assert.Equal("Test Project", created!.Name);
		Assert.Equal("TP-001", created.Code);
		Assert.Equal(measurementNetwork.Id, created.MeasurementNetwork.Id);

		connection.Close();
	}
	
	[Fact]
	public async Task CreateAsync_ShouldReturnConflict_WhenProjectHasAlreadyExistingCode() {
		// Arrange
		using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);
		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);
		
		db.Projects.Add(new Project("Project", "TP-001", new DateRange(new DateOnly(2025,1,1)),  measurementNetwork));
		await db.SaveChangesAsync();

		ProjectService service = new(db, null!);

		ProjectRequest.Create request = new() {
			Name = "Test Project",
			Code = "TP-001",
			MeasurementNetworkId = measurementNetwork.Id,
			StartDate = new DateTime(2025, 1, 1)
		};

		// Act
		Result<ProjectResponse.Create> result = await service.CreateAsync(request);

		// Assert
		Assert.Equal(ResultStatus.Conflict, result.Status);
		Assert.Equal($"Project met code 'TP-001' bestaat al.", result.Errors.First());

		connection.Close();
	}
	
	[Fact]
	public async Task CreateAsync_ShouldReturnNotFound_WhenMeasurementNetworkIsNotFound() {
		// Arrange
		using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);
		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);

		ProjectService service = new(db, null!);

		ProjectRequest.Create request = new() {
			Name = "Test Project",
			Code = "TP-001",
			MeasurementNetworkId = measurementNetwork.Id,
			StartDate = new DateTime(2025, 1, 1)
		};

		// Act
		Result<ProjectResponse.Create> result = await service.CreateAsync(request);

		// Assert
		Assert.Equal(ResultStatus.NotFound, result.Status);
		Assert.Equal($"Meetnet niet gevonden.", result.Errors.First());

		connection.Close();
	}
}