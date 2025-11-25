using Microsoft.EntityFrameworkCore;

using Rise.Domain.Common;
using Rise.Domain.MaintObjects;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Common;
using Rise.Shared.MaintObjects;

using MaintObjType = Rise.Domain.MaintObjects.MaintObjType;
using SharedMaintObjType = Rise.Shared.MaintObjects.MaintObjType;
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
			// safe parse of type code to shared enum
			MaintObjType parsedType = MaintObjType.Sonde;
			if (d.MaintObjTypeCode is not null &&
			    Enum.TryParse(d.MaintObjTypeCode, out MaintObjType tmp)) {
				parsedType = tmp;
			}

			DateTime? next = null;
			if (d.InstalledAt.HasValue && d.IntervalDays.HasValue) {
				next = d.InstalledAt.Value.AddDays(d.IntervalDays.Value);
			}

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

	public async Task<Result<MaintObjResponse.Create>> CreateAsync(MaintObjRequest.Create request,
		CancellationToken ctx = default) {
		if (await dbContext.MaintObjects.AnyAsync(x => x.SerialNumber == request.SerialNumber)) {
			Log.Warning("Onderhoudsobject met serienummer '{SerialNumber}' bestaat al.", request.SerialNumber);
			return Result.Conflict($"Onderhoudsobject met serienummer '{request.SerialNumber}' bestaat al.");
		}

		DateTime installedAt = request.InstalledAt ?? DateTime.UtcNow;

		MaintObj mo = new(request.Name, request.SerialNumber, (MaintObjType)request.MaintObjType,
			request.Brand, new Installation(installedAt));

		dbContext.MaintObjects.Add(mo);

		await dbContext.SaveChangesAsync(ctx);

		return Result.Created(new MaintObjResponse.Create {
			MaintObjId = mo.Id
		});
	}
}