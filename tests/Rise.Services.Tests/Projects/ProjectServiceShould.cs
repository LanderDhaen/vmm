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
	public async Task EditAsync_ShouldReturnResultSuccess() {
		// Arrange
		using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);

		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);
		Project project = new("Existing Project", "EP-001", new DateRange(new DateOnly(2025, 1, 1)),
			measurementNetwork);

		db.Zones.Add(zone);
		db.MeasurementNetworks.Add(measurementNetwork);
		db.Projects.Add(project);
		await db.SaveChangesAsync();

		ProjectService service = new(db, null!);

		ProjectRequest.Edit request = new() {
			ProjectId = project.Id,
			Name = "Updated Project",
			Code = project.Code,
			MeasurementNetworkId = project.MeasurementNetwork.Id,
			StartDate = new DateTime(2025, 1, 1),
			EndDate = new DateTime(2025, 12, 31),
		};

		// Act
		Result result = await service.EditAsync(request);

		// Assert
		Assert.True(result.IsSuccess);

		Project? updated = await db.Projects.FindAsync(project.Id);
		Assert.NotNull(updated);
		Assert.Equal("Updated Project", updated!.Name);
		Assert.Equal("EP-001", updated.Code);
		Assert.Equal(new DateOnly(2025, 1, 1), updated.DateRange.StartDate);
		Assert.Equal(new DateOnly(2025, 12, 31), updated.DateRange.EndDate);
		Assert.Equal(measurementNetwork.Id, updated.MeasurementNetwork.Id);

		connection.Close();
	}

	[Fact]
	public async Task EditAsync_ShouldReturnNotFound_WhenProjectIsNotFound() {
		// Arrange
		using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);
		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);
		Project project = new("Existing Project", "EP-001", new DateRange(new DateOnly(2025, 1, 1)),
			measurementNetwork);

		db.Zones.Add(zone);
		db.MeasurementNetworks.Add(measurementNetwork);
		db.Projects.Add(project);
		await db.SaveChangesAsync();

		ProjectService service = new(db, null!);

		ProjectRequest.Edit request = new() {
			ProjectId = Guid.NewGuid(),
			Name = "Updated Project",
			Code = project.Code,
			MeasurementNetworkId = project.MeasurementNetwork.Id,
			StartDate = new DateTime(2025, 1, 1)
		};

		// Act
		Result result = await service.EditAsync(request);

		// Assert
		Assert.Equal(ResultStatus.NotFound, result.Status);
		Assert.Equal($"Project met Id '{request.ProjectId}' was niet gevonden.", result.Errors.First());

		connection.Close();
	}

	[Fact]
	public async Task EditAsync_ShouldReturnConflict_WhenProjectHasAlreadyExistingCode() {
		// Arrange
		using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);
		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);
		Project project = new("Existing Project", "EP-001", new DateRange(new DateOnly(2025, 1, 1)),
			measurementNetwork);
		Project project1 = new("Existing Project", "EP-002", new DateRange(new DateOnly(2025, 3, 8)),
			measurementNetwork);

		db.Zones.Add(zone);
		db.MeasurementNetworks.Add(measurementNetwork);
		db.Projects.Add(project);
		db.Projects.Add(project1);
		await db.SaveChangesAsync();

		ProjectService service = new(db, null!);

		ProjectRequest.Edit request = new() {
			ProjectId = project1.Id,
			Name = project1.Name,
			Code = "EP-001",
			MeasurementNetworkId = project1.MeasurementNetwork.Id,
			StartDate = new DateTime(2025, 1, 1)
		};

		// Act
		Result result = await service.EditAsync(request);

		// Assert
		Assert.Equal(ResultStatus.Conflict, result.Status);
		Assert.Equal($"Project met code 'EP-001' bestaat al.", result.Errors.First());

		connection.Close();
	}

	[Fact]
	public async Task EditAsync_ShouldReturnNotFound_WhenMeasurementNetworkIsNotFound() {
		// Arrange
		using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);
		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);
		MeasurementNetwork measurementNetwork1 = new("TestNetwork1", zone);
		Project project = new("Existing Project", "EP-001", new DateRange(new DateOnly(2025, 1, 1)),
			measurementNetwork);

		db.Zones.Add(zone);
		db.MeasurementNetworks.Add(measurementNetwork);
		db.Projects.Add(project);
		await db.SaveChangesAsync();

		ProjectService service = new(db, null!);

		ProjectRequest.Edit request = new() {
			ProjectId = project.Id,
			Name = "Updated Project",
			Code = project.Code,
			MeasurementNetworkId = measurementNetwork1.Id,
			StartDate = new DateTime(2025, 1, 1)
		};

		// Act
		Result result = await service.EditAsync(request);

		// Assert
		Assert.Equal(ResultStatus.NotFound, result.Status);
		Assert.Equal("Meetnet niet gevonden.", result.Errors.First());

		connection.Close();
	}
}