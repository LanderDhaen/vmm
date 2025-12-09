using Ardalis.Result;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Rise.Domain.Common;
using Rise.Domain.Locations;
using Rise.Domain.Networks;
using Rise.Domain.Projects;
using Rise.Domain.Stations;
using Rise.Persistence;
using Rise.Services.Stations;
using Rise.Services.Tests.Common;
using Rise.Shared.Common;
using Rise.Shared.Locations;
using Rise.Shared.Stations;

using Testcontainers.MsSql;

namespace Rise.Services.Tests.Stations;

public class MeasurementStationServiceShould : IClassFixture<MsSqlContainerFixture>, IAsyncLifetime {
	private static readonly DateRange DummyDateRange = new(
		new DateOnly(2025, 1, 5),
		new DateOnly(2025, 5, 10)
	);

	private static readonly Zone DummyZone = new("DummyZone", "DummyCode");
	private static readonly MeasurementNetwork DummyMeasurementNetwork = new("DummyMeasurementNetwork", DummyZone);

	private static readonly Project DummyProject = new("dummyproject", "DP-1", DummyDateRange, DummyMeasurementNetwork);
	private readonly ApplicationDbContext _dbContext;
	private readonly MeasurementStationService _service;
	private IDbContextTransaction? _tx;

	#region Helpers

	private async Task<(Guid projectId, Project project, Location location)> CreateProjectAndLocationAsync() {
		Location location = new(
			"TestLoc",
			new Address("Line1", "Line2", "City", "1234"),
			new Coordinate(4.35, 50.85));

		_dbContext.Locations.Add(location);

		DateRange dateRange = new(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));
		Project project = new("Test Project", "TP2025", dateRange, DummyMeasurementNetwork);
		_dbContext.Projects.Add(project);

		await _dbContext.SaveChangesAsync();

		return (project.Id, project, location);
	}

	#endregion

	#region Setup

	public MeasurementStationServiceShould(MsSqlContainerFixture fixture) {
		MsSqlContainer container = fixture.Container!;

		DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseSqlServer(container.GetConnectionString())
			.Options;

		_dbContext = new ApplicationDbContext(options);
		_dbContext.Database.Migrate();

		_service = new MeasurementStationService(_dbContext);
	}

	// Start a transaction before each test and roll it back afterwards to avoid polluting the shared DB
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

	#region Index

	[Fact]
	public async Task GetIndexAsync_ShouldReturnPagedResults() {
		(_, Project project, Location loc) = await CreateProjectAndLocationAsync();

		for (int i = 0; i < 5; i++) {
			MeasurementStation station = new(
				$"STN-{i}",
				loc,
				new Installation(DateTime.UtcNow.AddDays(-i)));

			_dbContext.MeasurementStations.Add(station);
		}

		await _dbContext.SaveChangesAsync();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 3,
			OrderBy = "Code",
			OrderDescending = false
		};

		Result<MeasurementStationResponse.Index> result = await _service.GetIndexAsync(request);

		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);

		MeasurementStationResponse.Index payload = result.Value!;
		Assert.Equal(5, payload.TotalCount);
		Assert.Equal(3, payload.Stations.Count());

		string[] codes = payload.Stations.Select(s => s.Code).ToArray();
		Assert.Equal(new[] { "STN-0", "STN-1", "STN-2" }, codes);

		Assert.All(payload.Stations, s => Assert.Equal("TestLoc", s.Location));
		Assert.All(payload.Stations, s => Assert.Equal("City", s.City));
	}

	[Fact]
	public async Task GetIndexAsync_ShouldRespectOrderDescending() {
		(_, _, Location loc) = await CreateProjectAndLocationAsync();

		foreach (string code in new[] { "STN-1", "STN-2", "STN-3" }) {
			_dbContext.MeasurementStations.Add(new MeasurementStation(
				code,
				loc,
				new Installation(DateTime.UtcNow)));
		}

		await _dbContext.SaveChangesAsync();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 10,
			OrderBy = "Code",
			OrderDescending = true
		};

		Result<MeasurementStationResponse.Index> result = await _service.GetIndexAsync(request);
		Assert.True(result.IsSuccess);

		string[] codes = result.Value!.Stations.Select(s => s.Code).ToArray();
		Assert.Equal(new[] { "STN-3", "STN-2", "STN-1" }, codes);
	}

	#endregion

	#region Detail

	[Fact]
	public async Task GetDetailAsync_ShouldReturnStationDetails_WhenStationExists() {
		(_, _, Location loc) = await CreateProjectAndLocationAsync();

		DateTime installedAt = new(2024, 11, 6, 0, 0, 0, DateTimeKind.Utc);
		MeasurementStation station = new(
			"STN-002",
			loc,
			new Installation(installedAt));

		_dbContext.MeasurementStations.Add(station);
		await _dbContext.SaveChangesAsync();

		Result<MeasurementStationResponse.Detail> result = await _service.GetDetailAsync(station.Id);

		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);

		MeasurementStationDto.Detail dto = result.Value!.Station;
		Assert.Equal(station.Id, dto.Id);
		Assert.Equal("STN-002", dto.Code);
		Assert.Equal(installedAt, dto.InstalledAt);
		Assert.Null(dto.RemovedAt);
	}

	[Fact]
	public async Task GetDetailAsync_ShouldReturnNotFound_WhenStationDoesNotExist() {
		Result<MeasurementStationResponse.Detail> result = await _service.GetDetailAsync(Guid.NewGuid());
		Assert.Equal(ResultStatus.NotFound, result.Status);
	}

	#endregion

	#region Projects

	[Fact]
	public async Task GetProjectsAsync_ShouldReturnProjects_ForExistingStation() {
		(Guid projectId, Project project, Location loc) = await CreateProjectAndLocationAsync();

		MeasurementStation station = new(
			"STN-100",
			loc,
			new Installation(DateTime.UtcNow));

		station.Projects.Add(project);
		_dbContext.MeasurementStations.Add(station);

		Project p2 = new("Second Project", "SP-2",
			new DateRange(new DateOnly(2025, 2, 1)), DummyMeasurementNetwork);
		_dbContext.Projects.Add(p2);
		station.Projects.Add(p2);

		await _dbContext.SaveChangesAsync();

		Result<MeasurementStationResponse.ProjectList> result =
			await _service.GetProjectsAsync(station.Id);

		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
		MeasurementStationResponse.ProjectList? items = result.Value!;
		Assert.Equal(2, items.Projects.Count());
		Assert.Contains(items.Projects, x => x.Id == project.Id && x.Code == project.Code && x.Name == project.Name);
		Assert.Contains(items.Projects, x => x.Id == p2.Id && x.Code == p2.Code && x.Name == p2.Name);
	}

	[Fact]
	public async Task GetProjectsAsync_ShouldReturnEmptyList_WhenStationHasNoProjects() {
		(_, _, Location loc) = await CreateProjectAndLocationAsync();

		MeasurementStation station = new(
			"STN-EMPTY",
			loc,
			new Installation(DateTime.UtcNow));
		_dbContext.MeasurementStations.Add(station);
		await _dbContext.SaveChangesAsync();

		Result<MeasurementStationResponse.ProjectList> result =
			await _service.GetProjectsAsync(station.Id);

		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
		Assert.Empty(result.Value!.Projects);
	}

	[Fact]
	public async Task GetProjectsAsync_ShouldReturnNotFound_WhenStationDoesNotExist() {
		Result<MeasurementStationResponse.ProjectList> result =
			await _service.GetProjectsAsync(Guid.NewGuid());

		Assert.Equal(ResultStatus.NotFound, result.Status);
	}

	#endregion

	#region Create

	[Fact]
	public async Task CreateAsync_ShouldReturnNewMeasurementStationId_WithExistingLocation() {
		Location location = new(
			"Location", new Address(null, null, "TestCity", "1234"), new Coordinate(4.35, 50.85));

		_dbContext.Locations.Add(location);
		await _dbContext.SaveChangesAsync();

		MeasurementStationRequest.Create request = new() {
			Code = "ST-001",
			LocationId = location.Id,
			InstalledAt = new DateTime(2024, 10, 1),
			IsNewLocation = false
		};

		Result<MeasurementStationResponse.Create> result = await _service.CreateAsync(request);

		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
		Assert.NotEqual(Guid.Empty, result.Value!.MeasurementStationId);

		MeasurementStation? created = await _dbContext.MeasurementStations.FindAsync(result.Value.MeasurementStationId);
		Assert.NotNull(created);
		Assert.Equal("ST-001", created!.Code);
		Assert.Equal(location.Id, created.Location.Id);
		Assert.Equal(new DateTime(2024, 10, 1), created.Installation.InstalledAt);
	}

	[Fact]
	public async Task CreateAsync_ShouldReturnNewMeasurementStationId_WithNewLocation() {
		MeasurementStationRequest.Create request = new() {
			Code = "ST-001",
			Location = new LocationRequest.Create {
				Name = "New Location",
				Addressline1 = null,
				Addressline2 = null,
				City = "NewCity",
				PostalCode = "5678",
				CountryIso = "BE",
				X = 4.40,
				Y = 50.90
			},
			InstalledAt = new DateTime(2024, 10, 1),
			IsNewLocation = true
		};

		Result<MeasurementStationResponse.Create> result = await _service.CreateAsync(request);

		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
		Assert.NotEqual(Guid.Empty, result.Value!.MeasurementStationId);

		MeasurementStation? created = await _dbContext.MeasurementStations.FindAsync(result.Value.MeasurementStationId);
		Assert.NotNull(created);
		Assert.Equal("ST-001", created!.Code);
		Assert.NotEqual(Guid.Empty, created.Location.Id);
		Assert.Equal("New Location", created.Location.Name);
		Assert.Equal(new DateTime(2024, 10, 1), created.Installation.InstalledAt);
	}

	[Fact]
	public async Task CreateAsync_ShouldReturnConflict_WithNonExistingLocationId() {
		MeasurementStationRequest.Create request = new() {
			Code = "ST-001",
			LocationId = Guid.NewGuid(),
			InstalledAt = new DateTime(2024, 10, 1),
			IsNewLocation = false
		};

		Result<MeasurementStationResponse.Create> result = await _service.CreateAsync(request);

		Assert.Equal(ResultStatus.NotFound, result.Status);
		Assert.Equal("Locatie niet gevonden.", result.Errors.First());
	}

	#endregion

	#region Edit

	[Fact]
	public async Task EditAsync_ShouldUpdateFields_WhenValid() {
		(Guid _, Project project1, Location loc1) = await CreateProjectAndLocationAsync();
		Location loc2 = new("Loc-2", new Address("l1", "l2", "City2", "2222"), new Coordinate(4.4, 50.9));
		Project project2 = new("Second Project", "SP-2",
			new DateRange(new DateOnly(2025, 2, 1), new DateOnly(2025, 12, 31)), DummyMeasurementNetwork);
		_dbContext.Locations.Add(loc2);
		_dbContext.Projects.Add(project2);
		await _dbContext.SaveChangesAsync();

		DateTime originalInstalled = new(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc);
		MeasurementStation station = new(
			"STN-200",
			loc1,
			new Installation(originalInstalled));
		_dbContext.MeasurementStations.Add(station);
		await _dbContext.SaveChangesAsync();

		DateTime newInstalled = new(2024, 11, 1, 0, 0, 0, DateTimeKind.Utc);
		DateTime newRemoved = new(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc);

		MeasurementStationRequest.Edit req = new() {
			StationId = station.Id,
			Code = "STN-200",
			InstalledAt = newInstalled,
			RemovedAt = newRemoved
		};

		Result result = await _service.EditAsync(req);

		Assert.True(result.IsSuccess);

		MeasurementStation reloaded = await _dbContext.MeasurementStations
			.Include(s => s.Location)
			.Include(s => s.Projects)
			.SingleAsync(s => s.Id == station.Id);

		Assert.Equal(newInstalled, reloaded.Installation.InstalledAt);
		Assert.Equal(newRemoved, reloaded.Installation.RemovedAt);
	}

	[Fact]
	public async Task EditAsync_ShouldReturnNotFound_WhenStationDoesNotExist() {
		MeasurementStationRequest.Edit req = new() {
			StationId = Guid.NewGuid(),
			Code = "NOPE",
			InstalledAt = DateTime.UtcNow
		};

		Result result = await _service.EditAsync(req);

		Assert.Equal(ResultStatus.NotFound, result.Status);
	}

	[Fact]
	public async Task EditAsync_ShouldThrow_WhenRemovedBeforeInstalled() {
		(_, Project project, Location loc) = await CreateProjectAndLocationAsync();
		MeasurementStation station = new(
			"STN-DATE",
			loc,
			new Installation(DateTime.UtcNow.AddDays(-10)));
		_dbContext.MeasurementStations.Add(station);
		await _dbContext.SaveChangesAsync();

		DateTime installed = new(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc);
		DateTime removed = new(2025, 1, 5, 0, 0, 0, DateTimeKind.Utc);

		MeasurementStationRequest.Edit req = new() {
			StationId = station.Id,
			Code = station.Code,
			InstalledAt = installed,
			RemovedAt = removed
		};

		await Assert.ThrowsAsync<ArgumentException>(async () => await _service.EditAsync(req));
	}

	#endregion

	#region UpdateProjects

	[Fact]
	public async Task UpdateProjectsAsync_ShouldReturnSuccess() {
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

		MeasurementStationRequest.UpdateProjects request = new() {
			MeasurementStationId = measurementStation.Id,
			ProjectIds = [project.Id]
		};

		Result result = await _service.UpdateProjectsAsync(request);

		Assert.True(result.IsSuccess);

		MeasurementStation updated = await _dbContext.MeasurementStations.Include(s => s.Projects)
			.SingleAsync(s => s.Id == measurementStation.Id);
		Assert.NotNull(updated);
		Assert.NotEmpty(updated.Projects);
		Assert.Equal(project.Id, updated.Projects.First().Id);
		Assert.Equal(project.Code, updated.Projects.First().Code);
	}

	[Fact]
	public async Task UpdateProjectsAsync_ShouldReturnNotFound_WithNonExistingMeasurementStationId() {
		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);
		Project project = new("Test Project", "TP-001", new DateRange(new DateOnly(2025, 1, 1)),
			measurementNetwork);

		_dbContext.Zones.Add(zone);
		_dbContext.MeasurementNetworks.Add(measurementNetwork);
		_dbContext.Projects.Add(project);
		await _dbContext.SaveChangesAsync();

		MeasurementStationRequest.UpdateProjects request = new() {
			MeasurementStationId = Guid.Empty,
			ProjectIds = [project.Id]
		};

		Result result = await _service.UpdateProjectsAsync(request);

		Assert.Equal(ResultStatus.NotFound, result.Status);
		Assert.Equal($"Meetstation met id '{request.MeasurementStationId}' is niet gevonden.", result.Errors.First());
	}

	[Fact]
	public async Task CreateAsync_ShouldReturnError_WhenAProjectIsNotFound() {
		MeasurementStation measurementStation = new("ST-001",
			new Location("Loc1", new Address("Addr1", "Addr2", "City", "1234"), new Coordinate(4.35, 50.85)),
			new Installation(new DateTime(2025, 1, 1)));

		_dbContext.MeasurementStations.Add(measurementStation);
		await _dbContext.SaveChangesAsync();

		MeasurementStationRequest.UpdateProjects request = new() {
			MeasurementStationId = measurementStation.Id,
			ProjectIds = [Guid.NewGuid()]
		};

		Result result = await _service.UpdateProjectsAsync(request);

		Assert.Equal(ResultStatus.Error, result.Status);
		Assert.Equal("Een of meerdere projecten zijn niet gevonden.", result.Errors.First());
	}

	#endregion
}