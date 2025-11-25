using Microsoft.EntityFrameworkCore;

using Rise.Domain.MaintObjects;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Common;
using Rise.Shared.MaintObjects;

using MaintObjType = Rise.Shared.MaintObjects.MaintObjType;
using SharedStatus = Rise.Shared.MaintObjects.MaintObjStatus;


namespace Rise.Services.MaintObjects;

/// <summary>
///     Service for maintenance objects. Note the use of <see cref="ISessionContextProvider" /> to get the current user in
///     this layer
///     of the application.
/// </summary>
/// <param name="dbContext"></param>
/// <param name="sessionContextProvider"></param>
public class MaintObjService(ApplicationDbContext dbContext, ISessionContextProvider sessionContextProvider)
	: IMaintObjService {
	public async Task<Result<MaintObjResponse.Index>> GetIndexAsync(QueryRequest.SkipTake req,
		CancellationToken ctx = default) {
		IQueryable<MaintObj> query = dbContext.MaintObjects.AsQueryable();

		int totalCount = await query.CountAsync(ctx);

		if (!string.IsNullOrWhiteSpace(req.OrderBy)) {
			bool desc = req.OrderDescending;
			switch (req.OrderBy) {
			// TODO: sorting
			default:
				query = desc
					? query.OrderByDescending(e => EF.Property<object>(e, req.OrderBy))
					: query.OrderBy(e => EF.Property<object>(e, req.OrderBy));
				break;
			}
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
}