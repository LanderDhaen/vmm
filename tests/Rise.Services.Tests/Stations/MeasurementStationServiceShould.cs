using Ardalis.Result;

using Microsoft.EntityFrameworkCore;

using Rise.Domain.Locations;
using Rise.Domain.Networks;
using Rise.Domain.Projects;
using Rise.Domain.Stations;
using Rise.Domain.Stations.ValueObjects;
using Rise.Persistence;
using Rise.Services.Stations;
using Rise.Shared.Common;
using Rise.Shared.Stations;

namespace Rise.Services.Tests.Stations;

public class MeasurementStationServiceShould {
	private static readonly DateRange DummyDateRange = new(
		new DateOnly(2025, 1, 5),
		new DateOnly(2025, 5, 10)
	);

	private static readonly Zone DummyZone = new("DummyZone", "DummyCode");
	private static readonly MeasurementNetwork DummyMeasurementNetwork = new("DummyMeasurementNetwork", DummyZone);

	private static readonly Project DummyProject = new("dummyproject", "DP-1", DummyDateRange, DummyMeasurementNetwork);

	private static ApplicationDbContext
		CreateDbContext(out Guid projectId, out Project project, out Location location) {
		DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		ApplicationDbContext context = new(options);

		location = new Location(
			"TestLoc",
			new Address("Line1", "Line2", "City", "1234"),
			new Coordinate(4.35, 50.85));

		context.Locations.Add(location);
		context.SaveChanges();

		DateRange dateRange = new(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));
		project = new Project("Test Project", "TP2025", dateRange, DummyMeasurementNetwork);

		context.Projects.Add(project);
		context.SaveChanges();

		projectId = project.Id;
		return context;
	}

	[Fact]
	public async Task GetIndexAsync_ShouldReturnPagedResults() {
		ApplicationDbContext db = CreateDbContext(out Guid projectId, out Project project, out Location loc);
		MeasurementStationService service = new(db);

		for (int i = 0; i < 5; i++) {
			MeasurementStation station = new(
				$"STN-{i}",
				i % 2 == 0 ? "Active" : "Inactive",
				loc,
				new Installation(DateTime.UtcNow.AddDays(-i)),
				new[] { project }
			);

			db.MeasurementStations.Add(station);
		}

		await db.SaveChangesAsync();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 3,
			OrderBy = "Code",
			OrderDescending = false
		};

		Result<MeasurementStationResponse.Index> result = await service.GetIndexAsync(request);

		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);

		MeasurementStationResponse.Index payload = result.Value!;
		Assert.Equal(5, payload.TotalCount);
		Assert.Equal(3, payload.Stations.Count());

		string[] codes = payload.Stations.Select(s => s.Code).ToArray();
		Assert.Equal(new[] { "STN-0", "STN-1", "STN-2" }, codes);

		Assert.All(payload.Stations, s => Assert.Equal("TestLoc", s.Location));
		Assert.All(payload.Stations, s => Assert.Equal("City", s.City));

		Assert.All(payload.Stations, s => Assert.Equal(1, s.ProjectsCount));
	}

	[Fact]
	public async Task GetIndexAsync_ShouldRespectOrderDescending() {
		ApplicationDbContext db = CreateDbContext(out Guid projectId, out Project project, out Location loc);
		MeasurementStationService service = new(db);

		foreach (string code in new[] { "STN-1", "STN-2", "STN-3" }) {
			db.MeasurementStations.Add(new MeasurementStation(
				code,
				"Active",
				loc,
				new Installation(DateTime.UtcNow),
				new[] { project }
			));
		}

		await db.SaveChangesAsync();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 10,
			OrderBy = nameof(MeasurementStationDto.Index.Code),
			OrderDescending = true
		};

		Result<MeasurementStationResponse.Index> result = await service.GetIndexAsync(request);
		Assert.True(result.IsSuccess);

		string[] codes = result.Value!.Stations.Select(s => s.Code).ToArray();
		Assert.Equal(new[] { "STN-3", "STN-2", "STN-1" }, codes);
	}
}