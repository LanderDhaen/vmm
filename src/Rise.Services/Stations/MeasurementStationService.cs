using Microsoft.EntityFrameworkCore;

using Rise.Domain.Locations;
using Rise.Domain.Projects;
using Rise.Domain.Stations;
using Rise.Domain.Stations.ValueObjects;
using Rise.Persistence;
using Rise.Shared.Common;
using Rise.Shared.Locations;
using Rise.Shared.Stations;

namespace Rise.Services.Stations;

public class MeasurementStationService(ApplicationDbContext dbContext) : IMeasurementStationService {
	public async Task<Result<MeasurementStationResponse.Index>> GetIndexAsync(
		QueryRequest.SkipTake request,
		CancellationToken ctx = default) {
		IQueryable<MeasurementStation> query = dbContext.MeasurementStations.AsQueryable();

		int totalCount = await query.CountAsync(ctx);

		if (!string.IsNullOrWhiteSpace(request.OrderBy)) {
			if (request.OrderBy == nameof(MeasurementStationDto.Index.InstalledAt)) {
				query = query.OrderBy(s => s.Installation.InstalledAt);
			} else if (request.OrderBy == nameof(MeasurementStationDto.Index.RemovedAt)) {
				query = query.OrderBy(s => s.Installation.RemovedAt);
			} else {
				query = query.OrderBy(s => EF.Property<object>(s, request.OrderBy));
			}
		} else {
			query = query.OrderBy(s => s.Code);
		}

		IQueryable<MeasurementStation> page = query
			.AsNoTracking()
			.Skip(request.Skip)
			.Take(request.Take);

		List<MeasurementStationDto.Index> stations = await page
			.Select(s => new MeasurementStationDto.Index {
				Id = s.Id,
				Code = s.Code,
				Status = s.Status,
				InstalledAt = s.Installation.InstalledAt,
				RemovedAt = s.Installation.RemovedAt,

				Location = new LocationDto.Index {
					Id = s.Location.Id,
					Name = s.Location.Name,
					Address = new AddressDto.Index {
						AddressLine1 = s.Location.Address.Addressline1,
						AddressLine2 = s.Location.Address.Addressline2,
						City = s.Location.Address.City,
						PostalCode = s.Location.Address.PostalCode,
						CountryIso = s.Location.Address.CountryIso
					},
					Coordinate = new CoordinateDto.Index {
						X = s.Location.Coordinate.X,
						Y = s.Location.Coordinate.Y
					}
				},

				ProjectCode = s.Project.Code,
				ProjectName = s.Project.Name
			})
			.ToListAsync(ctx);

		return Result.Success(new MeasurementStationResponse.Index {
			Stations = stations,
			TotalCount = totalCount
		});
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
			project
		);

		dbContext.MeasurementStations.Add(station);
		await dbContext.SaveChangesAsync(ct);

		return Result.Success(station.Id);
	}

	public async Task<Result> UpdateAsync(Guid id, MeasurementStationRequest.Edit request,
		CancellationToken ct = default) {
		MeasurementStation? station = await dbContext.MeasurementStations
			.Include(s => s.Location)
			.Include(s => s.Project)
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

		// Change Project without needing a public setter or helper method:
		if (station.Project.Id != request.ProjectId) {
			Project? newProject = await dbContext.Set<Project>()
				.SingleOrDefaultAsync(p => p.Id == request.ProjectId, ct);
			if (newProject is null) {
				return Result.NotFound();
			}

			// Use EF reference API to set the navigation (works even with private setter)
			dbContext.Entry(station).Reference(s => s.Project).CurrentValue = newProject;
		}

		await dbContext.SaveChangesAsync(ct);
		return Result.Success();
	}
}