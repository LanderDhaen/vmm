using Ardalis.Result;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Rise.Domain.Common;
using Rise.Domain.Locations;
using Rise.Domain.MaintObjects;
using Rise.Domain.Projects;
using Rise.Domain.Stations;
using Rise.Domain.WorkOrders;
using Rise.Persistence;
using Rise.Services.Tests.Common;
using Rise.Services.WorkOrders;
using Rise.Shared.WorkOrders;

using Testcontainers.MsSql;

using DomainPriority = Rise.Domain.WorkOrders.Priority;
using MaintObjStatus = Rise.Shared.MaintObjects.MaintObjStatus;
using SharedPriority = Rise.Shared.WorkOrders.Priority;
using SharedStatus = Rise.Shared.WorkOrders.WorkOrderStatus;

namespace Rise.Services.Tests.WorkOrders;

public class WorkOrderServiceShould : IClassFixture<MsSqlContainerFixture>, IAsyncLifetime {
	private readonly ApplicationDbContext _dbContext;
	private readonly WorkOrderService _service;
	private IDbContextTransaction? _tx;

	public WorkOrderServiceShould(MsSqlContainerFixture fixture) {
		MsSqlContainer container = fixture.Container!;

		DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseSqlServer(container.GetConnectionString())
			.Options;

		_dbContext = new ApplicationDbContext(options);
		_dbContext.Database.Migrate();

		_service = new WorkOrderService(_dbContext);
	}

	// Begin a transaction per test and roll back afterwards to keep DB clean
	public async Task InitializeAsync() {
		_tx = await _dbContext.Database.BeginTransactionAsync();
	}

	public async Task DisposeAsync() {
		if (_tx is not null) {
			await _tx.RollbackAsync();
			await _tx.DisposeAsync();
		}
	}

	[Fact]
	public async Task GetDetailAsync_ShouldReturnWorkOrderDetails_WhenWorkOrderExists() {
		// Arrange
		Location location = new(
			"TestLoc",
			new Address("Street 1", "Bus 2", "City", "1234"),
			new Coordinate(4.35, 50.85));

		MaintObjPlan plan = new("Plan-1", 30, 60);

		MaintObj maintObj = new(
			"Pump A",
			"SN-001",
			MaintObjType.Sensor,
			plan,
			"RiseBrand",
			location);

		WorkOrder workOrder = new(
			"Fix leak",
			"Replace gasket",
			DomainPriority.High,
			maintObj);

		Technician tech = new("Temp", "Tech", "acc-1");

		_dbContext.Locations.Add(location);
		_dbContext.MaintObjPlans.Add(plan);
		_dbContext.MaintObjects.Add(maintObj);
		_dbContext.WorkOrders.Add(workOrder);
		_dbContext.Technicians.Add(tech);
		await _dbContext.SaveChangesAsync();

		workOrder.AssignTechnician(tech);
		workOrder.MarkAsCompleted();
		await _dbContext.SaveChangesAsync();

		// Act
		Result<WorkOrderResponse.Detail> result = await _service.GetDetailAsync(workOrder.Id);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);

		WorkOrderDto.Detail dto = result.Value!.WorkOrder;

		Assert.Equal(workOrder.Id, dto.Id);
		Assert.Equal("Fix leak", dto.Name);
		Assert.Equal("Replace gasket", dto.Description);
		Assert.Equal(SharedPriority.High, dto.Priority);
		Assert.Equal(SharedStatus.Completed, dto.Status);
		Assert.NotNull(dto.StartDate);
		Assert.NotNull(dto.EndDate);
		Assert.Equal("Temp Tech", dto.Technician);

		Assert.Equal(maintObj.Id, dto.MaintObj.Id);
		Assert.Equal("Pump A", dto.MaintObj.Name);
		Assert.Equal("SN-001", dto.MaintObj.SerialNumber);
		Assert.Equal("RiseBrand", dto.MaintObj.Brand);
		Assert.Equal(Shared.MaintObjects.MaintObjType.Sensor, dto.MaintObj.MaintObjType);
		Assert.Equal(MaintObjStatus.Inactive, dto.MaintObj.MaintObjStatus);

		Assert.Equal("TestLoc", dto.MaintObj.Location.Name);

		Assert.Null(dto.MaintObj.StationCode);

		Assert.Equal(location.Id, dto.Location.Id);
		Assert.Equal("TestLoc", dto.Location.Name);
		Assert.Equal("Street 1", dto.Location.AddressLine1);
		Assert.Equal("Bus 2", dto.Location.AddressLine2);
		Assert.Equal("City", dto.Location.City);
		Assert.Equal("1234", dto.Location.PostalCode);
		Assert.Equal("BE", dto.Location.Country);
		Assert.Equal(4.35, dto.Location.X);
		Assert.Equal(50.85, dto.Location.Y);
	}

	[Fact]
	public async Task GetDetailAsync_ShouldReturnStationCode_WhenMaintObjAssignedToStation() {
		// Arrange
		Location location = new(
			"Loc",
			new Address("L1", null, "City", "1000"),
			new Coordinate(1.0, 2.0));

		MaintObjPlan plan = new("Plan", 10, 20);

		MaintObj maintObj = new(
			"Device",
			"SER-9",
			MaintObjType.Sensor,
			plan,
			null,
			location);

		MeasurementStation station = new(
			"STN-42",
			location,
			new Installation(DateTime.UtcNow.AddDays(-3)));

		maintObj.AssignToStation(station);

		WorkOrder workOrder = new(
			"Test WO",
			"Desc",
			DomainPriority.Normal,
			maintObj);

		_dbContext.Locations.Add(location);
		_dbContext.MaintObjPlans.Add(plan);
		_dbContext.MeasurementStations.Add(station);
		_dbContext.MaintObjects.Add(maintObj);
		_dbContext.WorkOrders.Add(workOrder);
		await _dbContext.SaveChangesAsync();

		// Act
		Result<WorkOrderResponse.Detail> result = await _service.GetDetailAsync(workOrder.Id);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal("STN-42", result.Value!.WorkOrder.MaintObj.StationCode);
	}

	[Fact]
	public async Task GetDetailAsync_ShouldReturnNotFound_WhenWorkOrderDoesNotExist() {
		Result<WorkOrderResponse.Detail> result = await _service.GetDetailAsync(Guid.NewGuid());

		Assert.Equal(ResultStatus.NotFound, result.Status);
	}
}