using Ardalis.Result;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Rise.Domain.Locations;
using Rise.Domain.MaintObjects;
using Rise.Persistence;
using Rise.Services.MaintObjects;
using Rise.Services.Tests.Common;
using Rise.Shared.MaintObjects;

using MaintObjType = Rise.Domain.MaintObjects.MaintObjType;

namespace Rise.Services.Tests.MaintObjects;

public class MaintObjServiceShould {
	[Fact]
	public async Task GetBySerialAsync_ShouldReturnInvalid_WhenSerialIsEmpty() {
		using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);
		MaintObjService service = new(db, null!);

		Result<MaintObjResponse.BySerial> result = await service.GetBySerialAsync("");

		Assert.False(result.IsSuccess);
		Assert.Equal(ResultStatus.Invalid, result.Status);
		Assert.NotEmpty(result.ValidationErrors);
		Assert.Contains(result.ValidationErrors, e => e.ErrorMessage.Contains("Serienummer"));

		connection.Close();
	}

	[Fact]
	public async Task GetBySerialAsync_ShouldReturnNotFound_WhenSerialDoesNotExist() {
		using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);
		MaintObjService service = new(db, null!);

		Result<MaintObjResponse.BySerial> result = await service.GetBySerialAsync("DOES-NOT-EXIST");

		Assert.Equal(ResultStatus.NotFound, result.Status);

		connection.Close();
	}

	[Fact]
	public async Task GetBySerialAsync_ShouldReturnMaintObj_WhenSerialExists() {
		using ApplicationDbContext db = DbContextFactory.CreateContext(out SqliteConnection connection);

		var plan = new MaintObjPlan(
			name: "Test Plan",
			maintenanceIntervalDays: 30,
			calibrationIntervalDays: 60
		);

		Location location = new(
			"TestLoc",
			new Address("Street 1", "Bus 2", "City", "1234"),
			new Coordinate(4.35, 50.85)
		);

		db.MaintObjPlans.Add(plan);
		db.Locations.Add(location);
		await db.SaveChangesAsync();

		var serial = $"ABC-{Guid.NewGuid():N}";
		var mo = new MaintObj(
			name: "Test Device",
			serialNumber: serial,
			MaintObjType.Sensor,
			maintObjPlan: plan,
			brand: "RiseBrand",
			location: location
		);

		db.MaintObjects.Add(mo);
		await db.SaveChangesAsync();

		MaintObjService service = new(db, null!);

		Result<MaintObjResponse.BySerial> result = await service.GetBySerialAsync(serial);

		Assert.True(result.IsSuccess);
		Assert.Equal(ResultStatus.Ok, result.Status);
		Assert.Equal(mo.Id, result.Value.Id);
		Assert.Equal(serial, result.Value.SerialNumber);
		Assert.Equal(mo.Name, result.Value.Name);

		connection.Close();
	}



}