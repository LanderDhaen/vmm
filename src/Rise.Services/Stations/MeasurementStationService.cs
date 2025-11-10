using Microsoft.EntityFrameworkCore;

using Rise.Domain.Locations;
using Rise.Domain.Projects;
using Rise.Domain.Stations;
using Rise.Domain.Stations.ValueObjects;
using Rise.Persistence;
using Rise.Shared.Common;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

using SharedStatus = Rise.Shared.Projects.Status;

namespace Rise.Services.Stations;

public class MeasurementStationService(ApplicationDbContext dbContext) : IMeasurementStationService {
	public async Task<Result<MeasurementStationResponse.Index>> GetIndexAsync(
		QueryRequest.SkipTake request,
		CancellationToken ctx = default) {
		IQueryable<MeasurementStation> query = dbContext.MeasurementStations;

		int totalCount = await query.CountAsync(ctx);

		if (!string.IsNullOrWhiteSpace(request.OrderBy)) {
			bool desc = request.OrderDescending;

			query = request.OrderBy switch {
				nameof(MeasurementStationDto.Index.InstalledAt)
					=> desc
						? query.OrderByDescending(s => s.Installation.InstalledAt)
						: query.OrderBy(s => s.Installation.InstalledAt),

				nameof(MeasurementStationDto.Index.RemovedAt)
					=> desc
						? query.OrderByDescending(s => s.Installation.RemovedAt)
						: query.OrderBy(s => s.Installation.RemovedAt),

				nameof(MeasurementStationDto.Index.Location)
					=> desc
						? query.OrderByDescending(s => s.Location.Name)
						: query.OrderBy(s => s.Location.Name),

				nameof(MeasurementStationDto.Index.City)
					=> desc
						? query.OrderByDescending(s => s.Location.Address.City)
						: query.OrderBy(s => s.Location.Address.City),

				nameof(MeasurementStationDto.Index.ProjectsCount)
					=> desc
						? query.OrderByDescending(s => s.Projects.Count())
						: query.OrderBy(s => s.Projects.Count()),

				nameof(MeasurementStationDto.Index.Code)
					=> desc
						? query.OrderByDescending(s => s.Code)
						: query.OrderBy(s => s.Code),

				nameof(MeasurementStationDto.Index.Status)
					=> desc
						? query.OrderByDescending(s => s.Status)
						: query.OrderBy(s => s.Status),

				_ => desc
					? query.OrderByDescending(s => EF.Property<object>(s, request.OrderBy!))
					: query.OrderBy(s => EF.Property<object>(s, request.OrderBy!))
			};
		} else {
			query = query.OrderBy(s => s.Code);
		}

		List<MeasurementStationDto.Index> stations = await query
			.AsNoTracking()
			.Skip(request.Skip)
			.Take(request.Take)
			.Select(s => new MeasurementStationDto.Index {
				Id = s.Id,
				Code = s.Code,
				Status = s.Status,
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

	public async Task<Result<MeasurementStationResponse.Detail>> GetDetailAsync(Guid stationId,
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
			Status = station.Status,
			InstalledAt = station.Installation.InstalledAt,
			RemovedAt = station.Installation.RemovedAt,
			Location = station.Location.Name,
			City = station.Location.Address.City,
			ProjectsCount = station.Projects.Count()
		};

		return Result.Success(new MeasurementStationResponse.Detail { Station = dto });
	}

	public async Task<Result<List<ProjectDto.Index>>> GetProjectsAsync(Guid stationId, CancellationToken ct = default) {
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

	public async Task<Result<Guid>> CreateAsync(MeasurementStationRequest.Create request,
		CancellationToken ct = default) {
		Location? location = await dbContext.Set<Location>()
			.SingleOrDefaultAsync(l => l.Id == request.LocationId, ct);
		if (location is null) {
			return Result.NotFound();
		}

		Project? project = await dbContext.Set<Project>()
			.SingleOrDefaultAsync(p => p.Id == request.ProjectId, ct);
		if (project is null) {
			return Result.NotFound();
		}

		DateTime installedAt = request.InstalledAt ?? DateTime.UtcNow;

		MeasurementStation station = new(
			request.Code,
			request.Status,
			location,
			new Installation(installedAt),
			new[] { project }
		);

		dbContext.MeasurementStations.Add(station);
		await dbContext.SaveChangesAsync(ct);

		return Result.Success(station.Id);
	}

	public async Task<Result> UpdateAsync(Guid id, MeasurementStationRequest.Edit request,
		CancellationToken ct = default) {
		MeasurementStation? station = await dbContext.MeasurementStations
			.Include(s => s.Location)
			.Include(s => s.Projects)
			.SingleOrDefaultAsync(s => s.Id == id, ct);

		if (station is null) {
			return Result.NotFound();
		}

		station.UpdateStatus(request.Status);

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