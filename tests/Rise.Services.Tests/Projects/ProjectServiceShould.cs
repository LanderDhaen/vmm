using Ardalis.Result;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Rise.Domain.Common;
using Rise.Domain.Locations;
using Rise.Domain.Networks;
using Rise.Domain.Projects;
using Rise.Domain.Stations;
using Rise.Persistence;
using Rise.Services.Projects;
using Rise.Services.Tests.Common;
using Rise.Shared.Common;
using Rise.Shared.Projects;

using Testcontainers.MsSql;

using Status = Rise.Domain.Projects.Status;

namespace Rise.Services.Tests.Projects;

public class ProjectServiceShould : IClassFixture<MsSqlContainerFixture>, IAsyncLifetime {
	private readonly ApplicationDbContext _dbContext;
	private readonly ProjectService _service;
	private IDbContextTransaction? _tx;

	#region Setup
	public ProjectServiceShould(MsSqlContainerFixture fixture) {
		MsSqlContainer container = fixture.Container!;

		DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseSqlServer(container.GetConnectionString())
			.Options;

		_dbContext = new ApplicationDbContext(options);
		_dbContext.Database.Migrate();

		_service = new ProjectService(_dbContext);
	}

	// Begin a transaction for each test instance and roll it back afterwards to avoid duplicate inserts
	public async Task InitializeAsync() {
		_tx = await _dbContext.Database.BeginTransactionAsync();
	}

	public async Task DisposeAsync() {
		if (_tx is not null) {
			await _tx.RollbackAsync();
			await _tx.DisposeAsync();
		}
	}
	#endregion

	#region GetDetail

	[Fact]
	public async Task GetDetailAsync_ShouldReturnProjectDetails_WhenProjectExists() {
		// Arrange
		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);
		DateRange dateRange = new(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));

		Project project = new("Test Project", "TP-001", dateRange, measurementNetwork);
		_dbContext.Zones.Add(zone);
		_dbContext.MeasurementNetworks.Add(measurementNetwork);
		_dbContext.Projects.Add(project);
		await _dbContext.SaveChangesAsync();

		// Act
		Result<ProjectResponse.Detail> result = await _service.GetDetailAsync(project.Id);

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
		Assert.Equal((Shared.Projects.Status)Status.Actief, dto.Status);
	}

	[Fact]
	public async Task GetDetailAsync_ShouldReturnNotFound_WhenProjectDoesNotExist() {
		// Arrange
		Guid nonExistentId = Guid.NewGuid();

		// Act
		Result<ProjectResponse.Detail> result = await _service.GetDetailAsync(nonExistentId);

		// Assert
		Assert.Equal(ResultStatus.NotFound, result.Status);
	}

	#endregion

	#region UpdateStatus

	[Fact]
	public async Task UpdateStatusAsync_ShouldReturnNotFound_WhenProjectDoesNotExist() {
		// Arrange

		ProjectRequest.UpdateStatus req = new() {
			ProjectId = Guid.NewGuid(),
			NewStatus = (Shared.Projects.Status)Status.Afgesloten
		};

		// Act
		Result result = await _service.UpdateStatusAsync(req);

		// Assert
		Assert.Equal(ResultStatus.NotFound, result.Status);
	}

	[Fact]
	public async Task UpdateStatusAsync_ShouldSetProjectStatus_WhenProjectExists() {
		// Arrange

		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);
		DateRange dateRange = new(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));

		Project project = new("Test Project", "TP-002", dateRange, measurementNetwork);
		_dbContext.Zones.Add(zone);
		_dbContext.MeasurementNetworks.Add(measurementNetwork);
		_dbContext.Projects.Add(project);
		await _dbContext.SaveChangesAsync();


		ProjectRequest.UpdateStatus req = new() {
			ProjectId = project.Id,
			NewStatus = (Shared.Projects.Status)Status.Afgesloten
		};

		// Act
		Result result = await _service.UpdateStatusAsync(req);

		// Assert
		Assert.True(result.IsSuccess);

		// Reload from DB to verify change persisted
		Project updated = await _dbContext.Projects.SingleAsync(p => p.Id == project.Id);
		Assert.Equal(Status.Afgesloten, updated.Status);
	}

	#endregion

	#region Create

	[Fact]
	public async Task CreateAsync_ShouldReturnNewProjectId() {
		// Arrange
		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);

		_dbContext.Zones.Add(zone);
		_dbContext.MeasurementNetworks.Add(measurementNetwork);
		await _dbContext.SaveChangesAsync();


		ProjectRequest.Create request = new() {
			Name = "Test Project",
			Code = "TP-001",
			MeasurementNetworkId = measurementNetwork.Id,
			StartDate = new DateTime(2025, 1, 1)
		};

		// Act
		Result<ProjectResponse.Create> result = await _service.CreateAsync(request);

		// Assert
		Assert.True(result.IsCreated());
		Assert.NotNull(result.Value);
		Assert.NotEqual(Guid.Empty, result.Value!.ProjectId);

		Project? created = await _dbContext.Projects.FindAsync(result.Value.ProjectId);
		Assert.NotNull(created);
		Assert.Equal("Test Project", created!.Name);
		Assert.Equal("TP-001", created.Code);
		Assert.Equal(measurementNetwork.Id, created.MeasurementNetwork.Id);
	}

	[Fact]
	public async Task CreateAsync_ShouldReturnConflict_WhenProjectHasAlreadyExistingCode() {
		// Arrange
		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);

		_dbContext.Projects.Add(new Project("Project", "TP-001", new DateRange(new DateOnly(2025, 1, 1)),
			measurementNetwork));
		await _dbContext.SaveChangesAsync();

		ProjectRequest.Create request = new() {
			Name = "Test Project",
			Code = "TP-001",
			MeasurementNetworkId = measurementNetwork.Id,
			StartDate = new DateTime(2025, 1, 1)
		};

		// Act
		Result<ProjectResponse.Create> result = await _service.CreateAsync(request);

		// Assert
		Assert.Equal(ResultStatus.Conflict, result.Status);
		Assert.Equal("Project met code 'TP-001' bestaat al.", result.Errors.First());
	}

	[Fact]
	public async Task CreateAsync_ShouldReturnNotFound_WhenMeasurementNetworkIsNotFound() {
		// Arrange
		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);

		ProjectRequest.Create request = new() {
			Name = "Test Project",
			Code = "TP-001",
			MeasurementNetworkId = measurementNetwork.Id,
			StartDate = new DateTime(2025, 1, 1)
		};

		// Act
		Result<ProjectResponse.Create> result = await _service.CreateAsync(request);

		// Assert
		Assert.Equal(ResultStatus.NotFound, result.Status);
		Assert.Equal("Meetnet niet gevonden.", result.Errors.First());
	}

	#endregion

	#region Edit

	[Fact]
	public async Task EditAsync_ShouldReturnResultSuccess() {
		// Arrange
		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);
		Project project = new("Existing Project", "EP-001", new DateRange(new DateOnly(2025, 1, 1)),
			measurementNetwork);

		_dbContext.Zones.Add(zone);
		_dbContext.MeasurementNetworks.Add(measurementNetwork);
		_dbContext.Projects.Add(project);
		await _dbContext.SaveChangesAsync();


		ProjectRequest.Edit request = new() {
			ProjectId = project.Id,
			Name = "Updated Project",
			Code = project.Code,
			StartDate = new DateTime(2025, 1, 1),
			EndDate = new DateTime(2025, 12, 31)
		};

		// Act
		Result result = await _service.EditAsync(request);

		// Assert
		Assert.True(result.IsSuccess);

		Project? updated = await _dbContext.Projects.FindAsync(project.Id);
		Assert.NotNull(updated);
		Assert.Equal("Updated Project", updated!.Name);
		Assert.Equal("EP-001", updated.Code);
		Assert.Equal(new DateOnly(2025, 1, 1), updated.DateRange.StartDate);
		Assert.Equal(new DateOnly(2025, 12, 31), updated.DateRange.EndDate);
	}

	[Fact]
	public async Task EditAsync_ShouldReturnNotFound_WhenProjectIsNotFound() {
		// Arrange
		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);
		Project project = new("Existing Project", "EP-001", new DateRange(new DateOnly(2025, 1, 1)),
			measurementNetwork);

		_dbContext.Zones.Add(zone);
		_dbContext.MeasurementNetworks.Add(measurementNetwork);
		_dbContext.Projects.Add(project);
		await _dbContext.SaveChangesAsync();


		ProjectRequest.Edit request = new() {
			ProjectId = Guid.NewGuid(),
			Name = "Updated Project",
			Code = project.Code,
			StartDate = new DateTime(2025, 1, 1)
		};

		// Act
		Result result = await _service.EditAsync(request);

		// Assert
		Assert.Equal(ResultStatus.NotFound, result.Status);
		Assert.Equal($"Project met Id '{request.ProjectId}' was niet gevonden.", result.Errors.First());
	}

	[Fact]
	public async Task EditAsync_ShouldReturnConflict_WhenProjectHasAlreadyExistingCode() {
		// Arrange
		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);
		Project project = new("Existing Project", "EP-001", new DateRange(new DateOnly(2025, 1, 1)),
			measurementNetwork);
		Project project1 = new("Existing Project", "EP-002", new DateRange(new DateOnly(2025, 3, 8)),
			measurementNetwork);

		_dbContext.Zones.Add(zone);
		_dbContext.MeasurementNetworks.Add(measurementNetwork);
		_dbContext.Projects.Add(project);
		_dbContext.Projects.Add(project1);
		await _dbContext.SaveChangesAsync();

		ProjectRequest.Edit request = new() {
			ProjectId = project1.Id,
			Name = project1.Name,
			Code = "EP-001",
			StartDate = new DateTime(2025, 1, 1)
		};

		// Act
		Result result = await _service.EditAsync(request);

		// Assert
		Assert.Equal(ResultStatus.Conflict, result.Status);
		Assert.Equal("Project met code 'EP-001' bestaat al.", result.Errors.First());
	}

	#endregion

	#region Stations

	[Fact]
	public async Task GetStationsAsync_ShouldReturnStations_WhenProjectExists() {
		// Arrange
		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);
		DateRange dateRange = new(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));

		Project project = new("Test Project", "TP-003", dateRange, measurementNetwork);

		Location location = new(
			"TestLoc",
			new Address("Street 1", "Bus 2", "City", "1234"),
			new Coordinate(4.35, 50.85));

		MeasurementStation station = new(
			"ST-001",
			location,
			new Installation(DateTime.UtcNow));

		project.Stations.Add(station);

		_dbContext.Zones.Add(zone);
		_dbContext.MeasurementNetworks.Add(measurementNetwork);
		_dbContext.Projects.Add(project);
		_dbContext.Locations.Add(location);
		_dbContext.MeasurementStations.Add(station);
		await _dbContext.SaveChangesAsync();

		// Act
		Result<ProjectResponse.MeasurementStations> result =
			await _service.GetStationsAsync(project.Id, new QueryRequest.Search());

		// Assert
		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
		Assert.Single(result.Value.MeasurementStationsFromProject);
		Assert.Equal("ST-001", result.Value!.MeasurementStationsFromProject.First().Code);
	}

	[Fact]
	public async Task GetStationsAsync_ShouldReturnNotFound_WhenProjectDoesNotExist() {
		Guid nonExistentId = Guid.NewGuid();

		Result<ProjectResponse.MeasurementStations> result =
			await _service.GetStationsAsync(nonExistentId, new QueryRequest.Search());

		Assert.Equal(ResultStatus.NotFound, result.Status);
	}

	#endregion

	#region UpdateMeasurementStations

	[Fact]
	public async Task UpdateMeasurementStationsAsync_ShouldReturnSuccess() {
		// Arrange
		MeasurementStation measurementStation = new("ST-001",
			new Location("Loc1", new Address("Addr1", "Addr2", "City", "1234"), new Coordinate(4.35, 50.85)),
			new Installation(new DateTime(2025, 1, 1)));

		_dbContext.MeasurementStations.Add(measurementStation);

		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);
		Project project = new("Test Project", "TP-001", new DateRange(new DateOnly(2025, 1, 1)),
			measurementNetwork);

		_dbContext.Zones.Add(zone);
		_dbContext.MeasurementNetworks.Add(measurementNetwork);
		_dbContext.Projects.Add(project);
		await _dbContext.SaveChangesAsync();

		ProjectRequest.UpdateMeasurementStations request = new() {
			ProjectId = project.Id,
			MeasurementStationIds = [measurementStation.Id]
		};

		// Act
		Result result = await _service.UpdateMeasurementStationsAsync(request);

		// Assert
		Assert.True(result.IsSuccess);

		Project updated = _dbContext.Projects.Local.Single(p => p.Id == project.Id);
		Assert.NotNull(updated);
		Assert.NotEmpty(updated.Stations);
		Assert.Equal(measurementStation.Id, updated.Stations.First().Id);
		Assert.Equal(measurementStation.Code, updated.Stations.First().Code);
	}

	[Fact]
	public async Task UpdateMeasurementStationsAsync_ShouldReturnNotFound_WithNonExistingPrjectId() {
		// Arrange
		MeasurementStation measurementStation = new("ST-001",
			new Location("Loc1", new Address("Addr1", "Addr2", "City", "1234"), new Coordinate(4.35, 50.85)),
			new Installation(new DateTime(2025, 1, 1)));

		_dbContext.MeasurementStations.Add(measurementStation);
		await _dbContext.SaveChangesAsync();


		ProjectRequest.UpdateMeasurementStations request = new() {
			ProjectId = Guid.Empty,
			MeasurementStationIds = [measurementStation.Id]
		};

		// Act
		Result result = await _service.UpdateMeasurementStationsAsync(request);

		// Assert
		Assert.Equal(ResultStatus.NotFound, result.Status);
		Assert.Equal($"Project met id '{request.ProjectId}' is niet gevonden.", result.Errors.First());
	}

	[Fact]
	public async Task UpdateMeasurementStationsAsync_ShouldReturnError_WhenAMeasurementStationIsNotFound() {
		// Arrange
		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);
		Project project = new("Test Project", "TP-001", new DateRange(new DateOnly(2025, 1, 1)),
			measurementNetwork);

		_dbContext.Zones.Add(zone);
		_dbContext.MeasurementNetworks.Add(measurementNetwork);
		_dbContext.Projects.Add(project);
		await _dbContext.SaveChangesAsync();

		ProjectRequest.UpdateMeasurementStations request = new() {
			ProjectId = project.Id,
			MeasurementStationIds = [Guid.NewGuid()]
		};

		// Act
		Result result = await _service.UpdateMeasurementStationsAsync(request);

		// Assert
		Assert.Equal(ResultStatus.Error, result.Status);
		Assert.Equal("Een of meerdere meetstations zijn niet gevonden.", result.Errors.First());
	}

	#endregion
}