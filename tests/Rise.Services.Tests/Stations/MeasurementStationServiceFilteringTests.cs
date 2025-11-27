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