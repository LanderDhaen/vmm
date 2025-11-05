using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

using Rise.Domain.Locations;
using Rise.Domain.Networks;
using Rise.Domain.Projects;
using Rise.Domain.Stations;
using Rise.Domain.Stations.ValueObjects; 
using Rise.Persistence;
using Rise.Services.Stations;
using Rise.Shared.Common;
using Rise.Shared.Stations;

namespace Rise.Services.Tests.Stations
{
    public class MeasurementStationServiceShould
    {
	    
	    private static readonly Location DummyLocation = new(
		    "Aalst",
		    new Address("Koningstraat 12", "Bus 3A", "Brussel", "1000", "BE"),
		    new Coordinate(6.0, 7.0)
	    );

	    private static readonly DateRange DummyDateRange = new(
		    new DateOnly(2025, 1, 5),
		    new DateOnly(2025, 5, 10)
	    );
	    
	    private static readonly Zone DummyZone = new("DummyZone", "DummyCode");
	    private static readonly MeasurementNetwork DummyMeasurementNetwork = new("DummyMeasurementNetwork", DummyZone);

	    private static readonly Project DummyProject = new Project("DummyProject", "DP-1", DummyDateRange, DummyMeasurementNetwork);
        private static ApplicationDbContext CreateDbContext(out Guid projectId, out Project project, out Location location)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Do NOT use InMemoryDatabase... it's not reliable. Use a real database and come up with a strategy to clean up the database between tests.
                .Options;

            var context = new ApplicationDbContext(options);

            location = new Location(
                "TestLoc",
                new Address("Line1", "Line2", "City", "1234"),
                new Coordinate(4.35, 50.85));

            context.Locations.Add(location);
            context.SaveChanges();

            var dateRange = new DateRange(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));
            var measurementNetwork = new MeasurementNetwork("TestNetwork", new Zone("TestZone", "TestCode"));
            
            project = new Project("Test Project", "TP2025", dateRange, measurementNetwork);

            context.Projects.Add(project);
            context.SaveChanges();

            projectId = project.Id;
            return context;
        }

        [Fact]
        public async Task GetIndexAsync_ShouldReturnPagedResults()
        {
            var db = CreateDbContext(out var projectId, out var project, out var loc);
            var service = new MeasurementStationService(db);
            
            for (int i = 0; i < 5; i++)
            {
                var station = new MeasurementStation(
                    code: $"STN-{i}",
                    status: i % 2 == 0 ? "Active" : "Inactive",
                    location: loc,
                    installation: new Installation(DateTime.UtcNow.AddDays(-i)),
					project: DummyProject
                );

                db.MeasurementStations.Add(station);
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
            
            Assert.All(payload.Stations, s => Assert.Equal("DummyProject", s.ProjectName));
            Assert.All(payload.Stations, s => Assert.Equal("DP-1", s.ProjectCode));
        }
    }
}
