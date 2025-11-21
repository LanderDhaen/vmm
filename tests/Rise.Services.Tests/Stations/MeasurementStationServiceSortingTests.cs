using System.Data.Common;
using System.Runtime.CompilerServices;

using Ardalis.Result;

using Microsoft.Data.Sqlite;

using Rise.Domain.Locations;
using Rise.Domain.Networks;
using Rise.Domain.Projects;
using Rise.Domain.Stations;
using Rise.Persistence;
using Rise.Services.Stations;
using Rise.Services.Tests.Common;
using Rise.Shared.Common;
using Rise.Shared.Stations;

namespace Rise.Services.Tests.Stations;

public class MeasurementStationServiceSortingTests : IDisposable {
	private DbConnection? _connection;

	public void Dispose() {
		// Ensure we clean up any open connection if a test forgot to close it.
		try {
			_connection?.Close();
			_connection?.Dispose();
		} catch {
			// best-effort cleanup
		}
	}

	/// <summary>
	///     Create a new ApplicationDbContext via the shared test factory and seed it with dummy stations.
	///     Returns the created context and outputs the underlying SqliteConnection which the caller should close.
	/// </summary>
	private ApplicationDbContext CreateContextAndSeed(out SqliteConnection connection) {
		ApplicationDbContext db = DbContextFactory.CreateContext(out connection);
		DummyStationsFactory.SeedDummyStations(db);

		// Track the connection for teardown in Dispose if the test forgets to close it.
		_connection = connection;

		return db;
	}

	private static string GetSortableString(MeasurementStationDto.Index dto, string orderBy) {
		return orderBy switch {
			"Id" => dto.Id.ToString(),
			"Code" => dto.Code ?? string.Empty,
			"InstalledAt" => dto.InstalledAt != default ? dto.InstalledAt.ToString("yyyy-MM-dd") : string.Empty,
			"RemovedAt" => dto.RemovedAt.HasValue ? dto.RemovedAt.Value.ToString("yyyy-MM-dd") : string.Empty,
			"Location" => dto.Location ?? string.Empty,
			"City" => dto.City ?? string.Empty,
			"ProjectsCount" => dto.ProjectsCount.ToString()
				.PadLeft(6, '0'), // pad to preserve lexicographic order for numbers
			_ => dto.Id.ToString()
		};
	}

	private static bool IsOrdered(IReadOnlyList<string> keys, bool desc) {
		for (int i = 0; i + 1 < keys.Count; i++) {
			int cmp = string.Compare(keys[i], keys[i + 1], StringComparison.Ordinal);
			if (desc) {
				if (cmp < 0) {
					return false; // earlier < later -> not descending
				}
			} else {
				if (cmp > 0) {
					return false; // earlier > later -> not ascending
				}
			}
		}

		return true;
	}

	[Theory]
	// OrderBy values (asc)
	[InlineData("Id", false)]
	[InlineData("Code", false)]
	[InlineData("InstalledAt", false)]
	[InlineData("RemovedAt", false)]
	[InlineData("Location", false)]
	[InlineData("City", false)]
	[InlineData("ProjectsCount", false)]
	// OrderBy values (desc)
	[InlineData("Id", true)]
	[InlineData("Code", true)]
	[InlineData("InstalledAt", true)]
	[InlineData("RemovedAt", true)]
	[InlineData("Location", true)]
	[InlineData("City", true)]
	[InlineData("ProjectsCount", true)]
	public async Task GetIndexAsync_Returns_Results_In_Requested_Order(string orderBy, bool desc) {
		// Arrange
		await using ApplicationDbContext db = CreateContextAndSeed(out SqliteConnection connection);

		MeasurementStationService svc = new(db);

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 100,
			OrderBy = orderBy,
			OrderDescending = desc
		};

		// Act
		Result<MeasurementStationResponse.Index> result = await svc.GetIndexAsync(request);
		Assert.True(result.IsSuccess, "GetIndexAsync returned a non-success result.");

		IEnumerable<MeasurementStationDto.Index> dtoList = result.Value.Stations;

		// Build key list for DTOs using same ordering dimension
		List<string> keys = dtoList.Select(d => GetSortableString(d, orderBy)).ToList();

		// Assert that the result is ordered according to the requested key (asc/desc).
		Assert.True(IsOrdered(keys, desc), $"Results are not ordered by {orderBy} {(desc ? "DESC" : "ASC")}.");

		// Cleanup connection (mirrors other tests)
		connection.Close();
	}

	[Fact]
	public async Task GetIndexAsync_Paging_Is_Applied_After_Ordering() {
		// Arrange
		await using ApplicationDbContext db = CreateContextAndSeed(out SqliteConnection connection);

		MeasurementStationService svc = new(db);

		// Request page with skip/take
		QueryRequest.SkipTake requestFull = new()
			{ Skip = 0, Take = 100, OrderBy = "Location", OrderDescending = false };
		QueryRequest.SkipTake requestPage = new() { Skip = 2, Take = 3, OrderBy = "Location", OrderDescending = false };

		// Act
		IEnumerable<MeasurementStationDto.Index> full = (await svc.GetIndexAsync(requestFull)).Value.Stations;
		IEnumerable<MeasurementStationDto.Index> page = (await svc.GetIndexAsync(requestPage)).Value.Stations;

		// Determine expected page from the full ordered list
		List<Guid> expectedPage = full.Skip(2).Take(3).Select(p => p.Id).ToList();

		// Assert that the returned page corresponds to the ordered full list slice
		Assert.Equal(expectedPage, page.Select(p => p.Id).ToList());

		connection.Close();
	}

	// The factory is embedded in the test class so all test-data and seeding logic is local.
	private static class DummyStationsFactory {
		public static IReadOnlyList<MeasurementStation> CreateDummyStations() {
			// Create a few locations with distinct names/cities.
			Location locA = new("Loc-A", new Address("L1", "L2", "CityA", "1000"), new Coordinate(4.35, 50.85));
			Location locB = new("Loc-B", new Address("L1", "L2", "CityB", "2000"), new Coordinate(5.35, 51.85));
			Location locC = new("Loc-C", new Address("L1", "L2", "CityC", "3000"), new Coordinate(6.35, 52.85));

			// Create shared zones / networks so we don't violate UNIQUE constraints on Zone.Code.
			Zone zone1 = new("Zone-1", "Z1");
			Zone zone2 = new("Zone-2", "Z2");

			MeasurementNetwork net1 = new("Net-1", zone1);
			MeasurementNetwork net2 = new("Net-2", zone2);

			// Create some projects to attach to stations; reuse net1 for proj1 and proj3 to avoid duplicate Zone entries.
			Project proj1 = new("P1", "P-1", new DateRange(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31)), net1);
			Project proj2 = new("P2", "P-2", new DateRange(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31)), net2);
			Project proj3 = new("P3", "P-3", new DateRange(new DateOnly(2023, 6, 1), new DateOnly(2024, 6, 1)),
				net1); // reuse net1 / zone1

			// Create stations with varying properties to exercise ordering.
			MeasurementStation s1 = new("ST-001", locA, new Installation(new DateTime(2024, 01, 10)));
			MeasurementStation s2 = new("ST-002", locB,
				new Installation(new DateTime(2023, 05, 20), new DateTime(2024, 05, 20)));
			MeasurementStation s3 = new("ST-010", locC, new Installation(new DateTime(2025, 03, 15)));
			MeasurementStation s4 = new("A-100", locB, new Installation(new DateTime(2022, 07, 01)));
			MeasurementStation s5 = new("ZZ-999", locA,
				new Installation(new DateTime(2021, 12, 31), new DateTime(2022, 12, 31)));

			// add a station with same location name but different city to test city ordering
			Location locA2 = new("Loc-A", new Address("L1", "L2", "CityAA", "1100"), new Coordinate(4.36, 50.86));
			MeasurementStation s6 = new("M-007", locA2, new Installation(new DateTime(2024, 06, 01)));

			return new List<MeasurementStation> { s1, s2, s3, s4, s5, s6 };
		}

		public static void SeedDummyStations(ApplicationDbContext context) {
			ArgumentNullException.ThrowIfNull(context);

			// Create stations with shared locations/projects and insert them.
			IReadOnlyList<MeasurementStation> stations = CreateDummyStations();

			// Extract distinct locations and projects from stations.
			List<Location> locations = stations.Select(s => s.Location)
				.Distinct(new ReferenceEqualityComparer<Location>()).ToList();
			List<Project> projects = stations.SelectMany(s => s.Projects)
				.Distinct(new ReferenceEqualityComparer<Project>()).ToList();

			// Add locations and projects first to avoid duplicate zone/network inserts and unique constraint violations.
			context.Locations.AddRange(locations);
			context.Projects.AddRange(projects);

			// Add stations (they reference existing tracked locations/projects)
			context.MeasurementStations.AddRange(stations);

			context.SaveChanges();
		}

		// Helper comparer to distinct by reference (we intentionally created shared refs in factory)
		private class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class {
			public bool Equals(T? x, T? y) {
				return ReferenceEquals(x, y);
			}

			public int GetHashCode(T obj) {
				return RuntimeHelpers.GetHashCode(obj);
			}
		}
	}
}