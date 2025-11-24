using Microsoft.EntityFrameworkCore;

using Rise.Domain.MaintObjects;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Common;
using Rise.Shared.MaintObjects;

using MaintObjType = Rise.Shared.MaintObjects.MaintObjType;
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
			.Skip(req.Skip)
			.Take(req.Take);
		List<MaintObjDto.Index> maintObjects = await page
			.Select(d => new MaintObjDto.Index {
				Id = d.Id,
				Name = d.Name,
				SerialNumber = d.SerialNumber,
				MaintObjType = (MaintObjType)d.MaintObjType,
				Status = (SharedStatus)d.Status
			})
			.ToListAsync(ctx);

		return Result.Success(new MaintObjResponse.Index {
			MaintObjects = maintObjects,
			TotalCount = totalCount
		});
	}
}