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
			.Skip(req.Skip)
			.Take(req.Take);

		var raw = await page
			.Select(mo => new {
				mo.Id,
				mo.Name,
				mo.SerialNumber,
				MaintObjTypeCode = mo.MaintObjType != null ? mo.MaintObjType.Code : null,
				mo.MaintObjStatus
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
				MaintObjStatus = (SharedStatus)mo.MaintObjStatus
			};
		}).ToList();

		return Result.Success(new MaintObjResponse.Index {
			MaintObjects = maintObjects,
			TotalCount = totalCount
		});
	}
}