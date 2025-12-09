using Microsoft.EntityFrameworkCore;

using Rise.Domain.Locations;
using Rise.Domain.MaintObjects;
using Rise.Persistence;
using Rise.Shared.Common;
using Rise.Shared.Locations;
using Rise.Shared.MaintObjects;
using Rise.Shared.Stations;

using MaintObjStatus = Rise.Domain.MaintObjects.MaintObjStatus;
using MaintObjType = Rise.Domain.MaintObjects.MaintObjType;
using SharedStatus = Rise.Shared.MaintObjects.MaintObjStatus;
using SharedType = Rise.Shared.MaintObjects.MaintObjType;
using SharedMaintObjModificationType = Rise.Shared.MaintObjects.MaintObjModificationType;

namespace Rise.Services.MaintObjects;

/// <summary>
///     Service for maintenance objects.
/// </summary>
/// <param name="dbContext"></param>
public class MaintObjService(ApplicationDbContext dbContext)
	: IMaintObjService {
	public async Task<Result<MaintObjResponse.Index>> GetIndexAsync(QueryRequest.SkipTake req,
		CancellationToken ctx = default) {
		IQueryable<MaintObj> query = dbContext.MaintObjects.AsQueryable().AsNoTracking();

		if (req.Filters.TryGetValue("Status", out object? statusObj)
			&& statusObj is string and "Active") {
			if (req.Filters.TryGetValue("StationId", out object? stationIdObj)
				&& stationIdObj is string stationIdStr
				&& Guid.TryParse(stationIdStr, out Guid parsedId)) {
				query = query.Where(p => p.MaintObjStatus == MaintObjStatus.Inactive || p.Station!.Id == parsedId);
			}
		}

		query = query.Where(m =>
			m.SerialNumber.Contains(req.SearchTerm) ||
			m.Name.Contains(req.SearchTerm)
		);

		int totalCount = await query.CountAsync(ctx);

		// Sorting

		if (!string.IsNullOrWhiteSpace(req.OrderBy)) {
			bool desc = req.OrderDescending;
			query = req.OrderBy switch {
				nameof(MaintObj.MaintObjPlan)
					=> desc
						? query.OrderByDescending(d => d.MaintObjPlan.Name)
						: query.OrderBy(d => d.MaintObjPlan.Name),


				nameof(MaintObj.MaintObjStatus)
					=> desc
						? query.OrderByDescending(d => d.MaintObjStatus)
						: query.OrderBy(d => d.MaintObjStatus),

				nameof(MaintObj.Location)
					=> desc
						? query.OrderByDescending(d => d.Location.Name)
						: query.OrderBy(d => d.Location.Name),

				_ => desc
					? query.OrderByDescending(e => EF.Property<object>(e, req.OrderBy))
					: query.OrderBy(e => EF.Property<object>(e, req.OrderBy))
			};
		} else {
			query = query.OrderBy(d => d.Id);
		}

		// Paginating

		query = query
			.Skip(req.Skip)
			.Take(req.Take);

		// Selecting

		List<MaintObjDto.Index> maintObjects = await query
			.Select(m => new MaintObjDto.Index {
				Id = m.Id,
				Name = m.Name,
				SerialNumber = m.SerialNumber,
				Brand = m.Brand,
				MaintObjType = (SharedType)m.MaintObjType,
				MaintObjStatus = (SharedStatus)m.MaintObjStatus,
				Location = m.Location.Name,
				StationCode = m.Station != null ? m.Station.Code : null
			}).ToListAsync(ctx);

		return Result.Success(new MaintObjResponse.Index {
			MaintObjects = maintObjects,
			TotalCount = totalCount
		});
	}

	public async Task<Result<MaintObjResponse.Create>> CreateAsync(MaintObjRequest.Create request,
		CancellationToken ctx = default) {
		if (await dbContext.MaintObjects.AnyAsync(x => x.SerialNumber == request.SerialNumber, ctx)) {
			Log.Warning("Onderhoudsobject met serienummer '{SerialNumber}' bestaat al.", request.SerialNumber);
			return Result.Conflict($"Onderhoudsobject met serienummer '{request.SerialNumber}' bestaat al.");
		}

		Location? location = await dbContext.Locations
			.FirstOrDefaultAsync(x => x.Id == request.LocationId, ctx);

		if (location is null) {
			return Result.NotFound("De locatie werd niet gevonden.");
		}

		// TODO: Update the form to allow users to pick their own plan from the options in the db (if we have time)

		MaintObjPlan? plan = await dbContext.MaintObjPlans
			.FirstOrDefaultAsync(s => s.Name == "Standaard", ctx);

		if (plan is null) {
			return Result.NotFound("Het onderhoudstype werd niet gevonden.");
		}

		MaintObj mo = new(request.Name, request.SerialNumber, (MaintObjType)request.MaintObjType, plan, request.Brand,
			location);

		dbContext.MaintObjects.Add(mo);

		await dbContext.SaveChangesAsync(ctx);

		return Result.Created(new MaintObjResponse.Create {
			MaintObjId = mo.Id
		});
	}

	public async Task<Result<MaintObjResponse.Detail>> GetDetailAsync(Guid maintObjId, CancellationToken ct = default) {
		IQueryable<MaintObj> query = dbContext.MaintObjects.AsQueryable().AsNoTracking();

		MaintObjDto.Detail? maintObj = await query
			.Where(m => m.Id == maintObjId)
			.Select(m => new MaintObjDto.Detail {
				Id = m.Id,
				Name = m.Name,
				SerialNumber = m.SerialNumber,
				Brand = m.Brand,
				MaintObjType = (SharedType)m.MaintObjType,
				MaintObjStatus = (SharedStatus)m.MaintObjStatus,
				Location = new LocationDto.Detail {
					Id = m.Location.Id,
					Name = m.Location.Name,
					AddressLine1 = m.Location.Address.Addressline1,
					AddressLine2 = m.Location.Address.Addressline2,
					City = m.Location.Address.City,
					PostalCode = m.Location.Address.PostalCode,
					Country = m.Location.Address.CountryIso,
					X = m.Location.Coordinate.X,
					Y = m.Location.Coordinate.Y
				},
				StationCode = m.Station != null ? m.Station.Code : null,
				InstalledAt = m.Installation != null ? m.Installation.InstalledAt : null,
				RemovedAt = m.Installation != null ? m.Installation.RemovedAt : null,
				MaintenanceIntervalDays = m.MaintObjPlan.MaintenanceIntervalDays,
				CalibrationIntervalDays = m.MaintObjPlan.CalibrationIntervalDays,
				Station = m.Station != null
					? new MeasurementStationDto.Detail {
						Id = m.Station.Id,
						Code = m.Station.Code,
						InstalledAt = m.Station.Installation.InstalledAt,
						RemovedAt = m.Station.Installation.RemovedAt,
						Location = new LocationDto.Detail {
							Id = m.Station.Location.Id,
							Name = m.Station.Location.Name,
							AddressLine1 = m.Station.Location.Address.Addressline1,
							AddressLine2 = m.Station.Location.Address.Addressline2,
							City = m.Station.Location.Address.City,
							PostalCode = m.Station.Location.Address.PostalCode,
							Country = m.Station.Location.Address.CountryIso,
							X = m.Station.Location.Coordinate.X,
							Y = m.Station.Location.Coordinate.Y
						}
					}
					: null
			}).SingleOrDefaultAsync(ct);

		if (maintObj is null) {
			return Result.NotFound($"Onderhoudsobject met Id '{maintObjId}' was niet gevonden.");
		}

		return Result.Success(new MaintObjResponse.Detail {
			MaintObj = maintObj
		});
	}

	public async Task<Result<MaintObjResponse.History>>
		GetHistoryAsync(Guid maintObjId, CancellationToken ct = default) {

		MaintObj? maintObj = await dbContext.MaintObjects
			.AsNoTracking()
			.Include(x => x.MaintObjPlan)
			.Include(x => x.Installation)
			.Include(x => x.Station)
			.Include(x => x.Location)
			.Include(x => x.LastScannedBy)
			.Include(x => x.MaintObjHistories)
			.ThenInclude(h => h.MaintObjPlan)
			.Include(x => x.MaintObjHistories)
			.ThenInclude(h => h.Installation)
			.Include(x => x.MaintObjHistories)
			.ThenInclude(h => h.Station)
			.Include(x => x.MaintObjHistories)
			.ThenInclude(h => h.Location)
			.Include(x => x.MaintObjHistories)
			.ThenInclude(h => h.LastScannedBy)
			.Include(p => p.MaintObjHistories.OrderByDescending(h => h.ValidityRange.ValidFrom))
			.Where(x => x.Id == maintObjId)
			.SingleOrDefaultAsync(ct);

		if (maintObj is null) {
			return Result.NotFound($"Onderhoudsobject met Id '{maintObjId}' was niet gevonden.");
		}

		List<MaintObjDto.Modification> modifications = [];

		foreach (var m in maintObj.GetModifications()) {
			modifications.Add(new MaintObjDto.Modification {
				MaintObjModificationType = (SharedMaintObjModificationType)m.MaintObjModificationType,
				ModifiedAt = m.ModifiedAt,
				PreviousValue = m.PreviousValue,
				CurrentValue = m.CurrentValue,
			});
		}

		return Result.Success(new MaintObjResponse.History {
			Modifications = modifications
		});
	}

	public async Task<Result> EditAsync(MaintObjRequest.Edit req, CancellationToken ctx = default) {
		MaintObj? maintObj = await dbContext.MaintObjects
			.SingleOrDefaultAsync(x => x.Id == req.MaintObjId, ctx);

		if (maintObj is null) {
			return Result.NotFound($"Onderhoudsobject met Id '{req.MaintObjId}' was niet gevonden.");
		}

		if (await dbContext.MaintObjects.AnyAsync(x => x.SerialNumber == req.SerialNumber && x.Id != req.MaintObjId,
				ctx)) {
			Log.Warning("Onderhoudsobject met serienummer '{SerialNumber}' bestaat al.", req.SerialNumber);
			return Result.Conflict($"Onderhoudsobject met serienummer '{req.SerialNumber}' bestaat al.");
		}

		MaintObjPlan? plan = await dbContext.MaintObjPlans
			.FirstOrDefaultAsync(s => s.Name == "Standaard", ctx);

		if (plan is null) {
			return Result.NotFound("Het onderhoudstype werd niet gevonden.");
		}

		maintObj.Edit(req.Name, req.SerialNumber, plan, req.Brand);

		await dbContext.SaveChangesAsync(ctx);

		return Result.Success();
	}

	public async Task<Result> UpdateLocationAsync(MaintObjRequest.UpdateLocation req,
		CancellationToken ctx = default) {
		MaintObj? maintObj = await dbContext.MaintObjects
			.Include(p => p.Location)
			.SingleOrDefaultAsync(x => x.Id == req.MaintObjId, ctx);

		if (maintObj is null) {
			return Result.NotFound($"Onderhoudsobject met id '{req.MaintObjId}' is niet gevonden.");
		}

		Location location = await dbContext.Locations
			.SingleOrDefaultAsync(l => l.Id == req.LocationId, ctx);

		if (location is null) {
			return Result.NotFound("Locatie niet gevonden.");
		}

		maintObj.MoveToStockLocation(location);

		await dbContext.SaveChangesAsync(ctx);

		return Result.Success();
	}

	public async Task<Result<MaintObjResponse.BySerial>> GetBySerialAsync(string serialNumber,
		CancellationToken ct = default) {
		if (string.IsNullOrWhiteSpace(serialNumber)) {
			return Result.Invalid([
				new ValidationError {
					Identifier = nameof(serialNumber),
					ErrorMessage = "Serienummer is verplicht."
				}
			]);
		}

		MaintObj? maintObj = await dbContext.MaintObjects
			.AsNoTracking()
			.FirstOrDefaultAsync(m => m.SerialNumber == serialNumber, ct);

		if (maintObj is null) {
			return Result.NotFound();
		}

		return Result.Success(new MaintObjResponse.BySerial {
			Id = maintObj.Id,
			SerialNumber = maintObj.SerialNumber,
			Name = maintObj.Name
		});
	}
}