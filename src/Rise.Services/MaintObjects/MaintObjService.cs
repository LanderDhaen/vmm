using Microsoft.EntityFrameworkCore;

using Rise.Domain.Common;
using Rise.Domain.MaintObjects;
using Rise.Domain.Projects;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Common;
using Rise.Shared.MaintObjects;

using MaintObjType = Rise.Domain.MaintObjects.MaintObjType;
using SharedMaintObjType = Rise.Shared.MaintObjects.MaintObjType;
using SharedStatus = Rise.Shared.Projects.Status;

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
			// TODO
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
			.Skip(req.Skip)
			.Take(req.Take);
		List<MaintObjDto.Index> maintObjects = await page
			.Select(d => new MaintObjDto.Index {
				Id = d.Id,
				Name = d.Name,
				SerialNumber = d.SerialNumber,
				MaintObjType = (SharedMaintObjType)d.MaintObjType,
				Status = (SharedStatus)d.Status
			})
			.ToListAsync(ctx);

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