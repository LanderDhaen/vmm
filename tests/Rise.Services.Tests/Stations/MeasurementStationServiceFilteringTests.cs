using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Text.Json;

using Ardalis.Result;

using Microsoft.Data.Sqlite;

using Rise.Domain.Common;
using Rise.Domain.Locations;
using Rise.Domain.Projects;
using Rise.Domain.Stations;
using Rise.Persistence;
using Rise.Services.Stations;
using Rise.Services.Tests.Common;
using Rise.Shared.Common;
using Rise.Shared.Stations;

namespace Rise.Services.Tests.Stations;

public class MeasurementStationServiceFilteringTests : IDisposable {
	private DbConnection? _connection;

	public void Dispose() {
		try {
			_connection?.Close();
			_connection?.Dispose();
		} catch {
		}
	}
	private ApplicationDbContext CreateContextAndSeed(out SqliteConnection connection) {
		ApplicationDbContext db = DbContextFactory.CreateContext(out connection);
		DummyStationsFactory.SeedDummyStations(db);

		_connection = connection;

		return db;
	}

	[Fact]
	public async Task InstalledAt_Equals_Filter_Returns_Only_Expected() {
		await using ApplicationDbContext db = CreateContextAndSeed(out SqliteConnection connection);
		MeasurementStationService svc = new(db);
		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 100,
			Filters = new Dictionary<string, object?> {
				["InstalledAt"] = JsonDocument.Parse("{\"Operator\":\"Equals\",\"Value\":\"2024-01-10\"}").RootElement
			}
		};

		Result<MeasurementStationResponse.Index> result = await svc.GetIndexAsync(request);
		Assert.True(result.IsSuccess);

		var stations = result.Value.Stations.ToList();
		Assert.Single(stations);
		Assert.Equal(new DateTime(2024, 01, 10), stations.Single().InstalledAt);

		connection.Close();
	}

	[Fact]
	public async Task InstalledAt_GreaterThanOrEqual_Filter_Returns_Expected_Set() {
		await using ApplicationDbContext db = CreateContextAndSeed(out SqliteConnection connection);
		MeasurementStationService svc = new(db);

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 100,
			Filters = new Dictionary<string, object?> {
				["InstalledAt"] = JsonDocument.Parse("{\"Operator\":\"GreaterThanOrEqual\",\"Value\":\"2024-01-10\"}").RootElement
			}
		};

		Result<MeasurementStationResponse.Index> result = await svc.GetIndexAsync(request);
		Assert.True(result.IsSuccess);

		var stations = result.Value.Stations.ToList();

		Assert.Equal(3, stations.Count);
		Assert.Contains(stations, s => s.InstalledAt == new DateTime(2024, 01, 10));
		Assert.Contains(stations, s => s.InstalledAt == new DateTime(2025, 03, 15));
		Assert.Contains(stations, s => s.InstalledAt == new DateTime(2024, 06, 01));

		connection.Close();
	}

	[Fact]
	public async Task InstalledAt_LessThanOrEqual_Filter_Returns_Expected_Set() {

		await using ApplicationDbContext db = CreateContextAndSeed(out SqliteConnection connection);
		MeasurementStationService svc = new(db);

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 100,
			Filters = new Dictionary<string, object?> {
				["InstalledAt"] = JsonDocument.Parse("{\"Operator\":\"LessThanOrEqual\",\"Value\":\"2023-12-31\"}").RootElement
			}
		};

		Result<MeasurementStationResponse.Index> result = await svc.GetIndexAsync(request);
		Assert.True(result.IsSuccess);

		var stations = result.Value.Stations.ToList();

		Assert.Equal(3, stations.Count);
		Assert.Contains(stations, s => s.InstalledAt == new DateTime(2023, 05, 20));
		Assert.Contains(stations, s => s.InstalledAt == new DateTime(2022, 07, 01));
		Assert.Contains(stations, s => s.InstalledAt == new DateTime(2021, 12, 31));

		connection.Close();
	}
    [Fact]
  public async Task RemovedAt_IsEmpty_Filter_Returns_Stations_Without_Removal() {

    await using ApplicationDbContext db = CreateContextAndSeed(out SqliteConnection connection);
    MeasurementStationService svc = new(db);

    QueryRequest.SkipTake request = new() {
      Skip = 0,
      Take = 100,
      Filters = new Dictionary<string, object?> {
        ["RemovedAt"] = JsonDocument.Parse("{\"Operator\":\"IsEmpty\"}").RootElement
      }
    };

    Result<MeasurementStationResponse.Index> result = await svc.GetIndexAsync(request);
    Assert.True(result.IsSuccess);

    var stations = result.Value.Stations.ToList();

    Assert.Equal(4, stations.Count);
    Assert.All(stations, s => Assert.Null(s.RemovedAt));

    connection.Close();
  }

[Fact]
public async Task RemovedAt_GreaterThanOrEqual_Filter_Returns_Expected() {
  await using ApplicationDbContext db = CreateContextAndSeed(out SqliteConnection connection);
  MeasurementStationService svc = new(db);

  QueryRequest.SkipTake request = new() {
    Skip = 0,
    Take = 100,
    Filters = new Dictionary<string, object?> {
      ["RemovedAt"] = JsonDocument.Parse("{\"Operator\":\"GreaterThanOrEqual\",\"Value\":\"2024-01-01\"}").RootElement
    }
  };

  Result<MeasurementStationResponse.Index> result = await svc.GetIndexAsync(request);
  Assert.True(result.IsSuccess);

  var stations = result.Value.Stations.ToList();

  Assert.Single(stations);

  DateTime? removedAt = stations.Single().RemovedAt;
  Assert.True(removedAt.HasValue, "Expected RemovedAt to have a value for the single returned station.");
  Assert.Equal(new DateTime(2024, 05, 20), removedAt.Value.Date);

  connection.Close();
}
  [Fact]
  public async Task Location_Contains_Filter_Matches_Partial_Names() {
    await using ApplicationDbContext db = CreateContextAndSeed(out SqliteConnection connection);
    MeasurementStationService svc = new(db);

    QueryRequest.SkipTake request = new() {
      Skip = 0,
      Take = 100,
      Filters = new Dictionary<string, object?> {
        ["Location"] = "Loc-A"
      }
    };

    Result<MeasurementStationResponse.Index> result = await svc.GetIndexAsync(request);
    Assert.True(result.IsSuccess);

    var stations = result.Value.Stations.ToList();

    Assert.Equal(3, stations.Count);
    Assert.All(stations, s => Assert.Contains("loc-a", s.Location!.ToLower()));

    connection.Close();
  }

  [Fact]
  public async Task Location_Equals_Filter_Is_Case_Insensitive() {
    await using ApplicationDbContext db = CreateContextAndSeed(out SqliteConnection connection);
    MeasurementStationService svc = new(db);

    QueryRequest.SkipTake request = new() {
      Skip = 0,
      Take = 100,
      Filters = new Dictionary<string, object?> {
        ["Location"] = JsonDocument.Parse("{\"Operator\":\"Equals\",\"Value\":\"loc-a\"}").RootElement
      }
    };

    Result<MeasurementStationResponse.Index> result = await svc.GetIndexAsync(request);
    Assert.True(result.IsSuccess);

    var stations = result.Value.Stations.ToList();

    Assert.Equal(3, stations.Count);
    Assert.All(stations, s => Assert.Equal("loc-a", s.Location!.ToLower()));

    connection.Close();
  }
  [Fact]
  public async Task City_Filters_Contain_And_Equals_Behave_As_Expected() {
    await using ApplicationDbContext db = CreateContextAndSeed(out SqliteConnection connection);
    MeasurementStationService svc = new(db);

    QueryRequest.SkipTake containsRequest = new() {
      Skip = 0,
      Take = 100,
      Filters = new Dictionary<string, object?> {
        ["City"] = "CityA"
      }
    };

    Result<MeasurementStationResponse.Index> containsResult = await svc.GetIndexAsync(containsRequest);
    Assert.True(containsResult.IsSuccess);
    var containsStations = containsResult.Value.Stations.ToList();
    Assert.Equal(3, containsStations.Count);
    Assert.Contains(containsStations, s => s.City == "CityA");
    Assert.Contains(containsStations, s => s.City == "CityAA");

    QueryRequest.SkipTake equalsRequest = new() {
      Skip = 0,
      Take = 100,
      Filters = new Dictionary<string, object?> {
        ["City"] = JsonDocument.Parse("{\"Operator\":\"Equals\",\"Value\":\"CityAA\"}").RootElement
      }
    };

    Result<MeasurementStationResponse.Index> equalsResult = await svc.GetIndexAsync(equalsRequest);
    Assert.True(equalsResult.IsSuccess);
    var equalsStations = equalsResult.Value.Stations.ToList();
    Assert.Single(equalsStations);
    Assert.Equal("CityAA", equalsStations.Single().City);

    connection.Close();
  }

  [Fact]
  public async Task ProjectsCount_Filters_Work_For_Zero_And_NonZero() {
    await using ApplicationDbContext db = CreateContextAndSeed(out SqliteConnection connection);
    MeasurementStationService svc = new(db);

    QueryRequest.SkipTake eqZero = new() {
      Skip = 0,
      Take = 100,
      Filters = new Dictionary<string, object?> {
        ["ProjectsCount"] = JsonDocument.Parse("{\"Operator\":\"Equals\",\"Value\":\"0\"}").RootElement
      }
    };

    var eqRes = await svc.GetIndexAsync(eqZero);
    Assert.True(eqRes.IsSuccess);
    Assert.Equal(6, eqRes.Value.Stations.Count());

    QueryRequest.SkipTake gtZero = new() {
      Skip = 0,
      Take = 100,
      Filters = new Dictionary<string, object?> {
        ["ProjectsCount"] = JsonDocument.Parse("{\"Operator\":\"GreaterThan\",\"Value\":\"0\"}").RootElement
      }
    };

    var gtRes = await svc.GetIndexAsync(gtZero);
    Assert.True(gtRes.IsSuccess);
    Assert.Empty(gtRes.Value.Stations);

    connection.Close();
  }



	private static class DummyStationsFactory {
		private static IReadOnlyList<MeasurementStation> CreateDummyStations() {
			Location locA = new("Loc-A", new Address("L1", "L2", "CityA", "1000"), new Coordinate(4.35, 50.85));
			Location locB = new("Loc-B", new Address("L1", "L2", "CityB", "2000"), new Coordinate(5.35, 51.85));
			Location locC = new("Loc-C", new Address("L1", "L2", "CityC", "3000"), new Coordinate(6.35, 52.85));

			MeasurementStation s1 = new("ST-001", locA, new Installation(new DateTime(2024, 01, 10)));
			MeasurementStation s2 = new("ST-002", locB,
				new Installation(new DateTime(2023, 05, 20), new DateTime(2024, 05, 20)));
			MeasurementStation s3 = new("ST-010", locC, new Installation(new DateTime(2025, 03, 15)));
			MeasurementStation s4 = new("A-100", locB, new Installation(new DateTime(2022, 07, 01)));
			MeasurementStation s5 = new("ZZ-999", locA,
				new Installation(new DateTime(2021, 12, 31), new DateTime(2022, 12, 31)));

			Location locA2 = new("Loc-A", new Address("L1", "L2", "CityAA", "1100"), new Coordinate(4.36, 50.86));
			MeasurementStation s6 = new("M-007", locA2, new Installation(new DateTime(2024, 06, 01)));

			return new List<MeasurementStation> { s1, s2, s3, s4, s5, s6 };
		}

		public static void SeedDummyStations(ApplicationDbContext context) {
			ArgumentNullException.ThrowIfNull(context);

			IReadOnlyList<MeasurementStation> stations = CreateDummyStations();

			List<Location> locations = stations.Select(s => s.Location)
				.Distinct(new ReferenceEqualityComparer<Location>()).ToList();
			List<Project> projects = stations.SelectMany(s => s.Projects)
				.Distinct(new ReferenceEqualityComparer<Project>()).ToList();

			context.Locations.AddRange(locations);
			context.Projects.AddRange(projects);

			context.MeasurementStations.AddRange(stations);

			context.SaveChanges();
		}

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