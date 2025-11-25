using Microsoft.EntityFrameworkCore;

using Rise.Domain.Locations;
using Rise.Domain.MaintObjects;
using Rise.Domain.Projects;
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

		Rise.Domain.MaintObjects.MaintObjType? type = await dbContext.MaintObjTypes
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

		DateTime installedAt = req.InstalledAt ?? maintObj.Installation.InstalledAt;

		maintObj.Edit(req.Name, req.SerialNumber, (MaintObjType)req.MaintObjType, req.Brand, new Installation(installedAt, req.RemovedAt));

		await dbContext.SaveChangesAsync(ctx);

		return Result.Success();
	}
}