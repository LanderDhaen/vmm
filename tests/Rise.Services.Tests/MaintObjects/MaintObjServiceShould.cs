using Ardalis.Result;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Rise.Domain.Locations;
using Rise.Domain.MaintObjects;
using Rise.Persistence;
using Rise.Services.MaintObjects;
using Rise.Shared.Common;
using Rise.Shared.MaintObjects;

using Testcontainers.MsSql;

using MaintObjType = Rise.Domain.MaintObjects.MaintObjType;
using SharedMaintObjType = Rise.Shared.MaintObjects.MaintObjType;

namespace Rise.Services.Tests.MaintObjects;

public class MaintObjServiceShould : IAsyncLifetime {
	// One container for the entire test class
	private static readonly ApplicationDbContext? DbContext;
	private readonly MaintObjService _service = new(DbContext!);
	private IDbContextTransaction? _transaction;


	static MaintObjServiceShould() {
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

	#region Helper Methods

	private static Location CreateTestLocation(string name) {
		Address address = new("Street", "1", "City", "12345");
		Coordinate coordinate = new(50.0, 4.0);
		return new Location(name, address, coordinate, true);
	}

	#endregion

	#region GetIndexAsync Tests

	[Fact]
	public async Task GetIndexAsync_ShouldReturnMaintObjects_WithPagination() {
		// Arrange
		Location location = CreateTestLocation("Index_Pagination_Location");
		DbContext!.Locations.Add(location);

		MaintObjType maintObjType = new(Guid.NewGuid(), "Sonde_Pagination", "Test Sonde Pagination", 30, 60);
		DbContext.MaintObjTypes.Add(maintObjType);
		await DbContext.SaveChangesAsync();

		for (int i = 1; i <= 5; i++) {
			MaintObj maintObj = new($"MaintObj {i}", $"SERIAL_PAG_{i}", maintObjType, "TestBrand", null, true,
				location);
			DbContext.MaintObjects.Add(maintObj);
		}

		await DbContext.SaveChangesAsync();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 3,
			OrderBy = ""
		};

		// Act
		Result<MaintObjResponse.Index> result = await _service.GetIndexAsync(request);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(3, result.Value.MaintObjects.Count());
		Assert.True(result.Value.TotalCount >= 5);
	}

	[Fact]
	public async Task GetIndexAsync_ShouldSortByName() {
		// Arrange
		Location location = CreateTestLocation("Index_Sort_Location");
		DbContext!.Locations.Add(location);

		MaintObjType maintObjType = new(Guid.NewGuid(), "Logger_Sort", "Test Logger Sort", 30, 60);
		DbContext.MaintObjTypes.Add(maintObjType);
		await DbContext.SaveChangesAsync();

		MaintObj maintObjA = new("AAA MaintObj", "SERIAL_SORT_A", maintObjType, "BrandA", null, true, location);
		MaintObj maintObjZ = new("ZZZ MaintObj", "SERIAL_SORT_Z", maintObjType, "BrandZ", null, true, location);
		DbContext.MaintObjects.AddRange(maintObjA, maintObjZ);
		await DbContext.SaveChangesAsync();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 10,
			OrderBy = "Name",
			OrderDescending = false
		};

		// Act
		Result<MaintObjResponse.Index> result = await _service.GetIndexAsync(request);

		// Assert
		Assert.True(result.IsSuccess);
		List<MaintObjDto.Index> maintObjects = result.Value.MaintObjects.ToList();
		for (int i = 0; i < maintObjects.Count - 1; i++) {
			Assert.True(string.Compare(maintObjects[i].Name, maintObjects[i + 1].Name, StringComparison.Ordinal) <= 0);
		}
	}

	#endregion

	#region CreateAsync Tests

	[Fact]
	public async Task CreateAsync_ShouldCreateMaintObj_WhenRequestIsValid() {
		// Arrange
		Location location = CreateTestLocation("Create_Valid_Location");
		DbContext!.Locations.Add(location);

		MaintObjType maintObjType = new(Guid.NewGuid(), "Modem_Create", "Test Modem Create", 30, 60);
		DbContext.MaintObjTypes.Add(maintObjType);
		await DbContext.SaveChangesAsync();

		MaintObjRequest.Create request = new() {
			Name = "New MaintObj",
			SerialNumber = "CREATE_SERIAL_123",
			MaintObjType = SharedMaintObjType.Modem,
			Brand = "TestBrand",
			LocationId = location.Id
		};

		// Act
		Result<MaintObjResponse.Create> result = await _service.CreateAsync(request);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.NotEqual(Guid.Empty, result.Value.MaintObjId);

		MaintObj? createdMaintObj = await DbContext.MaintObjects.FindAsync(result.Value.MaintObjId);
		Assert.NotNull(createdMaintObj);
		Assert.Equal("New MaintObj", createdMaintObj.Name);
		Assert.Equal("CREATE_SERIAL_123", createdMaintObj.SerialNumber);
	}

	[Fact]
	public async Task CreateAsync_ShouldReturnConflict_WhenSerialNumberAlreadyExists() {
		// Arrange
		Location location = CreateTestLocation("Create_Conflict_Location");
		DbContext!.Locations.Add(location);

		MaintObjType maintObjType = new(Guid.NewGuid(), "Sim_Conflict", "Test Sim Conflict", 30, 60);
		DbContext.MaintObjTypes.Add(maintObjType);

		MaintObj existingMaintObj = new("Existing MaintObj", "CONFLICT_SERIAL", maintObjType, "Brand", null, true,
			location);
		DbContext.MaintObjects.Add(existingMaintObj);
		await DbContext.SaveChangesAsync();

		MaintObjRequest.Create request = new() {
			Name = "Another MaintObj",
			SerialNumber = "CONFLICT_SERIAL",
			MaintObjType = SharedMaintObjType.Sim,
			Brand = "AnotherBrand",
			LocationId = location.Id
		};

		// Act
		Result<MaintObjResponse.Create> result = await _service.CreateAsync(request);

		// Assert
		Assert.True(result.Status == ResultStatus.Conflict);
	}

	[Fact]
	public async Task CreateAsync_ShouldReturnNotFound_WhenLocationDoesNotExist() {
		// Arrange
		MaintObjType maintObjType = new(Guid.NewGuid(), "Voeding_NoLoc", "Test Voeding NoLoc", 30, 60);
		DbContext!.MaintObjTypes.Add(maintObjType);
		await DbContext.SaveChangesAsync();

		MaintObjRequest.Create request = new() {
			Name = "Test MaintObj",
			SerialNumber = "NOTFOUND_SERIAL",
			MaintObjType = SharedMaintObjType.Voeding,
			Brand = "TestBrand",
			LocationId = Guid.NewGuid()
		};

		// Act
		Result<MaintObjResponse.Create> result = await _service.CreateAsync(request);

		// Assert
		Assert.True(result.IsNotFound());
	}

	// Note: Cannot test "MaintObjType does not exist" scenario because all MaintObjTypes
	// are seeded in the database via MaintObjTypeConfig.HasData()

	#endregion

	#region GetDetailAsync Tests

	[Fact]
	public async Task GetDetailAsync_ShouldReturnNotFound_WhenMaintObjDoesNotExist() {
		// Act
		Result<MaintObjResponse.Detail> result = await _service.GetDetailAsync(Guid.NewGuid());

		// Assert
		Assert.True(result.IsNotFound());
	}

	[Fact]
	public async Task GetDetailAsync_ShouldReturnMaintObjDetails_WhenMaintObjExists() {
		// Arrange
		Location location = CreateTestLocation("Detail_Location");
		DbContext!.Locations.Add(location);

		MaintObjType maintObjType = new(Guid.NewGuid(), "Sonde_Detail", "Detail Sonde", 30, 60);
		DbContext.MaintObjTypes.Add(maintObjType);

		MaintObj maintObj = new("Detail MaintObj", "DETAIL_SERIAL", maintObjType, "DetailBrand", null, true, location);
		DbContext.MaintObjects.Add(maintObj);
		await DbContext.SaveChangesAsync();

		// Act
		Result<MaintObjResponse.Detail> result = await _service.GetDetailAsync(maintObj.Id);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal("Detail MaintObj", result.Value.MaintObj.Name);
		Assert.Equal("DETAIL_SERIAL", result.Value.MaintObj.SerialNumber);
	}

	#endregion

	#region EditAsync Tests

	[Fact]
	public async Task EditAsync_ShouldEditMaintObj_WhenRequestIsValid() {
		// Arrange
		Location location = CreateTestLocation("Edit_Valid_Location");
		DbContext!.Locations.Add(location);

		MaintObjType maintObjType = new(Guid.NewGuid(), "Logger_Edit", "Edit Logger", 30, 60);
		DbContext.MaintObjTypes.Add(maintObjType);

		MaintObj maintObj = new("Original Name", "EDIT_ORIG_SERIAL", maintObjType, "OriginalBrand", null, true,
			location);
		DbContext.MaintObjects.Add(maintObj);
		await DbContext.SaveChangesAsync();

		MaintObjRequest.Edit request = new() {
			MaintObjId = maintObj.Id,
			Name = "Updated Name",
			SerialNumber = "EDIT_UPD_SERIAL",
			MaintObjType = SharedMaintObjType.Logger,
			Brand = "UpdatedBrand",
			InstalledAt = DateTime.Today,
			RemovedAt = null,
			LocationId = location.Id
		};

		// Act
		Result result = await _service.EditAsync(request);

		// Assert
		Assert.True(result.IsSuccess);

		MaintObj? updatedMaintObj = await DbContext.MaintObjects.FindAsync(maintObj.Id);
		Assert.NotNull(updatedMaintObj);
		Assert.Equal("Updated Name", updatedMaintObj.Name);
		Assert.Equal("EDIT_UPD_SERIAL", updatedMaintObj.SerialNumber);
	}

	[Fact]
	public async Task EditAsync_ShouldReturnNotFound_WhenMaintObjDoesNotExist() {
		// Arrange
		Location location = CreateTestLocation("Edit_NotFound_Location");
		DbContext!.Locations.Add(location);
		await DbContext.SaveChangesAsync();

		MaintObjRequest.Edit request = new() {
			MaintObjId = Guid.NewGuid(),
			Name = "Test Name",
			SerialNumber = "EDIT_NOTFOUND_SERIAL",
			MaintObjType = SharedMaintObjType.Modem,
			Brand = "TestBrand",
			InstalledAt = DateTime.Today,
			LocationId = location.Id
		};

		// Act
		Result result = await _service.EditAsync(request);

		// Assert
		Assert.True(result.IsNotFound());
	}

	[Fact]
	public async Task EditAsync_ShouldReturnConflict_WhenSerialNumberAlreadyExistsForDifferentMaintObj() {
		// Arrange
		Location location = CreateTestLocation("Edit_Conflict_Location");
		DbContext!.Locations.Add(location);

		MaintObjType maintObjType = new(Guid.NewGuid(), "Sim_EditConflict", "Edit Sim Conflict", 30, 60);
		DbContext.MaintObjTypes.Add(maintObjType);

		MaintObj maintObj1 = new("MaintObj 1", "EDIT_CONFLICT_SERIAL1", maintObjType, "Brand1", null, true, location);
		MaintObj maintObj2 = new("MaintObj 2", "EDIT_CONFLICT_SERIAL2", maintObjType, "Brand2", null, true, location);
		DbContext.MaintObjects.AddRange(maintObj1, maintObj2);
		await DbContext.SaveChangesAsync();

		MaintObjRequest.Edit request = new() {
			MaintObjId = maintObj2.Id,
			Name = "MaintObj 2 Updated",
			SerialNumber = "EDIT_CONFLICT_SERIAL1", // Trying to use serial from maintObj1
			MaintObjType = SharedMaintObjType.Sim,
			Brand = "Brand2",
			InstalledAt = DateTime.Today,
			LocationId = location.Id
		};

		// Act
		Result result = await _service.EditAsync(request);

		// Assert
		Assert.True(result.Status == ResultStatus.Conflict);
	}

	[Fact]
	public async Task EditAsync_ShouldReturnNotFound_WhenLocationDoesNotExist() {
		// Arrange
		Location location = CreateTestLocation("Edit_NoLoc_Location");
		DbContext!.Locations.Add(location);

		MaintObjType maintObjType = new(Guid.NewGuid(), "Voeding_EditNoLoc", "Edit Voeding NoLoc", 30, 60);
		DbContext.MaintObjTypes.Add(maintObjType);

		MaintObj maintObj = new("Test MaintObj", "EDIT_NOLOC_SERIAL", maintObjType, "TestBrand", null, true, location);
		DbContext.MaintObjects.Add(maintObj);
		await DbContext.SaveChangesAsync();

		MaintObjRequest.Edit request = new() {
			MaintObjId = maintObj.Id,
			Name = "Updated Name",
			SerialNumber = "EDIT_NOLOC_SERIAL",
			MaintObjType = SharedMaintObjType.Voeding,
			Brand = "TestBrand",
			InstalledAt = DateTime.Today,
			LocationId = Guid.NewGuid()
		};

		// Act
		Result result = await _service.EditAsync(request);

		// Assert
		Assert.True(result.IsNotFound());
	}

	[Fact]
	public async Task EditAsync_ShouldReturnNotFound_WhenMaintObjTypeDoesNotExist() {
		// Arrange
		Location location = CreateTestLocation("Edit_NoType_Location");
		DbContext!.Locations.Add(location);

		MaintObjType maintObjType = new(Guid.NewGuid(), "Sonde_EditNoType", "Edit Sonde NoType", 30, 60);
		DbContext.MaintObjTypes.Add(maintObjType);

		MaintObj maintObj = new("Test MaintObj", "EDIT_NOTYPE_SERIAL", maintObjType, "TestBrand", null, true,
			location);
		DbContext.MaintObjects.Add(maintObj);
		await DbContext.SaveChangesAsync();

		// Remove the type from DB to simulate it not existing
		DbContext.MaintObjTypes.Remove(maintObjType);
		await DbContext.SaveChangesAsync();

		MaintObjRequest.Edit request = new() {
			MaintObjId = maintObj.Id,
			Name = "Updated Name",
			SerialNumber = "EDIT_NOTYPE_SERIAL",
			MaintObjType = SharedMaintObjType.Modem, // Different type that doesn't exist
			Brand = "TestBrand",
			InstalledAt = DateTime.Today,
			LocationId = location.Id
		};

		// Act
		Result result = await _service.EditAsync(request);

		// Assert
		Assert.True(result.IsNotFound());
	}

	#endregion
}