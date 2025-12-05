using Ardalis.Result;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Rise.Domain.Networks;
using Rise.Domain.Projects;
using Rise.Persistence;
using Rise.Services.Projects;
using Rise.Shared.Common;
using Rise.Shared.Projects;

using Testcontainers.MsSql;

using SharedStatus = Rise.Shared.Projects.Status;
using Status = Rise.Domain.Projects.Status;

namespace Rise.Services.Tests.Projects;

public class ProjectServiceShould : IAsyncLifetime {
	// One container for the entire test class
	private static readonly ApplicationDbContext? DbContext;
	private readonly ProjectService _service = new(DbContext!);
	private IDbContextTransaction? _transaction;

	static ProjectServiceShould() {
		MsSqlContainer? container = new MsSqlBuilder().WithImage("mcr.microsoft.com/mssql/server:2022-latest").Build();


		container.StartAsync().GetAwaiter().GetResult();

		DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseSqlServer(container.GetConnectionString())
			.Options;

		DbContext = new ApplicationDbContext(options);
		DbContext.Database.Migrate();
	}

	public async Task InitializeAsync() {
		// Start a transaction before each test
		_transaction = await DbContext!.Database.BeginTransactionAsync();
	}

	public async Task DisposeAsync() {
		// Rollback the transaction after each test to clean up
		if (_transaction != null) {
			await _transaction.RollbackAsync();
			await _transaction.DisposeAsync();
		}
	}

	#region CreateAsync Tests

	[Fact]
	public async Task CreateAsync_ShouldCreateProject_WhenRequestIsValid() {
		// Arrange
		MeasurementNetwork network = new("MN2", new Zone("Zone2", "Z2"));
		DbContext!.MeasurementNetworks.Add(network);
		await DbContext.SaveChangesAsync();

		ProjectRequest.Create request = new() {
			Name = "New Project",
			Code = "NEWCODE",
			StartDate = DateTime.Today,
			MeasurementNetworkId = network.Id
		};

		// Act
		Result<ProjectResponse.Create> result = await _service.CreateAsync(request);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.NotEqual(Guid.Empty, result.Value.ProjectId);

		Project? createdProject = await DbContext.Projects.FindAsync(result.Value.ProjectId);
		Assert.NotNull(createdProject);
		Assert.Equal("New Project", createdProject.Name);
		Assert.Equal("NEWCODE", createdProject.Code);
	}

	#endregion

	#region CreateAsync Conflict & NotFound Tests

	[Fact]
	public async Task CreateAsync_ShouldReturnConflict_WhenCodeAlreadyExists() {
		// Arrange
		MeasurementNetwork network = new("MN3", new Zone("Zone3", "Z3"));
		DbContext!.MeasurementNetworks.Add(network);
		Project existingProject = new("Existing Project", "DUPCODE",
			new DateRange(DateOnly.FromDateTime(DateTime.Today)), network);
		DbContext.Projects.Add(existingProject);
		await DbContext.SaveChangesAsync();

		ProjectRequest.Create request = new() {
			Name = "Another Project",
			Code = "DUPCODE",
			StartDate = DateTime.Today,
			MeasurementNetworkId = network.Id
		};

		// Act
		Result<ProjectResponse.Create> result = await _service.CreateAsync(request);

		// Assert
		Assert.True(result.Status == ResultStatus.Conflict);
	}

	[Fact]
	public async Task CreateAsync_ShouldReturnNotFound_WhenMeasurementNetworkDoesNotExist() {
		// Arrange
		ProjectRequest.Create request = new() {
			Name = "Test Project",
			Code = "TESTCODE",
			StartDate = DateTime.Today,
			MeasurementNetworkId = Guid.NewGuid()
		};

		// Act
		Result<ProjectResponse.Create> result = await _service.CreateAsync(request);

		// Assert
		Assert.True(result.IsNotFound());
	}

	#endregion

	#region DetailAsync Tests

	[Fact]
	public async Task GetDetailAsync_ShouldReturnNotFound_WhenProjectDoesNotExist() {
		Result<ProjectResponse.Detail> result = await _service.GetDetailAsync(Guid.NewGuid());
		Assert.True(result.IsNotFound());
	}

	[Fact]
	public async Task GetDetailAsync_ShouldReturnProjectDetails_WhenProjectExists() {
		MeasurementNetwork network = new("MN_Detail", new Zone("Zone_Detail", "ZD"));
		DbContext!.MeasurementNetworks.Add(network);
		Project project = new(
			"Test Project", "DETAIL_CODE",
			new DateRange(DateOnly.FromDateTime(DateTime.Today)), network);
		DbContext.Projects.Add(project);
		await DbContext.SaveChangesAsync();

		Result<ProjectResponse.Detail> result = await _service.GetDetailAsync(project.Id);

		Assert.True(result.IsSuccess);
		Assert.Equal("Test Project", result.Value!.Project.Name);
	}

	#endregion

	#region EditAsync Tests

	[Fact]
	public async Task EditAsync_ShouldEditProject_WhenRequestIsValid() {
		// Arrange
		MeasurementNetwork network = new("MN4", new Zone("Zone4", "Z4"));
		DbContext!.MeasurementNetworks.Add(network);
		Project project = new("Original Name", "ORIGCODE", new DateRange(DateOnly.FromDateTime(DateTime.Today)),
			network);
		DbContext.Projects.Add(project);
		await DbContext.SaveChangesAsync();

		ProjectRequest.Edit request = new() {
			ProjectId = project.Id,
			Name = "Updated Name",
			Code = "UPDCODE",
			StartDate = DateTime.Today,
			EndDate = DateTime.Today.AddDays(30),
			MeasurementNetworkId = network.Id
		};

		// Act
		Result result = await _service.EditAsync(request);

		// Assert
		Assert.True(result.IsSuccess);

		Project? updatedProject = await DbContext.Projects.FindAsync(project.Id);
		Assert.NotNull(updatedProject);
		Assert.Equal("Updated Name", updatedProject.Name);
		Assert.Equal("UPDCODE", updatedProject.Code);
	}

	[Fact]
	public async Task EditAsync_ShouldReturnNotFound_WhenProjectDoesNotExist() {
		// Arrange
		MeasurementNetwork network = new("MN5_EditNotFound", new Zone("Zone5", "Z5"));
		DbContext!.MeasurementNetworks.Add(network);
		await DbContext.SaveChangesAsync();

		ProjectRequest.Edit request = new() {
			ProjectId = Guid.NewGuid(),
			Name = "Test Name",
			Code = "EDIT_NOTFOUND_CODE",
			StartDate = DateTime.Today,
			MeasurementNetworkId = network.Id
		};

		// Act
		Result result = await _service.EditAsync(request);

		// Assert
		Assert.True(result.IsNotFound());
	}

	[Fact]
	public async Task EditAsync_ShouldReturnConflict_WhenCodeAlreadyExistsForDifferentProject() {
		// Arrange
		MeasurementNetwork network = new("MN6_EditConflict", new Zone("Zone6", "Z6"));
		DbContext!.MeasurementNetworks.Add(network);
		Project project1 = new("Project 1", "EDIT_CONFLICT_CODE1", new DateRange(DateOnly.FromDateTime(DateTime.Today)),
			network);
		Project project2 = new("Project 2", "EDIT_CONFLICT_CODE2", new DateRange(DateOnly.FromDateTime(DateTime.Today)),
			network);
		DbContext.Projects.AddRange(project1, project2);
		await DbContext.SaveChangesAsync();

		ProjectRequest.Edit request = new() {
			ProjectId = project2.Id,
			Name = "Project 2 Updated",
			Code = "EDIT_CONFLICT_CODE1", // Trying to use code from project1
			StartDate = DateTime.Today,
			MeasurementNetworkId = network.Id
		};

		// Act
		Result result = await _service.EditAsync(request);

		// Assert
		Assert.True(result.Status == ResultStatus.Conflict);
	}

	[Fact]
	public async Task EditAsync_ShouldReturnNotFound_WhenMeasurementNetworkDoesNotExist() {
		// Arrange
		MeasurementNetwork network = new("MN7_EditNetNotFound", new Zone("Zone7", "Z7"));
		DbContext!.MeasurementNetworks.Add(network);
		Project project = new("Test Project", "EDIT_NET_NOTFOUND_CODE",
			new DateRange(DateOnly.FromDateTime(DateTime.Today)), network);
		DbContext.Projects.Add(project);
		await DbContext.SaveChangesAsync();

		ProjectRequest.Edit request = new() {
			ProjectId = project.Id,
			Name = "Updated Name",
			Code = "EDIT_NET_NOTFOUND_CODE",
			StartDate = DateTime.Today,
			MeasurementNetworkId = Guid.NewGuid()
		};

		// Act
		Result result = await _service.EditAsync(request);

		// Assert
		Assert.True(result.IsNotFound());
	}

	#endregion

	#region GetIndexAsync Tests

	[Fact]
	public async Task GetIndexAsync_ShouldReturnProjects_WithPagination() {
		// Arrange
		MeasurementNetwork network = new("MN8_IndexPag", new Zone("Zone8", "Z8"));
		DbContext!.MeasurementNetworks.Add(network);

		for (int i = 1; i <= 5; i++) {
			Project project = new($"Project {i}", $"INDEX_PAG_CODE{i}",
				new DateRange(DateOnly.FromDateTime(DateTime.Today)), network);
			DbContext.Projects.Add(project);
		}

		await DbContext.SaveChangesAsync();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 3,
			SearchTerm = ""
		};

		// Act
		Result<ProjectResponse.Index> result = await _service.GetIndexAsync(request);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(3, result.Value.Projects.Count());
		Assert.True(result.Value.TotalCount >= 5);
	}

	[Fact]
	public async Task GetIndexAsync_ShouldFilterBySearchTerm() {
		// Arrange
		MeasurementNetwork network = new("MN9_IndexFilter", new Zone("Zone9", "Z9"));
		DbContext!.MeasurementNetworks.Add(network);
		Project project1 = new("Searchable Project", "INDEX_FILTER_SEARCH1",
			new DateRange(DateOnly.FromDateTime(DateTime.Today)), network);
		Project project2 = new("Another Project", "INDEX_FILTER_OTHER1",
			new DateRange(DateOnly.FromDateTime(DateTime.Today)), network);
		DbContext.Projects.AddRange(project1, project2);
		await DbContext.SaveChangesAsync();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 10,
			SearchTerm = "Searchable"
		};

		// Act
		Result<ProjectResponse.Index> result = await _service.GetIndexAsync(request);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Contains(result.Value.Projects, p => p.Name.Contains("Searchable"));
	}

	[Fact]
	public async Task GetIndexAsync_ShouldSortByName() {
		// Arrange
		MeasurementNetwork network = new("MN10_IndexSort", new Zone("Zone10", "Z10"));
		DbContext!.MeasurementNetworks.Add(network);
		Project projectA = new("AAA Project", "INDEX_SORT_CODEA", new DateRange(DateOnly.FromDateTime(DateTime.Today)),
			network);
		Project projectZ = new("ZZZ Project", "INDEX_SORT_CODEZ", new DateRange(DateOnly.FromDateTime(DateTime.Today)),
			network);
		DbContext.Projects.AddRange(projectA, projectZ);
		await DbContext.SaveChangesAsync();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 10,
			SearchTerm = "",
			OrderBy = "Name",
			OrderDescending = false
		};

		// Act
		Result<ProjectResponse.Index> result = await _service.GetIndexAsync(request);

		// Assert
		Assert.True(result.IsSuccess);
		List<ProjectDto.Index> projects = result.Value.Projects.ToList();
		for (int i = 0; i < projects.Count - 1; i++) {
			Assert.True(string.Compare(projects[i].Name, projects[i + 1].Name, StringComparison.Ordinal) <= 0);
		}
	}

	#endregion

	#region GetHistoryAsync Tests

	[Fact]
	public async Task GetHistoryAsync_ShouldReturnProjectHistory_WhenProjectExists() {
		// Arrange
		MeasurementNetwork network = new("MN11_History", new Zone("Zone11", "Z11"));
		DbContext!.MeasurementNetworks.Add(network);
		Project project = new("History Project", "HISTORY_CODE", new DateRange(DateOnly.FromDateTime(DateTime.Today)),
			network);
		DbContext.Projects.Add(project);
		await DbContext.SaveChangesAsync();

		// Act
		Result<ProjectResponse.History> result = await _service.GetHistoryAsync(project.Id);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(project.Id, result.Value.Project.Id);
		Assert.Equal("History Project", result.Value.Project.Name);
	}

	[Fact]
	public async Task GetHistoryAsync_ShouldReturnNotFound_WhenProjectDoesNotExist() {
		// Act
		Result<ProjectResponse.History> result = await _service.GetHistoryAsync(Guid.NewGuid());

		// Assert
		Assert.True(result.IsNotFound());
	}

	#endregion

	#region UpdateStatusAsync Tests

	[Fact]
	public async Task UpdateStatusAsync_ShouldUpdateStatus_WhenProjectExists() {
		// Arrange
		MeasurementNetwork network = new("MN12_Status", new Zone("Zone12", "Z12"));
		DbContext!.MeasurementNetworks.Add(network);
		Project project = new("Status Project", "STATUS_CODE", new DateRange(DateOnly.FromDateTime(DateTime.Today)),
			network);
		DbContext.Projects.Add(project);
		await DbContext.SaveChangesAsync();

		ProjectRequest.UpdateStatus request = new() {
			ProjectId = project.Id,
			NewStatus = SharedStatus.Afgesloten
		};

		// Act
		Result result = await _service.UpdateStatusAsync(request);

		// Assert
		Assert.True(result.IsSuccess);

		Project? updatedProject = await DbContext.Projects.FindAsync(project.Id);
		Assert.NotNull(updatedProject);
		Assert.Equal(Status.Afgesloten, updatedProject.Status);
	}

	[Fact]
	public async Task UpdateStatusAsync_ShouldReturnNotFound_WhenProjectDoesNotExist() {
		// Arrange
		ProjectRequest.UpdateStatus request = new() {
			ProjectId = Guid.NewGuid(),
			NewStatus = SharedStatus.Afgesloten
		};

		// Act
		Result result = await _service.UpdateStatusAsync(request);

		// Assert
		Assert.True(result.IsNotFound());
	}

	#endregion

	#region GetStationsAsync Tests

	[Fact]
	public async Task GetStationsAsync_ShouldReturnStations_WhenProjectExists() {
		// Arrange
		MeasurementNetwork network = new("MN13_Stations", new Zone("Zone13", "Z13"));
		DbContext!.MeasurementNetworks.Add(network);
		Project project = new("Station Project", "STATION_CODE", new DateRange(DateOnly.FromDateTime(DateTime.Today)),
			network);
		DbContext.Projects.Add(project);
		await DbContext.SaveChangesAsync();

		QueryRequest.Search request = new() {
			SearchTerm = ""
		};

		// Act
		Result<ProjectResponse.MeasurementStations> result = await _service.GetStationsAsync(project.Id, request);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value.MeasurementStationsFromProject);
	}

	[Fact]
	public async Task GetStationsAsync_ShouldReturnNotFound_WhenProjectDoesNotExist() {
		// Arrange
		QueryRequest.Search request = new() {
			SearchTerm = ""
		};

		// Act
		Result<ProjectResponse.MeasurementStations> result = await _service.GetStationsAsync(Guid.NewGuid(), request);

		// Assert
		Assert.True(result.IsNotFound());
	}

	#endregion

	#region UpdateMeasurementStationsAsync Tests

	[Fact]
	public async Task UpdateMeasurementStationsAsync_ShouldReturnNotFound_WhenProjectDoesNotExist() {
		// Arrange
		ProjectRequest.UpdateMeasurementStations request = new() {
			ProjectId = Guid.NewGuid(),
			MeasurementStationIds = new List<Guid>()
		};

		// Act
		Result result = await _service.UpdateMeasurementStationsAsync(request);

		// Assert
		Assert.True(result.IsNotFound());
	}

	[Fact]
	public async Task UpdateMeasurementStationsAsync_ShouldReturnError_WhenMeasurementStationNotFound() {
		// Arrange
		MeasurementNetwork network = new("MN14_UpdateStations", new Zone("Zone14", "Z14"));
		DbContext!.MeasurementNetworks.Add(network);
		Project project = new("Update Stations Project", "UPDATE_STATIONS_CODE",
			new DateRange(DateOnly.FromDateTime(DateTime.Today)), network);
		DbContext.Projects.Add(project);
		await DbContext.SaveChangesAsync();

		ProjectRequest.UpdateMeasurementStations request = new() {
			ProjectId = project.Id,
			MeasurementStationIds = new List<Guid> { Guid.NewGuid() } // Non-existent station
		};

		// Act
		Result result = await _service.UpdateMeasurementStationsAsync(request);

		// Assert
		Assert.True(result.Status == ResultStatus.Error);
	}

	#endregion
}