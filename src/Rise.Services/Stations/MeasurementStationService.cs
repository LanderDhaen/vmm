using Microsoft.EntityFrameworkCore;

using Rise.Domain.Common;
using Rise.Domain.Locations;
using Rise.Domain.Projects;
using Rise.Domain.Stations;
using Rise.Persistence;
using Rise.Shared.Common;
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
		MeasurementStation? station = await dbContext.MeasurementStations
			.AsNoTracking()
			.Include(s => s.Location)
			.Include(s => s.Projects)
			.SingleOrDefaultAsync(s => s.Id == stationId, ct);

		if (station is null) {
			return Result.NotFound();
		}

		MeasurementStationDto.Detail dto = new() {
			Id = station.Id,
			Code = station.Code,
			InstalledAt = station.Installation.InstalledAt,
			RemovedAt = station.Installation.RemovedAt,
			Location = station.Location.Name,
			City = station.Location.Address.City,
			ProjectsCount = station.Projects.Count()
		};

		return Result.Success(new MeasurementStationResponse.Detail { Station = dto });
	}

	public async Task<Result<MeasurementStationResponse.History>> GetHistoryAsync(Guid stationId,
		CancellationToken ct = default) {

		MeasurementStation? measurementStation =
			await dbContext.MeasurementStations
				.AsNoTracking()
				.Include(s => s.MeasurementStationHistories)
				.ThenInclude(h => h.Location)
				.Include(s => s.MeasurementStationHistories)
				.ThenInclude(h => h.Installation)
				.Include(s => s.MeasurementStationHistories.OrderByDescending(h => h.ValidityRange.ValidFrom))
				.SingleOrDefaultAsync(s => s.Id == stationId, ct);


		if (measurementStation is null) {
			return Result.NotFound($"Measurement station with Id '{stationId}' was not found");
		}

		var modifications = new List<MeasurementStationHistoryDto.Modification>();
		
		// TODO: 
		
		foreach (var m in measurementStation.GetModifications()) {	
			modifications.Add(new MeasurementStationHistoryDto.Modification {
				MeasurementStationModificationType = (MeasurementStationModificationType)m.MeasurementStationModificationType,
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

	public async Task<Result<List<ProjectDto.Index>>> GetProjectsAsync(
		Guid stationId,
		CancellationToken ct = default) {
		bool exists = await dbContext.MeasurementStations
			.AsNoTracking()
			.AnyAsync(s => s.Id == stationId, ct);

		if (!exists) {
			return Result.NotFound("Meetstation niet gevonden.");
		}

		List<ProjectDto.Index> projects = await dbContext.Projects
			.AsNoTracking()
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

		return Result.Success(projects);
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

	public async Task<Result> UpdateAsync(
		Guid id,
		MeasurementStationRequest.Edit request,
		CancellationToken ct = default) {
		// TODO: Potentially refactoring this to match project editing flow


		MeasurementStation? station = await dbContext.MeasurementStations
			.Include(s => s.Location)
			.Include(s => s.Projects)
			.SingleOrDefaultAsync(s => s.Id == id, ct);

		if (station is null) {
			return Result.NotFound();
		}

		DateTime installedAt = request.InstalledAt ?? station.Installation.InstalledAt;
		station.UpdateInstallation(new Installation(installedAt, request.RemovedAt));

		if (station.Location.Id != request.LocationId) {
			Location? newLocation = await dbContext.Set<Location>()
				.SingleOrDefaultAsync(l => l.Id == request.LocationId, ct);
			if (newLocation is null) {
				return Result.NotFound();
			}

			station.MoveTo(newLocation);
		}

		Project? newProject = await dbContext.Set<Project>()
			.SingleOrDefaultAsync(p => p.Id == request.ProjectId, ct);
		if (newProject is null) {
			return Result.NotFound();
		}

		station.Projects.Clear();
		station.Projects.Add(newProject);

		await dbContext.SaveChangesAsync(ct);
		return Result.Success();
	}
}