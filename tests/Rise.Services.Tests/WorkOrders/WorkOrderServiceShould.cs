using Ardalis.Result;

using Microsoft.EntityFrameworkCore;

using Rise.Domain.Common;
using Rise.Domain.Locations;
using Rise.Domain.MaintObjects;
using Rise.Domain.Projects;
using Rise.Domain.Stations;
using Rise.Domain.WorkOrders;
using Rise.Persistence;
using Rise.Services.WorkOrders;
using Rise.Shared.WorkOrders;

using DomainPriority = Rise.Domain.WorkOrders.Priority;
using SharedPriority = Rise.Shared.WorkOrders.Priority;
using SharedStatus = Rise.Shared.WorkOrders.WorkOrderStatus;
using WorkOrderStatus = Rise.Domain.WorkOrders.WorkOrderStatus;

namespace Rise.Services.Tests.WorkOrders;

public class WorkOrderServiceShould {

	private static ApplicationDbContext CreateDbContext() {
		DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		return new ApplicationDbContext(options);
	}

	[Fact]
	public async Task GetDetailAsync_ShouldReturnWorkOrderDetails_WhenWorkOrderExists() {
		ApplicationDbContext db = CreateDbContext();

		Location location = new(
			"TestLoc",
			new Address("Street 1", "Bus 2", "City", "1234"),
			new Coordinate(4.35, 50.85));

		MaintObjPlan plan = new("Plan-1", maintenanceIntervalDays: 30, calibrationIntervalDays: 60);

		MaintObj maintObj = new(
			name: "Pump A",
			serialNumber: "SN-001",
			MaintObjType.Sensor,
			maintObjPlan: plan,
			brand: "RiseBrand",
			location: location);

		WorkOrder workOrder = new(
			name: "Fix leak",
			description: "Replace gasket",
			priority: DomainPriority.High,
			maintObj: maintObj);

		Technician tech = new("Temp", "Tech", "acc-1");

		db.Locations.Add(location);
		db.MaintObjPlans.Add(plan);
		db.MaintObjects.Add(maintObj);
		db.WorkOrders.Add(workOrder);
		db.Technicians.Add(tech);
		await db.SaveChangesAsync();

		workOrder.AssignTechnician(tech);
		workOrder.UpdateStatus(WorkOrderStatus.Completed);
		await db.SaveChangesAsync();

		WorkOrderService service = new(db);

		Result<WorkOrderResponse.Detail> result = await service.GetDetailAsync(workOrder.Id);

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
		Assert.Equal(Shared.MaintObjects.MaintObjStatus.Inactive, dto.MaintObj.MaintObjStatus);

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
		ApplicationDbContext db = CreateDbContext();

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

		db.Locations.Add(location);
		db.MaintObjPlans.Add(plan);
		db.MeasurementStations.Add(station);
		db.MaintObjects.Add(maintObj);
		db.WorkOrders.Add(workOrder);
		await db.SaveChangesAsync();

		WorkOrderService service = new(db);

		Result<WorkOrderResponse.Detail> result = await service.GetDetailAsync(workOrder.Id);

		Assert.True(result.IsSuccess);
		Assert.Equal("STN-42", result.Value!.WorkOrder.MaintObj.StationCode);
	}

	[Fact]
	public async Task GetDetailAsync_ShouldReturnNotFound_WhenWorkOrderDoesNotExist() {
		ApplicationDbContext db = CreateDbContext();
		WorkOrderService service = new(db);

		Result<WorkOrderResponse.Detail> result = await service.GetDetailAsync(Guid.NewGuid());

		Assert.Equal(ResultStatus.NotFound, result.Status);
	}
}