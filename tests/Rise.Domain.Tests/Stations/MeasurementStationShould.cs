using System;
using Rise.Domain.Stations;
using Shouldly;
using Xunit;

namespace Rise.Domain.Tests.Stations;

public class MeasurementStationShould
{
	[Fact]
	public void BeCreated()
	{
		var id = Guid.NewGuid();
		
		var station = new MeasurementStation
		{
			Code = "STN-001",
			Status = "Active",
			InstalledAt = DateTime.UtcNow.AddDays(-10),
			RemovedAt = null,
			LocationId = id
		};
		
		station.Code.ShouldBe("STN-001");
		station.Status.ShouldBe("Active");
		station.InstalledAt.ShouldNotBeNull();
		station.RemovedAt.ShouldBeNull();
		station.LocationId.ShouldBe(id);
	}

	[Fact]
	public void AllowUpdatingBasicFields()
	{
		var station = new MeasurementStation
		{
			Code = "STN-001",
			Status = "Active",
			LocationId = Guid.NewGuid()
		};
		
		station.Code = "STN-002";
		station.Status = "Inactive";
		station.RemovedAt = DateTime.UtcNow;
		
		station.Code.ShouldBe("STN-002");
		station.Status.ShouldBe("Inactive");
		station.RemovedAt.ShouldNotBeNull();
	}

	[Fact]
	public void AllowOptionalDates()
	{
		var station = new MeasurementStation
		{
			Code = "STN-100",
			Status = "Active",
			InstalledAt = null, 
			RemovedAt = null,   
			LocationId = Guid.NewGuid()
		};

		station.InstalledAt.ShouldBeNull();
		station.RemovedAt.ShouldBeNull();
	}
}