using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

using Rise.Domain.Locations;
using Rise.Domain.Stations;
using Rise.Persistence;
using Rise.Services.Stations;
using Rise.Shared.Common;     
using Rise.Shared.Stations;   

namespace Rise.Services.Tests.Stations;

public class MeasurementStationServiceShould
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        
        var loc = new Location(
            "TestLoc",
            new Address("Line1", "Line2", "City", "1234"),
            new Coordinate(4.35, 50.85));

        context.Locations.Add(loc);
        context.SaveChanges();

        return context;
    }

    [Fact]
    public async Task GetIndexAsync_ShouldReturnPagedResults()
    {
        var db = CreateDbContext();
        var service = new MeasurementStationService(db);

        var locId = db.Locations.First().Id;
        
        for (int i = 0; i < 5; i++)
        {
            db.MeasurementStations.Add(new MeasurementStation
            {
                Code = $"STN-{i}",
                Status = i % 2 == 0 ? "Active" : "Inactive",
                LocationId = locId,
                InstalledAt = DateTime.UtcNow.AddDays(-i)
            });
        }

        await db.SaveChangesAsync();

        var request = new QueryRequest.SkipTake
        {
            Skip = 0,
            Take = 3,
            OrderBy = "Code"
        };
        
        var result = await service.GetIndexAsync(request);
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var payload = result.Value!;
        Assert.Equal(5, payload.TotalCount);
        Assert.Equal(3, payload.Stations.Count());
        
        var codes = payload.Stations.Select(s => s.Code).ToArray();
        Assert.Equal(new[] { "STN-0", "STN-1", "STN-2" }, codes);
    }
}
