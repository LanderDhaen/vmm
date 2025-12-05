using Microsoft.EntityFrameworkCore;

using Rise.Domain.Common;
using Rise.Domain.Locations;
using Rise.Domain.Projects;
using Rise.Domain.Stations;
using Rise.Persistence;
using Rise.Shared.Common;
using Rise.Shared.Locations;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

using MeasurementStationModificationType = Rise.Shared.Stations.MeasurementStationModificationType;
using SharedStatus = Rise.Shared.Projects.Status;

namespace Rise.Services.Stations;

public class MeasurementStationService(ApplicationDbContext dbContext) : IMeasurementStationService {
	public async Task<Result<MeasurementStationResponse.Index>> GetIndexAsync(
		QueryRequest.SkipTake request,
		CancellationToken ctx = default) {
		IQueryable<MeasurementStation> query = dbContext.MeasurementStations.AsQueryable().AsNoTracking();

		// Filtering (MSSQL handles casing)

		query = query.Where(s =>
			EF.Functions.Like(s.Code, $"%{request.SearchTerm}%") ||
			EF.Functions.Like(s.Location.Name, $"%{request.SearchTerm}%") ||
			EF.Functions.Like(s.Location.Address.Addressline1, $"%{request.SearchTerm}%") ||
			EF.Functions.Like(s.Location.Address.Addressline2, $"%{request.SearchTerm}%") ||
			EF.Functions.Like(s.Location.Address.City, $"%{request.SearchTerm}%") ||
			EF.Functions.Like(s.Location.Address.PostalCode, $"%{request.SearchTerm}%")
		);


		int totalCount = await query.CountAsync(ctx);

		// Sorting

		if (!string.IsNullOrWhiteSpace(request.OrderBy)) {
			bool desc = request.OrderDescending;

			query = request.OrderBy switch {
				nameof(Installation.InstalledAt)
					=> desc
						? query.OrderByDescending(s => s.Installation.InstalledAt)
						: query.OrderBy(s => s.Installation.InstalledAt),

				nameof(Installation.RemovedAt)
					=> desc
						? query.OrderByDescending(s => s.Installation.RemovedAt)
						: query.OrderBy(s => s.Installation.RemovedAt),

				nameof(Location)
					=> desc
						? query.OrderByDescending(s => s.Location.Name)
						: query.OrderBy(s => s.Location.Name),

				nameof(Location.Address.City)
					=> desc
						? query.OrderByDescending(s => s.Location.Address.City)
						: query.OrderBy(s => s.Location.Address.City),

				nameof(MeasurementStationDto.Index.ProjectsCount)
					=> desc
						? query.OrderByDescending(s => s.Projects.Count())
						: query.OrderBy(s => s.Projects.Count()),

				_ => desc
					? query.OrderByDescending(s => EF.Property<object>(s, request.OrderBy!))
					: query.OrderBy(s => EF.Property<object>(s, request.OrderBy!))
			};
		} else {
			query = query.OrderBy(s => s.Id);
		}

		// Paginating

		query = query.Skip(request.Skip).Take(request.Take);

		// Selecting

		List<MeasurementStationDto.Index> stations = await query
			.Select(s => new MeasurementStationDto.Index {
				Id = s.Id,
				Code = s.Code,
				InstalledAt = s.Installation.InstalledAt,
				RemovedAt = s.Installation.RemovedAt,
				Location = s.Location.Name,
				City = s.Location.Address.City,
				ProjectsCount = s.Projects.Count()
			})
			.ToListAsync(ctx);

		return Result.Success(new MeasurementStationResponse.Index {
			Stations = stations,
			TotalCount = totalCount
		});
	}

	public async Task<Result<MeasurementStationResponse.Detail>> GetDetailAsync(
		Guid stationId,
		CancellationToken ct = default) {
		IQueryable<MeasurementStation> query = dbContext.MeasurementStations.AsQueryable().AsNoTracking();

		MeasurementStationDto.Detail? station = await query
			.Where(s => s.Id == stationId)
			.Select(s => new MeasurementStationDto.Detail {
				Id = s.Id,
				Code = s.Code,
				InstalledAt = s.Installation.InstalledAt,
				RemovedAt = s.Installation.RemovedAt,
				Location = new LocationDto.Detail {
					Id = s.Location.Id,
					Name = s.Location.Name,
					AddressLine1 = s.Location.Address.Addressline1,
					AddressLine2 = s.Location.Address.Addressline2,
					City = s.Location.Address.City,
					PostalCode = s.Location.Address.PostalCode,
					Country = s.Location.Address.CountryIso,
					X = s.Location.Coordinate.X,
					Y = s.Location.Coordinate.Y,
				},
				ProjectsCount = s.Projects.Count()
			})
			.SingleOrDefaultAsync(ct);

		if (station is null) {
			return Result.NotFound($"Measurement station with Id '{stationId}' was not found");
		}

		return Result.Success(new MeasurementStationResponse.Detail {
			Station = station
		});
	}

	public async Task<Result<MeasurementStationResponse.History>> GetHistoryAsync(Guid stationId,
		CancellationToken ct = default) {
		MeasurementStation? measurementStation =
			await dbContext.MeasurementStations
				.AsNoTracking()
				.Include(s => s.Location)
				.Include(s => s.Installation)
				.Include(s => s.MeasurementStationHistories)
				.ThenInclude(h => h.Location)
				.Include(s => s.MeasurementStationHistories)
				.ThenInclude(h => h.Installation)
				.Include(s => s.MeasurementStationHistories.OrderByDescending(h => h.ValidityRange.ValidFrom))
				.SingleOrDefaultAsync(s => s.Id == stationId, ct);


		if (measurementStation is null) {
			return Result.NotFound($"Meetstation met Id '{stationId}' was niet gevonden.");
		}

		List<MeasurementStationDto.Modification> modifications = [];

		foreach (var m in measurementStation.GetModifications()) {
			modifications.Add(new MeasurementStationDto.Modification {
				MeasurementStationModificationType =
					(MeasurementStationModificationType)m.MeasurementStationModificationType,
				ModifiedAt = m.ModifiedAt,
				PreviousValue = m.PreviousValue,
				CurrentValue = m.CurrentValue,
			});
		}

		return Result.Success(new MeasurementStationResponse.History {
			Modifications = modifications
		});
	}

	public async Task<Result<MeasurementStationResponse.Create>> CreateAsync(MeasurementStationRequest.Create request,
		CancellationToken ct = default) {
		Location location;
		if (request.IsNewLocation) {
			location = new Location(
				request.Location!.Name,
				new Address(
					request.Location.Addressline1,
					request.Location.Addressline2,
					request.Location.City,
					request.Location.PostalCode,
					request.Location.CountryIso
				),
				new Coordinate(request.Location.X, request.Location.Y)
			);
		} else {
			location = await dbContext.Set<Location>()
				.SingleOrDefaultAsync(l => l.Id == request.LocationId, ct);
			if (location is null) {
				return Result.NotFound("Locatie niet gevonden.");
			}
		}

		DateTime installedAt = request.InstalledAt ?? DateTime.UtcNow;

		MeasurementStation station = new(
			request.Code,
			location,
			new Installation(installedAt)
		);

		dbContext.MeasurementStations.Add(station);
		await dbContext.SaveChangesAsync(ct);

		return Result.Success(new MeasurementStationResponse.Create {
			MeasurementStationId = station.Id
		});
	}

	public Task<Result> EditAsync(
		MeasurementStationRequest.Edit request,
		CancellationToken ct = default) {
		return UpdateAsync(request.StationId, request, ct);
	}

	public async Task<Result<MeasurementStationResponse.ProjectList>> GetProjectsAsync(
		Guid stationId,
		QueryRequest.Search request,
		CancellationToken ct = default) {
		bool exists = await dbContext.MeasurementStations
			.AsNoTracking()
			.AnyAsync(s => s.Id == stationId, ct);

		if (!exists) {
			return Result.NotFound("Meetstation niet gevonden.");
		}

		// Typed IQueryable<Project>
		IQueryable<Project> query = dbContext.Projects
			.AsNoTracking()
			.AsQueryable();

		if (!string.IsNullOrWhiteSpace(request.SearchTerm)) {
			query = query.Where(p =>
				EF.Functions.Like(p.Name, $"%{request.SearchTerm}%") ||
				EF.Functions.Like(p.Code, $"%{request.SearchTerm}%") ||
				EF.Functions.Like(p.MeasurementNetwork.Name, $"%{request.SearchTerm}%") ||
				EF.Functions.Like(p.MeasurementNetwork.Zone.Name, $"%{request.SearchTerm}%")
			);
		}

		List<ProjectDto.Index> projects = await query
			.Where(p => p.Stations.Any(s => s.Id == stationId))
			.Select(p => new ProjectDto.Index {
				Id = p.Id,
				Name = p.Name,
				Code = p.Code,
				StartDate = p.DateRange.StartDate,
				EndDate = p.DateRange.EndDate,
				MeasurementNetwork = p.MeasurementNetwork.Name,
				Zone = p.MeasurementNetwork.Zone.Name,
				Status = (SharedStatus)p.Status
			})
			.ToListAsync(ct);

		return Result.Success(new MeasurementStationResponse.ProjectList {
			Projects = projects
		});
	}

	public async Task<Result> UpdateProjectsAsync(MeasurementStationRequest.UpdateProjects req,
		CancellationToken ctx = default) {
		MeasurementStation? measurementStation = await dbContext.MeasurementStations
			.Include(p => p.Projects)
			.SingleOrDefaultAsync(x => x.Id == req.MeasurementStationId, ctx);

		if (measurementStation is null) {
			return Result.NotFound($"Meetstation met id '{req.MeasurementStationId}' is niet gevonden.");
		}

		List<Project> projects = await dbContext.Projects
			.Where(p => req.ProjectIds.Contains(p.Id))
			.ToListAsync(ctx);

		if (projects.Count != req.ProjectIds.Count()) {
			return Result.Error("Een of meerdere projecten zijn niet gevonden.");
		}

		measurementStation.UpdateProjectList(projects);

		await dbContext.SaveChangesAsync(ctx);

		return Result.Success();
	}

	public async Task<Result> UpdateLocationAsync(MeasurementStationRequest.UpdateLocation req,
		CancellationToken ctx = default) {
		MeasurementStation? measurementStation = await dbContext.MeasurementStations
			.Include(p => p.Location)
			.SingleOrDefaultAsync(x => x.Id == req.MeasurementStationId, ctx);

		if (measurementStation is null) {
			return Result.NotFound($"Meetstation met id '{req.MeasurementStationId}' is niet gevonden.");
		}

		Location location;
		if (req.IsNewLocation) {
			location = new Location(
				req.Location!.Name,
				new Address(
					req.Location.Addressline1,
					req.Location.Addressline2,
					req.Location.City,
					req.Location.PostalCode,
					req.Location.CountryIso
				),
				new Coordinate(req.Location.X, req.Location.Y)
			);
		} else {
			location = await dbContext.Locations
				.SingleOrDefaultAsync(l => l.Id == req.LocationId, ctx);
			if (location is null) {
				return Result.NotFound("Locatie niet gevonden.");
			}
		}

		measurementStation.MoveTo(location);

		await dbContext.SaveChangesAsync(ctx);

		return Result.Success();
	}

	private async Task<Result> UpdateAsync(
		Guid id,
		MeasurementStationRequest.Edit request,
		CancellationToken ct = default) {
		MeasurementStation? station = await dbContext.MeasurementStations
			.Include(s => s.Installation)
			.Include(s => s.Location).ThenInclude(l => l.Address)
			.Include(s => s.Location).ThenInclude(l => l.Coordinate)
			.Include(s => s.Projects)
			.SingleOrDefaultAsync(s => s.Id == id, ct);

		if (station is null) {
			return Result.NotFound();
		}

		DateTime installedAt = request.InstalledAt ?? station.Installation.InstalledAt;
		station.UpdateInstallation(new Installation(installedAt, request.RemovedAt));

		await dbContext.SaveChangesAsync(ct);
		return Result.Success();
	}
}