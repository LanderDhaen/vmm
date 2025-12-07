using Ardalis.Result;

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
using Rise.Shared.Projects;
using Rise.Shared.Stations;

using Testcontainers.MsSql;

namespace Rise.Services.Tests.Stations;

public class MeasurementStationServiceShould : IClassFixture<MsSqlContainerFixture> {
	private readonly ApplicationDbContext _dbContext;
	private readonly MeasurementStationService _service;

	public MeasurementStationServiceShould(MsSqlContainerFixture fixture) {
		MsSqlContainer container = fixture.Container!;

		DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseSqlServer(container.GetConnectionString())
			.Options;

		_dbContext = new ApplicationDbContext(options);
		_dbContext.Database.Migrate();

		_service = new MeasurementStationService(_dbContext);
	}

	#region CreateAsync Tests

	[Fact]
	public async Task CreateAsync_ShouldCreateStation_WithExistingLocation() {
		// Arrange
		Location location = new("Test Location", new Address("Street 1", "", "City", "1000"),
			new Coordinate(50.0, 4.0));
		_dbContext.Locations.Add(location);
		await _dbContext.SaveChangesAsync();

		MeasurementStationRequest.Create request = new() {
			Code = "MS_CREATE_001",
			LocationId = location.Id,
			IsNewLocation = false,
			InstalledAt = DateTime.UtcNow
		};

		// Act
		Result<MeasurementStationResponse.Create> result = await _service.CreateAsync(request);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.MeasurementStationId.ShouldNotBe(Guid.Empty);

		MeasurementStation? createdStation =
			await _dbContext.MeasurementStations.FindAsync(result.Value.MeasurementStationId);
		createdStation.ShouldNotBeNull();
		createdStation.Code.ShouldBe("MS_CREATE_001");
		createdStation.Location.Id.ShouldBe(location.Id);
	}

	[Fact]
	public async Task CreateAsync_ShouldCreateStation_WithNewLocation() {
		// Arrange
		MeasurementStationRequest.Create request = new() {
			Code = "MS_CREATE_002",
			IsNewLocation = true,
			Location = new LocationRequest.Create {
				Name = "New Location",
				Addressline1 = "New Street 1",
				Addressline2 = "",
				City = "New City",
				PostalCode = "2000",
				CountryIso = "BE",
				X = 51.0,
				Y = 5.0
			},
			InstalledAt = DateTime.UtcNow
		};

		// Act
		Result<MeasurementStationResponse.Create> result = await _service.CreateAsync(request);

		// Assert
		result.IsSuccess.ShouldBeTrue();

		MeasurementStation? createdStation =
			await _dbContext!.MeasurementStations.Include(s => s.Location).FirstOrDefaultAsync(s =>
				s.Id == result.Value.MeasurementStationId);
		createdStation.ShouldNotBeNull();
		createdStation.Code.ShouldBe("MS_CREATE_002");
		createdStation.Location.Name.ShouldBe("New Location");
		createdStation.Location.Address.City.ShouldBe("New City");
	}

	[Fact]
	public async Task CreateAsync_ShouldReturnNotFound_WhenLocationDoesNotExist() {
		// Arrange
		MeasurementStationRequest.Create request = new() {
			Code = "MS_CREATE_003",
			LocationId = Guid.NewGuid(),
			IsNewLocation = false
		};

		// Act
		Result<MeasurementStationResponse.Create> result = await _service.CreateAsync(request);

		// Assert
		result.IsNotFound().ShouldBeTrue();
	}

	#endregion

	#region GetDetailAsync Tests

	[Fact]
	public async Task GetDetailAsync_ShouldReturnStationDetails_WhenStationExists() {
		// Arrange
		Location location = new("Detail Location", new Address("Detail St", "", "Detail City", "3000"),
			new Coordinate(50.5, 4.5));
		_dbContext.Locations.Add(location);

		MeasurementStation station =
			new("MS_DETAIL_001", location, new Installation(DateTime.UtcNow.AddDays(-10)));
		_dbContext.MeasurementStations.Add(station);
		await _dbContext.SaveChangesAsync();

		// Act
		Result<MeasurementStationResponse.Detail> result = await _service.GetDetailAsync(station.Id);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.Station.Code.ShouldBe("MS_DETAIL_001");
		result.Value.Station.Location.ShouldBe("Detail Location");
		result.Value.Station.City.ShouldBe("Detail City");
	}

	[Fact]
	public async Task GetDetailAsync_ShouldReturnNotFound_WhenStationDoesNotExist() {
		// Act
		Result<MeasurementStationResponse.Detail> result = await _service.GetDetailAsync(Guid.NewGuid());

		// Assert
		result.IsNotFound().ShouldBeTrue();
	}

	#endregion

	#region GetIndexAsync Tests

	[Fact]
	public async Task GetIndexAsync_ShouldReturnStations_WithPagination() {
		// Arrange
		Location location = new("Index Location", new Address("Index St", "", "Index City", "4000"),
			new Coordinate(50.8, 4.8));
		_dbContext!.Locations.Add(location);

		for (int i = 1; i <= 5; i++) {
			MeasurementStation station =
				new($"MS_INDEX_{i:D3}", location, new Installation(DateTime.UtcNow.AddDays(-i)));
			_dbContext.MeasurementStations.Add(station);
		}

		await _dbContext.SaveChangesAsync();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 3,
			SearchTerm = ""
		};

		// Act
		Result<MeasurementStationResponse.Index> result = await _service.GetIndexAsync(request);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.Stations.Count().ShouldBe(3);
		result.Value.TotalCount.ShouldBeGreaterThanOrEqualTo(5);
	}

	[Fact]
	public async Task GetIndexAsync_ShouldFilterBySearchTerm_OnCode() {
		// Arrange
		Location location = new("Search Location", new Address("Search St", "", "Search City", "5000"),
			new Coordinate(51.0, 5.0));
		_dbContext!.Locations.Add(location);

		MeasurementStation station1 =
			new("SEARCHABLE_001", location, new Installation(DateTime.UtcNow));
		MeasurementStation station2 =
			new("OTHER_001", location, new Installation(DateTime.UtcNow));
		_dbContext.MeasurementStations.AddRange(station1, station2);
		await _dbContext.SaveChangesAsync();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 10,
			SearchTerm = "SEARCHABLE"
		};

		// Act
		Result<MeasurementStationResponse.Index> result = await _service.GetIndexAsync(request);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.Stations.ShouldAllBe(s => s.Code.Contains("SEARCHABLE"));
	}

	[Fact]
	public async Task GetIndexAsync_ShouldFilterBySearchTerm_OnLocationName() {
		// Arrange
		Location location1 = new("Unique Location Name", new Address("St 1", "", "City 1", "6000"),
			new Coordinate(51.2, 5.2));
		Location location2 = new("Other Location", new Address("St 2", "", "City 2", "6001"),
			new Coordinate(51.3, 5.3));
		_dbContext!.Locations.AddRange(location1, location2);

		MeasurementStation station1 = new("MS_FILTER_001", location1, new Installation(DateTime.UtcNow));
		MeasurementStation station2 = new("MS_FILTER_002", location2, new Installation(DateTime.UtcNow));
		_dbContext.MeasurementStations.AddRange(station1, station2);
		await _dbContext.SaveChangesAsync();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 10,
			SearchTerm = "Unique"
		};

		// Act
		Result<MeasurementStationResponse.Index> result = await _service.GetIndexAsync(request);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.Stations.ShouldAllBe(s => s.Location!.Contains("Unique"));
	}

	[Fact]
	public async Task GetIndexAsync_ShouldSortByInstalledAt_Descending() {
		// Arrange
		Location location = new("Sort Location", new Address("Sort St", "", "Sort City", "7000"),
			new Coordinate(51.5, 5.5));
		_dbContext!.Locations.Add(location);

		MeasurementStation stationOld =
			new("MS_SORT_001", location, new Installation(DateTime.UtcNow.AddDays(-30)));
		MeasurementStation stationNew =
			new("MS_SORT_002", location, new Installation(DateTime.UtcNow.AddDays(-1)));
		_dbContext.MeasurementStations.AddRange(stationOld, stationNew);
		await _dbContext.SaveChangesAsync();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 10,
			SearchTerm = "MS_SORT",
			OrderBy = nameof(Installation.InstalledAt),
			OrderDescending = true
		};

		// Act
		Result<MeasurementStationResponse.Index> result = await _service.GetIndexAsync(request);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		List<MeasurementStationDto.Index> stations = result.Value.Stations.ToList();
		stations.Count.ShouldBeGreaterThanOrEqualTo(2);
		stations[0].InstalledAt.ShouldBeGreaterThan(stations[1].InstalledAt);
	}

	#endregion

	#region EditAsync Tests

	[Fact]
	public async Task EditAsync_ShouldUpdateStation_WhenRequestIsValid() {
		// Arrange
		Location location1 = new("Edit Location 1", new Address("Edit St 1", "", "Edit City 1", "9000"),
			new Coordinate(52.5, 6.5));
		Location location2 = new("Edit Location 2", new Address("Edit St 2", "", "Edit City 2", "9001"),
			new Coordinate(52.6, 6.6));
		_dbContext!.Locations.AddRange(location1, location2);

		MeasurementNetwork network = new("Edit Network", new Zone("Edit Zone", "EZ"));
		_dbContext.MeasurementNetworks.Add(network);

		Project project = new("Edit Project", "EDIT_PROJ_001", new DateRange(DateOnly.FromDateTime(DateTime.Today)),
			network);
		_dbContext.Projects.Add(project);

		MeasurementStation station =
			new("MS_EDIT_001", location1, new Installation(DateTime.UtcNow.AddDays(-10)));
		_dbContext.MeasurementStations.Add(station);
		await _dbContext.SaveChangesAsync();

		MeasurementStationRequest.Edit request = new() {
			StationId = station.Id,
			Code = "MS_EDIT_001",
			LocationId = location2.Id,
			ProjectId = project.Id,
			InstalledAt = DateTime.UtcNow.AddDays(-5),
			RemovedAt = DateTime.UtcNow
		};

		// Act
		Result result = await _service.EditAsync(request);

		// Assert
		result.IsSuccess.ShouldBeTrue();

		MeasurementStation? updatedStation =
			await _dbContext.MeasurementStations.Include(s => s.Location)
				.Include(measurementStation => measurementStation.Installation)
				.FirstOrDefaultAsync(s => s.Id == station.Id);
		updatedStation.ShouldNotBeNull();
		updatedStation.Location.Id.ShouldBe(location2.Id);
		updatedStation.Installation.RemovedAt.ShouldNotBeNull();
	}

	[Fact]
	public async Task EditAsync_ShouldReturnNotFound_WhenStationDoesNotExist() {
		// Arrange
		Location location = new("NotFound Location", new Address("NF St", "", "NF City", "9999"),
			new Coordinate(53.0, 7.0));
		_dbContext!.Locations.Add(location);

		MeasurementNetwork network = new("NotFound Network", new Zone("NotFound Zone", "NFZ"));
		_dbContext.MeasurementNetworks.Add(network);

		Project project = new("NotFound Project", "NF_PROJ_001",
			new DateRange(DateOnly.FromDateTime(DateTime.Today)), network);
		_dbContext.Projects.Add(project);
		await _dbContext.SaveChangesAsync();

		MeasurementStationRequest.Edit request = new() {
			StationId = Guid.NewGuid(),
			Code = "MS_NOTFOUND",
			LocationId = location.Id,
			ProjectId = project.Id,
			InstalledAt = DateTime.UtcNow
		};

		// Act
		Result result = await _service.EditAsync(request);

		// Assert
		result.IsNotFound().ShouldBeTrue();
	}

	[Fact]
	public async Task EditAsync_ShouldReturnNotFound_WhenLocationDoesNotExist() {
		// Arrange
		Location location = new("Edit NoLoc Location", new Address("ENL St", "", "ENL City", "9100"),
			new Coordinate(53.5, 7.5));
		_dbContext!.Locations.Add(location);

		MeasurementNetwork network = new("Edit NoLoc Network", new Zone("Edit NoLoc Zone", "ENLZ"));
		_dbContext.MeasurementNetworks.Add(network);

		Project project = new("Edit NoLoc Project", "ENL_PROJ_001",
			new DateRange(DateOnly.FromDateTime(DateTime.Today)), network);
		_dbContext.Projects.Add(project);

		MeasurementStation station =
			new("MS_EDIT_NOLOC", location, new Installation(DateTime.UtcNow.AddDays(-10)));
		_dbContext.MeasurementStations.Add(station);
		await _dbContext.SaveChangesAsync();

		MeasurementStationRequest.Edit request = new() {
			StationId = station.Id,
			Code = "MS_EDIT_NOLOC",
			LocationId = Guid.NewGuid(),
			ProjectId = project.Id,
			InstalledAt = DateTime.UtcNow
		};

		// Act
		Result result = await _service.EditAsync(request);

		// Assert
		result.IsNotFound().ShouldBeTrue();
	}

	#endregion

	#region GetProjectsAsync Tests

	[Fact]
	public async Task GetProjectsAsync_ShouldReturnProjects_WhenStationHasProjects() {
		// Arrange
		Location location = new("GetProj Location", new Address("GP St", "", "GP City", "9200"),
			new Coordinate(54.0, 8.0));
		_dbContext!.Locations.Add(location);

		MeasurementNetwork network = new("GetProj Network", new Zone("GetProj Zone", "GPZ"));
		_dbContext.MeasurementNetworks.Add(network);

		Project project1 = new("GetProj Project 1", "GP_PROJ_001",
			new DateRange(DateOnly.FromDateTime(DateTime.Today)), network);
		Project project2 = new("GetProj Project 2", "GP_PROJ_002",
			new DateRange(DateOnly.FromDateTime(DateTime.Today)), network);
		_dbContext.Projects.AddRange(project1, project2);

		MeasurementStation station =
			new("MS_GETPROJ_001", location, new Installation(DateTime.UtcNow.AddDays(-10)));
		station.Projects.Add(project1);
		station.Projects.Add(project2);
		_dbContext.MeasurementStations.Add(station);
		await _dbContext.SaveChangesAsync();

		// Act
		Result<List<ProjectDto.Index>> result = await _service.GetProjectsAsync(station.Id);

		// Assert
		result.IsSuccess.ShouldBeTrue();
		result.Value.Count.ShouldBe(2);
		result.Value.ShouldContain(p => p.Code == "GP_PROJ_001");
		result.Value.ShouldContain(p => p.Code == "GP_PROJ_002");
	}

	[Fact]
	public async Task GetProjectsAsync_ShouldReturnNotFound_WhenStationDoesNotExist() {
		// Act
		Result<List<ProjectDto.Index>> result = await _service.GetProjectsAsync(Guid.NewGuid());

		// Assert
		result.IsNotFound().ShouldBeTrue();
	}

	#endregion

	#region UpdateProjectsAsync Tests

	[Fact]
	public async Task UpdateProjectsAsync_ShouldUpdateProjects_WhenRequestIsValid() {
		// Arrange
		Location location = new("UpdateProj Location", new Address("UP St", "", "UP City", "9300"),
			new Coordinate(54.5, 8.5));
		_dbContext!.Locations.Add(location);

		MeasurementNetwork network = new("UpdateProj Network", new Zone("UpdateProj Zone", "UPZ"));
		_dbContext.MeasurementNetworks.Add(network);

		Project project1 = new("UpdateProj Project 1", "UP_PROJ_001",
			new DateRange(DateOnly.FromDateTime(DateTime.Today)), network);
		Project project2 = new("UpdateProj Project 2", "UP_PROJ_002",
			new DateRange(DateOnly.FromDateTime(DateTime.Today)), network);
		Project project3 = new("UpdateProj Project 3", "UP_PROJ_003",
			new DateRange(DateOnly.FromDateTime(DateTime.Today)), network);
		_dbContext.Projects.AddRange(project1, project2, project3);

		MeasurementStation station =
			new("MS_UPDATEPROJ_001", location, new Installation(DateTime.UtcNow.AddDays(-10)));
		station.Projects.Add(project1);
		_dbContext.MeasurementStations.Add(station);
		await _dbContext.SaveChangesAsync();

		MeasurementStationRequest.UpdateProjects request = new() {
			MeasurementStationId = station.Id,
			ProjectIds = [project2.Id, project3.Id]
		};

		// Act
		Result result = await _service.UpdateProjectsAsync(request);

		// Assert
		result.IsSuccess.ShouldBeTrue();

		MeasurementStation? updatedStation = await _dbContext.MeasurementStations
			.Include(s => s.Projects)
			.FirstOrDefaultAsync(s => s.Id == station.Id);
		updatedStation.ShouldNotBeNull();
		updatedStation.Projects.Count.ShouldBe(2);
		updatedStation.Projects.ShouldContain(p => p.Id == project2.Id);
		updatedStation.Projects.ShouldContain(p => p.Id == project3.Id);
		updatedStation.Projects.ShouldNotContain(p => p.Id == project1.Id);
	}

	[Fact]
	public async Task UpdateProjectsAsync_ShouldReturnNotFound_WhenStationDoesNotExist() {
		// Arrange
		MeasurementStationRequest.UpdateProjects request = new() {
			MeasurementStationId = Guid.NewGuid(),
			ProjectIds = [Guid.NewGuid()]
		};

		// Act
		Result result = await _service.UpdateProjectsAsync(request);

		// Assert
		result.IsNotFound().ShouldBeTrue();
	}

	[Fact]
	public async Task UpdateProjectsAsync_ShouldReturnError_WhenProjectDoesNotExist() {
		// Arrange
		Location location = new("UpdateProj Error Location", new Address("UPE St", "", "UPE City", "9400"),
			new Coordinate(55.0, 9.0));
		_dbContext!.Locations.Add(location);

		MeasurementStation station =
			new("MS_UPDATEPROJ_ERR", location, new Installation(DateTime.UtcNow.AddDays(-10)));
		_dbContext.MeasurementStations.Add(station);
		await _dbContext.SaveChangesAsync();

		MeasurementStationRequest.UpdateProjects request = new() {
			MeasurementStationId = station.Id,
			ProjectIds = [Guid.NewGuid()]
		};

		// Act
		Result result = await _service.UpdateProjectsAsync(request);

		// Assert
		result.IsSuccess.ShouldBeFalse();
	}

	#endregion
}