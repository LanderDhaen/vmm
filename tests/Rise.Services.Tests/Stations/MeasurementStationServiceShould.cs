using Ardalis.Result;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

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
using Rise.Shared.MaintObjects;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

using DomainMaintObj = Rise.Domain.MaintObjects.MaintObj;
using DomainMaintObjType = Rise.Domain.MaintObjects.MaintObjType;

namespace Rise.Services.Tests.Stations;

public class MeasurementStationServiceShould {
	private static readonly DateRange DummyDateRange = new(
		new DateOnly(2025, 1, 5),
		new DateOnly(2025, 5, 10)
	);

	private static readonly Zone DummyZone = new("DummyZone", "DummyCode");
	private static readonly MeasurementNetwork DummyMeasurementNetwork = new("DummyMeasurementNetwork", DummyZone);

	private static readonly Project DummyProject = new("dummyproject", "DP-1", DummyDateRange, DummyMeasurementNetwork);

	private static ApplicationDbContext
		CreateDbContext(out Guid projectId, out Project project, out Location location) {
		DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		ApplicationDbContext context = new(options);

		location = new Location(
			"TestLoc",
			new Address("Line1", "Line2", "City", "1234"),
			new Coordinate(4.35, 50.85));

		context.Locations.Add(location);
		context.SaveChanges();

		DateRange dateRange = new(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));
		project = new Project("Test Project", "TP2025", dateRange, DummyMeasurementNetwork);

		context.Projects.Add(project);
		context.SaveChanges();

		projectId = project.Id;
		return context;
	}

	[Fact]
	public async Task GetIndexAsync_ShouldReturnPagedResults() {
		ApplicationDbContext db = CreateDbContext(out Guid projectId, out Project project, out Location loc);
		MeasurementStationService service = new(db);

		for (int i = 0; i < 5; i++) {
			MeasurementStation station = new(
				$"STN-{i}",
				loc,
				new Installation(DateTime.UtcNow.AddDays(-i)));

			db.MeasurementStations.Add(station);
		}

		await db.SaveChangesAsync();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 3,
			OrderBy = "Code",
			OrderDescending = false
		};

		Result<MeasurementStationResponse.Index> result = await service.GetIndexAsync(request);

		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);

		MeasurementStationResponse.Index payload = result.Value!;
		Assert.Equal(5, payload.TotalCount);
		Assert.Equal(3, payload.Stations.Count());

		string[] codes = payload.Stations.Select(s => s.Code).ToArray();
		Assert.Equal(new[] { "STN-0", "STN-1", "STN-2" }, codes);

		Assert.All(payload.Stations, s => Assert.Equal("TestLoc", s.Location));
		Assert.All(payload.Stations, s => Assert.Equal("City", s.City));
		//Assert.All(payload.Stations, s => Assert.Equal(1, s.ProjectsCount));
	}

	[Fact]
	public async Task GetIndexAsync_ShouldRespectOrderDescending() {
		ApplicationDbContext db = CreateDbContext(out Guid projectId, out Project project, out Location loc);
		MeasurementStationService service = new(db);

		foreach (string code in new[] { "STN-1", "STN-2", "STN-3" }) {
			db.MeasurementStations.Add(new MeasurementStation(
				code,
				loc,
				new Installation(DateTime.UtcNow)));
		}

		await db.SaveChangesAsync();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 10,
			OrderBy = nameof(MeasurementStationDto.Index.Code),
			OrderDescending = true
		};

		Result<MeasurementStationResponse.Index> result = await service.GetIndexAsync(request);
		Assert.True(result.IsSuccess);

		string[] codes = result.Value!.Stations.Select(s => s.Code).ToArray();
		Assert.Equal(new[] { "STN-3", "STN-2", "STN-1" }, codes);
	}

	[Fact]
	public async Task GetDetailAsync_ShouldReturnStationDetails_WhenStationExists() {
		ApplicationDbContext db = CreateDbContext(out Guid projectId, out Project project, out Location loc);
		MeasurementStationService service = new(db);

		DateTime installedAt = new(2024, 11, 6, 0, 0, 0, DateTimeKind.Utc);
		MeasurementStation station = new(
			"STN-002",
			loc,
			new Installation(installedAt));

		db.MeasurementStations.Add(station);
		await db.SaveChangesAsync();

		Result<MeasurementStationResponse.Detail> result = await service.GetDetailAsync(station.Id);

		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);

		MeasurementStationDto.Detail dto = result.Value!.Station;
		Assert.Equal(station.Id, dto.Id);
		Assert.Equal("STN-002", dto.Code);
		Assert.Equal(installedAt, dto.InstalledAt);
		Assert.Null(dto.RemovedAt);
		Assert.Equal("TestLoc", dto.Location);
		Assert.Equal("City", dto.City);
	}

	[Fact]
	public async Task GetDetailAsync_ShouldReturnNotFound_WhenStationDoesNotExist() {
		ApplicationDbContext db = CreateDbContext(out Guid projectId, out Project project, out Location loc);
		MeasurementStationService service = new(db);

		Result<MeasurementStationResponse.Detail> result = await service.GetDetailAsync(Guid.NewGuid());

		Assert.Equal(ResultStatus.NotFound, result.Status);
	}

	[Fact]
	public async Task GetProjectsAsync_ShouldReturnProjects_ForExistingStation() {
		ApplicationDbContext db = CreateDbContext(out Guid projectId, out Project project, out Location loc);
		MeasurementStationService service = new(db);

		MeasurementStation station = new(
			"STN-100",
			loc,
			new Installation(DateTime.UtcNow));

		station.Projects.Add(project);
		db.MeasurementStations.Add(station);

		Project p2 = new("Second Project", "SP-2",
			new DateRange(new DateOnly(2025, 2, 1)), DummyMeasurementNetwork);
		db.Projects.Add(p2);
		station.Projects.Add(p2);

		await db.SaveChangesAsync();

		Result<List<ProjectDto.Index>> result = await service.GetProjectsAsync(station.Id);

		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
		List<ProjectDto.Index>? items = result.Value!;
		Assert.Equal(2, items.Count);

		Assert.Contains(items, x => x.Id == project.Id && x.Code == project.Code && x.Name == project.Name);
		Assert.Contains(items, x => x.Id == p2.Id && x.Code == p2.Code && x.Name == p2.Name);
	}

	[Fact]
	public async Task GetProjectsAsync_ShouldReturnEmptyList_WhenStationHasNoProjects() {
		ApplicationDbContext db = CreateDbContext(out Guid projectId, out Project project, out Location loc);
		MeasurementStationService service = new(db);

		MeasurementStation station = new(
			"STN-EMPTY",
			loc,
			new Installation(DateTime.UtcNow));
		db.MeasurementStations.Add(station);
		await db.SaveChangesAsync();

		Result<List<ProjectDto.Index>> result = await service.GetProjectsAsync(station.Id);

		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
		Assert.Empty(result.Value);
	}

	[Fact]
	public async Task GetProjectsAsync_ShouldReturnNotFound_WhenStationDoesNotExist() {
		ApplicationDbContext db = CreateDbContext(out Guid projectId, out Project project, out Location loc);
		MeasurementStationService service = new(db);

		Result<List<ProjectDto.Index>> result = await service.GetProjectsAsync(Guid.NewGuid());

		Assert.Equal(ResultStatus.NotFound, result.Status);
	}

	[Fact]
	public async Task CreateAsync_ShouldReturnNewMeasurementStationId_WithExistingLocation() {
		// Arrange
		using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);

		Location location = new(
			"Location", new Address(null, null, "TestCity", "1234"), new Coordinate(4.35, 50.85));

		db.Locations.Add(location);
		await db.SaveChangesAsync();

		MeasurementStationService service = new(db);

		MeasurementStationRequest.Create request = new() {
			Code = "ST-001",
			LocationId = location.Id,
			InstalledAt = new DateTime(2024, 10, 1),
			IsNewLocation = false
		};

		// Act
		Result<MeasurementStationResponse.Create> result = await service.CreateAsync(request);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
		Assert.NotEqual(Guid.Empty, result.Value!.MeasurementStationId);

		MeasurementStation? created = await db.MeasurementStations.FindAsync(result.Value.MeasurementStationId);
		Assert.NotNull(created);
		Assert.Equal("ST-001", created!.Code);
		Assert.Equal(location.Id, created.Location.Id);
		Assert.Equal(new DateTime(2024, 10, 1), created.Installation.InstalledAt);

		connection.Close();
	}

	[Fact]
	public async Task CreateAsync_ShouldReturnNewMeasurementStationId_WithNewLocation() {
		// Arrange
		using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);

		MeasurementStationService service = new(db);

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

		// Act
		Result<MeasurementStationResponse.Create> result = await service.CreateAsync(request);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
		Assert.NotEqual(Guid.Empty, result.Value!.MeasurementStationId);

		MeasurementStation? created = await db.MeasurementStations.FindAsync(result.Value.MeasurementStationId);
		Assert.NotNull(created);
		Assert.Equal("ST-001", created!.Code);
		Assert.NotEqual(Guid.Empty, created.Location.Id);
		Assert.Equal("New Location", created.Location.Name);
		Assert.Equal(new DateTime(2024, 10, 1), created.Installation.InstalledAt);

		connection.Close();
	}

	[Fact]
	public async Task CreateAsync_ShouldReturnConflict_WithNonExistingLocationId() {
		// Arrange
		using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);

		MeasurementStationService service = new(db);

		MeasurementStationRequest.Create request = new() {
			Code = "ST-001",
			LocationId = Guid.NewGuid(),
			InstalledAt = new DateTime(2024, 10, 1),
			IsNewLocation = false
		};

		// Act
		Result<MeasurementStationResponse.Create> result = await service.CreateAsync(request);

		// Assert
		Assert.Equal(ResultStatus.NotFound, result.Status);
		Assert.Equal("Locatie niet gevonden.", result.Errors.First());

		connection.Close();
	}

	[Fact]
	public async Task EditAsync_ShouldUpdateFields_WhenValid() {
		ApplicationDbContext db = CreateDbContext(out Guid projectId, out Project project1, out Location loc1);
		Location loc2 = new("Loc-2", new Address("l1", "l2", "City2", "2222"), new Coordinate(4.4, 50.9));
		Project project2 = new("Second Project", "SP-2",
			new DateRange(new DateOnly(2025, 2, 1), new DateOnly(2025, 12, 31)), DummyMeasurementNetwork);
		db.Locations.Add(loc2);
		db.Projects.Add(project2);
		await db.SaveChangesAsync();

		DateTime originalInstalled = new(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc);
		MeasurementStation station = new(
			"STN-200",
			loc1,
			new Installation(originalInstalled));
		db.MeasurementStations.Add(station);
		await db.SaveChangesAsync();

		MeasurementStationService service = new(db);

		DateTime newInstalled = new(2024, 11, 1, 0, 0, 0, DateTimeKind.Utc);
		DateTime newRemoved = new(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc);

		MeasurementStationRequest.Edit req = new() {
			StationId = station.Id,
			Code = "STN-200",
			LocationId = loc2.Id,
			ProjectId = project2.Id,
			InstalledAt = newInstalled,
			RemovedAt = newRemoved
		};

		Result result = await service.EditAsync(req);

		Assert.True(result.IsSuccess);

		MeasurementStation reloaded = await db.MeasurementStations
			.Include(s => s.Location)
			.Include(s => s.Projects)
			.SingleAsync(s => s.Id == station.Id);

		Assert.Equal(loc2.Id, reloaded.Location.Id);
		Assert.Single(reloaded.Projects);
		Assert.Equal(project2.Id, reloaded.Projects.First().Id);
		Assert.Equal(newInstalled, reloaded.Installation.InstalledAt);
		Assert.Equal(newRemoved, reloaded.Installation.RemovedAt);
	}

	[Fact]
	public async Task EditAsync_ShouldReturnNotFound_WhenStationDoesNotExist() {
		ApplicationDbContext db = CreateDbContext(out _, out _, out _);
		MeasurementStationService service = new(db);

		MeasurementStationRequest.Edit req = new() {
			StationId = Guid.NewGuid(),
			Code = "NOPE",
			LocationId = Guid.NewGuid(),
			ProjectId = Guid.NewGuid(),
			InstalledAt = DateTime.UtcNow
		};

		Result result = await service.EditAsync(req);

		Assert.Equal(ResultStatus.NotFound, result.Status);
	}

	[Fact]
	public async Task EditAsync_ShouldReturnNotFound_WhenNewLocationDoesNotExist() {
		ApplicationDbContext db = CreateDbContext(out _, out Project project, out Location loc);
		MeasurementStation station = new(
			"STN-404L",
			loc,
			new Installation(DateTime.UtcNow.AddDays(-4)));
		db.MeasurementStations.Add(station);
		await db.SaveChangesAsync();

		MeasurementStationService service = new(db);

		MeasurementStationRequest.Edit req = new() {
			StationId = station.Id,
			Code = station.Code,
			LocationId = Guid.NewGuid(),
			ProjectId = project.Id,
			InstalledAt = station.Installation.InstalledAt
		};

		Result result = await service.EditAsync(req);

		Assert.Equal(ResultStatus.NotFound, result.Status);
	}

	[Fact]
	public async Task EditAsync_ShouldReturnNotFound_WhenNewProjectDoesNotExist() {
		ApplicationDbContext db = CreateDbContext(out _, out Project project, out Location loc);
		MeasurementStation station = new(
			"STN-404P",
			loc,
			new Installation(DateTime.UtcNow.AddDays(-4)));
		db.MeasurementStations.Add(station);
		await db.SaveChangesAsync();

		MeasurementStationService service = new(db);

		MeasurementStationRequest.Edit req = new() {
			StationId = station.Id,
			Code = station.Code,
			LocationId = loc.Id,
			ProjectId = Guid.NewGuid(),
			InstalledAt = station.Installation.InstalledAt
		};

		Result result = await service.EditAsync(req);

		Assert.Equal(ResultStatus.NotFound, result.Status);
	}

	[Fact]
	public async Task EditAsync_ShouldThrow_WhenRemovedBeforeInstalled() {
		ApplicationDbContext db = CreateDbContext(out _, out Project project, out Location loc);
		MeasurementStation station = new(
			"STN-DATE",
			loc,
			new Installation(DateTime.UtcNow.AddDays(-10)));
		db.MeasurementStations.Add(station);
		await db.SaveChangesAsync();

		MeasurementStationService service = new(db);

		DateTime installed = new(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc);
		DateTime removed = new(2025, 1, 5, 0, 0, 0, DateTimeKind.Utc);

		MeasurementStationRequest.Edit req = new() {
			StationId = station.Id,
			Code = station.Code,
			LocationId = loc.Id,
			ProjectId = project.Id,
			InstalledAt = installed,
			RemovedAt = removed
		};

		await Assert.ThrowsAsync<ArgumentException>(async () => await service.EditAsync(req));
	}

	[Fact]
	public async Task UpdateProjectsAsync_ShouldReturnSuccess() {
		// Arrange
		using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);

		MeasurementStation measurementStation = new("ST-001",
			new Location("Loc1", new Address("Addr1", "Addr2", "City", "1234"), new Coordinate(4.35, 50.85)),
			new Installation(new DateTime(2025, 1, 1)));

		db.MeasurementStations.Add(measurementStation);

		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);
		Project project = new("Test Project", "TP-001", new DateRange(new DateOnly(2025, 1, 1)),
			measurementNetwork);

		db.Zones.Add(zone);
		db.MeasurementNetworks.Add(measurementNetwork);
		db.Projects.Add(project);
		await db.SaveChangesAsync();

		MeasurementStationService service = new(db);

		MeasurementStationRequest.UpdateProjects request = new() {
			MeasurementStationId = measurementStation.Id,
			ProjectIds = new List<Guid> { project.Id }
		};

		// Act
		Result result = await service.UpdateProjectsAsync(request);

		// Assert
		Assert.True(result.IsSuccess);

		MeasurementStation updated = db.MeasurementStations.Local.Single(s => s.Id == measurementStation.Id);
		Assert.NotNull(updated);
		Assert.NotEmpty(updated.Projects);
		Assert.Equal(project.Id, updated.Projects.First().Id);
		Assert.Equal(project.Code, updated.Projects.First().Code);

		connection.Close();
	}

	[Fact]
	public async Task UpdateProjectsAsync_ShouldReturnNotFound_WithNonExistingMeasurementStationId() {
		// Arrange
		using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);

		Zone zone = new("TestZone", "Z-001");
		MeasurementNetwork measurementNetwork = new("TestNetwork", zone);
		Project project = new("Test Project", "TP-001", new DateRange(new DateOnly(2025, 1, 1)),
			measurementNetwork);

		db.Zones.Add(zone);
		db.MeasurementNetworks.Add(measurementNetwork);
		db.Projects.Add(project);
		await db.SaveChangesAsync();
		await db.SaveChangesAsync();

		MeasurementStationService service = new(db);

		MeasurementStationRequest.UpdateProjects request = new() {
			MeasurementStationId = Guid.Empty,
			ProjectIds = new List<Guid> { project.Id }
		};

		// Act
		Result result = await service.UpdateProjectsAsync(request);

		// Assert
		Assert.Equal(ResultStatus.NotFound, result.Status);
		Assert.Equal($"Meetstation met id '{request.MeasurementStationId}' is niet gevonden.", result.Errors.First());

		connection.Close();
	}

	[Fact]
	public async Task CreateAsync_ShouldReturnError_WhenAProjectIsNotFound() {
		// Arrange
		using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);

		MeasurementStation measurementStation = new("ST-001",
			new Location("Loc1", new Address("Addr1", "Addr2", "City", "1234"), new Coordinate(4.35, 50.85)),
			new Installation(new DateTime(2025, 1, 1)));

		db.MeasurementStations.Add(measurementStation);
		await db.SaveChangesAsync();

		MeasurementStationService service = new(db);

		MeasurementStationRequest.UpdateProjects request = new() {
			MeasurementStationId = measurementStation.Id,
			ProjectIds = new List<Guid> { Guid.NewGuid() }
		};

		// Act
		Result result = await service.UpdateProjectsAsync(request);

		// Assert
		Assert.Equal(ResultStatus.Error, result.Status);
		Assert.Equal("Een of meerdere projecten zijn niet gevonden.", result.Errors.First());

		connection.Close();
	}

	// ---------------------------
	// ✅ MaintObjects tests added
	// ---------------------------

	[Fact]
	public async Task GetMaintObjsAsync_ShouldReturnOnlyMaintObjsFromStation() {
		ApplicationDbContext db = CreateDbContext(out _, out _, out Location loc);
		MeasurementStationService service = new(db);

		MeasurementStation s1 = new("STN-1", loc, new Installation(DateTime.UtcNow));
		MeasurementStation s2 = new("STN-2", loc, new Installation(DateTime.UtcNow));
		db.MeasurementStations.AddRange(s1, s2);

		DomainMaintObjType type = new(Guid.NewGuid(), "SONDE", "Sonde", 30, 90);
		db.Set<DomainMaintObjType>().Add(type);

		DomainMaintObj m1 = new("M1", "SN-1", type, null, null, true, null);
		DomainMaintObj m2 = new("M2", "SN-2", type, null, null, true, null);
		DomainMaintObj m3 = new("M3", "SN-3", type, null, null, true, null);

		m1.AssignToStation(s1);
		m2.AssignToStation(s1);
		m3.AssignToStation(s2);

		db.Set<DomainMaintObj>().AddRange(m1, m2, m3);
		await db.SaveChangesAsync();

		Result<List<MaintObjDto.Index>> result = await service.GetMaintObjsAsync(s1.Id);

		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
		Assert.Equal(2, result.Value!.Count);
		Assert.All(result.Value!, dto => Assert.Contains(dto.Id, new[] { m1.Id, m2.Id }));
	}

	[Fact]
	public async Task UpdateMaintObjsAsync_ShouldAssignAndUnassignCorrectly() {
		ApplicationDbContext db = CreateDbContext(out _, out _, out Location loc);
		MeasurementStationService service = new(db);

		MeasurementStation station = new("STN-100", loc, new Installation(DateTime.UtcNow));
		db.MeasurementStations.Add(station);

		DomainMaintObjType type = new(Guid.NewGuid(), "LOGGER", "Logger", 30, 90);
		db.Set<DomainMaintObjType>().Add(type);

		DomainMaintObj a = new("A", "SN-A", type, null, null, true, null);
		DomainMaintObj b = new("B", "SN-B", type, null, null, true, null);
		DomainMaintObj c = new("C", "SN-C", type, null, null, true, null);

		a.AssignToStation(station);
		b.AssignToStation(station);

		db.Set<DomainMaintObj>().AddRange(a, b, c);
		await db.SaveChangesAsync();

		MeasurementStationRequest.UpdateMaintObjects req = new() {
			MeasurementStationId = station.Id,
			MaintObjIds = new List<Guid> { b.Id, c.Id }
		};

		Result result = await service.UpdateMaintObjsAsync(req);

		Assert.True(result.IsSuccess);

		DomainMaintObj aReloaded = await db.Set<DomainMaintObj>().Include(x => x.Station).SingleAsync(x => x.Id == a.Id);
		DomainMaintObj bReloaded = await db.Set<DomainMaintObj>().Include(x => x.Station).SingleAsync(x => x.Id == b.Id);
		DomainMaintObj cReloaded = await db.Set<DomainMaintObj>().Include(x => x.Station).SingleAsync(x => x.Id == c.Id);

		Assert.Null(aReloaded.Station);
		Assert.True(aReloaded.IsInStock);

		Assert.NotNull(bReloaded.Station);
		Assert.Equal(station.Id, bReloaded.Station!.Id);

		Assert.NotNull(cReloaded.Station);
		Assert.Equal(station.Id, cReloaded.Station!.Id);
	}

	[Fact]
	public async Task UpdateMaintObjsAsync_ShouldReturnNotFound_WhenStationDoesNotExist() {
		ApplicationDbContext db = CreateDbContext(out _, out _, out Location loc);
		MeasurementStationService service = new(db);

		DomainMaintObjType type = new(Guid.NewGuid(), "SIM", "Sim", 30, 90);
		db.Set<DomainMaintObjType>().Add(type);

		DomainMaintObj mo = new("X", "SN-X", type, null, null, true, null);
		db.Set<DomainMaintObj>().Add(mo);
		await db.SaveChangesAsync();

		MeasurementStationRequest.UpdateMaintObjects req = new() {
			MeasurementStationId = Guid.NewGuid(),
			MaintObjIds = new List<Guid> { mo.Id }
		};

		Result result = await service.UpdateMaintObjsAsync(req);

		Assert.Equal(ResultStatus.NotFound, result.Status);
	}

	[Fact]
	public async Task UpdateMaintObjsAsync_ShouldReturnError_WhenAnyMaintObjIdIsMissing() {
		ApplicationDbContext db = CreateDbContext(out _, out _, out Location loc);
		MeasurementStationService service = new(db);

		MeasurementStation station = new("STN-404", loc, new Installation(DateTime.UtcNow));
		db.MeasurementStations.Add(station);

		DomainMaintObjType type = new(Guid.NewGuid(), "VOED", "Voeding", 30, 90);
		db.Set<DomainMaintObjType>().Add(type);

		DomainMaintObj existing = new("Exists", "SN-E", type, null, null, true, null);
		db.Set<DomainMaintObj>().Add(existing);
		await db.SaveChangesAsync();

		MeasurementStationRequest.UpdateMaintObjects req = new() {
			MeasurementStationId = station.Id,
			MaintObjIds = new List<Guid> { existing.Id, Guid.NewGuid() }
		};

		Result result = await service.UpdateMaintObjsAsync(req);

		Assert.Equal(ResultStatus.Error, result.Status);
	}
}