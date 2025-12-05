using Ardalis.Result;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Rise.Domain.MaintObjects;
using Rise.Persistence;
using Rise.Services.MaintObjects;
using Rise.Services.Tests.Common;
using Rise.Shared.MaintObjects;

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

		var type = await db.MaintObjTypes.FirstAsync();

		var serial = $"ABC-{Guid.NewGuid():N}";
		var mo = new MaintObj(
			name: "Test Device",
			serialNumber: serial,
			maintObjPlan: type,
			brand: "RiseBrand",
			location: null
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