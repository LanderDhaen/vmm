using Ardalis.Result;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Rise.Domain.Networks;
using Rise.Domain.Projects;
using Rise.Persistence;
using Rise.Services.Projects;
using Rise.Services.Tests.Common;
using Rise.Shared.Projects;

using Status = Rise.Shared.Projects.Status;

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

		ProjectDto.Detail dto = result.Value!.Project;
		Assert.Equal(project.Id, dto.Id);
		Assert.Equal("Test Project", dto.Name);
		Assert.Equal("TP-001", dto.Code);
		Assert.Equal(dateRange.StartDate, dto.StartDate);
		Assert.Equal(dateRange.EndDate, dto.EndDate); // if end date was set
		Assert.Equal(measurementNetwork.Name, dto.MeasurementNetwork);
		Assert.Equal(zone.Name, dto.Zone);
		Assert.Equal(Status.Actief, dto.Status);

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
	public async Task UpdateStatusAsync_ShouldReturnNotFound_WhenProjectDoesNotExist() {
		// Arrange
		using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);
		ProjectService service = new(db, null!);

		ProjectRequest.UpdateStatus req = new() {
			ProjectId = Guid.NewGuid(),
			NewStatus = Status.Afgesloten
		};

		// Act
		Result result = await service.UpdateStatusAsync(req);

		// Assert
		Assert.Equal(ResultStatus.NotFound, result.Status);

		connection.Close();
	}

	[Fact]
	public async Task UpdateStatusAsync_ShouldSetProjectStatus_WhenProjectExists() {
		// Arrange
		using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);

		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);
		DateRange dateRange = new(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));

		Project project = new("Test Project", "TP-002", dateRange, measurementNetwork);
		db.Zones.Add(zone);
		db.MeasurementNetworks.Add(measurementNetwork);
		db.Projects.Add(project);
		await db.SaveChangesAsync();

		ProjectService service = new(db, null!);

		ProjectRequest.UpdateStatus req = new() {
			ProjectId = project.Id,
			NewStatus = Status.Afgesloten
		};

		// Act
		Result result = await service.UpdateStatusAsync(req);

		// Assert
		Assert.True(result.IsSuccess);

		// Reload from DB to verify change persisted
		Project updated = await db.Projects.SingleAsync(p => p.Id == project.Id);
		Assert.Equal(Domain.Projects.Status.Afgesloten, updated.Status);

		connection.Close();
	}
}