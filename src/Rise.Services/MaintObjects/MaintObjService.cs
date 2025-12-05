using Microsoft.EntityFrameworkCore;

using Rise.Domain.Common;
using Rise.Domain.Locations;
using Rise.Domain.MaintObjects;
using Rise.Persistence;
using Rise.Shared.Common;
using Rise.Shared.MaintObjects;

using MaintObjType = Rise.Shared.MaintObjects.MaintObjType;
using SharedStatus = Rise.Shared.MaintObjects.MaintObjStatus;

namespace Rise.Services.MaintObjects;

/// <summary>
///     Service for maintenance objects.
/// </summary>
/// <param name="dbContext"></param>
public class MaintObjService(ApplicationDbContext dbContext)
	: IMaintObjService {
	public async Task<Result<MaintObjResponse.Index>> GetIndexAsync(QueryRequest.SkipTake req,
		CancellationToken ctx = default) {
		IQueryable<MaintObj> query = dbContext.MaintObjects.AsQueryable();

		int totalCount = await query.CountAsync(ctx);

		if (!string.IsNullOrWhiteSpace(req.OrderBy)) {
			bool desc = req.OrderDescending;
			query = req.OrderBy switch {
				nameof(MaintObj.MaintObjType)
					=> desc
						? query.OrderByDescending(d => d.MaintObjType!.Code)
						: query.OrderBy(d => d.MaintObjType!.Code),


				nameof(MaintObj.MaintObjStatus)
					=> desc
						? query.OrderByDescending(d => d.MaintObjStatus)
						: query.OrderBy(d => d.MaintObjStatus),

				nameof(MaintObj.Location)
					=> desc
						? query.OrderByDescending(d => d.Location!.Name)
						: query.OrderBy(d => d.Location!.Name),

				_ => desc
					? query.OrderByDescending(e => EF.Property<object>(e, req.OrderBy))
					: query.OrderBy(e => EF.Property<object>(e, req.OrderBy))
			};
		} else {
			query = query.OrderBy(d => d.Id);
		}

		IQueryable<MaintObj> page = query
			.AsNoTracking()
			.Include(mo => mo.MaintObjType)
			.Include(mo => mo.Station)
			.Include(mo => mo.Location)
			.Skip(req.Skip)
			.Take(req.Take);

		var raw = await page
			.Select(mo => new {
				mo.Id,
				mo.Name,
				mo.SerialNumber,
				MaintObjTypeCode = mo.MaintObjType != null ? mo.MaintObjType.Code : null,
				mo.MaintObjStatus,
				LocationName = mo.Location != null ? mo.Location.Name :
					mo.Station != null ? mo.Station.Location.Name : null
			})
			.ToListAsync(ctx);

		List<MaintObjDto.Index> maintObjects = raw.Select(mo => {
			// safe parse of type code to shared enum
			MaintObjType parsedType = MaintObjType.Sonde;
			if (mo.MaintObjTypeCode is not null &&
			    Enum.TryParse(mo.MaintObjTypeCode, out MaintObjType tmp)) {
				parsedType = tmp;
			}

			return new MaintObjDto.Index {
				Id = mo.Id,
				Name = mo.Name,
				SerialNumber = mo.SerialNumber,
				MaintObjType = parsedType,
				MaintObjStatus = (SharedStatus)mo.MaintObjStatus,
				Location = mo.LocationName ?? string.Empty
			};
		}).ToList();

		return Result.Success(new MaintObjResponse.Index {
			MaintObjects = maintObjects,
			TotalCount = totalCount
		});
	}

	public async Task<Result<MaintObjResponse.Create>> CreateAsync(MaintObjRequest.Create request,
		CancellationToken ctx = default) {
		if (await dbContext.MaintObjects.AnyAsync(x => x.SerialNumber == request.SerialNumber)) {
			Log.Warning("Onderhoudsobject met serienummer '{SerialNumber}' bestaat al.", request.SerialNumber);
			return Result.Conflict($"Onderhoudsobject met serienummer '{request.SerialNumber}' bestaat al.");
		}

		Location? location = await dbContext.Locations
			.FirstOrDefaultAsync(x => x.Id == request.LocationId);

		if (location is null) {
			return Result.NotFound("De locatie werd niet gevonden.");
		}

		Domain.MaintObjects.MaintObjType? type = await dbContext.MaintObjTypes
			.FirstOrDefaultAsync(s => s.Name == request.MaintObjType.ToString(), ctx);

		if (type is null) {
			return Result.NotFound("Het onderhoudstype werd niet gevonden.");
		}

		MaintObj mo = new(request.Name, request.SerialNumber, type,
			request.Brand, null, true, location);

		dbContext.MaintObjects.Add(mo);

		await dbContext.SaveChangesAsync(ctx);

		return Result.Created(new MaintObjResponse.Create {
			MaintObjId = mo.Id
		});
	}

	public async Task<Result<MaintObjResponse.Detail>> GetDetailAsync(Guid maintObjId,
		CancellationToken ct = default) {
		MaintObj? maintObj = await dbContext.MaintObjects
			.AsNoTracking()
			.Include(d => d.Station)
			.Include(d => d.MaintObjType)
			.Include(d => d.Location)
			.SingleOrDefaultAsync(d => d.Id == maintObjId, ct);

		if (maintObj is null) {
			return Result.NotFound();
		}

		MaintObjType parsedType = MaintObjType.Sonde;

		if (maintObj.MaintObjType?.Code is { } code && Enum.TryParse(code, out MaintObjType tmp)) {
			parsedType = tmp;
		}

		DateTime? installedAtOpt = maintObj.Installation?.InstalledAt;
		DateTime? nextMaintenance = null;
		DateTime? nextCalibration = null;

		if (installedAtOpt is DateTime installedAtValue && maintObj.MaintObjType is not null) {
			nextMaintenance = installedAtValue.AddDays(maintObj.MaintObjType.MaintenanceIntervalDays);
			nextCalibration = installedAtValue.AddDays(maintObj.MaintObjType.CalibrationIntervalDays);
		}

		MaintObjDto.Detail dto = new() {
			Id = maintObj.Id,
			Name = maintObj.Name,
			SerialNumber = maintObj.SerialNumber,
			MaintObjType = parsedType,
			MaintObjStatus = (SharedStatus)maintObj.MaintObjStatus,
			Brand = maintObj.Brand,
			MeasurementStationCode = maintObj.Station?.Code,
			Location = maintObj.Location?.Name ?? maintObj.Station?.Location.Name,
			IsInStock = maintObj.IsInStock,
			InstalledAt = installedAtOpt,
			RemovedAt = maintObj.Installation?.RemovedAt,
			NextMaintenanceDate = nextMaintenance,
			NextCalibrationDate = nextCalibration
		};

		return Result.Success(new MaintObjResponse.Detail { MaintObj = dto });
	}

	public async Task<Result> EditAsync(MaintObjRequest.Edit req, CancellationToken ctx = default) {
		MaintObj? maintObj = await dbContext.MaintObjects
			.SingleOrDefaultAsync(x => x.Id == req.MaintObjId, ctx);

		if (maintObj is null) {
			return Result.NotFound($"Onderhoudsobject met Id '{req.MaintObjId}' was niet gevonden.");
		}

		if (await dbContext.MaintObjects.AnyAsync(x => x.SerialNumber == req.SerialNumber && x.Id != req.MaintObjId)) {
			Log.Warning("Onderhoudsobject met serienummer '{SerialNumber}' bestaat al.", req.SerialNumber);
			return Result.Conflict($"Onderhoudsobject met serienummer '{req.SerialNumber}' bestaat al.");
		}

		Location? location = await dbContext.Locations
			.FirstOrDefaultAsync(x => x.Id == req.LocationId);

		if (location is null) {
			return Result.NotFound("De locatie werd niet gevonden.");
		}

		Domain.MaintObjects.MaintObjType? type = await dbContext.MaintObjTypes
			.FirstOrDefaultAsync(s => s.Name == req.MaintObjType.ToString(), ctx);

		if (type is null) {
			return Result.NotFound("Het onderhoudstype werd niet gevonden.");
		}

		maintObj.Edit(req.Name, req.SerialNumber, type, req.Brand,
			new Installation((DateTime)req.InstalledAt, req.RemovedAt), location);

		await dbContext.SaveChangesAsync(ctx);

		return Result.Success();
	}

	public async Task<Result<MaintObjResponse.BySerial>> GetBySerialAsync(string serialNumber,
		CancellationToken ct = default) {
		if (string.IsNullOrWhiteSpace(serialNumber)) {
			return Result.Invalid(new[]
			{
				new ValidationError
				{
					Identifier = nameof(serialNumber),
					ErrorMessage = "Serienummer is verplicht."
				}
			});
		}

		MaintObj? maintObj = await dbContext.MaintObjects
			.AsNoTracking()
			.FirstOrDefaultAsync(m => m.SerialNumber == serialNumber, ct);

		if (maintObj is null)
			return Result.NotFound();

		return Result.Success(new MaintObjResponse.BySerial {
			Id = maintObj.Id,
			SerialNumber = maintObj.SerialNumber,
			Name = maintObj.Name
		});
	}

}