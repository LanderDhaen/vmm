using Ardalis.Result;

using Microsoft.EntityFrameworkCore;

using Rise.Domain.Locations;
using Rise.Domain.MaintObjects;
using Rise.Persistence;
using Rise.Services.MaintObjects;
using Rise.Services.Tests.Common;
using Rise.Shared.MaintObjects;

using Testcontainers.MsSql;

using MaintObjType = Rise.Domain.MaintObjects.MaintObjType;

namespace Rise.Services.Tests.MaintObjects;

public class MaintObjServiceShould : IClassFixture<MsSqlContainerFixture> {
	// One container for the entire test class
	private readonly ApplicationDbContext _dbContext;
	private readonly MaintObjService _service;

	#region Setup
	public MaintObjServiceShould(MsSqlContainerFixture fixture) {
		MsSqlContainer container = fixture.Container!;
		DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseSqlServer(container.GetConnectionString()).Options;

		_dbContext = new ApplicationDbContext(options);
		_dbContext.Database.Migrate();

		_service = new MaintObjService(_dbContext);
	}
	#endregion

	#region GetBySerial

	[Fact]
	public async Task GetBySerialAsync_ShouldReturnInvalid_WhenSerialIsEmpty() {
		Result<MaintObjResponse.BySerial> result = await _service.GetBySerialAsync("");

		Assert.False(result.IsSuccess);
		Assert.Equal(ResultStatus.Invalid, result.Status);
		Assert.NotEmpty(result.ValidationErrors);
		Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("Serienummer"));
	}

	[Fact]
	public async Task GetBySerialAsync_ShouldReturnNotFound_WhenSerialDoesNotExist() {
		Result<MaintObjResponse.BySerial> result = await _service.GetBySerialAsync("DOES-NOT-EXIST");

		Assert.Equal(ResultStatus.NotFound, result.Status);
	}

	[Fact]
	public async Task GetBySerialAsync_ShouldReturnMaintObj_WhenSerialExists() {
		MaintObjPlan plan = new(
			"Test Plan",
			30,
			60
		);

		Location location = new(
			"TestLoc",
			new Address("Street 1", "Bus 2", "City", "1234"),
			new Coordinate(4.35, 50.85)
		);

		_dbContext.MaintObjPlans.Add(plan);
		_dbContext.Locations.Add(location);
		await _dbContext.SaveChangesAsync();

		string serial = $"ABC-{Guid.NewGuid():N}";
		MaintObj mo = new(
			"Test Device",
			serial,
			MaintObjType.Sensor,
			plan,
			"RiseBrand",
			location
		);

		_dbContext.MaintObjects.Add(mo);
		await _dbContext.SaveChangesAsync();


		Result<MaintObjResponse.BySerial> result = await _service.GetBySerialAsync(serial);

		Assert.True(result.IsSuccess);
		Assert.Equal(ResultStatus.Ok, result.Status);
		Assert.Equal(mo.Id, result.Value.Id);
		Assert.Equal(serial, result.Value.SerialNumber);
		Assert.Equal(mo.Name, result.Value.Name);
	}

	#endregion
}