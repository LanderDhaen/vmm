using System;
using System.Linq;
using System.Threading.Tasks;

using Ardalis.Result;

using Microsoft.EntityFrameworkCore;

using Rise.Domain.Locations;
using Rise.Domain.Projects;
using Rise.Domain.Stations;
using Rise.Domain.Stations.ValueObjects;
using Rise.Persistence;
using Rise.Services.Stations;
using Rise.Shared.Common;
using Rise.Shared.Stations;

using Xunit;
namespace Rise.Services.Tests.Stations;

namespace Rise.Services.Tests.Stations {
	public class MeasurementStationServiceShould {
		private static readonly Location DummyLocation = new(
			"Aalst",
			new Address("Koningstraat 12", "Bus 3A", "Brussel", "1000", "BE"),
			new Coordinate(6.0, 7.0)
		);
public class MeasurementStationServiceShould {
	private static readonly Location DummyLocation = new(
		"Aalst",
		new Address("Koningstraat 12", "Bus 3A", "Brussel", "1000"),
		new Coordinate(6.0, 7.0)
	);

		private static readonly DateRange DummyDateRange = new(
			new DateOnly(2025, 1, 5),
			new DateOnly(2025, 5, 10)
		);
	private static readonly DateRange DummyDateRange = new(
		new DateOnly(2025, 1, 5),
		new DateOnly(2025, 5, 10)
	);

		private static readonly Project DummyProject = new Project("dummyproject", "DP-1", DummyDateRange, DummyLocation);
	private static readonly Zone DummyZone = new("DummyZone", "DummyCode");
	private static readonly MeasurementNetwork DummyMeasurementNetwork = new("DummyMeasurementNetwork", DummyZone);

		private static ApplicationDbContext CreateDbContext(out Guid projectId, out Project project, out Location location) {
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;
	private static readonly Project DummyProject = new("DummyProject", "DP-1", DummyDateRange, DummyMeasurementNetwork);

			var context = new ApplicationDbContext(options);
	private static ApplicationDbContext
		CreateDbContext(out Guid projectId, out Project project, out Location location) {
		DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid()
				.ToString()) // Do NOT use InMemoryDatabase... it's not reliable. Use a real database and come up with a strategy to clean up the database between tests.
			.Options;

			location = new Location(
				"TestLoc",
				new Address("Line1", "Line2", "City", "1234"),
				new Coordinate(4.35, 50.85));
		ApplicationDbContext context = new(options);

			context.Locations.Add(location);
			context.SaveChanges();
		location = new Location(
			"TestLoc",
			new Address("Line1", "Line2", "City", "1234"),
			new Coordinate(4.35, 50.85));

			var dateRange = new DateRange(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));
			project = new Project("Test Project", "TP2025", dateRange, location);
		context.Locations.Add(location);
		context.SaveChanges();

			context.Projects.Add(project);
			context.SaveChanges();
		DateRange dateRange = new(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));
		MeasurementNetwork measurementNetwork = new("TestNetwork", new Zone("TestZone", "TestCode"));

			projectId = project.Id;
			return context;
		}
		project = new Project("Test Project", "TP2025", dateRange, measurementNetwork);

		[Fact]
		public async Task GetIndexAsync_ShouldReturnPagedResults() {
			var db = CreateDbContext(out var projectId, out var project, out var loc);
			var service = new MeasurementStationService(db);
		context.Projects.Add(project);
		context.SaveChanges();

			for (int i = 0; i < 5; i++) {
				var station = new MeasurementStation(
					code: $"STN-{i}",
					status: i % 2 == 0 ? "Active" : "Inactive",
					location: loc,
					installation: new Installation(DateTime.UtcNow.AddDays(-i)),
					projects: new[] { project }
				);
		projectId = project.Id;
		return context;
	}

				db.MeasurementStations.Add(station);
			}
	[Fact]
	public async Task GetIndexAsync_ShouldReturnPagedResults() {
		ApplicationDbContext db = CreateDbContext(out Guid projectId, out Project project, out Location loc);
		MeasurementStationService service = new(db);

			await db.SaveChangesAsync();

			var request = new QueryRequest.SkipTake {
				Skip = 0,
				Take = 3,
				OrderBy = "Code",
				OrderDescending = false
			};

			var result = await service.GetIndexAsync(request);

			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Value);

			var payload = result.Value!;
			Assert.Equal(5, payload.TotalCount);
			Assert.Equal(3, payload.Stations.Count());

			var codes = payload.Stations.Select(s => s.Code).ToArray();
			Assert.Equal(new[] { "STN-0", "STN-1", "STN-2" }, codes);

			Assert.All(payload.Stations, s => Assert.Equal("TestLoc", s.Location));
			Assert.All(payload.Stations, s => Assert.Equal("City", s.City));

			Assert.All(payload.Stations, s => Assert.Equal(1, s.ProjectsCount));
		}

		[Fact]
		public async Task GetIndexAsync_ShouldRespectOrderDescending() {
			var db = CreateDbContext(out var projectId, out var project, out var loc);
			var service = new MeasurementStationService(db);

			foreach (var code in new[] { "STN-1", "STN-2", "STN-3" }) {
				db.MeasurementStations.Add(new MeasurementStation(
					code: code,
					status: "Active",
					location: loc,
					installation: new Installation(DateTime.UtcNow),
					projects: new[] { project }
				));
			}

			await db.SaveChangesAsync();

			var request = new QueryRequest.SkipTake {
				Skip = 0,
				Take = 10,
				OrderBy = nameof(MeasurementStationDto.Index.Code),
				OrderDescending = true
			};

			var result = await service.GetIndexAsync(request);
			Assert.True(result.IsSuccess);

			var codes = result.Value!.Stations.Select(s => s.Code).ToArray();
			Assert.Equal(new[] { "STN-3", "STN-2", "STN-1" }, codes);
		}
	}
}
		for (int i = 0; i < 5; i++) {
			MeasurementStation station = new(
				$"STN-{i}",
				i % 2 == 0 ? "Active" : "Inactive",
				loc,
				new Installation(DateTime.UtcNow.AddDays(-i)),
				DummyProject
			);

			db.MeasurementStations.Add(station);
		}

		await db.SaveChangesAsync();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 3,
			OrderBy = "Code"
		};

		Result<MeasurementStationResponse.Index> result = await service.GetIndexAsync(request);

		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);

		MeasurementStationResponse.Index payload = result.Value!;
		Assert.Equal(5, payload.TotalCount);
		Assert.Equal(3, payload.Stations.Count());

		string[] codes = payload.Stations.Select(s => s.Code).ToArray();
		Assert.Equal(new[] { "STN-0", "STN-1", "STN-2" }, codes);

		Assert.All(payload.Stations, s => Assert.Equal("DummyProject", s.ProjectName));
		Assert.All(payload.Stations, s => Assert.Equal("DP-1", s.ProjectCode));
	}
}