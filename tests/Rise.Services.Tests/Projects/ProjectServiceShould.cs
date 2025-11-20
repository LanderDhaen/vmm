using Ardalis.Result;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Rise.Domain.Locations;
using Rise.Domain.Networks;
using Rise.Domain.Projects;
using Rise.Domain.Stations;
using Rise.Persistence;
using Rise.Services.Projects;
using Rise.Services.Tests.Common;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

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
		Assert.Equal(measurementNetwork.Name, dto.MeasurementNetwork.Name);
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

	[Fact]
	public async Task EditAsync_ShouldReturnResultSuccess() {
		// Arrange
		await using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);

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
			EndDate = new DateTime(2025, 12, 31)
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
		await using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);
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
		await using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);
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
		Assert.Equal("Project met code 'EP-001' bestaat al.", result.Errors.First());

		connection.Close();
	}

	[Fact]
	public async Task EditAsync_ShouldReturnNotFound_WhenMeasurementNetworkIsNotFound() {
		// Arrange
		await using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);
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


	// [Fact]
	// public async Task GetStationsAsync_ShouldReturnStations_WhenProjectExists() {
	// 	await using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);
	//
	// 	Zone zone = new("TestZone", "Z-001");
	// 	MeasurementNetwork measurementNetwork = new("TestNetwork", zone);
	// 	DateRange dateRange = new(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));
	//
	// 	Project project = new("Test Project", "TP-003", dateRange, measurementNetwork);
	//
	// 	Location location = new(
	// 		"TestLoc",
	// 		new Address("Street 1", "Bus 2", "City", "1234"),
	// 		new Coordinate(4.35, 50.85));
	//
	// 	MeasurementStation station = new(
	// 		"ST-001",
	// 		location,
	// 		new Installation(DateTime.UtcNow));
	//
	// 	db.Zones.Add(zone);
	// 	db.MeasurementNetworks.Add(measurementNetwork);
	// 	db.Projects.Add(project);
	// 	db.Locations.Add(location);
	// 	db.MeasurementStations.Add(station);
	// 	await db.SaveChangesAsync();
	//
	// 	ProjectService service = new(db, null!);
	//
	// 	Result<List<MeasurementStationDto.Index>> result = await service.GetStationsAsync(project.Id);
	//
	// 	Assert.True(result.IsSuccess);
	// 	Assert.NotNull(result.Value);
	// 	Assert.Single(result.Value);
	// 	Assert.Equal("ST-001", result.Value![0].Code);
	//
	// 	connection.Close();
	// }

	[Fact]
	public async Task GetStationsAsync_ShouldReturnNotFound_WhenProjectDoesNotExist() {
		await using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);
		ProjectService service = new(db, null!);

		Guid nonExistentId = Guid.NewGuid();

		Result<List<MeasurementStationDto.Index>> result = await service.GetStationsAsync(nonExistentId);

		Assert.Equal(ResultStatus.NotFound, result.Status);

		connection.Close();
	}
}