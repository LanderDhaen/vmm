using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
using Rise.Shared.Projects;
using Rise.Shared.Stations;

using Xunit;

namespace Rise.Services.Tests.Stations {
	public class MeasurementStationServiceShould {
		private static readonly DateRange DummyDateRange = new(
			new DateOnly(2025, 1, 5),
			new DateOnly(2025, 5, 10)
		);

		private static readonly Zone DummyZone = new("DummyZone", "DummyCode");
		private static readonly MeasurementNetwork DummyMeasurementNetwork = new("DummyMeasurementNetwork", DummyZone);

		private static readonly Project DummyProject = new("dummyproject", "DP-1", DummyDateRange, DummyMeasurementNetwork);

		private static ApplicationDbContext CreateDbContext(out Guid projectId, out Project project, out Location location) {
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

			var context = new ApplicationDbContext(options);

			location = new Location(
				"TestLoc",
				new Address("Line1", "Line2", "City", "1234"),
				new Coordinate(4.35, 50.85));

			context.Locations.Add(location);
			context.SaveChanges();

			var dateRange = new DateRange(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));
			project = new Project("Test Project", "TP2025", dateRange, DummyMeasurementNetwork);

			context.Projects.Add(project);
			context.SaveChanges();

			projectId = project.Id;
			return context;
		}

		[Fact]
		public async Task GetIndexAsync_ShouldReturnPagedResults() {
			var db = CreateDbContext(out var projectId, out var project, out var loc);
			var service = new MeasurementStationService(db);

			for (int i = 0; i < 5; i++) {
				var station = new MeasurementStation(
					$"STN-{i}",
					i % 2 == 0 ? "Active" : "Inactive",
					loc,
					new Installation(DateTime.UtcNow.AddDays(-i)),
					new[] { project });

				db.MeasurementStations.Add(station);
			}

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

			foreach (string code in new[] { "STN-1", "STN-2", "STN-3" }) {
				db.MeasurementStations.Add(new MeasurementStation(
					code,
					"Active",
					loc,
					new Installation(DateTime.UtcNow),
					new[] { project }));
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

		[Fact]
		public async Task GetDetailAsync_ShouldReturnStationDetails_WhenStationExists() {
			var db = CreateDbContext(out var projectId, out var project, out var loc);
			var service = new MeasurementStationService(db);

			DateTime installedAt = new(2024, 11, 6, 0, 0, 0, DateTimeKind.Utc);
			var station = new MeasurementStation(
				"STN-002",
				"Inactive",
				loc,
				new Installation(installedAt),
				new[] { project });

			db.MeasurementStations.Add(station);
			await db.SaveChangesAsync();

			var result = await service.GetDetailAsync(station.Id);

			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Value);

			var dto = result.Value!.Station;
			Assert.Equal(station.Id, dto.Id);
			Assert.Equal("STN-002", dto.Code);
			Assert.Equal("Inactive", dto.Status);
			Assert.Equal(installedAt, dto.InstalledAt);
			Assert.Null(dto.RemovedAt);
			Assert.Equal("TestLoc", dto.Location);
			Assert.Equal("City", dto.City);
			Assert.Equal(1, dto.ProjectsCount);
		}

		[Fact]
		public async Task GetDetailAsync_ShouldReturnNotFound_WhenStationDoesNotExist() {
			var db = CreateDbContext(out var projectId, out var project, out var loc);
			var service = new MeasurementStationService(db);

			var result = await service.GetDetailAsync(Guid.NewGuid());

			Assert.Equal(ResultStatus.NotFound, result.Status);
		}

		[Fact]
		public async Task GetProjectsAsync_ShouldReturnProjects_ForExistingStation() {
			var db = CreateDbContext(out var projectId, out var project, out var loc);
			var service = new MeasurementStationService(db);

			var station = new MeasurementStation(
				"STN-100",
				"Active",
				loc,
				new Installation(DateTime.UtcNow),
				new[] { project });
			db.MeasurementStations.Add(station);

			var p2 = new Project("Second Project", "SP-2",
				new DateRange(new DateOnly(2025, 2, 1)), DummyMeasurementNetwork);
			db.Projects.Add(p2);
			station.Projects.Add(p2);

			await db.SaveChangesAsync();

			var result = await service.GetProjectsAsync(station.Id);

			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Value);
			var items = result.Value!;
			Assert.Equal(2, items.Count);

			Assert.Contains(items, x => x.Id == project.Id && x.Code == project.Code && x.Name == project.Name);
			Assert.Contains(items, x => x.Id == p2.Id && x.Code == p2.Code && x.Name == p2.Name);
		}

		[Fact]
		public async Task GetProjectsAsync_ShouldReturnEmptyList_WhenStationHasNoProjects() {
			var db = CreateDbContext(out var projectId, out var project, out var loc);
			var service = new MeasurementStationService(db);

			var station = new MeasurementStation(
				"STN-EMPTY",
				"Inactive",
				loc,
				new Installation(DateTime.UtcNow),
				Array.Empty<Project>());
			db.MeasurementStations.Add(station);
			await db.SaveChangesAsync();

			var result = await service.GetProjectsAsync(station.Id);

			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Value);
			Assert.Empty(result.Value);
		}

		[Fact]
		public async Task GetProjectsAsync_ShouldReturnNotFound_WhenStationDoesNotExist() {
			var db = CreateDbContext(out var projectId, out var project, out var loc);
			var service = new MeasurementStationService(db);

			var result = await service.GetProjectsAsync(Guid.NewGuid());

			Assert.Equal(ResultStatus.NotFound, result.Status);
		}

		[Fact]
		public async Task EditAsync_ShouldUpdateFields_WhenValid() {
			var db = CreateDbContext(out var projectId, out var project1, out var loc1);
			var loc2 = new Location("Loc-2", new Address("l1", "l2", "City2", "2222"), new Coordinate(4.4, 50.9));
			var project2 = new Project("Second Project", "SP-2",
				new DateRange(new DateOnly(2025, 2, 1), new DateOnly(2025, 12, 31)), DummyMeasurementNetwork);
			db.Locations.Add(loc2);
			db.Projects.Add(project2);
			await db.SaveChangesAsync();

			var originalInstalled = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc);
			var station = new MeasurementStation(
				code: "STN-200",
				status: "Active",
				location: loc1,
				installation: new Installation(originalInstalled),
				projects: new[] { project1 });
			db.MeasurementStations.Add(station);
			await db.SaveChangesAsync();

			var service = new MeasurementStationService(db);

			var newInstalled = new DateTime(2024, 11, 1, 0, 0, 0, DateTimeKind.Utc);
			var newRemoved = new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc);

			var req = new MeasurementStationRequest.Edit {
				StationId = station.Id,
				Code = "STN-200",
				Status = "Inactive",
				LocationId = loc2.Id,
				ProjectId = project2.Id,
				InstalledAt = newInstalled,
				RemovedAt = newRemoved
			};

			var result = await service.EditAsync(req);

			Assert.True(result.IsSuccess);

			var reloaded = await db.MeasurementStations
				.Include(s => s.Location)
				.Include(s => s.Projects)
				.SingleAsync(s => s.Id == station.Id);

			Assert.Equal("Inactive", reloaded.Status);
			Assert.Equal(loc2.Id, reloaded.Location.Id);
			Assert.Single(reloaded.Projects);
			Assert.Equal(project2.Id, reloaded.Projects.First().Id);
			Assert.Equal(newInstalled, reloaded.Installation.InstalledAt);
			Assert.Equal(newRemoved, reloaded.Installation.RemovedAt);
		}

		[Fact]
		public async Task EditAsync_ShouldReturnNotFound_WhenStationDoesNotExist() {
			var db = CreateDbContext(out var _, out var _, out var _);
			var service = new MeasurementStationService(db);

			var req = new MeasurementStationRequest.Edit {
				StationId = Guid.NewGuid(),
				Code = "NOPE",
				Status = "Active",
				LocationId = Guid.NewGuid(),
				ProjectId = Guid.NewGuid(),
				InstalledAt = DateTime.UtcNow
			};

			var result = await service.EditAsync(req);

			Assert.Equal(ResultStatus.NotFound, result.Status);
		}

		[Fact]
		public async Task EditAsync_ShouldReturnNotFound_WhenNewLocationDoesNotExist() {
			var db = CreateDbContext(out var _, out var project, out var loc);
			var station = new MeasurementStation(
				code: "STN-404L",
				status: "Active",
				location: loc,
				installation: new Installation(DateTime.UtcNow.AddDays(-4)),
				projects: new[] { project });
			db.MeasurementStations.Add(station);
			await db.SaveChangesAsync();

			var service = new MeasurementStationService(db);

			var req = new MeasurementStationRequest.Edit {
				StationId = station.Id,
				Code = station.Code,
				Status = "Active",
				LocationId = Guid.NewGuid(),
				ProjectId = project.Id,
				InstalledAt = station.Installation.InstalledAt
			};

			var result = await service.EditAsync(req);

			Assert.Equal(ResultStatus.NotFound, result.Status);
		}

		[Fact]
		public async Task EditAsync_ShouldReturnNotFound_WhenNewProjectDoesNotExist() {
			var db = CreateDbContext(out var _, out var project, out var loc);
			var station = new MeasurementStation(
				code: "STN-404P",
				status: "Active",
				location: loc,
				installation: new Installation(DateTime.UtcNow.AddDays(-4)),
				projects: new[] { project });
			db.MeasurementStations.Add(station);
			await db.SaveChangesAsync();

			var service = new MeasurementStationService(db);

			var req = new MeasurementStationRequest.Edit {
				StationId = station.Id,
				Code = station.Code,
				Status = "Active",
				LocationId = loc.Id,
				ProjectId = Guid.NewGuid(),
				InstalledAt = station.Installation.InstalledAt
			};

			var result = await service.EditAsync(req);

			Assert.Equal(ResultStatus.NotFound, result.Status);
		}

		[Fact]
		public async Task EditAsync_ShouldThrow_WhenRemovedBeforeInstalled() {
			var db = CreateDbContext(out var _, out var project, out var loc);
			var station = new MeasurementStation(
				code: "STN-DATE",
				status: "Active",
				location: loc,
				installation: new Installation(DateTime.UtcNow.AddDays(-10)),
				projects: new[] { project });
			db.MeasurementStations.Add(station);
			await db.SaveChangesAsync();

			var service = new MeasurementStationService(db);

			var installed = new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc);
			var removed = new DateTime(2025, 1, 5, 0, 0, 0, DateTimeKind.Utc);

			var req = new MeasurementStationRequest.Edit {
				StationId = station.Id,
				Code = station.Code,
				Status = "Active",
				LocationId = loc.Id,
				ProjectId = project.Id,
				InstalledAt = installed,
				RemovedAt = removed
			};

			await Assert.ThrowsAsync<ArgumentException>(async () => await service.EditAsync(req));
		}
	}
}