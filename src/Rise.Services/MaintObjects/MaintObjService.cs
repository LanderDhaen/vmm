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
			.Include(d => d.MaintObjType)
			.Skip(req.Skip)
			.Take(req.Take);

		var raw = await page
			.Select(d => new {
				d.Id,
				d.Name,
				d.SerialNumber,
				MaintObjTypeCode = d.MaintObjType != null ? d.MaintObjType.Code : null,
				d.MaintObjStatus,
				InstalledAt = d.Installation != null ? d.Installation.InstalledAt : (DateTime?)null,
				IntervalDays = d.MaintObjType != null ? d.MaintObjType.DefaultMaintenanceIntervalDays : null
			})
			.ToListAsync(ctx);

		List<MaintObjDto.Index> maintObjects = raw.Select(d => {
			MaintObjType parsedType =
				Enum.TryParse(d.MaintObjTypeCode, out MaintObjType tmp) ? tmp : MaintObjType.Sonde;

			DateTime? next = d.InstalledAt.HasValue && d.IntervalDays.HasValue
				? d.InstalledAt.Value.AddDays(d.IntervalDays.Value)
				: null;

			return new MaintObjDto.Index {
				Id = d.Id,
				Name = d.Name,
				SerialNumber = d.SerialNumber,
				MaintObjType = parsedType,
				MaintObjStatus = (SharedStatus)d.MaintObjStatus,
				NextMaintenanceDate = next
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
			.SingleOrDefaultAsync(d => d.Id == maintObjId, ct);

		if (maintObj is null) {
			return Result.NotFound();
		}

		MaintObjType parsedType = MaintObjType.Sonde;

		if (maintObj.MaintObjType?.Code is { } code && Enum.TryParse(code, out MaintObjType tmp)) {
			parsedType = tmp;
		}

		MaintObjDto.Detail dto = new() {
			Id = maintObj.Id,
			Name = maintObj.Name,
			SerialNumber = maintObj.SerialNumber,
			MaintObjType = parsedType,
			MaintObjStatus = (SharedStatus)maintObj.MaintObjStatus,
			Brand = maintObj.Brand,
			MeasurementStationId = maintObj.MeasurementStationId,
			MeasurementStationCode = maintObj.Station?.Code,
			IsInStock = maintObj.IsInStock,
			InstalledAt = maintObj.Installation?.InstalledAt,
			RemovedAt = maintObj.Installation?.RemovedAt
		};

		// compute next maintenance date if possible
		if (dto.InstalledAt.HasValue && maintObj.MaintObjType?.DefaultMaintenanceIntervalDays.HasValue == true) {
			dto.NextMaintenanceDate =
				dto.InstalledAt.Value.AddDays(maintObj.MaintObjType.DefaultMaintenanceIntervalDays.Value);
		} else {
			dto.NextMaintenanceDate = null;
		}

		return Result.Success(new MaintObjResponse.Detail { MaintObj = dto });
	}
}